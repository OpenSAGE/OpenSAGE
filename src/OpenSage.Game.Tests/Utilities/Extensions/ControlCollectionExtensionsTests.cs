using System;
using System.Collections.Generic;
using System.Text;
using OpenSage.Data.Ini;
using OpenSage.Graphics.Util;
using OpenSage.Gui.Wnd.Controls;
using OpenSage.Utilities.Extensions;
using Xunit;

namespace OpenSage.Tests.Utilities.Extensions
{
    public class ControlCollectionExtensionsTests
    {
        [Fact]
        public void FindControlsStratsWith_When_TowControlsMatch_Then_ReturnTowControls()
        {
            var model = BuildTestModel();
            var result = model.FindControlsStratsWith<ComboBox>("ComboBox");

            Assert.Equal(2, result.Length);
        }

        [Fact]
        public void FindControlsStratsWith_When_NoControlMatch_Then_ReturnEmptyArray()
        {
            var model = BuildTestModel();
            var result = model.FindControlsStratsWith<ComboBox>("SOME");

            Assert.Empty(result);
        }

        [Fact]
        public void FindControlsByType_When_TypeControlTextBox_Then_ReturnSixControls()
        {
            var model = BuildTestModel();
            var result = model.FindControlsByType<TextBox>();

            //4 TextBoxes and 2 from the ComboBoxes
            Assert.Equal(6, result.Length);
        }

        [Fact]
        public void FindControlsByType_When_NoTypeMatch_Then_ReturnEmptyArray()
        {
            var model = BuildTestModel();
            var result = model.FindControlsByType<Button>(false);

            Assert.Empty(result);
        }


        private ControlCollection BuildTestModel()
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

            var collection = new ControlCollection(owner);
            tb1.Controls.Add(cb1);
            tb2.Controls.Add(tb4);
            tb3.Controls.Add(cb2);
            tb1.Controls.Add(tb3);
            collection.Add(tb1);
            collection.Add(tb2);

            return collection;
        }

    }
}
