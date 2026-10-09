#nullable enable
using System;

namespace BloodSeal.Core
{
    public class TutorialManager
    {
        private static TutorialManager? _instance;
        public static TutorialManager Instance => _instance ??= new TutorialManager();

        public const int RequiredTapCount = 3;

        public TutorialStep CurrentStep { get; private set; } = TutorialStep.TapToAttack;
        public int TapCount { get; private set; } = 0;
        public bool IsCompleted => CurrentStep == TutorialStep.Completed;

        public event Action<TutorialStep>? OnTutorialStepChanged;
        public event Action? OnTutorialCompleted;

        public static void SetInstance(TutorialManager? instance) => _instance = instance;

        public void Reset()
        {
            CurrentStep = TutorialStep.TapToAttack;
            TapCount = 0;
        }

        public void SetStep(TutorialStep step)
        {
            if (CurrentStep == step) return;
            CurrentStep = step;
            OnTutorialStepChanged?.Invoke(CurrentStep);
            if (step == TutorialStep.Completed)
            {
                OnTutorialCompleted?.Invoke();
            }
        }

        public void RegisterTap()
        {
            if (CurrentStep != TutorialStep.TapToAttack) return;
            TapCount++;
            if (TapCount >= RequiredTapCount)
            {
                SetStep(TutorialStep.UpgradeAttack);
            }
        }

        public void RecordEnemyDefeated()
        {
            if (CurrentStep == TutorialStep.TapToAttack)
            {
                SetStep(TutorialStep.UpgradeAttack);
            }
        }

        public void CheckGoldProgress(double currentGold)
        {
            // Optional helper for dynamic gold evaluation
        }

        public void RecordStatUpgraded()
        {
            if (CurrentStep == TutorialStep.UpgradeAttack)
            {
                SetStep(TutorialStep.ActivateBerserk);
            }
        }

        public void CheckRageProgress(bool isRageActive)
        {
            if (CurrentStep == TutorialStep.ActivateBerserk && isRageActive)
            {
                SetStep(TutorialStep.VisitManorGate);
            }
        }

        public void RecordGateVisited()
        {
            if (CurrentStep == TutorialStep.VisitManorGate)
            {
                SetStep(TutorialStep.Completed);
            }
        }
    }
}
