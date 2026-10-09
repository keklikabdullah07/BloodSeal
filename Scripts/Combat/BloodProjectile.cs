using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class BloodProjectile : Node2D, IPoolable
    {
        [Export] public float Speed = 750f;
        public Node2D Target { get; set; }
        public float Damage { get; set; } = 5f;

        public override void _Process(double delta)
        {
            if (!IsInstanceValid(Target))
            {
                Despawn();
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
                Despawn();
            }
        }

        private void Despawn()
        {
            if (PetCompanion.Instance != null)
            {
                PetCompanion.Instance.ReleaseProjectile(this);
            }
            else
            {
                QueueFree();
            }
        }

        public void OnSpawnFromPool()
        {
            Visible = true;
            SetProcess(true);
        }

        public void OnReturnToPool()
        {
            Target = null;
            Damage = 5f;
            GlobalPosition = Vector2.Zero;
            Rotation = 0f;
            SetProcess(false);
        }
    }
}
