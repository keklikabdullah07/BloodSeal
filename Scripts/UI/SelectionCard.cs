using Godot;
using System;

namespace BloodSeal.UI
{
    public partial class SelectionCard : PanelContainer
    {
        [Export] public Label TitleLabel;
        [Export] public Label DescLabel;
        [Export] public ColorRect SelectionBorder;
        [Export] public Button ClickButton;

        public event Action<SelectionCard> OnCardSelected;
        public int CardIndex { get; set; }

        public override void _Ready()
        {
            if (ClickButton != null)
            {
                ClickButton.Connect("pressed", Callable.From(OnPressed));
            }
        }

        public void Setup(int index, string title, string desc, bool isSelected = false)
        {
            CardIndex = index;
            if (TitleLabel != null) TitleLabel.Text = title;
            if (DescLabel != null) DescLabel.Text = desc;
            SetSelected(isSelected);
        }

        public void SetSelected(bool isSelected)
        {
            if (SelectionBorder != null)
            {
                SelectionBorder.Visible = isSelected;
            }
            Modulate = isSelected ? new Color(1.2f, 1.05f, 1.05f) : Colors.White;
        }

        private void OnPressed()
        {
            OnCardSelected?.Invoke(this);
        }
    }
}
