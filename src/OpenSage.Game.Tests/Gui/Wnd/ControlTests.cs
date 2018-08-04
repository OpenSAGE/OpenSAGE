using System.Linq;
using OpenSage.Gui.Wnd.Controls;
using Xunit;

namespace OpenSage.Tests.Gui.Wnd
{
    public class ControlTests
    {
        [Fact]
        public void GetSelfAndDescendants_When_TowControlsMatch_Then_ReturnTowControls()
        {
            var model = BuildTestModel();
            var result = Control.GetSelfAndDescendants(model).OfType<ComboBox>()
                .Where(i => i.Name.StartsWith("ComboBox"));

            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void GetSelfAndDescendants_When_NoControlMatch_Then_ReturnEmptyArray()
        {
            var model = BuildTestModel();
            var result = Control.GetSelfAndDescendants(model).OfType<ComboBox>()
                .Where(i => i.Name.StartsWith("Some"));

            Assert.Empty(result);
        }

        [Fact]
        public void GetSelfAndDescendants_When_TypeControlTextBox_Then_ReturnSixControls()
        {
            var model = BuildTestModel();
            var result = Control.GetSelfAndDescendants(model).OfType<TextBox>();

            //4 TextBoxes and 2 from the ComboBoxes
            Assert.Equal(6, result.Count());
        }

        [Fact]
        public void GetSelfAndDescendants_When_NoTypeMatch_Then_ReturnEmptyArray()
        {
            var model = BuildTestModel();
            var result = Control.GetSelfAndDescendants(model, false).OfType<Button>();

            Assert.Empty(result);
        }

        private Control BuildTestModel()
        {
            var owner = new Control();
            owner.Name = "Window";
            
            var tb1 = new TextBox();
            tb1.Name = "TextBox.1";
            var tb2 = new TextBox();
            tb2.Name = "TextBox.2";
            var tb3 = new TextBox();
            tb3.Name = "TextBox.3";
            var tb4 = new TextBox();
            tb4.Name = "TextBox.4";

            var cb1 = new ComboBox();
            cb1.Name = "ComboBox.1";
            var cb2 = new ComboBox();
            cb2.Name = "ComboBox.2";

            tb1.Controls.Add(cb1);
            tb2.Controls.Add(tb4);
            tb3.Controls.Add(cb2);
            tb1.Controls.Add(tb3);
            owner.Controls.Add(tb1);
            owner.Controls.Add(tb2);

            return owner;
        }
    }
}
