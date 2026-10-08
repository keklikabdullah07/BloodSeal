#nullable enable
using System;
using System.Collections.Generic;

namespace BloodSeal.Core
{
    public enum RelicStatType
    {
        Damage = 0,
        MaxHp = 1,
        Gold = 2,
        RageGain = 3,
        TapDamage = 4,
        OfflineIncome = 5,
        Lifesteal = 6,
        AttackSpeed = 7,
        AwakeningPoints = 8,
        AllStats = 9
    }

    public class RelicDefinition
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int MilestoneWave { get; set; }
        public string IconSymbol { get; set; } = "💍";
        public string LoreText { get; set; } = string.Empty;
        public RelicStatType StatType { get; set; }
        public float BonusValue { get; set; }

        public string BonusDisplay => StatType switch
        {
            RelicStatType.Damage => $"+{BonusValue * 100f:F0}% Kahraman Hasarı",
            RelicStatType.MaxHp => $"+{BonusValue * 100f:F0}% Maksimum Can",
            RelicStatType.Gold => $"+{BonusValue * 100f:F0}% Altın Kazanımı",
            RelicStatType.RageGain => $"+{BonusValue * 100f:F0}% Öfke Dolum Hızı",
            RelicStatType.TapDamage => $"+{BonusValue * 100f:F0}% Tıklama Hasarı",
            RelicStatType.OfflineIncome => $"+{BonusValue * 100f:F0}% Çevrimdışı Gelir",
            RelicStatType.Lifesteal => $"+{BonusValue:F1}% Taban Can Çalma",
            RelicStatType.AttackSpeed => $"+{BonusValue * 100f:F0}% Saldırı Hızı",
            RelicStatType.AwakeningPoints => $"+{BonusValue * 100f:F0}% Uyanış Puanı Çarpanı",
            RelicStatType.AllStats => $"+{BonusValue * 100f:F0}% Tüm İstatistikler Çarpanı",
            _ => "+0%"
        };
    }

    public static class RelicDatabase
    {
        private static readonly Dictionary<string, RelicDefinition> RelicsById = new();
        private static readonly Dictionary<int, RelicDefinition> RelicsByWave = new();

        static RelicDatabase()
        {
            Register(new RelicDefinition
            {
                Id = "Relic_DariusRing",
                Name = "Darius'un Kanlı Mührü",
                MilestoneWave = 10,
                IconSymbol = "💍",
                LoreText = "\"Darius son nefesinde mührü avucuma bastırdığında kanı henüz sıcaktı. 'Ahit'i durdur,' dedi, 'küllerimiz onların sunağı olmasın.'\"",
                StatType = RelicStatType.Damage,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_TornPortrait",
                Name = "Yırtık Aile Portresi",
                MilestoneWave = 20,
                IconSymbol = "🖼️",
                LoreText = "\"Yüzleri jiletle kazınmış bir soylu ailesi. Altında soluk bir imza: 'Kan bağı asla çözülmez, sadece pıhtılaşır.'\"",
                StatType = RelicStatType.MaxHp,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_CovenantMedallion",
                Name = "Kızıl Ahit Madalyonu",
                MilestoneWave = 30,
                IconSymbol = "📿",
                LoreText = "\"Tarikatın yüksek rahiplerinin taktığı ters pentagram madalyon. Dokunduğunda parmak uçlarında açgözlü bir sızı bırakıyor.\"",
                StatType = RelicStatType.Gold,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_BlackenedBell",
                Name = "Kararmış Zangoç Çanı",
                MilestoneWave = 40,
                IconSymbol = "🔔",
                LoreText = "\"Varnath Katedrali'nin veba gecesinde çaldığı son çan. Sesi artık kulaklarda değil, doğrudan damarlarda yankılanıyor.\"",
                StatType = RelicStatType.RageGain,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_InquisitorMask",
                Name = "Engizisyon Maskesi",
                MilestoneWave = 50,
                IconSymbol = "🎭",
                LoreText = "\"Kuş gagası biçiminde dövülmüş demir maske. İç yüzeyinde kuruyan kan, takan kişinin kendi çığlıklarına ait.\"",
                StatType = RelicStatType.TapDamage,
                BonusValue = 0.10f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_CryptKey",
                Name = "Kadim Kripta Anahtarı",
                MilestoneWave = 60,
                IconSymbol = "🗝️",
                LoreText = "\"Malikane'nin unutulmuş alt mahzenlerini açan ağır pirinç anahtar. Zamanın bile unuttuğu hazinelerin bekçisi.\"",
                StatType = RelicStatType.OfflineIncome,
                BonusValue = 0.10f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_BoneChalice",
                Name = "Kemik Kadeh",
                MilestoneWave = 70,
                IconSymbol = "🍷",
                LoreText = "\"İlk mühür taşıyıcısının kaval kemiğinden oyulmuş kadeh. İçine dökülen her damla kan, içenin susuzluğunu ebediyen dindiriyor.\"",
                StatType = RelicStatType.Lifesteal,
                BonusValue = 0.5f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_KnightSpur",
                Name = "Kan Şövalyesi Mahmuzu",
                MilestoneWave = 80,
                IconSymbol = "⚔️",
                LoreText = "\"Kızıl orduların öncülerine ait paslanmış mahmuz. Savaş alanında durmak bilmeyen bir vahşetin hatırası.\"",
                StatType = RelicStatType.AttackSpeed,
                BonusValue = 0.05f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_ExtinguishedLantern",
                Name = "Sönmüş Ruh Feneri",
                MilestoneWave = 90,
                IconSymbol = "🏮",
                LoreText = "\"İçinde bir zamanlar hapsolmuş yüzlerce gölge ruhunun fısıltıları olan fener. Ölüm ve yeniden doğuş arasındaki köprü.\"",
                StatType = RelicStatType.AwakeningPoints,
                BonusValue = 0.10f
            });
            Register(new RelicDefinition
            {
                Id = "Relic_FirstScroll",
                Name = "Kökenin İlk Parşömeni",
                MilestoneWave = 100,
                IconSymbol = "📜",
                LoreText = "\"13 Mührün yaratıldığı gün yazılan ilk kutsal parşömen. Varnath'ın gerçek yaratılış sırrını fısıldıyor.\"",
                StatType = RelicStatType.AllStats,
                BonusValue = 0.15f
            });
        }

        private static void Register(RelicDefinition relic)
        {
            RelicsById[relic.Id] = relic;
            RelicsByWave[relic.MilestoneWave] = relic;
        }

        public static RelicDefinition? GetRelic(string id) => RelicsById.GetValueOrDefault(id);
        public static RelicDefinition? GetRelicForWave(int wave) => RelicsByWave.GetValueOrDefault(wave);
        public static IReadOnlyList<RelicDefinition> AllRelics => new List<RelicDefinition>(RelicsById.Values);
    }
}
