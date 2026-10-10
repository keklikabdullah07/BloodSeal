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
            var data = new SaveData();
            if (gm != null)
            {
                data.Version = CurrentSaveVersion;
                data.Gold = gm.Gold;
                data.CurrentWave = gm.CurrentWave;
                data.HighestWave = gm.HighestWave;
                data.IsInSafeFarmMode = gm.IsInSafeFarmMode;
                data.PlayerName = gm.Profile?.PlayerName ?? "Valerius";
                data.Bloodline = gm.Profile?.Bloodline ?? BloodlineType.BoneWeaver;
                data.Origin = gm.Profile?.Origin ?? StreetOriginType.PitFighter;
                data.HasCompletedPrologue = gm.Profile?.HasCompletedPrologue ?? false;
                data.AtkLevel = gm.Stats?.AtkLevel ?? 1;
                data.AtkSpeedLevel = gm.Stats?.AtkSpeedLevel ?? 1;
                data.LifestealLevel = gm.Stats?.LifestealLevel ?? 1;
                data.MaxHpLevel = gm.Stats?.MaxHpLevel ?? 1;
                data.RangeLevel = gm.Stats?.RangeLevel ?? 1;
                data.LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                data.ActiveRune = gm.ActiveRune;
                data.SelectedGateApproach = gm.SelectedGateApproach;
                data.HasEncounteredGate = gm.HasEncounteredGate;
                data.HasClaimedGateReward = gm.HasClaimedGateReward;
                data.HasFirstLoreScroll = gm.HasFirstLoreScroll;
            }

            var rm = ResearchManager.Instance;
            if (rm != null)
            {
                data.LoreScrolls = rm.LoreScrolls;
                data.ResearchLevels = rm.GetAllLevels();
                data.DefeatedMilestoneBosses = rm.GetDefeatedMilestones();
            }

            var am = AwakeningManager.Instance;
            if (am != null) { data.AwakeningPoints = am.AwakeningPoints; data.TotalAwakenings = am.TotalAwakenings; data.AwakeningLevels = am.GetAllLevels(); }

            var relics = RelicManager.Instance;
            if (relics != null) data.CollectedRelics = relics.GetAllCollectedIds();

            var audio = AudioManager.Instance;
            if (audio != null) { data.MasterVolume = audio.MasterVolume; data.BgmVolume = audio.BgmVolume; data.SfxVolume = audio.SfxVolume; data.IsMuted = audio.IsMuted; }

            data.TutorialStep = (int)(TutorialManager.Instance?.CurrentStep ?? TutorialStep.TapToAttack);

            if (QuestManager.Instance != null)
            {
                data.QuestProgresses = new System.Collections.Generic.Dictionary<string, QuestProgress>(QuestManager.Instance.AllProgress);
                data.ActiveDailyQuestIds = new System.Collections.Generic.List<string>(QuestManager.Instance.ActiveDailyQuestIds);
                data.LastDailyResetTimestamp = QuestManager.Instance.LastDailyResetTimestamp;
            }
            return data;
        }

        public static void ApplySaveData(SaveData data, GameManager gm)
        {
            if (data == null) return;

            var rm = ResearchManager.Instance;
            if (rm != null)
            {
                rm.Reset();
                if (data.LoreScrolls > 0) rm.AddLoreScrolls(data.LoreScrolls);
                if (data.ResearchLevels != null)
                {
                    foreach (var kvp in data.ResearchLevels)
                        rm.SetResearchLevel(kvp.Key, kvp.Value);
                }
                if (data.DefeatedMilestoneBosses != null)
                {
                    foreach (int w in data.DefeatedMilestoneBosses)
                        rm.RecordMilestoneBossDefeated(w);
                }
                // Geriye dönük uyumluluk: Kapıdan parşömen kazanılmışsa ve liste boşsa
                if (data.HasFirstLoreScroll && rm.LoreScrolls == 0 && data.ResearchLevels?.Count == 0)
                {
                    rm.AddLoreScrolls(1);
                }
            }

            var am2 = AwakeningManager.Instance;
            if (am2 != null)
            {
                am2.Reset();
                if (data.AwakeningPoints > 0) am2.AddAwakeningPoints(data.AwakeningPoints);
                am2.SetTotalAwakenings(data.TotalAwakenings);
                if (data.AwakeningLevels != null)
                {
                    foreach (var kvp in data.AwakeningLevels)
                        am2.SetSealLevel(kvp.Key, kvp.Value);
                }
            }

            var relics2 = RelicManager.Instance;
            if (relics2 != null)
            {
                relics2.Reset();
                if (data.CollectedRelics != null)
                {
                    foreach (var id in data.CollectedRelics)
                        relics2.UnlockRelic(id);
                }
            }

            var audio2 = AudioManager.Instance;
            if (audio2 != null)
                audio2.ApplySettings(data.MasterVolume, data.BgmVolume, data.SfxVolume, data.IsMuted);

            if (TutorialManager.Instance != null)
                TutorialManager.Instance.SetStep(data.TutorialStep <= 0 ? TutorialStep.TapToAttack : (TutorialStep)data.TutorialStep);

            if (QuestManager.Instance != null)
                QuestManager.Instance.LoadState(data.QuestProgresses, data.ActiveDailyQuestIds, data.LastDailyResetTimestamp);

            if (gm == null) return;

            gm.SetProfile(data.PlayerName, data.Bloodline, data.Origin);
            if (gm.Profile != null) gm.Profile.HasCompletedPrologue = data.HasCompletedPrologue;

            if (gm.Stats != null)
            {
                gm.Stats.AtkLevel = Math.Max(1, data.AtkLevel);
                gm.Stats.AtkSpeedLevel = Math.Max(1, data.AtkSpeedLevel);
                gm.Stats.LifestealLevel = Math.Max(1, data.LifestealLevel);
                gm.Stats.MaxHpLevel = Math.Max(1, data.MaxHpLevel);
                gm.Stats.RangeLevel = Math.Max(1, data.RangeLevel);
            }

            gm.ActiveRune = data.ActiveRune;
            gm.SelectedGateApproach = data.SelectedGateApproach;
            gm.HasEncounteredGate = data.HasEncounteredGate;
            gm.HasClaimedGateReward = data.HasClaimedGateReward;
            gm.HasFirstLoreScroll = data.HasFirstLoreScroll;

            if (gm.Stats != null)
            {
                gm.Stats.ActiveRune = data.ActiveRune;
                gm.Stats.Profile = gm.Profile;
            }

            // Doğrudan altın ve dalga yüklemesi
            double goldDiff = data.Gold - gm.Gold;
            gm.AddGold(goldDiff);
            gm.SetWave(Math.Max(1, data.CurrentWave), data.IsInSafeFarmMode);
        }

        public static OfflineEarningsResult CalculateOfflineEarnings(SaveData data, long nowUtcSeconds = 0)
        {
            if (data == null || data.LastSaveTimestamp <= 0) return new OfflineEarningsResult();
            if (nowUtcSeconds <= 0) nowUtcSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return OfflineProgressCalculator.Calculate(data.HighestWave, data.LastSaveTimestamp, nowUtcSeconds);
        }

        public static bool SaveAtomic(SaveData data, string savePath = DefaultSavePath)
        {
            if (data == null) return false;
            data.LastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            string targetPath = savePath.StartsWith("user://") ? ProjectSettings.GlobalizePath(savePath) : savePath;
            return WriteAtomicToDisk(targetPath, json);
        }

        public static SaveData Load(string savePath = DefaultSavePath)
        {
            string targetPath = savePath.StartsWith("user://")
                ? ProjectSettings.GlobalizePath(savePath)
                : savePath;
            string backupPath = targetPath + ".bak";

            SaveData data = TryReadFile(targetPath);
            if (data != null) return data;

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

                System.IO.File.WriteAllText(tmpPath, json);
                if (System.IO.File.Exists(targetPath))
                {
                    System.IO.File.Copy(targetPath, bakPath, true);
                }

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
                return JsonSerializer.Deserialize<SaveData>(System.IO.File.ReadAllText(path));
            }
            catch (Exception ex)
            {
                GD.PrintErr($"SaveSystem okuma hatası ({path}): {ex.Message}");
                return null;
            }
        }
    }
}
