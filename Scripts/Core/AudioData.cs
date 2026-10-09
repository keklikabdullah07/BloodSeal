#nullable enable
using System;

namespace BloodSeal.Core
{
    public enum BgmTrackType
    {
        GothicAmbient = 0,
        BossCombat = 1
    }

    public enum AudioCueType
    {
        Slash = 0,
        Hit = 1,
        CritHit = 2,
        Tap = 3,
        RageBurst = 4,
        BossEnrage = 5,
        Coin = 6,
        BossVictory = 7,
        HeroDeath = 8,
        RelicUnlock = 9,
        AwakeningRitual = 10,
        ButtonClick = 11,
        ModalOpen = 12,
        ModalClose = 13
    }

    public static class AudioData
    {
        public const float MinDb = -80.0f;
        public const float MaxDb = 0.0f;

        public static float LinearToDb(float linear)
        {
            if (linear <= 0.0001f) return MinDb;
            float db = (float)(20.0 * Math.Log10(Math.Clamp(linear, 0.0001f, 1.0f)));
            return Math.Clamp(db, MinDb, MaxDb);
        }

        public static float DbToLinear(float db)
        {
            if (db <= MinDb) return 0.0f;
            return (float)Math.Pow(10.0, Math.Clamp(db, MinDb, MaxDb) / 20.0);
        }
    }
}
