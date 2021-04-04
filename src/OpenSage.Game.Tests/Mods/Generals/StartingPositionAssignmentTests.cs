using OpenSage.Mods.Generals.Gui;
using OpenSage.Network;
using Xunit;

namespace OpenSage.Tests.Mods.Generals
{
    public class StartingPositionAssignmentTests
    {
        [Fact]
        public void JustHost_HostClickedEmpty_AssignsHost()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };

            GameOptionsUtil.StartingPositionClicked(settings, 2);

            Assert.Equal(2, settings.Slots[0].StartPosition);
        }

        [Fact]
        public void JustHost_HostClickedHost_UnassignsHost()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 2;

            GameOptionsUtil.StartingPositionClicked(settings, 2);

            Assert.Equal(0, settings.Slots[0].StartPosition);
        }

        [Fact]
        public void JustHost_HostClickedEmpty_UpdatesHost()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 3;

            GameOptionsUtil.StartingPositionClicked(settings, 1);

            Assert.Equal(1, settings.Slots[0].StartPosition);
        }

        [Fact]
        public void HostAndAI_HostClickedEmptyTwice_AssignsAI()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[1].State = SkirmishSlotState.EasyArmy;

            GameOptionsUtil.StartingPositionClicked(settings, 4);
            GameOptionsUtil.StartingPositionClicked(settings, 4);

            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(4, settings.Slots[1].StartPosition);
        }

        [Fact]
        public void HostAndAI_HostClickedEmptyThreeTimes_DoesNothing()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[1].State = SkirmishSlotState.EasyArmy;

            GameOptionsUtil.StartingPositionClicked(settings, 4);
            GameOptionsUtil.StartingPositionClicked(settings, 4);
            GameOptionsUtil.StartingPositionClicked(settings, 4);

            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(0, settings.Slots[1].StartPosition);
        }

        [Fact]
        public void HostAndTwoAIs_HostClickedEmpty_AssignsHost()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[1].State = SkirmishSlotState.EasyArmy;
            settings.Slots[2].State = SkirmishSlotState.HardArmy;

            GameOptionsUtil.StartingPositionClicked(settings, 4);

            Assert.Equal(4, settings.Slots[0].StartPosition);
            Assert.Equal(0, settings.Slots[1].StartPosition);
            Assert.Equal(0, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostAndTwoAIs_HostClickedEmptyTwice_AssignsFirstAI()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[1].State = SkirmishSlotState.EasyArmy;
            settings.Slots[2].State = SkirmishSlotState.HardArmy;

            GameOptionsUtil.StartingPositionClicked(settings, 4);
            GameOptionsUtil.StartingPositionClicked(settings, 4);

            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(4, settings.Slots[1].StartPosition);
            Assert.Equal(0, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostAndTwoAIs_HostClickedEmptyThreeTimes_AssignsSecondAI()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[1].State = SkirmishSlotState.EasyArmy;
            settings.Slots[2].State = SkirmishSlotState.HardArmy;

            GameOptionsUtil.StartingPositionClicked(settings, 4);
            GameOptionsUtil.StartingPositionClicked(settings, 4);
            GameOptionsUtil.StartingPositionClicked(settings, 4);

            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(0, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostAndAI_HostClickedHost_UnassignsHost()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.EasyArmy;
            settings.Slots[1].StartPosition = 2;

            GameOptionsUtil.StartingPositionClicked(settings, 1);
            
            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(2, settings.Slots[1].StartPosition);
        }

        [Fact]
        public void HostAndAI_HostClickedAI_UnassignsAI()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.MediumArmy;
            settings.Slots[1].StartPosition = 2;

            GameOptionsUtil.StartingPositionClicked(settings, 2);

            Assert.Equal(1, settings.Slots[0].StartPosition);
            Assert.Equal(0, settings.Slots[1].StartPosition);
        }

        [Fact]
        public void HostAndHuman_HostClickedEmptyTwice_DoesNothing()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[1].State = SkirmishSlotState.Human;

            GameOptionsUtil.StartingPositionClicked(settings, 4);
            GameOptionsUtil.StartingPositionClicked(settings, 4);

            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(0, settings.Slots[1].StartPosition);
        }

        [Fact]
        public void HostAndHuman_HostClickedEmptyThreeTimes_AssignsHost()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[1].State = SkirmishSlotState.Human;

            GameOptionsUtil.StartingPositionClicked(settings, 4);
            GameOptionsUtil.StartingPositionClicked(settings, 4);
            GameOptionsUtil.StartingPositionClicked(settings, 4);

            Assert.Equal(4, settings.Slots[0].StartPosition);
            Assert.Equal(0, settings.Slots[1].StartPosition);
        }

        [Fact]
        public void HostAndHuman_HostClickedHost_UnassignsHost()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.Human;
            settings.Slots[1].StartPosition = 2;

            GameOptionsUtil.StartingPositionClicked(settings, 1);

            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(2, settings.Slots[1].StartPosition);
        }

        [Fact]
        public void HostAndHuman_HostClickedHuman_DoesNothing()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.Human;
            settings.Slots[1].StartPosition = 2;

            GameOptionsUtil.StartingPositionClicked(settings, 2);

            Assert.Equal(1, settings.Slots[0].StartPosition);
            Assert.Equal(2, settings.Slots[1].StartPosition);
        }

        [Fact]
        public void HostAIAndHuman_HostClickedHost_UnassignsHost()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.EasyArmy;
            settings.Slots[1].StartPosition = 2;
            settings.Slots[2].State = SkirmishSlotState.Human;
            settings.Slots[2].StartPosition = 4;

            GameOptionsUtil.StartingPositionClicked(settings, 1);

            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(2, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostAIAndHuman_HostClickedAI_UnassignsAI()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.MediumArmy;
            settings.Slots[1].StartPosition = 2;
            settings.Slots[2].State = SkirmishSlotState.Human;
            settings.Slots[2].StartPosition = 4;

            GameOptionsUtil.StartingPositionClicked(settings, 2);

            Assert.Equal(1, settings.Slots[0].StartPosition);
            Assert.Equal(0, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostAIAndHuman_HostClickedHuman_DoesNothing()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.HardArmy;
            settings.Slots[1].StartPosition = 2;
            settings.Slots[2].State = SkirmishSlotState.Human;
            settings.Slots[2].StartPosition = 4;

            GameOptionsUtil.StartingPositionClicked(settings, 4);

            Assert.Equal(1, settings.Slots[0].StartPosition);
            Assert.Equal(2, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostAIAndHuman_HostClickedEmpty_AssignsHost()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.EasyArmy;
            settings.Slots[1].StartPosition = 2;
            settings.Slots[2].State = SkirmishSlotState.Human;
            settings.Slots[2].StartPosition = 4;

            GameOptionsUtil.StartingPositionClicked(settings, 3);

            Assert.Equal(3, settings.Slots[0].StartPosition);
            Assert.Equal(2, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostHumanAndAI_HumanClickedHost_DoesNothing()
        {
            var settings = new SkirmishGameSettings(false) { LocalSlotIndex = 1 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.Human;
            settings.Slots[1].StartPosition = 2;
            settings.Slots[2].State = SkirmishSlotState.EasyArmy;
            settings.Slots[2].StartPosition = 4;

            GameOptionsUtil.StartingPositionClicked(settings, 1);

            Assert.Equal(1, settings.Slots[0].StartPosition);
            Assert.Equal(2, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostHumanAndAI_HumanClickedHuman_UnassignsHuman()
        {
            var settings = new SkirmishGameSettings(false) { LocalSlotIndex = 1 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.Human;
            settings.Slots[1].StartPosition = 2;
            settings.Slots[2].State = SkirmishSlotState.EasyArmy;
            settings.Slots[2].StartPosition = 4;

            GameOptionsUtil.StartingPositionClicked(settings, 2);

            Assert.Equal(1, settings.Slots[0].StartPosition);
            Assert.Equal(0, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostHumanAndAI_HumanClickedAI_DoesNothing()
        {
            var settings = new SkirmishGameSettings(false) { LocalSlotIndex = 1 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.Human;
            settings.Slots[1].StartPosition = 2;
            settings.Slots[2].State = SkirmishSlotState.EasyArmy;
            settings.Slots[2].StartPosition = 4;

            GameOptionsUtil.StartingPositionClicked(settings, 4);

            Assert.Equal(1, settings.Slots[0].StartPosition);
            Assert.Equal(2, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void HostHumanAndAI_HumanClickedEmpty_AssignsHuman()
        {
            var settings = new SkirmishGameSettings(false) { LocalSlotIndex = 1 };
            settings.Slots[0].StartPosition = 1;
            settings.Slots[1].State = SkirmishSlotState.Human;
            settings.Slots[1].StartPosition = 2;
            settings.Slots[2].State = SkirmishSlotState.EasyArmy;
            settings.Slots[2].StartPosition = 4;

            GameOptionsUtil.StartingPositionClicked(settings, 3);

            Assert.Equal(1, settings.Slots[0].StartPosition);
            Assert.Equal(3, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
        }

        [Fact]
        public void ComplexInteraction()
        {
            var settings = new SkirmishGameSettings(true) { LocalSlotIndex = 0 };

            // host clicks on 1 and assigns host
            GameOptionsUtil.StartingPositionClicked(settings, 1);
            Assert.Equal(1, settings.Slots[0].StartPosition);

            // host clicks on 2 and updates host
            GameOptionsUtil.StartingPositionClicked(settings, 2);
            Assert.Equal(2, settings.Slots[0].StartPosition);

            // host clicks on 2 again and unassigns host
            GameOptionsUtil.StartingPositionClicked(settings, 2);
            Assert.Equal(0, settings.Slots[0].StartPosition);

            // host adds AI
            settings.Slots[1].State = SkirmishSlotState.EasyArmy;

            // host clicks on 3 and assigns host
            GameOptionsUtil.StartingPositionClicked(settings, 3);
            Assert.Equal(3, settings.Slots[0].StartPosition);

            // host clicks on 3 again and assigns AI
            GameOptionsUtil.StartingPositionClicked(settings, 3);
            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(3, settings.Slots[1].StartPosition);

            // human joins
            settings.Slots[2].State = SkirmishSlotState.Human;

            // host clicks on 3 and unassigns AI
            GameOptionsUtil.StartingPositionClicked(settings, 3);
            Assert.Equal(0, settings.Slots[1].StartPosition);

            // human selected position 4
            settings.Slots[2].StartPosition = 4;

            // host clicks on 4 and nothing happens
            GameOptionsUtil.StartingPositionClicked(settings, 4);
            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);

            // host closes slot 3
            settings.Slots[3].State = SkirmishSlotState.Closed;

            // host adds another AI
            settings.Slots[4].State = SkirmishSlotState.HardArmy;

            // host clicks on 2 and assigns host
            GameOptionsUtil.StartingPositionClicked(settings, 2);
            Assert.Equal(2, settings.Slots[0].StartPosition);

            // host clicks on 2 again and assigns first AI
            GameOptionsUtil.StartingPositionClicked(settings, 2);
            Assert.Equal(0, settings.Slots[0].StartPosition);
            Assert.Equal(2, settings.Slots[1].StartPosition);

            // host clicks on 2 again and assigns second AI
            GameOptionsUtil.StartingPositionClicked(settings, 2);
            Assert.Equal(0, settings.Slots[1].StartPosition);
            Assert.Equal(2, settings.Slots[4].StartPosition);

            // host clicks on 2 again and unassigns second AI
            GameOptionsUtil.StartingPositionClicked(settings, 2);
            Assert.Equal(0, settings.Slots[4].StartPosition);

            // host clicks on 2 and assigns host
            GameOptionsUtil.StartingPositionClicked(settings, 2);
            Assert.Equal(2, settings.Slots[0].StartPosition);

            // host clicks on 1 and assigns first AI
            GameOptionsUtil.StartingPositionClicked(settings, 1);
            Assert.Equal(1, settings.Slots[1].StartPosition);

            // host clicks on 3 and assigns second AI
            GameOptionsUtil.StartingPositionClicked(settings, 3);
            Assert.Equal(3, settings.Slots[4].StartPosition);

            // host clicks on 6 and assigns host
            GameOptionsUtil.StartingPositionClicked(settings, 6);
            Assert.Equal(6, settings.Slots[0].StartPosition);

            Assert.Equal(1, settings.Slots[1].StartPosition);
            Assert.Equal(4, settings.Slots[2].StartPosition);
            Assert.Equal(3, settings.Slots[4].StartPosition);
        }
    }
}
