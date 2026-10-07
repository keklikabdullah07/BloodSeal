using System;

namespace BloodSeal.Core
{
    public enum GateApproachType
    {
        None = 0,
        FrontGate = 1,        // Ön Kapıyı Kır
        Sewers = 2,           // Kanalizasyondan Sız
        BloodSeal = 3,        // Kanınla Mühürle
        RoofInfiltration = 4  // Rüşvet / Çatıdan Sızma
    }

    public enum RuneType
    {
        None = 0,
        BloodArmor = 1,   // +%10 Saldırı Gücü, +%10 Maksimum Can
        ShadowSpeed = 2,  // +%10 Saldırı Hızı, +25px Saldırı Menzili
        SoulLeech = 3,    // +%2.5 Can Çalma + Hikaye Parşömeni #1
        WealthGreed = 4   // +%20 Altın Kazanımı
    }

    public static class ManorGateHelper
    {
        public const int GateUnlockWave = 5;

        public static RuneType GetRuneForApproach(GateApproachType approach) => approach switch
        {
            GateApproachType.FrontGate => RuneType.BloodArmor,
            GateApproachType.Sewers => RuneType.ShadowSpeed,
            GateApproachType.BloodSeal => RuneType.SoulLeech,
            GateApproachType.RoofInfiltration => RuneType.WealthGreed,
            _ => RuneType.None
        };

        public static string GetApproachName(GateApproachType approach) => approach switch
        {
            GateApproachType.FrontGate => "Ön Kapıyı Kır",
            GateApproachType.Sewers => "Kanalizasyondan Sız",
            GateApproachType.BloodSeal => "Kanınla Mühürle",
            GateApproachType.RoofInfiltration => "Çatıdan Sız / Rüşvet",
            _ => "Bilinmeyen Yaklaşım"
        };

        public static string GetApproachBossName(GateApproachType approach) => approach switch
        {
            GateApproachType.FrontGate => "Zırhlı Malikane Muhafızı",
            GateApproachType.Sewers => "Karanlık Kanalizasyon Yaratığı",
            GateApproachType.BloodSeal => "Kadim Kan Başrahibi",
            GateApproachType.RoofInfiltration => "Gözcü Muhafız Kaptanı",
            _ => "Malikane Gardiyanı"
        };

        public static string GetApproachBossDesc(GateApproachType approach) => approach switch
        {
            GateApproachType.FrontGate => "Yüksek Can ve Zırh. Doğrudan sert çarpışma gerektirir.",
            GateApproachType.Sewers => "Hızlı ve çevik yaratık. Düşük zırh ancak seri darbeler.",
            GateApproachType.BloodSeal => "Büyülü kan savunucusu. Mühürleri koruyan ezoterik güç.",
            GateApproachType.RoofInfiltration => "Geçmişinize özel taktiksel zayıflık ile savaşır.",
            _ => ""
        };

        public static string GetApproachTacticalAdvantage(GateApproachType approach, StreetOriginType origin)
        {
            if (approach != GateApproachType.RoofInfiltration)
                return "Doğrudan savaş stratejisi.";

            return origin switch
            {
                StreetOriginType.PitFighter => "Kafes Dövüşçüsü etkisi: Boss çatışmaya -%30 Can ile başlar!",
                StreetOriginType.StreetThief => "Sokak Hırsızı etkisi: Boss zırhı soyuldu (-%40 Hasar)! +%20 Altın bonusu.",
                StreetOriginType.ExMercenary => "Eski Paralı Asker etkisi: Nöbetçi rüşvetiyle şaşırdı (-%25 Saldırı Hızı)!",
                StreetOriginType.UnderAlchemist => "Kimyager asit tuzağı: Boss çatışmaya -%35 Can zayıflığı ile başlar!",
                StreetOriginType.GangLeader => "Çete şaşırtmacası: Boss savunması çöktü, pet hasarı artar!",
                _ => "Taktiksel zayıflatma uygulandı."
            };
        }

        public static string GetRuneName(RuneType rune) => rune switch
        {
            RuneType.BloodArmor => "Kan Zırhı Rünü",
            RuneType.ShadowSpeed => "Gölge Hızı Rünü",
            RuneType.SoulLeech => "Ruh Çalma Rünü",
            RuneType.WealthGreed => "Zenginlik Rünü",
            _ => "Rün Yok"
        };

        public static string GetRuneDesc(RuneType rune) => rune switch
        {
            RuneType.BloodArmor => "+%10 Saldırı Gücü & +%10 Maksimum Can",
            RuneType.ShadowSpeed => "+%10 Saldırı Hızı & +25px Saldırı Menzili",
            RuneType.SoulLeech => "+%2.5 Can Çalma & Kadim Parşömen #1",
            RuneType.WealthGreed => "+%20 Kalıcı Altın Kazanımı",
            _ => ""
        };
    }
}
