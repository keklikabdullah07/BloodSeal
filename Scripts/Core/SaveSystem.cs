using System;
using System.IO;
using System.Text.Json;
using Godot;

namespace BloodSeal.Core
{

    public static class SaveSystem
    {
        public const string DefaultSavePath = "user://bloodseal_save.json";
        public const int CurrentSaveVersion = 1;
        public const long MaxOfflineSeconds = 21600; // 6 saat (21.600 sn)

        public static SaveData CaptureSaveData(GameManager gm)
        {
            if (gm == null) return new SaveData();

            return new SaveData
            {
                Version = CurrentSaveVersion,
                Gold = gm.Gold,
                CurrentWave = gm.CurrentWave,
                HighestWave = gm.HighestWave,
                IsInSafeFarmMode = gm.IsInSafeFarmMode,
                PlayerName = gm.Profile?.PlayerName ?? "Valerius",
                Bloodline = gm.Profile?.Bloodline ?? BloodlineType.BoneWeaver,
                Origin = gm.Profile?.Origin ?? StreetOriginType.PitFighter,
                HasCompletedPrologue = gm.Profile?.HasCompletedPrologue ?? false,
                AtkLevel = gm.Stats?.AtkLevel ?? 1,
                AtkSpeedLevel = gm.Stats?.AtkSpeedLevel ?? 1,
                LifestealLevel = gm.Stats?.LifestealLevel ?? 1,
                MaxHpLevel = gm.Stats?.MaxHpLevel ?? 1,
                RangeLevel = gm.Stats?.RangeLevel ?? 1,
                LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
        }

        public static void ApplySaveData(SaveData data, GameManager gm)
        {
            if (data == null || gm == null) return;

            gm.SetProfile(data.PlayerName, data.Bloodline, data.Origin);
            if (gm.Profile != null)
            {
                gm.Profile.HasCompletedPrologue = data.HasCompletedPrologue;
            }

            if (gm.Stats != null)
            {
                gm.Stats.AtkLevel = Math.Max(1, data.AtkLevel);
                gm.Stats.AtkSpeedLevel = Math.Max(1, data.AtkSpeedLevel);
                gm.Stats.LifestealLevel = Math.Max(1, data.LifestealLevel);
                gm.Stats.MaxHpLevel = Math.Max(1, data.MaxHpLevel);
                gm.Stats.RangeLevel = Math.Max(1, data.RangeLevel);
            }

            // Doğrudan altın ve dalga yüklemesi
            double goldDiff = data.Gold - gm.Gold;
            gm.AddGold(goldDiff);
            gm.SetWave(Math.Max(1, data.CurrentWave), data.IsInSafeFarmMode);
        }

        public static OfflineEarningsResult CalculateOfflineEarnings(SaveData data, long nowUtcSeconds = 0)
        {
            if (data == null || data.LastSaveTimestamp <= 0) return new OfflineEarningsResult();

            if (nowUtcSeconds <= 0)
            {
                nowUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }

            return OfflineProgressCalculator.Calculate(data.HighestWave, data.LastSaveTimestamp, nowUtcSeconds);
        }

        public static bool SaveAtomic(SaveData data, string savePath = DefaultSavePath)
        {
            if (data == null) return false;
            data.LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            // Godot user:// veya yerel dosya yolu ayrımı
            if (savePath.StartsWith("user://"))
            {
                string globalPath = ProjectSettings.GlobalizePath(savePath);
                return WriteAtomicToDisk(globalPath, json);
            }
            else
            {
                return WriteAtomicToDisk(savePath, json);
            }
        }

        public static SaveData Load(string savePath = DefaultSavePath)
        {
            string targetPath = savePath.StartsWith("user://")
                ? ProjectSettings.GlobalizePath(savePath)
                : savePath;

            string backupPath = targetPath + ".bak";

            // 1. Ana dosyadan okuma
            SaveData data = TryReadFile(targetPath);
            if (data != null) return data;

            // 2. Yedek (.bak) dosyadan kurtarma denemesi
            if (System.IO.File.Exists(backupPath))
            {
                GD.PrintRich("[color=yellow]SaveSystem: Ana kayıt bozuk veya bulunamadı, .bak yedeğinden yükleniyor...[/color]");
                data = TryReadFile(backupPath);
                if (data != null) return data;
            }

            return null;
        }

        private static bool WriteAtomicToDisk(string targetPath, string json)
        {
            try
            {
                string dir = System.IO.Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(dir) && !System.IO.Directory.Exists(dir))
                {
                    System.IO.Directory.CreateDirectory(dir);
                }

                string tmpPath = targetPath + ".tmp";
                string bakPath = targetPath + ".bak";

                // 1. .tmp dosyasına yaz ve diske flush et
                System.IO.File.WriteAllText(tmpPath, json);

                // 2. Önceki çalışan dosya varsa .bak olarak sakla
                if (System.IO.File.Exists(targetPath))
                {
                    System.IO.File.Copy(targetPath, bakPath, true);
                }

                // 3. .tmp dosyasını hedef dosyanın üzerine taşı (Atomic Replace)
                System.IO.File.Move(tmpPath, targetPath, true);
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SaveSystem Atomic Write Hatası: {ex.Message}");
                return false;
            }
        }

        private static SaveData TryReadFile(string path)
        {
            if (!System.IO.File.Exists(path)) return null;

            try
            {
                string json = System.IO.File.ReadAllText(path);
                var data = JsonSerializer.Deserialize<SaveData>(json);
                return data;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SaveSystem okuma hatası ({path}): {ex.Message}");
                return null;
            }
        }
    }
}
