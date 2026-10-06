using System;

namespace BloodSeal.Combat
{
    public class PentagramStats
    {
        public int AtkLevel { get; set; } = 1;
        public int AtkSpeedLevel { get; set; } = 1;
        public int LifestealLevel { get; set; } = 1;
        public int MaxHpLevel { get; set; } = 1;
        public int RangeLevel { get; set; } = 1;

        // Stat Calculations
        public float Atk => 10f + (AtkLevel - 1) * 3f;
        public float AtkSpeed => Math.Min(3.5f, 1.0f + (AtkSpeedLevel - 1) * 0.05f);
        public float LifestealPercent => Math.Min(25f, 1.0f + (LifestealLevel - 1) * 0.5f);
        public float MaxHp => 100f + (MaxHpLevel - 1) * 25f;
        public float Range => Math.Min(350f, 180f + (RangeLevel - 1) * 10f);

        // Upgrade Costs (Base * 1.15^(level-1))
        public long GetAtkCost() => (long)(20 * Math.Pow(1.15, AtkLevel - 1));
        public long GetAtkSpeedCost() => (long)(30 * Math.Pow(1.18, AtkSpeedLevel - 1));
        public long GetLifestealCost() => (long)(40 * Math.Pow(1.22, LifestealLevel - 1));
        public long GetMaxHpCost() => (long)(25 * Math.Pow(1.15, MaxHpLevel - 1));
        public long GetRangeCost() => (long)(20 * Math.Pow(1.14, RangeLevel - 1));
    }
}
