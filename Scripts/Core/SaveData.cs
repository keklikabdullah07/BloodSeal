namespace BloodSeal.Core
{
    public class SaveData
    {
        public int Version { get; set; } = 1;
        public double Gold { get; set; } = 100.0;
        public int CurrentWave { get; set; } = 1;
        public int HighestWave { get; set; } = 1;
        public bool IsInSafeFarmMode { get; set; } = false;

        // Character Profile
        public string PlayerName { get; set; } = "Valerius";
        public BloodlineType Bloodline { get; set; } = BloodlineType.BoneWeaver;
        public StreetOriginType Origin { get; set; } = StreetOriginType.PitFighter;
        public bool HasCompletedPrologue { get; set; } = false;

        // Pentagram Stats
        public int AtkLevel { get; set; } = 1;
        public int AtkSpeedLevel { get; set; } = 1;
        public int LifestealLevel { get; set; } = 1;
        public int MaxHpLevel { get; set; } = 1;
        public int RangeLevel { get; set; } = 1;

        // Offline Progress (Unix epoch seconds)
        public long LastSaveTimestamp { get; set; } = 0;
    }
}
