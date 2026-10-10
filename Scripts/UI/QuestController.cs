#nullable enable
using Godot;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class QuestController : CanvasLayer
    {
        private Button? _questBtn;
        private QuestModal? _questModal;

        public override void _Ready()
        {
            Layer = 104;

            _questModal = new QuestModal();
            AddChild(_questModal);

            CreateHUDButton();

            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestProgressUpdated += OnProgressUpdated;
                QuestManager.Instance.OnQuestCompleted += OnQuestCompleted;
                QuestManager.Instance.OnQuestRewardClaimed += OnRewardClaimed;
                QuestManager.Instance.OnDailyQuestsRefreshed += RefreshNotificationUI;
            }

            Callable.From(RefreshNotificationUI).CallDeferred();
        }

        public override void _ExitTree()
        {
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestProgressUpdated -= OnProgressUpdated;
                QuestManager.Instance.OnQuestCompleted -= OnQuestCompleted;
                QuestManager.Instance.OnQuestRewardClaimed -= OnRewardClaimed;
                QuestManager.Instance.OnDailyQuestsRefreshed -= RefreshNotificationUI;
            }
        }

        private void CreateHUDButton()
        {
            _questBtn = new Button
            {
                Text = "📜 GÖREVLER",
                CustomMinimumSize = new Vector2(120, 44),
                Position = new Vector2(1710, 16)
            };

            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.20f, 0.05f, 0.08f, 0.95f),
                BorderColor = new Color(0.75f, 0.2f, 0.25f, 1f),
                BorderWidthBottom = 2, BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2,
                CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8, CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8
            };
            _questBtn.AddThemeStyleboxOverride("normal", style);
            _questBtn.AddThemeFontSizeOverride("font_size", 14);
            _questBtn.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));

            _questBtn.Connect("pressed", Callable.From(() =>
            {
                AudioManager.Instance?.PlayButtonClick();
                _questModal?.ShowModal();
            }));

            AddChild(_questBtn);
        }

        private void OnProgressUpdated(QuestProgress progress) => RefreshNotificationUI();
        private void OnQuestCompleted(QuestDefinition def) => RefreshNotificationUI();
        private void OnRewardClaimed(QuestDefinition def, QuestReward reward) => RefreshNotificationUI();

        private void RefreshNotificationUI()
        {
            if (_questBtn == null) return;
            int unclaimed = QuestManager.Instance?.GetUnclaimedRewardCount() ?? 0;
            if (unclaimed > 0)
            {
                _questBtn.Text = $"📜 GÖREVLER ({unclaimed}) 🔥";
                _questBtn.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.25f));
            }
            else
            {
                _questBtn.Text = "📜 GÖREVLER";
                _questBtn.AddThemeColorOverride("font_color", new Color(0.9f, 0.85f, 0.75f));
            }
        }
    }
}
