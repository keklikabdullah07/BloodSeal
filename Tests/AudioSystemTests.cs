using System;
using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class AudioSystemTests
    {
        [Theory]
        [InlineData(1.0f, 0.0f)]
        [InlineData(0.5f, -6.02f)]
        [InlineData(0.1f, -20.0f)]
        [InlineData(0.0f, -80.0f)]
        [InlineData(-0.5f, -80.0f)]
        public void AudioData_LinearToDb_CalculatesCorrectly(float linear, float expectedDb)
        {
            float db = AudioData.LinearToDb(linear);
            if (expectedDb == -80f)
            {
                Assert.Equal(-80f, db);
            }
            else
            {
                Assert.InRange(db, expectedDb - 0.2f, expectedDb + 0.2f);
            }
        }

        [Fact]
        public void AudioCueType_Contains_All_Expected_Cues()
        {
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "Slash"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "Hit"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "CritHit"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "Tap"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "RageBurst"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "BossEnrage"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "Coin"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "BossVictory"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "HeroDeath"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "RelicUnlock"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "AwakeningRitual"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "ButtonClick"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "ModalOpen"));
            Assert.True(Enum.IsDefined(typeof(AudioCueType), "ModalClose"));
        }

        [Fact]
        public void SaveData_SerializesAndRestores_AudioSettings()
        {
            var data = new SaveData
            {
                MasterVolume = 0.75f,
                BgmVolume = 0.60f,
                SfxVolume = 0.90f,
                IsMuted = true
            };

            string json = System.Text.Json.JsonSerializer.Serialize(data);
            var restored = System.Text.Json.JsonSerializer.Deserialize<SaveData>(json);

            Assert.NotNull(restored);
            Assert.Equal(0.75f, restored.MasterVolume);
            Assert.Equal(0.60f, restored.BgmVolume);
            Assert.Equal(0.90f, restored.SfxVolume);
            Assert.True(restored.IsMuted);
        }
    }
}
