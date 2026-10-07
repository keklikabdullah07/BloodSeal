using System;
using System.Text.Json;
using Godot;

namespace BloodSeal.Core
{
    public class BossEnrageConfig
    {
        public double IntervalSeconds { get; set; } = 5.0;
        public double DamageStepMultiplier { get; set; } = 0.25;
        public bool IsMultiplicative { get; set; } = false;
    }

    public static class BalanceConfig
    {
        private static BossEnrageConfig _bossEnrage;
        private static bool _isLoaded = false;

        public static BossEnrageConfig BossEnrage
        {
            get
            {
                if (!_isLoaded)
                {
                    Load();
                }
                return _bossEnrage ?? new BossEnrageConfig();
            }
        }

        public static void Load()
        {
            string json = null;

            // 1. Try Godot FileAccess (runtime PCK / editor)
            if (Godot.FileAccess.FileExists("res://Data/BalanceConfig.json"))
            {
                using var file = Godot.FileAccess.Open("res://Data/BalanceConfig.json", Godot.FileAccess.ModeFlags.Read);
                if (file != null)
                {
                    json = file.GetAsText();
                }
            }

            // 2. Fallback to System.IO for test runs or headless direct CLI
            if (string.IsNullOrEmpty(json))
            {
                string localPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "BalanceConfig.json");
                if (System.IO.File.Exists(localPath))
                {
                    json = System.IO.File.ReadAllText(localPath);
                }
                else
                {
                    string parentPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Data", "BalanceConfig.json");
                    if (System.IO.File.Exists(parentPath))
                    {
                        json = System.IO.File.ReadAllText(parentPath);
                    }
                }
            }

            if (!string.IsNullOrEmpty(json))
            {
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    if (root.TryGetProperty("bossEnrage", out var enrageElem))
                    {
                        _bossEnrage = new BossEnrageConfig
                        {
                            IntervalSeconds = enrageElem.TryGetProperty("intervalSeconds", out var i) ? i.GetDouble() : 5.0,
                            DamageStepMultiplier = enrageElem.TryGetProperty("damageStepMultiplier", out var d) ? d.GetDouble() : 0.25,
                            IsMultiplicative = enrageElem.TryGetProperty("isMultiplicative", out var m) && m.GetBoolean()
                        };
                    }
                }
                catch (Exception ex)
                {
                    GD.PrintErr($"BalanceConfig JSON parse error: {ex.Message}");
                }
            }

            _bossEnrage ??= new BossEnrageConfig();
            _isLoaded = true;
        }

        public static void Reset()
        {
            _isLoaded = false;
            _bossEnrage = null;
        }
    }
}
