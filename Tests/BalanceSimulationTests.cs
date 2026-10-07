using System;
using System.IO;
using System.Text.Json;
using Xunit;
using Xunit.Abstractions;

namespace BloodSeal.Tests
{
    public class BalanceConfigModel
    {
        public JsonElement statGrowthFactors { get; set; }
        public JsonElement baseCosts { get; set; }
        public JsonElement heroBaseStats { get; set; }
        public JsonElement waveScaling { get; set; }
        public JsonElement bossEnrage { get; set; }
    }

    public class BalanceSimulationTests
    {
        private readonly ITestOutputHelper _output;

        public BalanceSimulationTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void Run_Wave_And_Boss_Progression_Simulation()
        {
            // 1. Locate and read Data/BalanceConfig.json
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Data", "BalanceConfig.json");
            configPath = Path.GetFullPath(configPath);
            Assert.True(File.Exists(configPath), $"BalanceConfig.json bulunamadi: {configPath}");

            string json = File.ReadAllText(configPath);
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var waveScaling = root.GetProperty("waveScaling");
            var heroStats = root.GetProperty("heroBaseStats");
            var enrageConfig = root.GetProperty("bossEnrage");

            // Hero state
            int atkLvl = 1;
            int atkSpdLvl = 1;
            int hpLvl = 1;

            double heroBaseAtk = heroStats.GetProperty("atk").GetDouble();
            double heroAtkPerLvl = heroStats.GetProperty("atkPerLevel").GetDouble();
            double heroBaseSpd = heroStats.GetProperty("atkSpeed").GetDouble();
            double heroSpdPerLvl = heroStats.GetProperty("atkSpeedPerLevel").GetDouble();
            double heroBaseHp = heroStats.GetProperty("maxHp").GetDouble();
            double heroHpPerLvl = heroStats.GetProperty("maxHpPerLevel").GetDouble();
            double lifestealPct = heroStats.GetProperty("lifestealPercent").GetDouble() / 100.0;

            double gold = 0.0;
            double totalElapsedTime = 0.0;

            _output.WriteLine("=========================================================================================");
            _output.WriteLine("🩸 BLOODSEAL DENGE SİMÜLASYONU RAPORU (Veriler: Data/BalanceConfig.json)");
            _output.WriteLine("=========================================================================================");
            _output.WriteLine($"| Dalga | Tip    | Düşman HP | Düşman ATK | Kahraman ATK | Süre (sn) | Toplam Süre | Altın  |");
            _output.WriteLine($"|-------|--------|-----------|------------|--------------|-----------|-------------|--------|");

            // Simulate Waves 1 to 9 (Minions)
            for (int wave = 1; wave <= 9; wave++)
            {
                double enemyHp = waveScaling.GetProperty("minionHpBase").GetDouble() + wave * waveScaling.GetProperty("minionHpPerWave").GetDouble();
                double enemyAtk = waveScaling.GetProperty("minionAtkBase").GetDouble() + wave * waveScaling.GetProperty("minionAtkPerWave").GetDouble();
                double enemyGold = waveScaling.GetProperty("minionGoldBase").GetDouble() + wave * waveScaling.GetProperty("minionGoldPerWave").GetDouble();

                double heroAtk = heroBaseAtk + (atkLvl - 1) * heroAtkPerLvl;
                double heroSpd = Math.Min(3.5, heroBaseSpd + (atkSpdLvl - 1) * heroSpdPerLvl);
                double heroDps = heroAtk * heroSpd + (heroAtk * 0.75 * 2.0); // 2 taps/sec tap dmg

                double timeToKill = enemyHp / heroDps;
                totalElapsedTime += timeToKill;
                gold += enemyGold;

                // Try upgrading ATK if affordable: Cost = 20 * 1.15^(level - 1)
                double atkCost = 20.0 * Math.Pow(1.15, atkLvl - 1);
                if (gold >= atkCost)
                {
                    gold -= atkCost;
                    atkLvl++;
                }

                _output.WriteLine($"| {wave,5} | Minyon | {enemyHp,9:F0} | {enemyAtk,10:F1} | {heroAtk,12:F1} | {timeToKill,9:F1} | {totalElapsedTime,11:F1} | {gold,6:F0} |");
            }

            double timeToFirstBoss = totalElapsedTime;
            _output.WriteLine("-----------------------------------------------------------------------------------------");
            _output.WriteLine($"⚡ 10. Dalga Boss'una Ulaşma Süresi: {timeToFirstBoss:F1} saniye (~{timeToFirstBoss / 60.0:F1} dakika)");

            // Simulate Wave 10 Boss
            int bossWave = 10;
            double bossBaseHp = waveScaling.GetProperty("bossHpBase").GetDouble() + bossWave * waveScaling.GetProperty("bossHpPerWave").GetDouble();
            double bossBaseAtk = waveScaling.GetProperty("bossAtkBase").GetDouble() + bossWave * waveScaling.GetProperty("bossAtkPerWave").GetDouble();
            double bossGold = waveScaling.GetProperty("bossGoldBase").GetDouble() + bossWave * waveScaling.GetProperty("bossGoldPerWave").GetDouble();

            double enrageInterval = enrageConfig.GetProperty("intervalSeconds").GetDouble();
            double enrageStep = enrageConfig.GetProperty("damageStepMultiplier").GetDouble();
            bool isMultiplicative = enrageConfig.TryGetProperty("isMultiplicative", out var multProp) && multProp.GetBoolean();

            int farmRetries = 0;
            bool bossDefeated = false;
            double bossFightDuration = 0.0;

            while (!bossDefeated && farmRetries < 20)
            {
                double currentBossHp = bossBaseHp;
                double heroMaxHp = heroBaseHp + (hpLvl - 1) * heroHpPerLvl;
                double currentHeroHp = heroMaxHp;
                double heroAtk = heroBaseAtk + (atkLvl - 1) * heroAtkPerLvl;
                double heroSpd = Math.Min(3.5, heroBaseSpd + (atkSpdLvl - 1) * heroSpdPerLvl);
                double heroDps = heroAtk * heroSpd + (heroAtk * 0.75 * 2.0);

                double fightTime = 0.0;
                double bossAttackTimer = 0.0;
                double dt = 0.1; // simulation tick

                while (currentBossHp > 0 && currentHeroHp > 0 && fightTime < 180.0)
                {
                    fightTime += dt;
                    bossAttackTimer += dt;

                    // Berserk active for first 10 seconds of boss fight
                    bool isBerserk = fightTime <= 10.0;
                    double currentHeroDps;
                    if (isBerserk)
                    {
                        // 2x ATK * 2x Speed + 2x Tap + 2x Pets
                        double rageAtk = heroAtk * 2.0;
                        double rageSpd = Math.Min(7.0, heroSpd * 2.0);
                        double tapDps = (rageAtk * 0.75 * 2.0);
                        double petDps = 2.0 * (rageAtk * 0.4 / 1.2);
                        currentHeroDps = rageAtk * rageSpd + tapDps + petDps;
                    }
                    else
                    {
                        double tapDps = (heroAtk * 0.75 * 2.0);
                        double petDps = 2.0 * (heroAtk * 0.4 / 1.2);
                        currentHeroDps = heroAtk * heroSpd + tapDps + petDps;
                    }

                    // Enrage multiplier (single source of truth: isMultiplicative)
                    int enrageSteps = (int)(fightTime / enrageInterval);
                    double enrageMultiplier = isMultiplicative
                        ? Math.Pow(1.0 + enrageStep, enrageSteps)
                        : 1.0 + (enrageSteps * enrageStep);
                    double currentBossAtk = bossBaseAtk * enrageMultiplier;

                    // Hero damages Boss
                    currentBossHp -= currentHeroDps * dt;

                    // Hero heals via lifesteal
                    currentHeroHp = Math.Min(heroMaxHp, currentHeroHp + (currentHeroDps * dt * lifestealPct));

                    // Boss attacks Hero (every 1s)
                    if (bossAttackTimer >= 1.0)
                    {
                        bossAttackTimer = 0.0;
                        currentHeroHp -= currentBossAtk;
                    }
                }

                if (currentBossHp <= 0)
                {
                    bossDefeated = true;
                    bossFightDuration = fightTime;
                    totalElapsedTime += fightTime;
                    gold += bossGold;
                    _output.WriteLine($"|    10 | BOSS   | {bossBaseHp,9:F0} | {bossBaseAtk,10:F1} | {heroAtk,12:F1} | {fightTime,9:F1} | {totalElapsedTime,11:F1} | {gold,6:F0} |");
                    break;
                }
                else
                {
                    // Hero died to boss! Safe farm wave 9 loop to gain gold and upgrade stats
                    farmRetries++;
                    double safeFarmGoldPerMinion = waveScaling.GetProperty("minionGoldBase").GetDouble() + 9 * waveScaling.GetProperty("minionGoldPerWave").GetDouble();
                    double safeFarmTimePerMinion = (waveScaling.GetProperty("minionHpBase").GetDouble() + 9 * waveScaling.GetProperty("minionHpPerWave").GetDouble()) / Math.Max(1.0, heroDps);

                    // 10 farm minions per retry cycle (~2 waves)
                    totalElapsedTime += safeFarmTimePerMinion * 10;
                    gold += safeFarmGoldPerMinion * 10;

                    // Spend gold on ATK and HP upgrades
                    double atkCost = 20.0 * Math.Pow(1.15, atkLvl - 1);
                    if (gold >= atkCost) { gold -= atkCost; atkLvl++; }

                    double hpCost = 25.0 * Math.Pow(1.15, hpLvl - 1);
                    if (gold >= hpCost) { gold -= hpCost; hpLvl++; }

                    if (farmRetries % 5 == 0 || farmRetries == 1)
                    {
                        _output.WriteLine($"[Farm #{farmRetries}] Kalan Boss HP: {currentBossHp:F0}/{bossBaseHp:F0} | Kahraman ATK Lv.{atkLvl}, HP Lv.{hpLvl} ({heroMaxHp:F0} HP)");
                    }
                }
            }

            _output.WriteLine("=========================================================================================");
            _output.WriteLine($"🏆 1. Boss'u Kesme Süresi (Savaş): {bossFightDuration:F1} sn (Gereken Farm Turu: {farmRetries})");
            _output.WriteLine($"⏱️ Toplam Oynanış Süresi (İlk Boss Dahil): {totalElapsedTime:F1} sn (~{totalElapsedTime / 60.0:F2} dk)");
            _output.WriteLine($"📈 Bitiş Seviyeleri: ATK Lv.{atkLvl}, ATK Hızı Lv.{atkSpdLvl}, Max HP Lv.{hpLvl}");
            _output.WriteLine("=========================================================================================");

            Assert.True(bossDefeated, "Boss 10 makul farm denemeleri içinde kesilebilmelidir.");
            Assert.True(timeToFirstBoss < 180.0, "10. Dalga Boss'una 3 dakikadan kısa sürede ulaşılmalıdır.");
        }

        [Fact]
        public void Verify_Boss_Enrage_Multiplier_At_60_Seconds()
        {
            string configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "Data", "BalanceConfig.json");
            configPath = Path.GetFullPath(configPath);
            Assert.True(File.Exists(configPath), $"BalanceConfig.json bulunamadi: {configPath}");

            string json = File.ReadAllText(configPath);
            using var doc = JsonDocument.Parse(json);
            var enrageConfig = doc.RootElement.GetProperty("bossEnrage");

            double interval = enrageConfig.GetProperty("intervalSeconds").GetDouble();
            double step = enrageConfig.GetProperty("damageStepMultiplier").GetDouble();
            bool isMultiplicative = enrageConfig.TryGetProperty("isMultiplicative", out var multProp) && multProp.GetBoolean();

            int stepsAt60Sec = (int)(60.0 / interval);
            Assert.Equal(12, stepsAt60Sec);

            double multiplierAt60Sec = isMultiplicative
                ? Math.Pow(1.0 + step, stepsAt60Sec)
                : 1.0 + (stepsAt60Sec * step);

            if (isMultiplicative)
            {
                // Çarpımsal mod: 1.25^12 = 14.551915... (~14.55)
                Assert.InRange(multiplierAt60Sec, 14.54, 14.56);
                _output.WriteLine($"[Enrage Doğrulama] 60. saniyedeki çarpan (Çarpımsal): {multiplierAt60Sec:F2}x (Beklenen: ~14.55)");
            }
            else
            {
                // Toplamsal mod: 1.0 + 12 * 0.25 = 4.0
                Assert.Equal(4.0, multiplierAt60Sec, precision: 4);
                _output.WriteLine($"[Enrage Doğrulama] 60. saniyedeki çarpan (Toplamsal): {multiplierAt60Sec:F1}x (Beklenen: 4.0)");
            }
        }

        [Theory]
        [InlineData(false, 4.0)]
        [InlineData(true, 14.5519)]
        public void Verify_Enrage_Formulas_Mathematical_Expectations_At_60_Seconds(bool isMultiplicative, double expected)
        {
            double interval = 5.0;
            double step = 0.25;
            int stepsAt60Sec = (int)(60.0 / interval);

            double multiplier = isMultiplicative
                ? Math.Pow(1.0 + step, stepsAt60Sec)
                : 1.0 + (stepsAt60Sec * step);

            if (isMultiplicative)
            {
                Assert.InRange(multiplier, 14.54, 14.56);
                _output.WriteLine($"[Formül Testi] Çarpımsal 60. sn: {multiplier:F4}x (~14.55)");
            }
            else
            {
                Assert.Equal(expected, multiplier, precision: 4);
                _output.WriteLine($"[Formül Testi] Toplamsal 60. sn: {multiplier:F1}x (4.0)");
            }
        }
    }
}

