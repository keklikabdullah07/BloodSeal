namespace BloodSeal.Core
{
    public class SaveData
    {
        public long Gold { get; set; } = 0;
        public int CurrentWave { get; set; } = 1;
        public int HighestWave { get; set; } = 1;
        public int AtkLevel { get; set; } = 1;
        public int AtkSpeedLevel { get; set; } = 1;
        public int LifestealLevel { get; set; } = 1;
        public int MaxHpLevel { get; set; } = 1;
        public int RangeLevel { get; set; } = 1;
    }
}
