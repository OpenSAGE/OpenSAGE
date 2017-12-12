using System.Collections.Generic;
using System.Linq;
using Eto.Forms;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.DataViewer.Controls;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class MapView : Splitter
    {
        public MapView(FileSystemEntry entry, Game game)
        {
            game.Scene = game.ContentManager.Load<Scene>(entry.FilePath);

            var timesOfDayList = new RadioButtonList
            {
                Orientation = Orientation.Vertical,
                DataStore = GetTimesOfDay().Cast<object>()
            };

            timesOfDayList.SelectedValueChanged += (sender, e) =>
            {
                game.Scene.Settings.TimeOfDay = (TimeOfDay) timesOfDayList.SelectedValue;
            };

            timesOfDayList.SelectedIndex = 1;

            Panel1 = new StackLayout
            {
                Orientation = Orientation.Vertical,
                Items =
                {
                    new GroupBox
                    {
                        Text = "Time of day",
                        Padding = 10,
                        Width = 200,
                        Content = new StackLayout
                        {
                            Orientation = Orientation.Vertical,
                            Items = { timesOfDayList }
                        }
                    }
                }
            };

            Panel2 = new GameControl
            {
                Game = game
            };
        }

        private static IEnumerable<TimeOfDay> GetTimesOfDay()
        {
            yield return TimeOfDay.Morning;
            yield return TimeOfDay.Afternoon;
            yield return TimeOfDay.Evening;
            yield return TimeOfDay.Night;
        }
    }
}
