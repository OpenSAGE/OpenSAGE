using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using OpenSage.Data.Ini;
using OpenSage.Viewer.UI.Views.Ini;

namespace OpenSage.Viewer.UI.Views
{
    internal sealed class IniView : AssetView
    {
        private readonly List<IniEntry> _subObjects;
        private IniEntry _selectedSubObject;
        private AssetView _selectedSubObjectView;

        public IniView(AssetViewContext context)
        {
            var iniDataContext = new IniDataContext(context.Entry.FileSystem);
            iniDataContext.LoadIniFile(context.Entry);

            _subObjects = new List<IniEntry>();

            foreach (var objectDefinition in iniDataContext.Objects)
            {
                _subObjects.Add(new IniEntry(objectDefinition.Name, () => new ObjectDefinitionView(context, objectDefinition)));
            }

            foreach (var particleSystem in iniDataContext.ParticleSystems)
            {
                _subObjects.Add(new IniEntry(particleSystem.Name, () => new ParticleSystemView(context, particleSystem)));
            }
        }

        public override void Draw(ref bool isGameViewFocused)
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
    }
}
