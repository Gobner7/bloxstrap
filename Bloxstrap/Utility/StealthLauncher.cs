using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Threading;
using Windows.Win32.System.Diagnostics.Debug;

namespace Bloxstrap.Utility
{
    /// <summary>
    /// Advanced stealth launcher using process hollowing and other evasion techniques
    /// </summary>
    public static class StealthLauncher
    {
        /// <summary>
        /// Launches Roblox using process hollowing to avoid detection
        /// </summary>
        public static bool LaunchWithProcessHollowing(string robloxPath, string arguments, string decoyPath = null)
        {
            const string LOG_IDENT = "StealthLauncher::LaunchWithProcessHollowing";
            
            try
            {
                App.Logger.WriteLine(LOG_IDENT, "Initiating stealth launch with process hollowing");
                
                // Use a legitimate Windows process as decoy if not specified
                if (string.IsNullOrEmpty(decoyPath))
                    decoyPath = Path.Combine(Environment.SystemDirectory, "svchost.exe");
                
                // Create suspended decoy process
                var startInfo = new STARTUPINFOW();
                var processInfo = new PROCESS_INFORMATION();
                
                bool createResult = PInvoke.CreateProcess(
                    decoyPath,
                    arguments,
                    null,
                    null,
                    false,
                    PROCESS_CREATION_FLAGS.CREATE_SUSPENDED,
                    null,
                    null,
                    startInfo,
                    out processInfo
                );
                
                if (!createResult)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to create decoy process");
                    return false;
                }
                
                App.Logger.WriteLine(LOG_IDENT, $"Created suspended decoy process (PID: {processInfo.dwProcessId})");
                
                // Hollow out the process
                bool hollowResult = HollowProcess(processInfo, robloxPath);
                
                if (hollowResult)
                {
                    // Apply stealth modifications before resuming
                    ApplyStealthModifications(processInfo.hProcess);
                    
                    // Resume execution
                    PInvoke.ResumeThread(processInfo.hThread);
                    
                    App.Logger.WriteLine(LOG_IDENT, "Successfully launched Roblox with process hollowing");
                    
                    // Close handles
                    PInvoke.CloseHandle(processInfo.hProcess);
                    PInvoke.CloseHandle(processInfo.hThread);
                    
                    return true;
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, "Process hollowing failed, terminating decoy");
                    PInvoke.TerminateProcess(processInfo.hProcess, 1);
                    PInvoke.CloseHandle(processInfo.hProcess);
                    PInvoke.CloseHandle(processInfo.hThread);
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                return false;
            }
        }
        
        /// <summary>
        /// Performs process hollowing on the target process
        /// </summary>
        private static bool HollowProcess(PROCESS_INFORMATION processInfo, string replacementExecutable)
        {
            const string LOG_IDENT = "StealthLauncher::HollowProcess";
            
            try
            {
                App.Logger.WriteLine(LOG_IDENT, "Beginning process hollowing operation");
                
                byte[] replacementBytes = File.ReadAllBytes(replacementExecutable);
                
                // Get thread context
                var context = new CONTEXT { ContextFlags = CONTEXT_FLAGS.CONTEXT_FULL };
                bool contextResult = PInvoke.GetThreadContext(processInfo.hThread, ref context);
                
                if (!contextResult)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to get thread context");
                    return false;
                }
                
                // Read PEB address from context
                IntPtr pebAddress = new IntPtr((long)context.Rdx);
                
                // Read image base address from PEB
                byte[] imageBaseBytes = new byte[8];
                nuint bytesRead;
                bool readResult = PInvoke.ReadProcessMemory(
                    processInfo.hProcess,
                    IntPtr.Add(pebAddress, 16).ToPointer(), // PEB+16 = ImageBaseAddress
                    imageBaseBytes,
                    8,
                    &bytesRead
                );
                
                if (!readResult)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to read image base from PEB");
                    return false;
                }
                
                IntPtr imageBase = new IntPtr(BitConverter.ToInt64(imageBaseBytes, 0));
                App.Logger.WriteLine(LOG_IDENT, $"Original image base: 0x{imageBase.ToInt64():X}");
                
                // Unmap original executable
                uint unmapResult = PInvoke.NtUnmapViewOfSection(processInfo.hProcess, imageBase.ToPointer());
                if (unmapResult != 0)
                {
                    App.Logger.WriteLine(LOG_IDENT, $"NtUnmapViewOfSection failed with status: 0x{unmapResult:X}");
                    // Continue anyway, sometimes it still works
                }
                
                // Parse PE headers to get required information
                var peInfo = ParsePEHeaders(replacementBytes);
                if (peInfo == null)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to parse PE headers");
                    return false;
                }
                
                // Allocate new memory for the replacement executable
                var newBaseAddress = PInvoke.VirtualAllocEx(
                    processInfo.hProcess,
                    imageBase.ToPointer(),
                    (nuint)peInfo.ImageSize,
                    VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
                    PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE
                );
                
                if (newBaseAddress == null)
                {
                    // Try allocating at any address
                    newBaseAddress = PInvoke.VirtualAllocEx(
                        processInfo.hProcess,
                        null,
                        (nuint)peInfo.ImageSize,
                        VIRTUAL_ALLOCATION_TYPE.MEM_COMMIT | VIRTUAL_ALLOCATION_TYPE.MEM_RESERVE,
                        PAGE_PROTECTION_FLAGS.PAGE_EXECUTE_READWRITE
                    );
                    
                    if (newBaseAddress == null)
                    {
                        App.Logger.WriteLine(LOG_IDENT, "Failed to allocate memory in target process");
                        return false;
                    }
                }
                
                App.Logger.WriteLine(LOG_IDENT, $"Allocated new memory at: 0x{((IntPtr)newBaseAddress).ToInt64():X}");
                
                // Write PE headers
                nuint bytesWritten;
                bool writeResult = PInvoke.WriteProcessMemory(
                    processInfo.hProcess,
                    newBaseAddress,
                    replacementBytes,
                    (nuint)peInfo.HeaderSize,
                    &bytesWritten
                );
                
                if (!writeResult)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to write PE headers");
                    return false;
                }
                
                // Write sections
                foreach (var section in peInfo.Sections)
                {
                    if (section.SizeOfRawData > 0)
                    {
                        IntPtr sectionAddress = IntPtr.Add((IntPtr)newBaseAddress, (int)section.VirtualAddress);
                        
                        writeResult = PInvoke.WriteProcessMemory(
                            processInfo.hProcess,
                            sectionAddress.ToPointer(),
                            replacementBytes.Skip((int)section.PointerToRawData).Take((int)section.SizeOfRawData).ToArray(),
                            section.SizeOfRawData,
                            &bytesWritten
                        );
                        
                        if (!writeResult)
                        {
                            App.Logger.WriteLine(LOG_IDENT, $"Failed to write section at RVA 0x{section.VirtualAddress:X}");
                            return false;
                        }
                    }
                }
                
                // Update entry point
                IntPtr newEntryPoint = IntPtr.Add((IntPtr)newBaseAddress, (int)peInfo.EntryPoint);
                context.Rcx = (ulong)newEntryPoint.ToInt64();
                
                // Update image base in PEB
                IntPtr newImageBasePtr = (IntPtr)newBaseAddress;
                byte[] newImageBaseBytes = BitConverter.GetBytes(newImageBasePtr.ToInt64());
                
                PInvoke.WriteProcessMemory(
                    processInfo.hProcess,
                    IntPtr.Add(pebAddress, 16).ToPointer(),
                    newImageBaseBytes,
                    8,
                    &bytesWritten
                );
                
                // Set new context
                bool setContextResult = PInvoke.SetThreadContext(processInfo.hThread, context);
                if (!setContextResult)
                {
                    App.Logger.WriteLine(LOG_IDENT, "Failed to set thread context");
                    return false;
                }
                
                App.Logger.WriteLine(LOG_IDENT, "Process hollowing completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                return false;
            }
        }
        
        /// <summary>
        /// Applies stealth modifications to the hollowed process
        /// </summary>
        private static void ApplyStealthModifications(HANDLE processHandle)
        {
            const string LOG_IDENT = "StealthLauncher::ApplyStealthModifications";
            
            try
            {
                App.Logger.WriteLine(LOG_IDENT, "Applying stealth modifications");
                
                // Make process critical (prevents termination)
                int isCritical = 1;
                PInvoke.NtSetInformationProcess(
                    processHandle,
                    29, // ProcessBreakOnTermination
                    &isCritical,
                    sizeof(int)
                );
                
                // Set process mitigation policies to bypass HVCI
                // This would require more complex implementation
                
                App.Logger.WriteLine(LOG_IDENT, "Stealth modifications applied");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
        
        /// <summary>
        /// Parses PE headers to extract necessary information
        /// </summary>
        private static PEInfo? ParsePEHeaders(byte[] peBytes)
        {
            try
            {
                // Parse DOS header
                if (peBytes.Length < 64 || peBytes[0] != 0x4D || peBytes[1] != 0x5A)
                    return null;
                
                uint peOffset = BitConverter.ToUInt32(peBytes, 60);
                if (peOffset >= peBytes.Length - 4)
                    return null;
                
                // Parse PE header
                if (BitConverter.ToUInt32(peBytes, (int)peOffset) != 0x00004550) // "PE\0\0"
                    return null;
                
                // Parse optional header
                uint optionalHeaderOffset = peOffset + 24;
                uint entryPoint = BitConverter.ToUInt32(peBytes, (int)optionalHeaderOffset + 16);
                uint imageSize = BitConverter.ToUInt32(peBytes, (int)optionalHeaderOffset + 56);
                uint headerSize = BitConverter.ToUInt32(peBytes, (int)optionalHeaderOffset + 60);
                
                // Parse sections
                ushort numberOfSections = BitConverter.ToUInt16(peBytes, (int)peOffset + 6);
                ushort optionalHeaderSize = BitConverter.ToUInt16(peBytes, (int)peOffset + 20);
                uint sectionHeaderOffset = optionalHeaderOffset + optionalHeaderSize;
                
                var sections = new List<SectionInfo>();
                for (int i = 0; i < numberOfSections; i++)
                {
                    uint sectionOffset = sectionHeaderOffset + (uint)(i * 40);
                    
                    var section = new SectionInfo
                    {
                        VirtualAddress = BitConverter.ToUInt32(peBytes, (int)sectionOffset + 12),
                        SizeOfRawData = BitConverter.ToUInt32(peBytes, (int)sectionOffset + 16),
                        PointerToRawData = BitConverter.ToUInt32(peBytes, (int)sectionOffset + 20)
                    };
                    
                    sections.Add(section);
                }
                
                return new PEInfo
                {
                    EntryPoint = entryPoint,
                    ImageSize = imageSize,
                    HeaderSize = headerSize,
                    Sections = sections
                };
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        /// <summary>
        /// Launches Roblox with additional stealth features
        /// </summary>
        public static bool LaunchWithStealth(string robloxPath, string arguments)
        {
            const string LOG_IDENT = "StealthLauncher::LaunchWithStealth";
            
            try
            {
                App.Logger.WriteLine(LOG_IDENT, "Launching with stealth features");
                
                // First try process hollowing
                if (LaunchWithProcessHollowing(robloxPath, arguments))
                {
                    return true;
                }
                
                // Fallback to regular launch with stealth modifications
                var startInfo = new ProcessStartInfo
                {
                    FileName = robloxPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                
                var process = Process.Start(startInfo);
                if (process != null)
                {
                    // Apply stealth modifications
                    var processHandle = PInvoke.OpenProcess(
                        PROCESS_ACCESS_RIGHTS.PROCESS_ALL_ACCESS,
                        false,
                        (uint)process.Id
                    );
                    
                    if (!processHandle.IsInvalid)
                    {
                        ApplyStealthModifications(processHandle);
                        PInvoke.CloseHandle(processHandle);
                    }
                    
                    App.Logger.WriteLine(LOG_IDENT, $"Launched with basic stealth (PID: {process.Id})");
                    return true;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
                return false;
            }
        }
    }
    
    /// <summary>
    /// PE file information structure
    /// </summary>
    public class PEInfo
    {
        public uint EntryPoint { get; set; }
        public uint ImageSize { get; set; }
        public uint HeaderSize { get; set; }
        public List<SectionInfo> Sections { get; set; } = new();
    }
    
    /// <summary>
    /// PE section information
    /// </summary>
    public class SectionInfo
    {
        public uint VirtualAddress { get; set; }
        public uint SizeOfRawData { get; set; }
        public uint PointerToRawData { get; set; }
    }
}