using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class TutorialSystemTests
    {
        public TutorialSystemTests()
        {
            TutorialManager.Instance.Reset();
        }

        [Fact]
        public void InitialState_DefaultsToTapToAttack()
        {
            var mgr = TutorialManager.Instance;
            Assert.Equal(TutorialStep.TapToAttack, mgr.CurrentStep);
            Assert.False(mgr.IsCompleted);
        }

        [Fact]
        public void RegisterTap_AdvancesToUpgradeAttack_AfterThreeTaps()
        {
            var mgr = TutorialManager.Instance;
            mgr.RegisterTap();
            mgr.RegisterTap();
            Assert.Equal(TutorialStep.TapToAttack, mgr.CurrentStep);

            mgr.RegisterTap();
            Assert.Equal(TutorialStep.UpgradeAttack, mgr.CurrentStep);
        }

        [Fact]
        public void RecordEnemyDefeated_ImmediatelyAdvancesStep1()
        {
            var mgr = TutorialManager.Instance;
            mgr.RecordEnemyDefeated();
            Assert.Equal(TutorialStep.UpgradeAttack, mgr.CurrentStep);
        }

        [Fact]
        public void RecordStatUpgraded_AdvancesToActivateBerserk()
        {
            var mgr = TutorialManager.Instance;
            mgr.SetStep(TutorialStep.UpgradeAttack);

            mgr.RecordStatUpgraded();
            Assert.Equal(TutorialStep.ActivateBerserk, mgr.CurrentStep);
        }

        [Fact]
        public void CheckRageProgress_AdvancesToVisitManorGate_WhenActive()
        {
            var mgr = TutorialManager.Instance;
            mgr.SetStep(TutorialStep.ActivateBerserk);

            mgr.CheckRageProgress(false);
            Assert.Equal(TutorialStep.ActivateBerserk, mgr.CurrentStep);

            mgr.CheckRageProgress(true);
            Assert.Equal(TutorialStep.VisitManorGate, mgr.CurrentStep);
        }

        [Fact]
        public void RecordGateVisited_CompletesTutorial()
        {
            var mgr = TutorialManager.Instance;
            mgr.SetStep(TutorialStep.VisitManorGate);

            bool completedFired = false;
            mgr.OnTutorialCompleted += () => completedFired = true;

            mgr.RecordGateVisited();

            Assert.Equal(TutorialStep.Completed, mgr.CurrentStep);
            Assert.True(mgr.IsCompleted);
            Assert.True(completedFired);
        }
    }
}
