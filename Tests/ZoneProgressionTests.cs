using Xunit;
using BloodSeal.Combat;

namespace BloodSeal.Tests
{
    public class ZoneProgressionTests
    {
        [Theory]
        [InlineData(1, ZoneType.Ruins, "Bölge I: Terk Edilmiş Harabeler", "enemy_cultist.png", "boss_abomination.png")]
        [InlineData(10, ZoneType.Ruins, "Bölge I: Terk Edilmiş Harabeler", "enemy_cultist.png", "boss_abomination.png")]
        [InlineData(11, ZoneType.Crypt, "Bölge II: Kadim Kripta", "enemy_skeleton_warrior.png", "boss_crypt_revenant.png")]
        [InlineData(20, ZoneType.Crypt, "Bölge II: Kadim Kripta", "enemy_skeleton_warrior.png", "boss_crypt_revenant.png")]
        [InlineData(21, ZoneType.Cathedral, "Bölge III: Kan Katedrali", "enemy_gargoyle.png", "boss_vampire_patriarch.png")]
        [InlineData(30, ZoneType.Cathedral, "Bölge III: Kan Katedrali", "enemy_gargoyle.png", "boss_vampire_patriarch.png")]
        public void ZoneHelper_ReturnsCorrectZoneAndTextures(int wave, ZoneType expectedZone, string expectedName, string expectedMinionTex, string expectedBossTex)
        {
            var zone = ZoneHelper.GetZoneForWave(wave);
            Assert.Equal(expectedZone, zone);
            Assert.Equal(expectedName, ZoneHelper.GetZoneName(zone));

            string minionPath = ZoneHelper.GetMinionTexturePath(wave);
            Assert.Contains(expectedMinionTex, minionPath);

            string bossPath = ZoneHelper.GetBossTexturePath(wave);
            Assert.Contains(expectedBossTex, bossPath);
        }
    }
}
