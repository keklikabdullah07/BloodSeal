using System;
using System.Text.Json;
using BloodSeal.Core;
using Xunit;

namespace BloodSeal.Tests
{
    public class SaveSystemTests
    {
        [Fact]
        public void SaveData_Serialization_Preserves_Double_Precision_And_Fields()
        {
            var data = new SaveData
            {
                Version = 1,
                Gold = 12345678.95,
                CurrentWave = 42,
                HighestWave = 45,
                IsInSafeFarmMode = true,
                PlayerName = "Kaelen",
                Bloodline = BloodlineType.ShadowVeined,
                Origin = StreetOriginType.UnderAlchemist,
                HasCompletedPrologue = true,
                AtkLevel = 15,
                AtkSpeedLevel = 8,
                LifestealLevel = 5,
                MaxHpLevel = 20,
                RangeLevel = 3,
                LastSaveTimestamp = 1700000000
            };

            string json = JsonSerializer.Serialize(data);
            var deserialized = JsonSerializer.Deserialize<SaveData>(json);

            Assert.NotNull(deserialized);
            Assert.Equal(1, deserialized.Version);
            Assert.Equal(12345678.95, deserialized.Gold, precision: 2);
            Assert.Equal(42, deserialized.CurrentWave);
            Assert.Equal(45, deserialized.HighestWave);
            Assert.True(deserialized.IsInSafeFarmMode);
            Assert.Equal("Kaelen", deserialized.PlayerName);
            Assert.Equal(BloodlineType.ShadowVeined, deserialized.Bloodline);
            Assert.Equal(StreetOriginType.UnderAlchemist, deserialized.Origin);
            Assert.True(deserialized.HasCompletedPrologue);
            Assert.Equal(15, deserialized.AtkLevel);
            Assert.Equal(1700000000, deserialized.LastSaveTimestamp);
        }

        [Fact]
        public void OfflineProgress_Under_60_Seconds_Returns_No_Claimable_Earnings()
        {
            long lastSave = 1000000;
            long now = 1000045; // 45 saniye sonra

            var result = OfflineProgressCalculator.Calculate(highestWave: 10, lastSaveTimestamp: lastSave, currentTimestamp: now);

            Assert.Equal(45, result.ElapsedSeconds);
            Assert.False(result.HasClaimableEarnings);
            Assert.Equal(0.0, result.GoldEarned);
        }

        [Fact]
        public void OfflineProgress_Clamps_At_Max_6_Hours()
        {
            long lastSave = 1000000;
            long now = 1000000 + 40000; // 40.000 saniye (11 saatten fazla)

            var result = OfflineProgressCalculator.Calculate(highestWave: 10, lastSaveTimestamp: lastSave, currentTimestamp: now);

            // 6 saat = 21.600 saniye ile sınırlandırılmalı
            Assert.Equal(21600, result.ElapsedSeconds);
            Assert.True(result.HasClaimableEarnings);
            Assert.True(result.GoldEarned > 0);
        }

        [Fact]
        public void OfflineProgress_Protects_Against_Clock_Manipulation_Negative_Time()
        {
            long lastSave = 2000000;
            long now = 1500000; // Cihaz saati geriye alınmış

            var result = OfflineProgressCalculator.Calculate(highestWave: 10, lastSaveTimestamp: lastSave, currentTimestamp: now);

            // Math.Clamp(negative, 0, 21600) -> 0 saniye
            Assert.Equal(0, result.ElapsedSeconds);
            Assert.False(result.HasClaimableEarnings);
            Assert.Equal(0.0, result.GoldEarned);
        }

        [Fact]
        public void OfflineProgress_Calculates_Correct_Gold_For_Wave_10()
        {
            // HighestWave = 10 -> En son güvenli dalga = 9
            // Dalga 9 altın geliri: 5 * (10 + 9 * 5) = 5 * 55 = 275 altın/dalga
            // Saniye başı: 275 / 15.0 = 18.333 altın/sn
            // 2 saat (7200 saniye) için: 7200 * 18.333... = 132000 altın
            long lastSave = 1000000;
            long now = lastSave + 7200; // 2 saat

            var result = OfflineProgressCalculator.Calculate(highestWave: 10, lastSaveTimestamp: lastSave, currentTimestamp: now);

            Assert.Equal(7200, result.ElapsedSeconds);
            Assert.Equal(9, result.FarmWave);
            Assert.InRange(result.GoldEarned, 131900, 132100);
            Assert.True(result.HasClaimableEarnings);
        }

        [Fact]
        public void SaveData_SerializesAndDeserializes_ResearchFields()
        {
            var data = new SaveData
            {
                LoreScrolls = 7,
                ResearchLevels = new System.Collections.Generic.Dictionary<string, int>
                {
                    ["Econ_GoldBounty"] = 4,
                    ["Mem_OfflineCap"] = 2
                },
                DefeatedMilestoneBosses = new System.Collections.Generic.List<int> { 10, 20 }
            };

            string json = JsonSerializer.Serialize(data);
            var deserialized = JsonSerializer.Deserialize<SaveData>(json);

            Assert.NotNull(deserialized);
            Assert.Equal(7, deserialized.LoreScrolls);
            Assert.Equal(4, deserialized.ResearchLevels["Econ_GoldBounty"]);
            Assert.Equal(2, deserialized.ResearchLevels["Mem_OfflineCap"]);
            Assert.Contains(10, deserialized.DefeatedMilestoneBosses);
            Assert.Contains(20, deserialized.DefeatedMilestoneBosses);
        }

        [Fact]
        public void SaveData_SerializesAndDeserializes_AwakeningFields()
        {
            var data = new SaveData
            {
                AwakeningPoints = 15,
                TotalAwakenings = 3,
                AwakeningLevels = new System.Collections.Generic.Dictionary<string, int>
                {
                    ["War_PrimordialMight"] = 4,
                    ["Flow_WaveLeap"] = 2
                }
            };

            string json = JsonSerializer.Serialize(data);
            var deserialized = JsonSerializer.Deserialize<SaveData>(json);

            Assert.NotNull(deserialized);
            Assert.Equal(15, deserialized.AwakeningPoints);
            Assert.Equal(3, deserialized.TotalAwakenings);
            Assert.Equal(4, deserialized.AwakeningLevels["War_PrimordialMight"]);
            Assert.Equal(2, deserialized.AwakeningLevels["Flow_WaveLeap"]);
        }
    }
}
