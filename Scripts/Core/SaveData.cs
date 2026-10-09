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

        // Manor Gate Progression & Runes (GDD Section 5)
        public RuneType ActiveRune { get; set; } = RuneType.None;
        public GateApproachType SelectedGateApproach { get; set; } = GateApproachType.None;
        public bool HasEncounteredGate { get; set; } = false;
        public bool HasClaimedGateReward { get; set; } = false;
        public bool HasFirstLoreScroll { get; set; } = false;

        // Manor Library Research Progression (GDD Section 2 & 4.4)
        public int LoreScrolls { get; set; } = 0;
        public System.Collections.Generic.Dictionary<string, int> ResearchLevels { get; set; } = new();
        public System.Collections.Generic.List<int> DefeatedMilestoneBosses { get; set; } = new();

        // Awakening / Rebirth Progression (GDD Section 4.5)
        public int AwakeningPoints { get; set; } = 0;
        public int TotalAwakenings { get; set; } = 0;
        public System.Collections.Generic.Dictionary<string, int> AwakeningLevels { get; set; } = new();

        // Lore Relics Progression (GDD Section 2 & 6)
        public System.Collections.Generic.List<string> CollectedRelics { get; set; } = new();

        // Audio Settings
        public float MasterVolume { get; set; } = 1.0f;
        public float BgmVolume { get; set; } = 0.8f;
        public float SfxVolume { get; set; } = 1.0f;
        public bool IsMuted { get; set; } = false;

        // Tutorial / Onboarding (FTUE)
        public int TutorialStep { get; set; } = 1;
    }
}
