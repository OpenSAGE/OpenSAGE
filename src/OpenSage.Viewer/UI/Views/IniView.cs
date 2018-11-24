using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Viewer.UI.Views.Ini;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class IniView : AssetView
    {
        private ViewMode _currentView;

        private readonly List<IniEntry> _subObjects;
        private IniEntry _selectedSubObject;
        private AssetView _selectedSubObjectView;

        private readonly string _iniString;

        public IniView(AssetViewContext context)
        {
            var iniDataContext = new IniDataContext(context.Entry.FileSystem, context.Game.SageGame);
            iniDataContext.LoadIniFile(context.Entry);

            using (var stream = context.Entry.Open())
            {
                using (var reader = new StreamReader(stream))
                {
                    _iniString = reader.ReadToEnd();
                }
            }

            _subObjects = new List<IniEntry>();

            foreach (var objectDefinition in iniDataContext.Objects)
            {
                _subObjects.Add(new IniEntry(objectDefinition.Name, () => new ObjectDefinitionView(context, objectDefinition)));
            }

            foreach (var particleSystem in iniDataContext.ParticleSystems)
            {
                _subObjects.Add(new IniEntry(particleSystem.Name, () => new ParticleSystemView(context, particleSystem)));
            }

            // If we can't show this file in object view, default to text view.
            _currentView = _subObjects.Count == 0 ? ViewMode.TextView : ViewMode.ObjectView;
        }

        private void DrawObjectMode(ref bool isGameViewFocused)
        {
            ImGui.BeginChild("ini sidebar", new Vector2(200, 0), true, 0);

            foreach (var subObject in _subObjects)
            {
                if (ImGui.Selectable(subObject.Name, subObject == _selectedSubObject))
                {
                    if (_selectedSubObject != null)
                    {
                        _selectedSubObjectView.Dispose();
                        _selectedSubObjectView = null;
                    }

                    _selectedSubObject = subObject;

                    _selectedSubObjectView = subObject.CreateView();
                }
            }

            ImGui.EndChild();

            ImGui.SameLine();

            _selectedSubObjectView?.Draw(ref isGameViewFocused);

        }

        private void DrawTextMode()
        {
            ImGui.TextUnformatted(_iniString);

            if (ImGui.BeginPopupContextItem())
            {
                if (ImGui.Selectable("Copy to clipboard"))
                {
                    ImGui.SetClipboardText(_iniString);
                }
                ImGui.EndPopup();
            }
        }

        public override void Draw(ref bool isGameViewFocused)
        {
            // Only show mode selection when there are objects to show.
            if (_subObjects.Count > 0)
            {
                if (ImGui.RadioButton("Object view", _currentView == ViewMode.ObjectView))
                {
                    _currentView = ViewMode.ObjectView;
                }

                ImGui.SameLine();

                if (ImGui.RadioButton("Text view", _currentView == ViewMode.TextView))
                {
                    _currentView = ViewMode.TextView;
                }
            }

            ImGui.BeginChild("mode view", ImGui.GetContentRegionAvail(), false, ImGuiWindowFlags.AlwaysAutoResize);

            switch (_currentView)
            {
                case ViewMode.ObjectView:
                    DrawObjectMode(ref isGameViewFocused);
                    break;
                case ViewMode.TextView:
                    DrawTextMode();
                    break;
                default: throw new ArgumentOutOfRangeException();
            }

            ImGui.EndChild();
        }

        private enum ViewMode
        {
            ObjectView,
            TextView
        }
    }
}
