using Godot;

namespace BloodSeal.Combat
{
    public partial class CameraShake : Camera2D
    {
        public static CameraShake Instance { get; private set; }

        [Export] public float DecayRate = 1.8f;
        [Export] public float MaxOffset = 22f;

        private float _trauma = 0f;
        private Vector2 _initialPosition;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            _initialPosition = Position;
        }

        public override void _Process(double delta)
        {
            if (_trauma > 0f)
            {
                _trauma = Mathf.Max(0f, _trauma - DecayRate * (float)delta);
                float shakeAmount = _trauma * _trauma;
                float offsetX = (float)GD.RandRange(-1.0, 1.0) * MaxOffset * shakeAmount;
                float offsetY = (float)GD.RandRange(-1.0, 1.0) * MaxOffset * shakeAmount;
                Position = _initialPosition + new Vector2(offsetX, offsetY);
            }
            else
            {
                Position = _initialPosition;
            }
        }

        public void AddTrauma(float amount)
        {
            _trauma = Mathf.Clamp(_trauma + amount, 0f, 1f);
        }
    }
}
