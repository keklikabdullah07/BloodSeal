using Godot;
using System;
using BloodSeal.Core;
using BloodSeal.Combat;

namespace BloodSeal.UI
{
    public partial class OfflineProgressModal : Control
    {
        [Export] public Label TimeLabel;
        [Export] public Label WaveLabel;
        [Export] public Label GoldLabel;
        [Export] public Button ClaimButton;

        private double _claimedGold = 0;

        public override void _Ready()
        {
            Visible = false;
            ClaimButton?.Connect("pressed", Callable.From(OnClaimPressed));

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnOfflineEarningsReady += ShowModal;
                if (GameManager.Instance.PendingOfflineEarnings != null)
                {
                    ShowModal(GameManager.Instance.PendingOfflineEarnings);
                }
            }
        }

        public override void _ExitTree()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnOfflineEarningsReady -= ShowModal;
            }
        }

        public void ShowModal(OfflineEarningsResult result)
        {
            if (result == null || !result.HasClaimableEarnings) return;

            _claimedGold = result.GoldEarned;
            long hours = result.ElapsedSeconds / 3600;
            long minutes = (result.ElapsedSeconds % 3600) / 60;
            string timeStr = hours > 0
                ? $"{hours} saat {minutes} dakika"
                : $"{minutes} dakika";

            if (TimeLabel != null)
                TimeLabel.Text = $"Karanlıkta Geçen Süre: {timeStr}";

            if (WaveLabel != null)
                WaveLabel.Text = $"Güvenli Farm Alanı: Dalga {result.FarmWave}";

            if (GoldLabel != null)
                GoldLabel.Text = $"+{BigNumberFormatter.Format(result.GoldEarned)} Kan Altını";

            Visible = true;
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.35f);
        }

        private void OnClaimPressed()
        {
            if (_claimedGold > 0)
            {
                FloatingTextManager.Instance?.SpawnGold(new Vector2(960, 480), _claimedGold);
            }

            GameManager.Instance?.ClaimOfflineEarnings();
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.2f);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }
    }
}
