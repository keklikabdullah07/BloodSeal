namespace BloodSeal.Core
{
    public enum BloodlineType
    {
        BoneWeaver,    // Kemik Dokulu (+%10 Maksimum Can)
        ShadowVeined,  // Gölge Damarlı (+35px Saldırı Menzili)
        BloodClawed,   // Kan Pençeli (+%2.5 Doğuştan Can Çalma)
        SteelFleshed,  // Çelik Dokulu (Gelen hasardan -3 düz azaltma)
        SoulDrinker    // Ruh Emici (-%15 Saldırı bekleme süresi)
    }

    public enum StreetOriginType
    {
        PitFighter,      // Kafes Dövüşçüsü (+%10 Saldırı Gücü)
        StreetThief,     // Sokak Hırsızı (+%15 Altın Kazanımı)
        ExMercenary,     // Eski Paralı Asker (+%8 Saldırı Hızı)
        UnderAlchemist,  // Yeraltı Kimyageri (+%25 Tıklama Hasarı)
        GangLeader       // Çete Lideri (+%25 Ruh/Pet Atış Hızı)
    }

    public class CharacterProfile
    {
        public string PlayerName { get; set; } = "Valerius";
        public BloodlineType Bloodline { get; set; } = BloodlineType.BoneWeaver;
        public StreetOriginType Origin { get; set; } = StreetOriginType.PitFighter;
        public bool HasCompletedPrologue { get; set; } = false;

        public static string GetBloodlineName(BloodlineType type) => type switch
        {
            BloodlineType.BoneWeaver => "Kemik Dokulu",
            BloodlineType.ShadowVeined => "Gölge Damarlı",
            BloodlineType.BloodClawed => "Kan Pençeli",
            BloodlineType.SteelFleshed => "Çelik Dokulu",
            BloodlineType.SoulDrinker => "Ruh Emici",
            _ => type.ToString()
        };

        public static string GetBloodlineDesc(BloodlineType type) => type switch
        {
            BloodlineType.BoneWeaver => "+%10 Maksimum Can",
            BloodlineType.ShadowVeined => "+35px Saldırı Menzili",
            BloodlineType.BloodClawed => "+%2.5 Doğuştan Can Çalma",
            BloodlineType.SteelFleshed => "Gelen Hasardan -3 Azaltma",
            BloodlineType.SoulDrinker => "-%15 Saldırı Bekleme Süresi",
            _ => ""
        };

        public static string GetOriginName(StreetOriginType type) => type switch
        {
            StreetOriginType.PitFighter => "Kafes Dövüşçüsü",
            StreetOriginType.StreetThief => "Sokak Hırsızı",
            StreetOriginType.ExMercenary => "Eski Paralı Asker",
            StreetOriginType.UnderAlchemist => "Yeraltı Kimyageri",
            StreetOriginType.GangLeader => "Çete Lideri",
            _ => type.ToString()
        };

        public static string GetOriginDesc(StreetOriginType type) => type switch
        {
            StreetOriginType.PitFighter => "+%10 Saldırı Gücü",
            StreetOriginType.StreetThief => "+%15 Ekstra Altın Kazanımı",
            StreetOriginType.ExMercenary => "+%8 Temel Saldırı Hızı",
            StreetOriginType.UnderAlchemist => "+%25 Tıklama (Tap) Hasarı",
            StreetOriginType.GangLeader => "+%25 Ruh / Pet Atış Hızı",
            _ => ""
        };
    }
}
