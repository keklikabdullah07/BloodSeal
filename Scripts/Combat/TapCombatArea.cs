using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class TapCombatArea : Control
    {
        private double _tapTokens = 16.0;
        private const double MaxTapTokens = 16.0;
        private const double TapRefillRate = 16.0;
        private ulong _lastTapTicks = 0;

        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                TryTap(mouseEvent.GlobalPosition);
            }
            else if (@event is InputEventScreenTouch touchEvent && touchEvent.Pressed)
            {
                // Multi-touch: Her parmak (touchEvent.Index) koordinatında bağımsız dokunuş işlenir
                TryTap(touchEvent.Position);
            }
        }

        private void TryTap(Vector2 tapPos)
        {
            ulong now = Time.GetTicksMsec();
            if (_lastTapTicks > 0)
            {
                double elapsed = (now - _lastTapTicks) / 1000.0;
                _tapTokens = Mathf.Min(MaxTapTokens, _tapTokens + elapsed * TapRefillRate);
            }
            _lastTapTicks = now;

            if (_tapTokens < 1.0) return; // Anti-macro / autoclicker sınırlayıcı (maks 16 tap/sn)
            _tapTokens -= 1.0;

            OnTap(tapPos);
        }

        private void OnTap(Vector2 tapPos)
        {
            var enemies = GetTree().GetNodesInGroup("Enemies");
            Enemy frontEnemy = null;
            float minX = float.MaxValue;

            foreach (var node in enemies)
            {
                if (node is Enemy e && !e.IsDead)
                {
                    if (e.GlobalPosition.X < minX)
                    {
                        minX = e.GlobalPosition.X;
                        frontEnemy = e;
                    }
                }
            }

            float tapDmg = GameManager.Instance != null ? GameManager.Instance.Stats.Atk * 0.75f : 8f;
            if (GameManager.Instance?.Profile?.Origin == StreetOriginType.UnderAlchemist)
            {
                tapDmg *= 1.25f; // +%25 Tıklama Hasarı
            }
            tapDmg *= ResearchManager.Instance?.GetTapDamageMultiplier() ?? 1.0f;
            tapDmg *= (1.0f + (RelicManager.Instance?.GetTapDamageBonus() ?? 0f));
            bool isCrit = GameManager.Instance != null && GameManager.Instance.IsRageActive;
            if (isCrit) tapDmg *= 2f;

            if (frontEnemy != null)
            {
                frontEnemy.TakeDamage(tapDmg, isCrit);
            }

            if (GameManager.Instance != null)
            {
                float rageMult = AwakeningManager.Instance?.GetRageGainMultiplier() ?? 1.0f;
                float relicRage = RelicManager.Instance?.GetRageGainBonus() ?? 0f;
                GameManager.Instance.AddRage(1.5f * rageMult * (1.0f + relicRage));
            }

            TutorialManager.Instance?.RegisterTap();
            QuestManager.Instance?.RecordTapAttack();

            // Görsel halka (ripple), SFX, haptik titreşim ve hafif sarsıntı
            FXManager.Instance?.PlayTapRipple(tapPos);
            Core.AudioManager.Instance?.PlayTap();
            HapticManager.Instance.VibrateLight();

            if (GameManager.Instance == null || !GameManager.Instance.IsRageActive)
            {
                CameraShake.Instance?.AddTrauma(0.08f);
            }

            // Tıklanan noktada hasar sayısı
            FloatingTextManager.Instance?.SpawnDamage(tapPos, tapDmg, isCrit);
        }
    }
}
