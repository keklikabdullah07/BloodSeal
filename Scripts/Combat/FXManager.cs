using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class FXManager : Node2D
    {
        public static FXManager Instance { get; private set; }

        [Export] public PackedScene SlashScene;
        [Export] public PackedScene BloodSplatterScene;
        [Export] public PackedScene DeathExplosionScene;
        [Export] public PackedScene TapRippleScene;

        private NodePool<SlashEffect> _slashPool;
        private NodePool<OneShotParticle> _bloodPool;
        private NodePool<OneShotParticle> _deathPool;
        private NodePool<TapRipple> _tapPool;

        public override void _EnterTree()
        {
            Instance = this;
        }

        public override void _Ready()
        {
            if (SlashScene != null)
                _slashPool = new NodePool<SlashEffect>(SlashScene, this, 10);
            if (BloodSplatterScene != null)
                _bloodPool = new NodePool<OneShotParticle>(BloodSplatterScene, this, 15);
            if (DeathExplosionScene != null)
                _deathPool = new NodePool<OneShotParticle>(DeathExplosionScene, this, 10);
            if (TapRippleScene != null)
                _tapPool = new NodePool<TapRipple>(TapRippleScene, this, 10);
        }

        public void ReleaseSlash(SlashEffect slash) => _slashPool?.Release(slash);
        public void ReleaseBloodSplatter(OneShotParticle blood) => _bloodPool?.Release(blood);
        public void ReleaseDeathExplosion(OneShotParticle expl) => _deathPool?.Release(expl);
        public void ReleaseTapRipple(TapRipple rip) => _tapPool?.Release(rip);

        public void PlaySlash(Vector2 pos, bool isBerserk)
        {
            if (_slashPool != null)
            {
                var slash = _slashPool.Acquire();
                slash.Play(pos, isBerserk);
            }
            else if (SlashScene != null)
            {
                var slash = SlashScene.Instantiate<SlashEffect>();
                AddChild(slash);
                slash.Play(pos, isBerserk);
            }
        }

        public void PlayBloodSplatter(Vector2 pos, Vector2 direction)
        {
            if (_bloodPool != null)
            {
                var blood = _bloodPool.Acquire();
                blood.OnFinished = ReleaseBloodSplatter;
                blood.Play(pos, 1.0f, direction.Angle());
            }
            else if (BloodSplatterScene != null)
            {
                var blood = BloodSplatterScene.Instantiate<OneShotParticle>();
                AddChild(blood);
                blood.Play(pos, 1.0f, direction.Angle());
            }
        }

        public void PlayDeathExplosion(Vector2 pos, bool isBoss)
        {
            if (_deathPool != null)
            {
                var expl = _deathPool.Acquire();
                expl.OnFinished = ReleaseDeathExplosion;
                expl.Play(pos, isBoss ? 2.5f : 1.0f, 0f);
            }
            else if (DeathExplosionScene != null)
            {
                var expl = DeathExplosionScene.Instantiate<OneShotParticle>();
                AddChild(expl);
                expl.Play(pos, isBoss ? 2.5f : 1.0f, 0f);
            }
        }

        public void PlayTapRipple(Vector2 pos)
        {
            if (_tapPool != null)
            {
                var rip = _tapPool.Acquire();
                rip.Play(pos);
            }
            else if (TapRippleScene != null)
            {
                var rip = TapRippleScene.Instantiate<TapRipple>();
                AddChild(rip);
                rip.Play(pos);
            }
        }

        private double _lastHitFreezeTime = -10.0;
        private const double HitFreezeCooldown = 0.4;

        public async void TriggerLocalHitFreeze(Node2D actorA, Node2D actorB, float duration = 0.040f)
        {
            double now = Time.GetTicksMsec() / 1000.0;
            if (now - _lastHitFreezeTime < HitFreezeCooldown) return;
            _lastHitFreezeTime = now;

            // Local actor pause: do not touch global Engine.TimeScale
            if (GodotObject.IsInstanceValid(actorA)) actorA.ProcessMode = ProcessModeEnum.Disabled;
            if (GodotObject.IsInstanceValid(actorB)) actorB.ProcessMode = ProcessModeEnum.Disabled;

            await ToSignal(GetTree().CreateTimer(duration, true, false, true), "timeout");

            if (GodotObject.IsInstanceValid(actorA)) actorA.ProcessMode = ProcessModeEnum.Inherit;
            if (GodotObject.IsInstanceValid(actorB)) actorB.ProcessMode = ProcessModeEnum.Inherit;
        }
    }
}
