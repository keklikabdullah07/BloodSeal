using Godot;
using BloodSeal.Core;
using System;

namespace BloodSeal.Combat
{
    public partial class OneShotParticle : CpuParticles2D, IPoolable
    {
        public Action<OneShotParticle> OnFinished;

        public void Play(Vector2 pos, float scale = 1.0f, float rotation = 0f)
        {
            GlobalPosition = pos;
            Scale = Vector2.One * scale;
            Rotation = rotation;
            Restart();
            Emitting = true;
            GetTree().CreateTimer(Lifetime + 0.05).Connect("timeout", Callable.From(ReturnToPool));
        }

        private void ReturnToPool()
        {
            Emitting = false;
            if (OnFinished != null)
            {
                OnFinished(this);
            }
            else
            {
                QueueFree();
            }
        }

        public void OnSpawnFromPool()
        {
            Visible = true;
        }

        public void OnReturnToPool()
        {
            Emitting = false;
        }
    }
}
