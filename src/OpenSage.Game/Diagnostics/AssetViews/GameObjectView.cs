using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage.Diagnostics.AssetViews
{
    internal sealed class GameObjectView : AssetView
    {
        private GameObject _gameObject;
        private readonly List<BitArray<ModelConditionFlag>> _modelConditionStates;
        private int _selectedIndex;

        private readonly RenderedView _renderedView;

        public GameObjectView(DiagnosticViewContext context, ObjectDefinition objectDefinition)
            : base(context)
        {
            _renderedView = AddDisposable(new RenderedView(context,
                createGameObjects: gameObjects =>
                {
                    _gameObject = gameObjects.Add(objectDefinition, context.Game.CivilianPlayer);
                }));

            _modelConditionStates = _gameObject.ModelConditionStates.ToList();
            _selectedIndex = 0;
        }

        public override void Draw()
        {
            ImGui.BeginChild("object states", new Vector2(200, 0), true, 0);

            for (var i = 0; i < _modelConditionStates.Count; i++)
            {
                var modelConditionState = _modelConditionStates[i];

                if (ImGui.Selectable(modelConditionState.DisplayName, i == _selectedIndex))
                {
                    _gameObject.SetModelConditionFlags(modelConditionState);
                    _selectedIndex = i;
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            _renderedView.Draw();
        }
    }
}
