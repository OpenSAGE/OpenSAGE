using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eto.Drawing;
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

            var mapPanelDropDown = new DropDown
            {
                DataStore = new MapPanel[]
                {
                    new GeneralPanel(),
                    new ScriptPanel(game.Scene.MapFile.SidesList.PlayerScripts ?? game.Scene.MapFile.PlayerScriptsList),
                },
                ItemTextBinding = Binding.Property((MapPanel l) => l.Name)
            };

            var mapPanel = new Panel();

            mapPanelDropDown.SelectedValueChanged += (sender, e) =>
            {
                mapPanel.Content = ((MapPanel) mapPanelDropDown.SelectedValue).CreateControl(game);
            };

            mapPanelDropDown.SelectedIndex = 0;

            Panel1 = new StackLayout
            {
                Width = 300,
                Orientation = Orientation.Vertical,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                Items =
                {
                    mapPanelDropDown,
                    new StackLayoutItem(mapPanel, expand: true),
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

        private abstract class MapPanel
        {
            public abstract string Name { get; }

            public abstract Control CreateControl(Game game);
        }

        private sealed class GeneralPanel : MapPanel
        {
            public override string Name { get; } = "General";

            public override Control CreateControl(Game game)
            {
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

                return new GroupBox
                {
                    Text = "Time of day",
                    Padding = 10,
                    Content = new StackLayout
                    {
                        Orientation = Orientation.Vertical,
                        Items = { timesOfDayList }
                    }
                };
            }
        }

        private sealed class ScriptPanel : MapPanel
        {
            private readonly PlayerScriptsList _scriptsList;

            public override string Name { get; } = "Scripts";

            public ScriptPanel(PlayerScriptsList scriptsList)
            {
                _scriptsList = scriptsList;
            }

            public override Control CreateControl(Game game)
            {
                var treeItem = new TreeItem();
                for (var i = 0; i < _scriptsList.ScriptLists.Length; i++)
                {
                    treeItem.Children.Add(CreateScriptListTreeItem(i, _scriptsList.ScriptLists[i]));
                }

                Button toggleScriptsButton = null;

                void toggleScripts()
                {
                    game.Scripting.Active = !game.Scripting.Active;

                    toggleScriptsButton.Text = game.Scripting.Active
                        ? "Stop Scripts"
                        : "Start Scripts";
                }

                toggleScriptsButton = new Button((sender, e) => toggleScripts())
                {
                    Text = "Start Scripts"
                };

                var statePanel = new TextArea
                {
                    ReadOnly = true,
                    Height = 400
                };
                var stateContent = new StringBuilder();

                game.Scripting.OnUpdateFinished += (sender, scripting) =>
                {
                    stateContent.Clear();

                    stateContent.AppendLine("Counters:");

                    foreach (var kv in scripting.Counters)
                    {
                        stateContent.AppendFormat("  {0}: {1}\n", kv.Key, kv.Value);
                    }

                    stateContent.AppendLine("Flags:");

                    foreach (var kv in scripting.Flags)
                    {
                        stateContent.AppendFormat("  {0}: {1}\n", kv.Key, kv.Value);
                    }

                    stateContent.AppendLine("Timers:");

                    foreach (var kv in scripting.Timers)
                    {
                        stateContent.AppendFormat("  {0}: {1} ({2})\n", kv.Key, kv.Value, scripting.Counters[kv.Key]);
                    }

                    statePanel.Text = stateContent.ToString();
                };

                return new StackLayout
                {
                    Orientation = Orientation.Vertical,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    Items =
                    {
                        new GroupBox
                        {
                            Text = "Script control",
                            Padding = 10,
                            Content = toggleScriptsButton
                        },
                        new StackLayoutItem
                        {
                            Expand = true,
                            Control = new Splitter
                            {
                                Orientation = Orientation.Vertical,
                                Panel1 = new GroupBox
                                {
                                    Text = "Script state",
                                    Padding = 10,
                                    Content = statePanel
                                },
                                Panel2 = new TreeView
                                {
                                    DataStore = treeItem
                                }
                            }
                        }
                    }
                };
            }

            private static ITreeItem CreateScriptListTreeItem(int index, ScriptList scriptList)
            {
                var treeItem = new TreeItem
                {
                    Text = $"Script List {index}",
                    Expanded = true
                };

                foreach (var childGroup in scriptList.ScriptGroups)
                {
                    treeItem.Children.Add(CreateScriptGroupTreeItem(childGroup));
                }

                foreach (var script in scriptList.Scripts)
                {
                    treeItem.Children.Add(CreateScriptTreeItem(script));
                }

                return treeItem;
            }

            private static ITreeItem CreateScriptGroupTreeItem(ScriptGroup scriptGroup)
            {
                var treeItem = new TreeItem
                {
                    Text = GetScriptDetails(scriptGroup.Name, scriptGroup.IsActive, scriptGroup.IsSubroutine),
                    Expanded = true
                };

                foreach (var childGroup in scriptGroup.Groups)
                {
                    treeItem.Children.Add(CreateScriptGroupTreeItem(childGroup));
                }

                foreach (var script in scriptGroup.Scripts)
                {
                    treeItem.Children.Add(CreateScriptTreeItem(script));
                }

                return treeItem;
            }

            private static ITreeItem CreateScriptTreeItem(Script script)
            {
                return new TreeItem
                {
                    Text = GetScriptDetails(script.Name, script.IsActive, script.IsSubroutine),
                    Expanded = true
                };
            }

            private static string GetScriptDetails(string name, bool isActive, bool isSubroutine)
            {
                var result = name;

                result += isActive ? " (Active" : "(Inactive";

                if (isSubroutine)
                {
                    result += ", Subroutine";
                }

                result += ")";

                return result;
            }
        }
    }
}
