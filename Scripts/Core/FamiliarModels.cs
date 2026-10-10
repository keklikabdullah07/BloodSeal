namespace BloodSeal.Core
{
    public enum FamiliarType
    {
        BloodRaven,
        ShadowBat,
        CrimsonHound,
        StoneGargoyle
    }

    public class FamiliarDefinition
    {
        public FamiliarType Type { get; set; }
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public string PassiveDescription { get; set; } = "";
        public string IconPath { get; set; } = "";
        public string AuraColorHex { get; set; } = "#E51940";
        public float BaseAttackMultiplier { get; set; } = 0.4f;
        public float BaseAttackInterval { get; set; } = 1.2f;
        public double BaseUpgradeCost { get; set; } = 100.0;
        public int UnlockWaveRequirement { get; set; } = 1;
    }

    public class FamiliarProgress
    {
        public string Id { get; set; } = "";
        public int Level { get; set; } = 1;
        public bool IsUnlocked { get; set; } = false;

        public int Tier => (Level - 1) / 10 + 1;
        public bool IsMilestoneLevel => (Level % 10) == 0;
    }
}
