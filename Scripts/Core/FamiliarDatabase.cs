using System.Collections.Generic;

namespace BloodSeal.Core
{
    public static class FamiliarDatabase
    {
        private static readonly Dictionary<string, FamiliarDefinition> _familiars = new()
        {
            ["blood_raven"] = new FamiliarDefinition
            {
                Type = FamiliarType.BloodRaven,
                Id = "blood_raven",
                Name = "Kan Kargası",
                Title = "Kızıl Casus",
                Description = "Göklerin kan kokusunu takip eden kadim karga. Hızlı kan küreleri fırlatır.",
                PassiveDescription = "Seviye başına yoldaş hasarı +%10 artar. Her Tier'da Kahraman ATK Hızı +%3 artar.",
                IconPath = "res://Assets/Sprites/Pet/pet_blood_raven.png",
                AuraColorHex = "#E51940",
                BaseAttackMultiplier = 0.40f,
                BaseAttackInterval = 1.20f,
                BaseUpgradeCost = 100.0,
                UnlockWaveRequirement = 1
            },
            ["shadow_bat"] = new FamiliarDefinition
            {
                Type = FamiliarType.ShadowBat,
                Id = "shadow_bat",
                Name = "Gölge Yarasası",
                Title = "Gece Avcısı",
                Description = "Karanlık mahzenlerin sinsi yarasası. Çift gölge küresi fırlatır.",
                PassiveDescription = "Kahramana +%5..%20 Kritik Şans ve +%15..%60 Kritik Hasar aurası bağışlar.",
                IconPath = "res://Assets/Sprites/Pet/pet_blood_raven.png",
                AuraColorHex = "#8C26D9",
                BaseAttackMultiplier = 0.35f,
                BaseAttackInterval = 1.40f,
                BaseUpgradeCost = 500.0,
                UnlockWaveRequirement = 15
            },
            ["crimson_hound"] = new FamiliarDefinition
            {
                Type = FamiliarType.CrimsonHound,
                Id = "crimson_hound",
                Name = "Kan Tazısı",
                Title = "Kızıl Takipçi",
                Description = "Kan kokusuyla doymayan cehennem köpeği. Güçlü kan dalgaları fırlatır.",
                PassiveDescription = "Düşmanlardan düşen Altını +%15..%60 artırır; Boss savunmasını kırar.",
                IconPath = "res://Assets/Sprites/Pet/pet_blood_raven.png",
                AuraColorHex = "#D90D0D",
                BaseAttackMultiplier = 0.55f,
                BaseAttackInterval = 1.60f,
                BaseUpgradeCost = 2500.0,
                UnlockWaveRequirement = 25
            },
            ["stone_gargoyle"] = new FamiliarDefinition
            {
                Type = FamiliarType.StoneGargoyle,
                Id = "stone_gargoyle",
                Name = "Gece Heykeli",
                Title = "Kadim Gargoyle",
                Description = "Katedral çatılarının taştan muhafızı. Ağır taş şoku ve alan sarsıntısı yaratır.",
                PassiveDescription = "Kahramanın aldığı hasarı %10..%35 azaltır (Zırh); Berserk dolumunu hızlandırır.",
                IconPath = "res://Assets/Sprites/Pet/pet_blood_raven.png",
                AuraColorHex = "#B3A694",
                BaseAttackMultiplier = 0.80f,
                BaseAttackInterval = 2.20f,
                BaseUpgradeCost = 10000.0,
                UnlockWaveRequirement = 40
            }
        };

        public static IEnumerable<FamiliarDefinition> GetAll() => _familiars.Values;

        public static FamiliarDefinition Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return _familiars["blood_raven"];
            return _familiars.TryGetValue(id, out var def) ? def : _familiars["blood_raven"];
        }
    }
}
