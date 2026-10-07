#nullable enable
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

        public CharacterProfile? Profile { get; set; }
        public RuneType ActiveRune { get; set; } = RuneType.None;

        public PentagramStats(CharacterProfile? profile = null)
        {
            Profile = profile;
        }

        // Stat Calculations (Base + Level Scaling + Bloodline/Origin Passives + Runes)
        public float Atk
        {
            get
            {
                float val = 10f + (AtkLevel - 1) * 3f;
                if (Profile?.Origin == StreetOriginType.PitFighter)
                    val *= 1.10f; // +%10 ATK
                if (ActiveRune == RuneType.BloodArmor)
                    val *= 1.10f; // Kan Zırhı Rünü (+%10 ATK)
                return val;
            }
        }

        public float AtkSpeed
        {
            get
            {
                float val = 1.0f + (AtkSpeedLevel - 1) * 0.05f;
                if (Profile?.Origin == StreetOriginType.ExMercenary)
                    val *= 1.08f; // +%8 Saldırı Hızı
                if (Profile?.Bloodline == BloodlineType.SoulDrinker)
                    val *= 1.15f; // -%15 Bekleme süresi eşdeğeri hız
                if (ActiveRune == RuneType.ShadowSpeed)
                    val *= 1.10f; // Gölge Hızı Rünü (+%10 Saldırı Hızı)
                return Math.Min(3.5f, val);
            }
        }

        public float LifestealPercent
        {
            get
            {
                float val = 1.0f + (LifestealLevel - 1) * 0.5f;
                if (Profile?.Bloodline == BloodlineType.BloodClawed)
                    val += 2.5f; // +%2.5 Doğuştan Can Çalma
                if (ActiveRune == RuneType.SoulLeech)
                    val += 2.5f; // Ruh Çalma Rünü (+%2.5 Can Çalma)
                return Math.Min(25f, val);
            }
        }

        public float MaxHp
        {
            get
            {
                float val = 100f + (MaxHpLevel - 1) * 25f;
                if (Profile?.Bloodline == BloodlineType.BoneWeaver)
                    val *= 1.10f; // +%10 Maksimum Can
                if (ActiveRune == RuneType.BloodArmor)
                    val *= 1.10f; // Kan Zırhı Rünü (+%10 Max HP)
                return val;
            }
        }

        public float Range
        {
            get
            {
                float val = 180f + (RangeLevel - 1) * 10f;
                if (Profile?.Bloodline == BloodlineType.ShadowVeined)
                    val += 35f; // +35px Menzil
                if (ActiveRune == RuneType.ShadowSpeed)
                    val += 25f; // Gölge Hızı Rünü (+25px Menzil)
                return Math.Min(350f, val);
            }
        }

        // Upgrade Costs (Base * 1.15^(level-1) - GDD Standardı, double currency)
        public double GetAtkCost() => 20.0 * Math.Pow(1.15, AtkLevel - 1);
        public double GetAtkSpeedCost() => 30.0 * Math.Pow(1.15, AtkSpeedLevel - 1);
        public double GetLifestealCost() => 40.0 * Math.Pow(1.15, LifestealLevel - 1);
        public double GetMaxHpCost() => 25.0 * Math.Pow(1.15, MaxHpLevel - 1);
        public double GetRangeCost() => 20.0 * Math.Pow(1.15, RangeLevel - 1);
    }
}
