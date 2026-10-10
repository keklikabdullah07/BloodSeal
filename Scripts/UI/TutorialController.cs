using Godot;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class TutorialController : CanvasLayer
    {
        private TutorialHintCallout _callout;

        public override void _Ready()
        {
            Layer = 105;
            _callout = new TutorialHintCallout();
            AddChild(_callout);

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnTutorialStepChanged += OnStepChanged;
                TutorialManager.Instance.OnTutorialCompleted += OnTutorialCompleted;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged += OnGoldChanged;
                GameManager.Instance.OnRageChanged += OnRageChanged;
                GameManager.Instance.OnRageStateChanged += OnRageStateChanged;
                GameManager.Instance.OnStatsUpgraded += OnStatsUpgraded;
                GameManager.Instance.OnGateNotificationAvailable += OnGateAvailable;
            }

            Callable.From(() => RefreshCurrentStepUI()).CallDeferred();
        }

        public override void _ExitTree()
        {
            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnTutorialStepChanged -= OnStepChanged;
                TutorialManager.Instance.OnTutorialCompleted -= OnTutorialCompleted;
            }
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged -= OnGoldChanged;
                GameManager.Instance.OnRageChanged -= OnRageChanged;
                GameManager.Instance.OnRageStateChanged -= OnRageStateChanged;
                GameManager.Instance.OnStatsUpgraded -= OnStatsUpgraded;
                GameManager.Instance.OnGateNotificationAvailable -= OnGateAvailable;
            }
        }

        private void OnStepChanged(TutorialStep step) => RefreshCurrentStepUI();
        private void OnTutorialCompleted() => _callout?.HideHint();

        private void OnGoldChanged(double gold)
        {
            if (TutorialManager.Instance?.CurrentStep == TutorialStep.UpgradeAttack)
            {
                RefreshCurrentStepUI();
            }
        }

        private void OnRageChanged(float rage)
        {
            if (TutorialManager.Instance?.CurrentStep == TutorialStep.ActivateBerserk)
            {
                RefreshCurrentStepUI();
            }
        }

        private void OnRageStateChanged(bool isActive)
        {
            if (TutorialManager.Instance?.CurrentStep == TutorialStep.ActivateBerserk && isActive)
            {
                TutorialManager.Instance.CheckRageProgress(true);
            }
        }

        private void OnStatsUpgraded()
        {
            if (TutorialManager.Instance?.CurrentStep == TutorialStep.UpgradeAttack)
            {
                TutorialManager.Instance.RecordStatUpgraded();
            }
        }

        private void OnGateAvailable()
        {
            if (TutorialManager.Instance?.CurrentStep == TutorialStep.VisitManorGate)
            {
                RefreshCurrentStepUI();
            }
        }

        private void RefreshCurrentStepUI()
        {
            if (TutorialManager.Instance == null || _callout == null) return;
            var step = TutorialManager.Instance.CurrentStep;

            switch (step)
            {
                case TutorialStep.TapToAttack:
                    _callout.ShowHint("DÖVÜŞ", "Düşmanlara vurmak ve öfke toplamak için savaş alanına dokun!", new Vector2(960, 520), pointUp: false);
                    break;

                case TutorialStep.UpgradeAttack:
                    var atkBtn = GetTree().Root.FindChild("UpgradeAtkBtn", true, false) as Button;
                    Vector2 atkPos = atkBtn != null && atkBtn.IsVisibleInTree()
                        ? atkBtn.GlobalPosition + atkBtn.Size * 0.5f
                        : new Vector2(360, 950);
                    _callout.ShowHint("GELİŞİM", "Saldırı gücünü artırmak için Saldırı (ATK) yükselt!", atkPos, pointUp: false);
                    break;

                case TutorialStep.ActivateBerserk:
                    if (GameManager.Instance != null && GameManager.Instance.RagePercentage >= 100f)
                    {
                        var rageBtn = GetTree().Root.FindChild("RageButton", true, false) as Button;
                        Vector2 ragePos = rageBtn != null && rageBtn.IsVisibleInTree()
                            ? rageBtn.GlobalPosition + rageBtn.Size * 0.5f
                            : new Vector2(960, 850);
                        _callout.ShowHint("GÜÇ PATLAMASI", "Öfken taştı! Berserk modunu başlat!", ragePos, pointUp: false);
                    }
                    else
                    {
                        _callout.HideHint();
                    }
                    break;

                case TutorialStep.VisitManorGate:
                    var gateBtn = GetTree().Root.FindChild("GateNotificationBtn", true, false) as Button;
                    Vector2 gatePos = gateBtn != null && gateBtn.IsVisibleInTree()
                        ? gateBtn.GlobalPosition + gateBtn.Size * 0.5f
                        : new Vector2(960, 180);
                    _callout.ShowHint("KEŞİF", "Malikane Kapısı belirdi! Mührünü seç ve ödülünü al!", gatePos, pointUp: true);
                    break;

                case TutorialStep.Completed:
                default:
                    _callout.HideHint();
                    break;
            }
        }
    }
}
