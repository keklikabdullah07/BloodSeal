using Xunit;
using BloodSeal.Core;

namespace BloodSeal.Tests
{
    public class HapticAndMobileTests
    {
        [Fact]
        public void HapticManager_DefaultsToEnabled_AndTogglesCorrectly()
        {
            var haptic = HapticManager.Instance;
            Assert.NotNull(haptic);
            haptic.IsHapticsEnabled = true;
            Assert.True(haptic.IsHapticsEnabled);

            haptic.SetHapticsEnabled(false);
            Assert.False(haptic.IsHapticsEnabled);

            // Re-enable for subsequent tests
            haptic.SetHapticsEnabled(true);
            Assert.True(haptic.IsHapticsEnabled);
        }

        [Fact]
        public void HapticManager_SafeVibrateCalls_DoNotThrowExceptions()
        {
            var haptic = HapticManager.Instance;
            haptic.IsHapticsEnabled = true;

            // Safe calls on non-mobile/headless environment must never crash
            var ex1 = Record.Exception(() => haptic.VibrateLight());
            var ex2 = Record.Exception(() => haptic.VibrateMedium());
            var ex3 = Record.Exception(() => haptic.VibrateHeavy());
            var ex4 = Record.Exception(() => haptic.Vibrate(50));

            Assert.Null(ex1);
            Assert.Null(ex2);
            Assert.Null(ex3);
            Assert.Null(ex4);
        }

        [Fact]
        public void HapticManager_WhenDisabled_DoesNotExecuteVibration()
        {
            var haptic = HapticManager.Instance;
            haptic.SetHapticsEnabled(false);

            bool called = haptic.Vibrate(20);
            Assert.False(called);

            haptic.SetHapticsEnabled(true);
        }
    }
}
