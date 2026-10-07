using Godot;
using BloodSeal.Core;

namespace BloodSeal.Combat
{
    public partial class TapCombatArea : Control
    {
        public override void _GuiInput(InputEvent @event)
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                OnTap(mouseEvent.GlobalPosition);
            }
            else if (@event is InputEventScreenTouch touchEvent && touchEvent.Pressed)
            {
                OnTap(touchEvent.Position);
            }
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
            bool isCrit = GameManager.Instance != null && GameManager.Instance.IsRageActive;
            if (isCrit) tapDmg *= 2f;

            if (frontEnemy != null)
            {
                frontEnemy.TakeDamage(tapDmg, isCrit);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddRage(1.5f);
            }

            // Visual tap ripple, SFX & light screen shake (outside Berserk)
            FXManager.Instance?.PlayTapRipple(tapPos);
            Core.AudioManager.Instance?.PlayTap();
            if (GameManager.Instance == null || !GameManager.Instance.IsRageActive)
            {
                CameraShake.Instance?.AddTrauma(0.08f);
            }

            // Spawn visual tap damage popup at clicked point
            FloatingTextManager.Instance?.SpawnDamage(tapPos, tapDmg, isCrit);
        }
    }
}
