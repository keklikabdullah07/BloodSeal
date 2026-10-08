#nullable enable
using System;

namespace BloodSeal.Core
{
    public class OfflineEarningsResult
    {
        public long ElapsedSeconds { get; set; } = 0;
        public double GoldEarned { get; set; } = 0.0;
        public int FarmWave { get; set; } = 1;
        public bool HasClaimableEarnings => ElapsedSeconds >= 60 && GoldEarned > 0;
    }

    public static class OfflineProgressCalculator
    {
        public const long MaxOfflineSeconds = 21600; // 6 saat (21.600 saniye)

        public static OfflineEarningsResult Calculate(int highestWave, long lastSaveTimestamp, long currentTimestamp, ResearchManager? research = null)
        {
            var result = new OfflineEarningsResult();
            if (lastSaveTimestamp <= 0 || currentTimestamp <= 0) return result;

            research ??= ResearchManager.Instance;
            long rawElapsed = currentTimestamp - lastSaveTimestamp;
            long effectiveMaxSeconds = MaxOfflineSeconds + (research?.GetOfflineCapBonusSeconds() ?? 0);
            result.ElapsedSeconds = Math.Clamp(rawElapsed, 0, effectiveMaxSeconds);

            // 60 saniyeden az sürede çevrimdışı kazancı tetiklenmez (anti-spam / debounce)
            if (result.ElapsedSeconds < 60) return result;

            // En son temizlenen güvenli farm dalgası (Wave - 1)
            int farmWave = Math.Max(1, highestWave > 1 ? highestWave - 1 : 1);
            result.FarmWave = farmWave;

            // Denge formülü: Dalga başına 5 minyon * (10 + farmWave * 5) altın
            double goldPerWave = 5.0 * (10.0 + farmWave * 5.0);
            double estimatedWaveDurationSeconds = 15.0; // Güvenli farm dalga temizleme süresi
            double goldPerSecond = goldPerWave / estimatedWaveDurationSeconds;
            double yieldMultiplier = research?.GetOfflineYieldMultiplier() ?? 1.0;

            result.GoldEarned = Math.Round(result.ElapsedSeconds * goldPerSecond * yieldMultiplier, 0);
            return result;
        }
    }
}
