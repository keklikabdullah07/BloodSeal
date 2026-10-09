namespace BloodSeal.Combat
{
    public enum ZoneType
    {
        Ruins = 1,      // Dalga 1 - 10: Terk Edilmiş Harabeler
        Crypt = 2,      // Dalga 11 - 20: Kadim Kripta
        Cathedral = 3   // Dalga 21+: Kan Katedrali
    }

    public static class ZoneHelper
    {
        public static ZoneType GetZoneForWave(int wave)
        {
            if (wave <= 10) return ZoneType.Ruins;
            if (wave <= 20) return ZoneType.Crypt;
            return ZoneType.Cathedral;
        }

        public static string GetZoneName(ZoneType zone) => zone switch
        {
            ZoneType.Ruins => "Bölge I: Terk Edilmiş Harabeler",
            ZoneType.Crypt => "Bölge II: Kadim Kripta",
            ZoneType.Cathedral => "Bölge III: Kan Katedrali",
            _ => "Bölge I: Terk Edilmiş Harabeler"
        };

        public static string GetMinionTexturePath(int wave)
        {
            var zone = GetZoneForWave(wave);
            return zone switch
            {
                ZoneType.Ruins => "res://Assets/Sprites/Characters/enemy_cultist.png",
                ZoneType.Crypt => "res://Assets/Sprites/Characters/enemy_skeleton_warrior.png",
                ZoneType.Cathedral => "res://Assets/Sprites/Characters/enemy_gargoyle.png",
                _ => "res://Assets/Sprites/Characters/enemy_cultist.png"
            };
        }

        public static string GetBossTexturePath(int wave)
        {
            if (wave <= 10) return "res://Assets/Sprites/Characters/boss_abomination.png";
            if (wave <= 20) return "res://Assets/Sprites/Characters/boss_crypt_revenant.png";
            return "res://Assets/Sprites/Characters/boss_vampire_patriarch.png";
        }

        public static string GetBossName(int wave)
        {
            if (wave <= 10) return "Heybetli Kan Lordu";
            if (wave <= 20) return "Kripta Heyulası";
            return "Vampir Patriği";
        }
    }
}
