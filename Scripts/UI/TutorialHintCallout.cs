using Godot;

namespace BloodSeal.UI
{
    public partial class TutorialHintCallout : Control
    {
        private PanelContainer _container;
        private Label _titleLabel;
        private Label _messageLabel;
        private Label _arrowLabel;
        private Tween _pulseTween;
        private Tween _fadeTween;

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            BuildUI();
            Modulate = new Color(1, 1, 1, 0);
            Visible = false;
        }

        private void BuildUI()
        {
            _container = new PanelContainer
            {
                MouseFilter = MouseFilterEnum.Ignore,
                CustomMinimumSize = new Vector2(280, 80)
            };

            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.07f, 0.04f, 0.06f, 0.94f),
                BorderColor = new Color(0.85f, 0.18f, 0.22f, 0.95f),
                CornerRadiusBottomLeft = 6,
                CornerRadiusBottomRight = 6,
                CornerRadiusTopLeft = 6,
                CornerRadiusTopRight = 6,
                BorderWidthBottom = 2,
                BorderWidthTop = 2,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                ContentMarginBottom = 10,
                ContentMarginTop = 10,
                ContentMarginLeft = 14,
                ContentMarginRight = 14
            };
            _container.AddThemeStyleboxOverride("panel", style);

            var vbox = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
            _container.AddChild(vbox);

            _titleLabel = new Label
            {
                MouseFilter = MouseFilterEnum.Ignore,
                Text = "ÖĞRETİCİ"
            };
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.45f));
            _titleLabel.AddThemeFontSizeOverride("font_size", 14);
            vbox.AddChild(_titleLabel);

            _messageLabel = new Label
            {
                MouseFilter = MouseFilterEnum.Ignore,
                Text = "",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            _messageLabel.AddThemeColorOverride("font_color", new Color(0.92f, 0.92f, 0.92f));
            _messageLabel.AddThemeFontSizeOverride("font_size", 13);
            vbox.AddChild(_messageLabel);

            _arrowLabel = new Label
            {
                MouseFilter = MouseFilterEnum.Ignore,
                Text = "▼",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _arrowLabel.AddThemeColorOverride("font_color", new Color(1.0f, 0.25f, 0.25f));
            _arrowLabel.AddThemeFontSizeOverride("font_size", 22);

            AddChild(_container);
            AddChild(_arrowLabel);
        }

        public void ShowHint(string title, string message, Vector2 targetScreenPos, bool pointUp = false)
        {
            _titleLabel.Text = title;
            _messageLabel.Text = message;
            _arrowLabel.Text = pointUp ? "▲" : "▼";

            _container.ResetSize();
            Vector2 boxSize = _container.GetCombinedMinimumSize();
            _container.Size = boxSize;
            if (pointUp)
            {
                _container.Position = new Vector2(
                    Mathf.Clamp(targetScreenPos.X - boxSize.X * 0.5f, 30f, 1890f - boxSize.X),
                    targetScreenPos.Y + 28f);
                _arrowLabel.Position = new Vector2(targetScreenPos.X - 10f, targetScreenPos.Y + 4f);
            }
            else
            {
                _container.Position = new Vector2(
                    Mathf.Clamp(targetScreenPos.X - boxSize.X * 0.5f, 30f, 1890f - boxSize.X),
                    targetScreenPos.Y - boxSize.Y - 28f);
                _arrowLabel.Position = new Vector2(targetScreenPos.X - 10f, targetScreenPos.Y - 26f);
            }

            Visible = true;
            _fadeTween?.Kill();
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(this, "modulate:a", 1.0f, 0.25f);

            StartPulse();
        }

        private void StartPulse()
        {
            _pulseTween?.Kill();
            _pulseTween = CreateTween().SetLoops();
            _pulseTween.TweenProperty(_arrowLabel, "scale", new Vector2(1.25f, 1.25f), 0.45f).SetTrans(Tween.TransitionType.Sine);
            _pulseTween.TweenProperty(_arrowLabel, "scale", Vector2.One, 0.45f).SetTrans(Tween.TransitionType.Sine);
        }

        public void HideHint()
        {
            _pulseTween?.Kill();
            _fadeTween?.Kill();
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(this, "modulate:a", 0.0f, 0.2f);
            _fadeTween.Finished += () => Visible = false;
        }
    }
}
