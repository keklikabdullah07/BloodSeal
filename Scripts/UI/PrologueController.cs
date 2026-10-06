using Godot;
using System;
using BloodSeal.Core;

namespace BloodSeal.UI
{
    public partial class PrologueController : Control
    {
        [Export] public LineEdit NameInput;
        [Export] public Control Step1Panel;
        [Export] public Control Step2Panel;
        [Export] public Control Step3Panel;
        [Export] public Control Step4Panel;

        [Export] public HBoxContainer BloodlineCardsContainer;
        [Export] public HBoxContainer OriginCardsContainer;
        [Export] public PackedScene CardScene;

        [Export] public Label SummaryLabel;
        [Export] public Button AwakenButton;

        private string _playerName = "Valerius";
        private BloodlineType _selectedBloodline = BloodlineType.BoneWeaver;
        private StreetOriginType _selectedOrigin = StreetOriginType.PitFighter;

        private SelectionCard[] _bloodlineCards = new SelectionCard[5];
        private SelectionCard[] _originCards = new SelectionCard[5];

        public override void _Ready()
        {
            // Auto bypass if already completed
            if (GameManager.Instance != null && GameManager.Instance.Profile.HasCompletedPrologue)
            {
                GetTree().ChangeSceneToFile("res://Scenes/MainCombat.tscn");
                return;
            }

            BuildBloodlineCards();
            BuildOriginCards();
            ShowStep(1);

            if (AwakenButton != null)
            {
                AwakenButton.Connect("pressed", Callable.From(OnAwakenPressed));
            }
        }

        private void BuildBloodlineCards()
        {
            if (BloodlineCardsContainer == null || CardScene == null) return;

            for (int i = 0; i < 5; i++)
            {
                var type = (BloodlineType)i;
                var card = CardScene.Instantiate<SelectionCard>();
                BloodlineCardsContainer.AddChild(card);
                card.Setup(i, CharacterProfile.GetBloodlineName(type), CharacterProfile.GetBloodlineDesc(type), i == 0);
                card.OnCardSelected += OnBloodlineSelected;
                _bloodlineCards[i] = card;
            }
        }

        private void BuildOriginCards()
        {
            if (OriginCardsContainer == null || CardScene == null) return;

            for (int i = 0; i < 5; i++)
            {
                var type = (StreetOriginType)i;
                var card = CardScene.Instantiate<SelectionCard>();
                OriginCardsContainer.AddChild(card);
                card.Setup(i, CharacterProfile.GetOriginName(type), CharacterProfile.GetOriginDesc(type), i == 0);
                card.OnCardSelected += OnOriginSelected;
                _originCards[i] = card;
            }
        }

        private void OnBloodlineSelected(SelectionCard selectedCard)
        {
            _selectedBloodline = (BloodlineType)selectedCard.CardIndex;
            for (int i = 0; i < _bloodlineCards.Length; i++)
            {
                if (_bloodlineCards[i] != null)
                {
                    _bloodlineCards[i].SetSelected(i == selectedCard.CardIndex);
                }
            }
        }

        private void OnOriginSelected(SelectionCard selectedCard)
        {
            _selectedOrigin = (StreetOriginType)selectedCard.CardIndex;
            for (int i = 0; i < _originCards.Length; i++)
            {
                if (_originCards[i] != null)
                {
                    _originCards[i].SetSelected(i == selectedCard.CardIndex);
                }
            }
        }

        public void ShowStep(int step)
        {
            if (Step1Panel != null) Step1Panel.Visible = (step == 1);
            if (Step2Panel != null) Step2Panel.Visible = (step == 2);
            if (Step3Panel != null) Step3Panel.Visible = (step == 3);
            if (Step4Panel != null) Step4Panel.Visible = (step == 4);

            if (step == 4 && SummaryLabel != null)
            {
                if (NameInput != null && !string.IsNullOrWhiteSpace(NameInput.Text))
                {
                    _playerName = NameInput.Text.Trim();
                }

                string blName = CharacterProfile.GetBloodlineName(_selectedBloodline);
                string blDesc = CharacterProfile.GetBloodlineDesc(_selectedBloodline);
                string orName = CharacterProfile.GetOriginName(_selectedOrigin);
                string orDesc = CharacterProfile.GetOriginDesc(_selectedOrigin);

                SummaryLabel.Text = $"Savaşçı: {_playerName}\n\n" +
                                     $"🩸 Kan Soyu: {blName} ({blDesc})\n" +
                                     $"⚔️ Geçmiş: {orName} ({orDesc})\n\n" +
                                     $"Köken Mührü bedeninle bütünleşmeye hazır.";
            }
        }

        public void OnStep1NextPressed()
        {
            if (NameInput != null && !string.IsNullOrWhiteSpace(NameInput.Text))
            {
                _playerName = NameInput.Text.Trim();
            }
            ShowStep(2);
        }

        public void OnStep2NextPressed() => ShowStep(3);
        public void OnStep2BackPressed() => ShowStep(1);
        public void OnStep3NextPressed() => ShowStep(4);
        public void OnStep3BackPressed() => ShowStep(2);
        public void OnStep4BackPressed() => ShowStep(3);

        private void OnAwakenPressed()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetProfile(_playerName, _selectedBloodline, _selectedOrigin);
            }
            GetTree().ChangeSceneToFile("res://Scenes/MainCombat.tscn");
        }
    }
}
