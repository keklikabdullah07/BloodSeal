using System;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public class PentagramStats
    {
        public int AtkLevel { get; set; } = 1;
        public int AtkSpeedLevel { get; set; } = 1;
        public int LifestealLevel { get; set; } = 1;
        public int MaxHpLevel { get; set; } = 1;
        public int RangeLevel { get; set; } = 1;

        // Stat Calculations (Base + Level Scaling + Bloodline/Origin Passives)
        public float Atk
        {
            get
            {
                float val = 10f + (AtkLevel - 1) * 3f;
                if (GameManager.Instance?.Profile?.Origin == StreetOriginType.PitFighter)
                    val *= 1.10f; // +%10 ATK
                return val;
            }
        }

        public float AtkSpeed
        {
            get
            {
                float val = Math.Min(3.5f, 1.0f + (AtkSpeedLevel - 1) * 0.05f);
                if (GameManager.Instance?.Profile?.Origin == StreetOriginType.ExMercenary)
                    val *= 1.08f; // +%8 Saldırı Hızı
                if (GameManager.Instance?.Profile?.Bloodline == BloodlineType.SoulDrinker)
                    val *= 1.15f; // -%15 Bekleme süresi eşdeğeri hız
                return val;
            }
        }

        public float LifestealPercent
        {
            get
            {
                float val = Math.Min(25f, 1.0f + (LifestealLevel - 1) * 0.5f);
                if (GameManager.Instance?.Profile?.Bloodline == BloodlineType.BloodClawed)
                    val += 2.5f; // +%2.5 Doğuştan Can Çalma
                return val;
            }
        }

        public float MaxHp
        {
            get
            {
                float val = 100f + (MaxHpLevel - 1) * 25f;
                if (GameManager.Instance?.Profile?.Bloodline == BloodlineType.BoneWeaver)
                    val *= 1.10f; // +%10 Maksimum Can
                return val;
            }
        }

        public float Range
        {
            get
            {
                float val = Math.Min(350f, 180f + (RangeLevel - 1) * 10f);
                if (GameManager.Instance?.Profile?.Bloodline == BloodlineType.ShadowVeined)
                    val += 35f; // +35px Menzil
                return val;
            }
        }

        // Upgrade Costs (Base * 1.15^(level-1) - GDD Standardı)
        public long GetAtkCost() => (long)(20 * Math.Pow(1.15, AtkLevel - 1));
        public long GetAtkSpeedCost() => (long)(30 * Math.Pow(1.15, AtkSpeedLevel - 1));
        public long GetLifestealCost() => (long)(40 * Math.Pow(1.15, LifestealLevel - 1));
        public long GetMaxHpCost() => (long)(25 * Math.Pow(1.15, MaxHpLevel - 1));
        public long GetRangeCost() => (long)(20 * Math.Pow(1.15, RangeLevel - 1));
    }
}
