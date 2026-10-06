using Godot;

namespace BloodSeal.Combat
{
    public partial class BloodProjectile : Node2D
    {
        [Export] public float Speed = 750f;
        public Node2D Target { get; set; }
        public float Damage { get; set; } = 5f;

        public override void _Process(double delta)
        {
            if (!IsInstanceValid(Target))
            {
                QueueFree();
                return;
            }

            Vector2 dir = (Target.GlobalPosition - GlobalPosition).Normalized();
            GlobalPosition += dir * Speed * (float)delta;
            LookAt(Target.GlobalPosition);

            if (GlobalPosition.DistanceTo(Target.GlobalPosition) < 25f)
            {
                if (Target is Enemy enemy)
                {
                    enemy.TakeDamage(Damage, false);
                }
                QueueFree();
            }
        }
    }
}
