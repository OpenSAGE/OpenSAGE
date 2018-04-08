using System;
using System.Linq;
using System.Numerics;
using Eto.Forms;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Controls;
using OpenSage.Graphics.Cameras.Controllers;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Settings;

namespace OpenSage.DataViewer.UI.Viewers.Ini
{
    public sealed class ObjectDefinitionView : Splitter
    {
        public ObjectDefinitionView(Func<IntPtr, Game> createGame, ObjectDefinition objectDefinition)
        {
            var listBox = new ListBox
            {
                Width = 200,
                ItemTextBinding = Binding.Property((BitArray<ModelConditionFlag> x) => x.DisplayName)
            };

            Panel1 = listBox;

            Panel2 = new GameControl
            {
                CreateGame = h =>
                {
                    var game = createGame(h);

                    var gameObjects = new GameObjectCollection(game.ContentManager);
                    var gameObject = gameObjects.Add(objectDefinition);

                    listBox.SelectedValueChanged += (sender, e) =>
                    {
                        var modelConditionState = (BitArray<ModelConditionFlag>) listBox.SelectedValue;
                        gameObject.SetModelConditionFlags(modelConditionState);
                    };
                    listBox.DataStore = gameObject.ModelConditionStates.ToList();
                    listBox.SelectedIndex = 0;

                    game.Scene3D = new Scene3D(
                        game,
                        new ArcballCameraController(Vector3.Zero, 200),
                        null,
                        null,
                        null,
                        gameObjects,
                        new WaypointCollection(),
                        new WaypointPathCollection(),
                        WorldLighting.CreateDefault(),
                        Array.Empty<Player>(),
                        Array.Empty<Team>());

                    return game;
                }
            };
        }

        protected override void Dispose(bool disposing)
        {
            Panel1.Dispose();
            Panel2.Dispose();

            base.Dispose(disposing);
        }
    }
}
