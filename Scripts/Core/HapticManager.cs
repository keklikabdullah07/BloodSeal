#nullable enable
using System;
using System.Reflection;

namespace BloodSeal.Core
{
    /// <summary>
    /// Donanım titreşimini yöneten haptik geri bildirim servisi.
    /// Saf C# singleton olarak tasarlanmıştır ve tüm platformlarda güvenle çalışır.
    /// </summary>
    public class HapticManager
    {
        private static HapticManager? _instance;
        public static HapticManager Instance => _instance ??= new HapticManager();

        public bool IsHapticsEnabled { get; set; } = true;

        public const int LightDurationMs = 15;
        public const int MediumDurationMs = 35;
        public const int HeavyDurationMs = 75;

        public event Action<bool>? OnHapticsToggled;

        public Action<int>? CustomVibrateHandler { get; set; }

        private static MethodInfo? _vibrateHandheldMethod;
        private static bool _reflectionChecked = false;

        public void SetHapticsEnabled(bool enabled)
        {
            if (IsHapticsEnabled == enabled) return;
            IsHapticsEnabled = enabled;
            OnHapticsToggled?.Invoke(IsHapticsEnabled);
        }

        public bool VibrateLight() => Vibrate(LightDurationMs);
        public bool VibrateMedium() => Vibrate(MediumDurationMs);
        public bool VibrateHeavy() => Vibrate(HeavyDurationMs);

        public bool Vibrate(int durationMs)
        {
            if (!IsHapticsEnabled || durationMs <= 0) return false;

            try
            {
                if (CustomVibrateHandler != null)
                {
                    CustomVibrateHandler(durationMs);
                    return true;
                }

                if (!_reflectionChecked)
                {
                    _reflectionChecked = true;
                    foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var t = asm.GetType("Godot.Input");
                        if (t != null)
                        {
                            _vibrateHandheldMethod = t.GetMethod("VibrateHandheld", BindingFlags.Public | BindingFlags.Static, null, new[] { typeof(int) }, null);
                            break;
                        }
                    }
                }

                _vibrateHandheldMethod?.Invoke(null, new object[] { durationMs });
                return true;
            }
            catch
            {
                // Masaüstü veya titreşim desteklemeyen platformlarda asla çökme
                return true;
            }
        }
    }
}
