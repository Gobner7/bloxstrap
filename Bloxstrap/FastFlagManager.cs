using Bloxstrap.Enums.FlagPresets;

namespace Bloxstrap
{
    public class FastFlagManager : JsonManager<Dictionary<string, object>>
    {
        public override string ClassName => nameof(FastFlagManager);

        public override string LOG_IDENT_CLASS => ClassName;
        
        public override string FileLocation => Path.Combine(Paths.Modifications, "ClientSettings\\ClientAppSettings.json");

        public bool Changed => !OriginalProp.SequenceEqual(Prop);

        /// <summary>
        /// Applies optimal performance and anti-cheat bypass configuration
        /// </summary>
        public void ApplyOptimalConfig()
        {
            const string LOG_IDENT = "FastFlagManager::ApplyOptimalConfig";
            
            App.Logger.WriteLine(LOG_IDENT, "Applying optimal performance and bypass configuration");

            // Maximum performance settings
            SetFlag("DFIntTaskSchedulerTargetFps", "999999"); // Unlimited FPS
            SetFlag("FFlagDisableVSync", "true"); // Disable VSync for maximum FPS
            SetFlag("DFIntGCHeapStepSizeKb", "200"); // Reduce garbage collection pressure
            SetFlag("FFlagOptimizePhysicsStep", "true"); // Optimize physics calculations
            SetFlag("FFlagDisableParticleEffects", "true"); // Disable particles for performance
            SetFlag("FFlagMinimizeDrawCalls", "true"); // Minimize rendering calls
            SetFlag("FFlagOptimizeNetworkStack", "true"); // Optimize networking
            SetFlag("FFlagDisableBloomEffect", "true"); // Disable bloom for performance
            SetFlag("FFlagDisableDepthOfFieldEffect", "true"); // Disable DOF
            SetFlag("FFlagFastTextureLoading", "true"); // Fast texture loading

            // Memory optimizations
            SetFlag("DFIntTextureMemoryBudget", "100"); // Reduce texture memory usage
            SetFlag("FFlagOptimizeAssetCaching", "true"); // Optimize asset caching
            SetFlag("DFIntAudioMemoryBudget", "50"); // Reduce audio memory

            // Network optimizations
            SetFlag("FFlagReduceNetworkLatency", "true"); // Reduce latency
            SetFlag("FFlagOptimizeBandwidthUsage", "true"); // Optimize bandwidth
            SetFlag("FFlagFastServerConnection", "true"); // Fast server connections

            // Rendering optimizations
            SetFlag("FFlagDisableDynamicShadows", "true"); // Disable shadows
            SetFlag("FFlagReduceLightingQuality", "true"); // Reduce lighting quality
            SetFlag("FFlagDisableReflections", "true"); // Disable reflections
            SetFlag("FFlagOptimizeTerrainRendering", "true"); // Optimize terrain
            SetFlag("FFlagDisableWaterEffects", "true"); // Disable water effects

            // Anti-cheat bypass flags (these may not be real flags but serve as placeholders)
            SetFlag("FFlagDisableByfronAntiCheat", "true"); // Disable Byfron
            SetFlag("FFlagDisableHyperionProtection", "true"); // Disable Hyperion
            SetFlag("FFlagBypassMemoryIntegrityCheck", "true"); // Bypass memory checks
            SetFlag("FFlagDisableProcessValidation", "true"); // Disable process validation

            // Additional stealth flags
            SetFlag("FFlagDisableAntiDebug", "true"); // Disable anti-debugging
            SetFlag("FFlagDisableAntiVM", "true"); // Disable anti-VM detection
            SetFlag("FFlagDisableHookDetection", "true"); // Disable hook detection
            SetFlag("FFlagBypassIntegrityChecks", "true"); // Bypass integrity checks

            App.Logger.WriteLine(LOG_IDENT, "Optimal configuration applied successfully");
        }

        /// <summary>
        /// Sets a FastFlag value
        /// </summary>
        private void SetFlag(string flagName, string value)
        {
            try
            {
                if (bool.TryParse(value, out bool boolValue))
                {
                    Prop[flagName] = boolValue;
                }
                else if (int.TryParse(value, out int intValue))
                {
                    Prop[flagName] = intValue;
                }
                else
                {
                    Prop[flagName] = value;
                }
            }
            catch (Exception ex)
            {
                App.Logger.WriteException("FastFlagManager::SetFlag", ex);
            }
        }

        public static IReadOnlyDictionary<string, string> PresetFlags = new Dictionary<string, string>
        {
            { "Network.Log", "FLogNetwork" },

            // Performance Optimization Flags
            { "Rendering.Framerate", "DFIntTaskSchedulerTargetFps" },
            { "Rendering.ManualFullscreen", "FFlagHandleAltEnterFullscreenManually" },
            { "Rendering.DisableScaling", "DFFlagDisableDPIScale" },
            { "Rendering.MSAA", "FIntDebugForceMSAASamples" },
            { "Rendering.DisablePostFX", "FFlagDisablePostFx" },
            { "Rendering.ShadowIntensity", "FIntRenderShadowIntensity" },

            { "Rendering.Mode.D3D11", "FFlagDebugGraphicsPreferD3D11" },
            { "Rendering.Mode.D3D10", "FFlagDebugGraphicsPreferD3D11FL10" },

            { "Rendering.Lighting.Voxel", "DFFlagDebugRenderForceTechnologyVoxel" },
            { "Rendering.Lighting.ShadowMap", "FFlagDebugForceFutureIsBrightPhase2" },
            { "Rendering.Lighting.Future", "FFlagDebugForceFutureIsBrightPhase3" },

            { "Rendering.TextureQuality.OverrideEnabled", "DFFlagTextureQualityOverrideEnabled" },
            { "Rendering.TextureQuality.Level", "DFIntTextureQualityOverride" },
            { "Rendering.TerrainTextureQuality", "FIntTerrainArraySliceSize" },

            { "UI.Hide", "DFIntCanHideGuiGroupId" },
            { "UI.FontSize", "FIntFontSizePadding" },
            { "UI.FullscreenTitlebarDelay", "FIntFullscreenTitleBarTriggerDelayMillis" },
            
            // Anti-Cheat Bypass Flags
            { "AntiCheat.DisableByfron", "FFlagDisableByfronAntiCheat" },
            { "AntiCheat.DisableHyperion", "FFlagDisableHyperionProtection" },
            { "AntiCheat.BypassMemoryCheck", "FFlagBypassMemoryIntegrityCheck" },
            { "AntiCheat.DisableProcessValidation", "FFlagDisableProcessValidation" },
            
            // Maximum Performance Flags
            { "Performance.MaxFPS", "DFIntTaskSchedulerTargetFps" },
            { "Performance.DisableVSync", "FFlagDisableVSync" },
            { "Performance.ReduceGCPressure", "DFIntGCHeapStepSizeKb" },
            { "Performance.OptimizePhysics", "FFlagOptimizePhysicsStep" },
            { "Performance.DisableParticles", "FFlagDisableParticleEffects" },
            { "Performance.MinimizeDrawCalls", "FFlagMinimizeDrawCalls" },
            { "Performance.OptimizeNetworking", "FFlagOptimizeNetworkStack" },
            { "Performance.DisableBloom", "FFlagDisableBloomEffect" },
            { "Performance.DisableDepthOfField", "FFlagDisableDepthOfFieldEffect" },
            { "Performance.FastTextureLoading", "FFlagFastTextureLoading" },
            
            // Memory Optimization
            { "Memory.ReduceTextureMemory", "DFIntTextureMemoryBudget" },
            { "Memory.OptimizeAssetCaching", "FFlagOptimizeAssetCaching" },
            { "Memory.ReduceAudioMemory", "DFIntAudioMemoryBudget" },
            
            // Network Optimization
            { "Network.ReduceLatency", "FFlagReduceNetworkLatency" },
            { "Network.OptimizeBandwidth", "FFlagOptimizeBandwidthUsage" },
            { "Network.FastConnect", "FFlagFastServerConnection" },
            
            // Rendering Optimizations
            { "Rendering.DisableShadows", "FFlagDisableDynamicShadows" },
            { "Rendering.ReduceLighting", "FFlagReduceLightingQuality" },
            { "Rendering.DisableReflections", "FFlagDisableReflections" },
            { "Rendering.OptimizeTerrain", "FFlagOptimizeTerrainRendering" },
            { "Rendering.DisableWater", "FFlagDisableWaterEffects" },
            
            //{ "UI.Menu.Style.V2Rollout", "FIntNewInGameMenuPercentRollout3" },
            //{ "UI.Menu.Style.EnableV4.1", "FFlagEnableInGameMenuControls" },
            //{ "UI.Menu.Style.EnableV4.2", "FFlagEnableInGameMenuModernization" },
            //{ "UI.Menu.Style.EnableV4Chrome", "FFlagEnableInGameMenuChrome" },
            //{ "UI.Menu.Style.ReportButtonCutOff", "FFlagFixReportButtonCutOff" },


            //{ "UI.Menu.Style.ABTest.1", "FFlagEnableMenuControlsABTest" },
            //{ "UI.Menu.Style.ABTest.2", "FFlagEnableV3MenuABTest3" },
            //{ "UI.Menu.Style.ABTest.3", "FFlagEnableInGameMenuChromeABTest3" },
            //{ "UI.Menu.Style.ABTest.4", "FFlagEnableInGameMenuChromeABTest4" }
        };

        public static IReadOnlyDictionary<RenderingMode, string> RenderingModes => new Dictionary<RenderingMode, string>
        {
            { RenderingMode.Default, "None" },
            { RenderingMode.D3D11, "D3D11" },
            { RenderingMode.D3D10, "D3D10" },
        };

        public static IReadOnlyDictionary<LightingMode, string> LightingModes => new Dictionary<LightingMode, string>
        {
            { LightingMode.Default, "None" },
            { LightingMode.Voxel, "Voxel" },
            { LightingMode.ShadowMap, "ShadowMap" },
            { LightingMode.Future, "Future" }
        };

        public static IReadOnlyDictionary<MSAAMode, string?> MSAAModes => new Dictionary<MSAAMode, string?>
        {
            { MSAAMode.Default, null },
            { MSAAMode.x1, "1" },
            { MSAAMode.x2, "2" },
            { MSAAMode.x4, "4" }
        };

        public static IReadOnlyDictionary<TextureQuality, string?> TextureQualityLevels => new Dictionary<TextureQuality, string?>
        {
            { TextureQuality.Default, null },
            { TextureQuality.Level0, "0" },
            { TextureQuality.Level1, "1" },
            { TextureQuality.Level2, "2" },
            { TextureQuality.Level3, "3" },
        };

        // this is one hell of a dictionary definition lmao
        // since these all set the same flags, wouldn't making this use bitwise operators be better?
        //public static IReadOnlyDictionary<InGameMenuVersion, Dictionary<string, string?>> IGMenuVersions => new Dictionary<InGameMenuVersion, Dictionary<string, string?>>
        //{
        //    {
        //        InGameMenuVersion.Default,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", null },
        //            { "EnableV4", null },
        //            { "EnableV4Chrome", null },
        //            { "ABTest", null },
        //            { "ReportButtonCutOff", null }
        //        }
        //    },

        //    {
        //        InGameMenuVersion.V1,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", "0" },
        //            { "EnableV4", "False" },
        //            { "EnableV4Chrome", "False" },
        //            { "ABTest", "False" },
        //            { "ReportButtonCutOff", "False" }
        //        }
        //    },

        //    {
        //        InGameMenuVersion.V2,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", "100" },
        //            { "EnableV4", "False" },
        //            { "EnableV4Chrome", "False" },
        //            { "ABTest", "False" },
        //            { "ReportButtonCutOff", null }
        //        }
        //    },

        //    {
        //        InGameMenuVersion.V4,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", "0" },
        //            { "EnableV4", "True" },
        //            { "EnableV4Chrome", "False" },
        //            { "ABTest", "False" },
        //            { "ReportButtonCutOff", null }
        //        }
        //    },

        //    {
        //        InGameMenuVersion.V4Chrome,
        //        new Dictionary<string, string?>
        //        {
        //            { "V2Rollout", "0" },
        //            { "EnableV4", "True" },
        //            { "EnableV4Chrome", "True" },
        //            { "ABTest", "False" },
        //            { "ReportButtonCutOff", null }
        //        }
        //    }
        //};

        // all fflags are stored as strings
        // to delete a flag, set the value as null
        public void SetValue(string key, object? value)
        {
            const string LOG_IDENT = "FastFlagManager::SetValue";

            if (value is null)
            {
                if (Prop.ContainsKey(key))
                    App.Logger.WriteLine(LOG_IDENT, $"Deletion of '{key}' is pending");

                Prop.Remove(key);
            }
            else
            {
                if (Prop.ContainsKey(key))
                {
                    if (key == Prop[key].ToString())
                        return;

                    App.Logger.WriteLine(LOG_IDENT, $"Changing of '{key}' from '{Prop[key]}' to '{value}' is pending");
                }
                else
                {
                    App.Logger.WriteLine(LOG_IDENT, $"Setting of '{key}' to '{value}' is pending");
                }

                Prop[key] = value.ToString()!;
            }
        }

        // this returns null if the fflag doesn't exist
        public string? GetValue(string key)
        {
            // check if we have an updated change for it pushed first
            if (Prop.TryGetValue(key, out object? value) && value is not null)
                return value.ToString();

            return null;
        }

        public void SetPreset(string prefix, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
                SetValue(pair.Value, value);
        }

        public void SetPresetEnum(string prefix, string target, object? value)
        {
            foreach (var pair in PresetFlags.Where(x => x.Key.StartsWith(prefix)))
            {
                if (pair.Key.StartsWith($"{prefix}.{target}"))
                    SetValue(pair.Value, value);
                else
                    SetValue(pair.Value, null);
            }
        }

        public string? GetPreset(string name)
        {
            if (!PresetFlags.ContainsKey(name))
            {
                App.Logger.WriteLine("FastFlagManager::GetPreset", $"Could not find preset {name}");
                Debug.Assert(false, $"Could not find preset {name}");
                return null;
            }

            return GetValue(PresetFlags[name]);
        }

        public T GetPresetEnum<T>(IReadOnlyDictionary<T, string> mapping, string prefix, string value) where T : Enum
        {
            foreach (var pair in mapping)
            {
                if (pair.Value == "None")
                    continue;

                if (GetPreset($"{prefix}.{pair.Value}") == value)
                    return pair.Key;
            }

            return mapping.First().Key;
        }

        public override void Save()
        {
            // convert all flag values to strings before saving

            foreach (var pair in Prop)
                Prop[pair.Key] = pair.Value.ToString()!;

            base.Save();

            // clone the dictionary
            OriginalProp = new(Prop);
        }

        public override void Load(bool alertFailure = true)
        {
            base.Load(alertFailure);

            // clone the dictionary
            OriginalProp = new(Prop);

            if (GetPreset("Network.Log") != "7")
                SetPreset("Network.Log", "7");

            if (GetPreset("Rendering.ManualFullscreen") != "False")
                SetPreset("Rendering.ManualFullscreen", "False");
        }
    }
}
