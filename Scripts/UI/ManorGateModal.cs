using Godot;
using System;
using BloodSeal.Core;
using BloodSeal.Combat;

namespace BloodSeal.UI
{
    public partial class ManorGateModal : Control
    {
        [Export] public Button FrontGateBtn;
        [Export] public Button SewersBtn;
        [Export] public Button BloodSealBtn;
        [Export] public Button RoofBtn;
        [Export] public Button CloseBtn;
        [Export] public Label TacticalInfoLabel;

        public event Action<GateApproachType> OnApproachSelected;

        public override void _Ready()
        {
            Visible = false;

            FrontGateBtn?.Connect("pressed", Callable.From(() => SelectApproach(GateApproachType.FrontGate)));
            SewersBtn?.Connect("pressed", Callable.From(() => SelectApproach(GateApproachType.Sewers)));
            BloodSealBtn?.Connect("pressed", Callable.From(() => SelectApproach(GateApproachType.BloodSeal)));
            RoofBtn?.Connect("pressed", Callable.From(() => SelectApproach(GateApproachType.RoofInfiltration)));
            CloseBtn?.Connect("pressed", Callable.From(CloseModal));
        }

        public void ShowModal()
        {
            var origin = GameManager.Instance?.Profile?.Origin ?? StreetOriginType.PitFighter;
            if (TacticalInfoLabel != null)
            {
                TacticalInfoLabel.Text = $"💡 {ManorGateHelper.GetApproachTacticalAdvantage(GateApproachType.RoofInfiltration, origin)}";
            }

            Visible = true;
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.25f);
        }

        public void CloseModal()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.2f);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }

        private void SelectApproach(GateApproachType approach)
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.ClaimGateReward(approach);
            var rune = ManorGateHelper.GetRuneForApproach(approach);
            string runeName = ManorGateHelper.GetRuneName(rune);

            FloatingTextManager.Instance?.SpawnMessage(
                new Vector2(960, 420),
                $"✨ {runeName} KUŞANILDI! ✨",
                new Color(1f, 0.85f, 0.2f)
            );

            if (approach == GateApproachType.BloodSeal)
            {
                FloatingTextManager.Instance?.SpawnMessage(
                    new Vector2(960, 480),
                    "📜 Kadim Parşömen #1 Ele Geçirildi!",
                    new Color(0.9f, 0.4f, 1f)
                );
            }

            OnApproachSelected?.Invoke(approach);
            CloseModal();
        }
    }
}
