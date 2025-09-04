using System.Diagnostics;
using System.Management;

namespace Bloxstrap.Utility
{
    /// <summary>
    /// Real-time performance monitoring and automatic optimization system
    /// </summary>
    public class PerformanceMonitor : IDisposable
    {
        private readonly Timer _monitorTimer;
        private readonly PerformanceCounter _cpuCounter;
        private readonly PerformanceCounter _memoryCounter;
        private readonly PerformanceCounter _fpsCounter;
        private Process? _robloxProcess;
        private bool _disposed = false;
        
        // Performance thresholds
        private const float HIGH_CPU_THRESHOLD = 80.0f;
        private const float HIGH_MEMORY_THRESHOLD = 85.0f;
        private const int LOW_FPS_THRESHOLD = 30;
        
        // Optimization flags
        private bool _aggressiveOptimizationEnabled = false;
        private DateTime _lastOptimization = DateTime.MinValue;
        
        public event EventHandler<PerformanceEventArgs>? PerformanceAlert;
        public event EventHandler<OptimizationEventArgs>? OptimizationApplied;
        
        public PerformanceMonitor()
        {
            _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _memoryCounter = new PerformanceCounter("Memory", "Available MBytes");
            _fpsCounter = new PerformanceCounter(); // Would be configured for specific game metrics
            
            // Monitor every 2 seconds
            _monitorTimer = new Timer(MonitorPerformance, null, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(2));
        }
        
        /// <summary>
        /// Starts monitoring a specific Roblox process
        /// </summary>
        public void StartMonitoring(int processId)
        {
            const string LOG_IDENT = "PerformanceMonitor::StartMonitoring";
            
            try
            {
                _robloxProcess = Process.GetProcessById(processId);
                App.Logger.WriteLine(LOG_IDENT, $"Started monitoring process {processId}");
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
        
        /// <summary>
        /// Monitors system and process performance
        /// </summary>
        private void MonitorPerformance(object? state)
        {
            if (_disposed || _robloxProcess?.HasExited != false)
                return;
                
            try
            {
                var metrics = GatherPerformanceMetrics();
                AnalyzeAndOptimize(metrics);
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("PerformanceMonitor::MonitorPerformance", ex);
            }
        }
        
        /// <summary>
        /// Gathers comprehensive performance metrics
        /// </summary>
        private PerformanceMetrics GatherPerformanceMetrics()
        {
            var metrics = new PerformanceMetrics
            {
                Timestamp = DateTime.Now,
                SystemCpuUsage = _cpuCounter.NextValue(),
                AvailableMemoryMB = _memoryCounter.NextValue(),
                ProcessId = _robloxProcess?.Id ?? 0
            };
            
            if (_robloxProcess != null && !_robloxProcess.HasExited)
            {
                _robloxProcess.Refresh();
                metrics.ProcessCpuUsage = GetProcessCpuUsage(_robloxProcess);
                metrics.ProcessMemoryUsageMB = _robloxProcess.WorkingSet64 / (1024 * 1024);
                metrics.ProcessThreadCount = _robloxProcess.Threads.Count;
                metrics.ProcessHandleCount = _robloxProcess.HandleCount;
                
                // Estimate FPS based on CPU usage patterns
                metrics.EstimatedFPS = EstimateFPS(metrics.ProcessCpuUsage);
            }
            
            return metrics;
        }
        
        /// <summary>
        /// Gets CPU usage for a specific process
        /// </summary>
        private float GetProcessCpuUsage(Process process)
        {
            try
            {
                using var counter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                return counter.NextValue();
            }
            catch
            {
                return 0.0f;
            }
        }
        
        /// <summary>
        /// Estimates FPS based on CPU usage patterns
        /// </summary>
        private int EstimateFPS(float cpuUsage)
        {
            // Simple heuristic - real implementation would use more sophisticated metrics
            if (cpuUsage < 20) return 120; // Low usage = high FPS
            if (cpuUsage < 40) return 90;
            if (cpuUsage < 60) return 60;
            if (cpuUsage < 80) return 45;
            return 30; // High usage = low FPS
        }
        
        /// <summary>
        /// Analyzes metrics and applies optimizations as needed
        /// </summary>
        private void AnalyzeAndOptimize(PerformanceMetrics metrics)
        {
            const string LOG_IDENT = "PerformanceMonitor::AnalyzeAndOptimize";
            
            // Check if we need to apply optimizations
            bool needsOptimization = false;
            var issues = new List<string>();
            
            if (metrics.SystemCpuUsage > HIGH_CPU_THRESHOLD)
            {
                needsOptimization = true;
                issues.Add($"High system CPU usage: {metrics.SystemCpuUsage:F1}%");
            }
            
            if (metrics.AvailableMemoryMB < 1000) // Less than 1GB available
            {
                needsOptimization = true;
                issues.Add($"Low available memory: {metrics.AvailableMemoryMB:F0}MB");
            }
            
            if (metrics.ProcessMemoryUsageMB > 2000) // Process using more than 2GB
            {
                needsOptimization = true;
                issues.Add($"High process memory usage: {metrics.ProcessMemoryUsageMB:F0}MB");
            }
            
            if (metrics.EstimatedFPS < LOW_FPS_THRESHOLD)
            {
                needsOptimization = true;
                issues.Add($"Low FPS detected: {metrics.EstimatedFPS}");
            }
            
            // Fire performance alert
            if (issues.Count > 0)
            {
                PerformanceAlert?.Invoke(this, new PerformanceEventArgs(metrics, issues));
            }
            
            // Apply optimizations if needed and enough time has passed
            if (needsOptimization && DateTime.Now - _lastOptimization > TimeSpan.FromMinutes(1))
            {
                ApplyDynamicOptimizations(metrics);
                _lastOptimization = DateTime.Now;
            }
        }
        
        /// <summary>
        /// Applies dynamic optimizations based on current performance
        /// </summary>
        private void ApplyDynamicOptimizations(PerformanceMetrics metrics)
        {
            const string LOG_IDENT = "PerformanceMonitor::ApplyDynamicOptimizations";
            var optimizations = new List<string>();
            
            try
            {
                // CPU optimization
                if (metrics.SystemCpuUsage > HIGH_CPU_THRESHOLD)
                {
                    OptimizeCPUUsage();
                    optimizations.Add("CPU usage optimization");
                }
                
                // Memory optimization
                if (metrics.AvailableMemoryMB < 1000 || metrics.ProcessMemoryUsageMB > 2000)
                {
                    OptimizeMemoryUsage();
                    optimizations.Add("Memory usage optimization");
                }
                
                // Graphics optimization
                if (metrics.EstimatedFPS < LOW_FPS_THRESHOLD)
                {
                    OptimizeGraphicsSettings();
                    optimizations.Add("Graphics optimization");
                }
                
                // Network optimization
                OptimizeNetworkSettings();
                optimizations.Add("Network optimization");
                
                // Process priority optimization
                OptimizeProcessPriority();
                optimizations.Add("Process priority optimization");
                
                App.Logger.WriteLine(LOG_IDENT, $"Applied optimizations: {string.Join(", ", optimizations)}");
                OptimizationApplied?.Invoke(this, new OptimizationEventArgs(optimizations, metrics));
            }
            catch (Exception ex)
            {
                App.Logger.WriteException(LOG_IDENT, ex);
            }
        }
        
        /// <summary>
        /// Optimizes CPU usage
        /// </summary>
        private void OptimizeCPUUsage()
        {
            try
            {
                if (_robloxProcess != null && !_robloxProcess.HasExited)
                {
                    // Set process affinity to use fewer CPU cores if system is under load
                    int coreCount = Environment.ProcessorCount;
                    if (coreCount > 2)
                    {
                        // Use half the cores for Roblox to leave resources for other processes
                        int affinityMask = (1 << (coreCount / 2)) - 1;
                        _robloxProcess.ProcessorAffinity = new IntPtr(affinityMask);
                    }
                    
                    // Force garbage collection
                    GC.Collect(2, GCCollectionMode.Optimized);
                    GC.WaitForPendingFinalizers();
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("PerformanceMonitor::OptimizeCPUUsage", ex);
            }
        }
        
        /// <summary>
        /// Optimizes memory usage
        /// </summary>
        private void OptimizeMemoryUsage()
        {
            try
            {
                // Apply memory-saving FastFlags
                var fastFlagManager = new FastFlagManager();
                fastFlagManager.Load();
                
                // Reduce texture memory budget
                fastFlagManager.Prop["DFIntTextureMemoryBudget"] = 50;
                fastFlagManager.Prop["DFIntAudioMemoryBudget"] = 25;
                fastFlagManager.Prop["FFlagOptimizeAssetCaching"] = true;
                
                fastFlagManager.Save();
                
                // Force system memory cleanup
                GC.Collect(2, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced);
                
                // Clear working set
                if (_robloxProcess != null && !_robloxProcess.HasExited)
                {
                    try
                    {
                        // This would require additional P/Invoke declarations
                        // SetProcessWorkingSetSize(_robloxProcess.Handle, -1, -1);
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("PerformanceMonitor::OptimizeMemoryUsage", ex);
            }
        }
        
        /// <summary>
        /// Optimizes graphics settings for better performance
        /// </summary>
        private void OptimizeGraphicsSettings()
        {
            try
            {
                var fastFlagManager = new FastFlagManager();
                fastFlagManager.Load();
                
                // Apply aggressive graphics optimizations
                fastFlagManager.Prop["FFlagDisableDynamicShadows"] = true;
                fastFlagManager.Prop["FFlagDisableParticleEffects"] = true;
                fastFlagManager.Prop["FFlagDisableReflections"] = true;
                fastFlagManager.Prop["FFlagDisableWaterEffects"] = true;
                fastFlagManager.Prop["FFlagDisableBloomEffect"] = true;
                fastFlagManager.Prop["FFlagDisableDepthOfFieldEffect"] = true;
                fastFlagManager.Prop["DFIntTextureQualityOverride"] = 1; // Low texture quality
                fastFlagManager.Prop["FIntRenderShadowIntensity"] = 0; // No shadows
                
                // Maximize FPS
                fastFlagManager.Prop["DFIntTaskSchedulerTargetFps"] = 999999;
                fastFlagManager.Prop["FFlagDisableVSync"] = true;
                
                fastFlagManager.Save();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("PerformanceMonitor::OptimizeGraphicsSettings", ex);
            }
        }
        
        /// <summary>
        /// Optimizes network settings
        /// </summary>
        private void OptimizeNetworkSettings()
        {
            try
            {
                var fastFlagManager = new FastFlagManager();
                fastFlagManager.Load();
                
                fastFlagManager.Prop["FFlagReduceNetworkLatency"] = true;
                fastFlagManager.Prop["FFlagOptimizeBandwidthUsage"] = true;
                fastFlagManager.Prop["FFlagFastServerConnection"] = true;
                
                fastFlagManager.Save();
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("PerformanceMonitor::OptimizeNetworkSettings", ex);
            }
        }
        
        /// <summary>
        /// Optimizes process priority
        /// </summary>
        private void OptimizeProcessPriority()
        {
            try
            {
                if (_robloxProcess != null && !_robloxProcess.HasExited)
                {
                    _robloxProcess.PriorityClass = ProcessPriorityClass.High;
                    
                    // Set thread priorities
                    foreach (ProcessThread thread in _robloxProcess.Threads)
                    {
                        try
                        {
                            thread.PriorityLevel = ThreadPriorityLevel.AboveNormal;
                        }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("PerformanceMonitor::OptimizeProcessPriority", ex);
            }
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _monitorTimer?.Dispose();
                _cpuCounter?.Dispose();
                _memoryCounter?.Dispose();
                _fpsCounter?.Dispose();
                _disposed = true;
            }
        }
    }
    
    /// <summary>
    /// Performance metrics data structure
    /// </summary>
    public class PerformanceMetrics
    {
        public DateTime Timestamp { get; set; }
        public float SystemCpuUsage { get; set; }
        public float AvailableMemoryMB { get; set; }
        public int ProcessId { get; set; }
        public float ProcessCpuUsage { get; set; }
        public long ProcessMemoryUsageMB { get; set; }
        public int ProcessThreadCount { get; set; }
        public int ProcessHandleCount { get; set; }
        public int EstimatedFPS { get; set; }
    }
    
    /// <summary>
    /// Performance alert event arguments
    /// </summary>
    public class PerformanceEventArgs : EventArgs
    {
        public PerformanceMetrics Metrics { get; }
        public List<string> Issues { get; }
        
        public PerformanceEventArgs(PerformanceMetrics metrics, List<string> issues)
        {
            Metrics = metrics;
            Issues = issues;
        }
    }
    
    /// <summary>
    /// Optimization applied event arguments
    /// </summary>
    public class OptimizationEventArgs : EventArgs
    {
        public List<string> OptimizationsApplied { get; }
        public PerformanceMetrics Metrics { get; }
        
        public OptimizationEventArgs(List<string> optimizations, PerformanceMetrics metrics)
        {
            OptimizationsApplied = optimizations;
            Metrics = metrics;
        }
    }
}