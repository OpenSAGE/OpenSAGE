using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Settings;
using OpenSage.Viewer.Framework;

namespace OpenSage.Viewer.UI.Views.Ini
{
    internal sealed class ObjectDefinitionView : GameView
    {
        private readonly GameObject _gameObject;
        private readonly List<BitArray<ModelConditionFlag>> _modelConditionStates;
        private int _selectedIndex;

        public ObjectDefinitionView(AssetViewContext context, ObjectDefinition objectDefinition)
            : base(context)
        {
            var gameObjects = new GameObjectCollection(context.Game.ContentManager);
            _gameObject = gameObjects.Add(objectDefinition);

            _modelConditionStates = _gameObject.ModelConditionStates.ToList();
            _selectedIndex = 0;

            context.Game.Scene3D = new Scene3D(
                context.Game,
                new ArcballCameraController(Vector3.Zero, 200),
                null,
                null,
                Array.Empty<Terrain.Road>(),
                Array.Empty<Terrain.Bridge>(),
                null,
                gameObjects,
                new WaypointCollection(),
                new WaypointPathCollection(),
                WorldLighting.CreateDefault(),
                Array.Empty<Player>(),
                Array.Empty<Team>());
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("object states", new Vector2(200, 0), true, 0);

            for (var i =  0; i<_modelConditionStates.Count; i++)
            {
                var modelConditionState = _modelConditionStates[i];

                if (ImGui.Selectable(modelConditionState.DisplayName, i == _selectedIndex))
                {
                    _gameObject.SetModelConditionFlags(modelConditionState);
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            base.Draw(ref isGameViewFocused);
        }
    }
}
