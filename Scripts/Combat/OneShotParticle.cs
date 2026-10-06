using Godot;

namespace BloodSeal.Combat
{
    public partial class OneShotParticle : CpuParticles2D
    {
        public override void _Ready()
        {
            Emitting = true;
            GetTree().CreateTimer(Lifetime + 0.1).Connect("timeout", Callable.From(QueueFree));
        }
    }
}
