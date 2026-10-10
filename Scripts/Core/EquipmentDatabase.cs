#nullable enable
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public static class EquipmentDatabase
    {
        private static readonly Dictionary<string, EquipmentDefinition> _items = new()
        {
            // --- Silahlar (Weapon) ---
            ["weap_blood_rapier"] = new EquipmentDefinition
            {
                Id = "weap_blood_rapier", Name = "Kızıl Meç", Slot = EquipmentSlot.Weapon,
                IconSymbol = "🗡️", Lore = "Kraliyet muhafızlarının kanla bilenen zarif düello kılıcı.",
                BasePrimaryValue = 15.0f, PrimaryStatLabel = "ATK", BaseUpgradeCost = 150.0
            },
            ["weap_executioner_blade"] = new EquipmentDefinition
            {
                Id = "weap_executioner_blade", Name = "İnfazcı Palası", Slot = EquipmentSlot.Weapon,
                IconSymbol = "⚔️", Lore = "Katedral zindanlarında yüzlerce kafayı uçurmuş devasa pala.",
                BasePrimaryValue = 28.0f, PrimaryStatLabel = "ATK", BaseUpgradeCost = 250.0
            },
            ["weap_crypt_scythe"] = new EquipmentDefinition
            {
                Id = "weap_crypt_scythe", Name = "Kripta Tırpanı", Slot = EquipmentSlot.Weapon,
                IconSymbol = "🪓", Lore = "Mahzenlerin karanlığında ruhları biçen kavisli antik tırpan.",
                BasePrimaryValue = 20.0f, PrimaryStatLabel = "ATK", BaseUpgradeCost = 200.0
            },
            ["weap_vampire_fang"] = new EquipmentDefinition
            {
                Id = "weap_vampire_fang", Name = "Vampir Hançeri", Slot = EquipmentSlot.Weapon,
                IconSymbol = "🔪", Lore = "İlk kan emicinin dişinden oyulmuş, durmaksızın kan damlatan hançer.",
                BasePrimaryValue = 18.0f, PrimaryStatLabel = "ATK", BaseUpgradeCost = 180.0
            },

            // --- Zırhlar (Armor) ---
            ["arm_night_cloak"] = new EquipmentDefinition
            {
                Id = "arm_night_cloak", Name = "Gece Muhafızı Pelerini", Slot = EquipmentSlot.Armor,
                IconSymbol = "🧥", Lore = "Gölgelerle örülmüş, darbeleri sönümleyen ağır gotik pelerin.",
                BasePrimaryValue = 80.0f, PrimaryStatLabel = "Max HP", BaseUpgradeCost = 150.0
            },
            ["arm_bone_carapace"] = new EquipmentDefinition
            {
                Id = "arm_bone_carapace", Name = "Kemik Zırhı", Slot = EquipmentSlot.Armor,
                IconSymbol = "🦴", Lore = "Kadim savaşçıların göğüs kafeslerinden örülmüş zırh plakası.",
                BasePrimaryValue = 140.0f, PrimaryStatLabel = "Max HP", BaseUpgradeCost = 220.0
            },
            ["arm_inquisitor_tunic"] = new EquipmentDefinition
            {
                Id = "arm_inquisitor_tunic", Name = "Engizisyon Cübbesi", Slot = EquipmentSlot.Armor,
                IconSymbol = "🥋", Lore = "Kutsal mühürlerle dokunmuş, acıya karşı hissizleştiren cübbe.",
                BasePrimaryValue = 110.0f, PrimaryStatLabel = "Max HP", BaseUpgradeCost = 180.0
            },
            ["arm_blood_regalia"] = new EquipmentDefinition
            {
                Id = "arm_blood_regalia", Name = "Kadim Kan Zırhı", Slot = EquipmentSlot.Armor,
                IconSymbol = "🛡️", Lore = "Pıhtılaşmış kandan dövülmüş, taşıyıcısına hanedan direnci veren zırh.",
                BasePrimaryValue = 220.0f, PrimaryStatLabel = "Max HP", BaseUpgradeCost = 300.0
            },

            // --- Tılsımlar (Amulet) ---
            ["amu_covenant_pendant"] = new EquipmentDefinition
            {
                Id = "amu_covenant_pendant", Name = "Ahit Madalyonu", Slot = EquipmentSlot.Amulet,
                IconSymbol = "📿", Lore = "Ters pentagram işlemeli bakır madalyon. Düşmanın canını emer.",
                BasePrimaryValue = 2.5f, PrimaryStatLabel = "% Can Çalma", BaseUpgradeCost = 150.0
            },
            ["amu_ashen_rosary"] = new EquipmentDefinition
            {
                Id = "amu_ashen_rosary", Name = "Kül Tespihi", Slot = EquipmentSlot.Amulet,
                IconSymbol = "🧿", Lore = "Veba kurbanlarının küllerinden şekillendirilmiş karanlık tespih.",
                BasePrimaryValue = 2.0f, PrimaryStatLabel = "% Can Çalma", BaseUpgradeCost = 180.0
            },
            ["amu_heart_locket"] = new EquipmentDefinition
            {
                Id = "amu_heart_locket", Name = "Taşlaşmış Kalp", Slot = EquipmentSlot.Amulet,
                IconSymbol = "🖤", Lore = "Kömürleşmiş bir vampir kalbi barındıran zincirli madalyon.",
                BasePrimaryValue = 3.0f, PrimaryStatLabel = "% Can Çalma", BaseUpgradeCost = 220.0
            },
            ["amu_blood_tear"] = new EquipmentDefinition
            {
                Id = "amu_blood_tear", Name = "Vampir Gözyaşı", Slot = EquipmentSlot.Amulet,
                IconSymbol = "🩸", Lore = "Yakut bir damla biçimindeki tılsım. Kritik vuruşları derinleştirir.",
                BasePrimaryValue = 3.5f, PrimaryStatLabel = "% Can Çalma", BaseUpgradeCost = 260.0
            },

            // --- Yüzükler (Ring) ---
            ["ring_darius_signet"] = new EquipmentDefinition
            {
                Id = "ring_darius_signet", Name = "Darius Mührü", Slot = EquipmentSlot.Ring,
                IconSymbol = "💍", Lore = "Hanedanın kayıp varisine ait mühür yüzüğü. Saldırı hızını artırır.",
                BasePrimaryValue = 0.08f, PrimaryStatLabel = "% Atk Hızı", BaseUpgradeCost = 150.0
            },
            ["ring_ruby_band"] = new EquipmentDefinition
            {
                Id = "ring_ruby_band", Name = "Yakut Kan Halkası", Slot = EquipmentSlot.Ring,
                IconSymbol = "🔴", Lore = "Karanlıkta kan damlası gibi parıldayan yakut yüzük.",
                BasePrimaryValue = 0.10f, PrimaryStatLabel = "% Atk Hızı", BaseUpgradeCost = 200.0
            },
            ["ring_thorn_circle"] = new EquipmentDefinition
            {
                Id = "ring_thorn_circle", Name = "Dikenli Yüzük", Slot = EquipmentSlot.Ring,
                IconSymbol = "⭕", Lore = "Parmak derisine batan dikenli halka; öfkeyi körükler.",
                BasePrimaryValue = 0.12f, PrimaryStatLabel = "% Atk Hızı", BaseUpgradeCost = 220.0
            },
            ["ring_eternal_seal"] = new EquipmentDefinition
            {
                Id = "ring_eternal_seal", Name = "Ebedi Ahit Yüzüğü", Slot = EquipmentSlot.Ring,
                IconSymbol = "👑", Lore = "13 Mührün gizemini taşıyan ebedi halka.",
                BasePrimaryValue = 0.15f, PrimaryStatLabel = "% Atk Hızı", BaseUpgradeCost = 280.0
            }
        };

        public static IEnumerable<EquipmentDefinition> GetAll() => _items.Values;
        public static EquipmentDefinition Get(string id) => _items.TryGetValue(id, out var d) ? d : _items["weap_blood_rapier"];
        public static IEnumerable<EquipmentDefinition> GetBySlot(EquipmentSlot slot)
        {
            foreach (var item in _items.Values)
                if (item.Slot == slot) yield return item;
        }
    }
}
