using System;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui;
using OpenSage.Gui.Elements;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class WndView : GameControl
    {
        private readonly FileSystemEntry _entry;

        public WndView(FileSystemEntry entry, Game game)
        {
            Game = game;

            _entry = entry;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            // TODO
            if (Width > 0 && Height > 0)
            {
                var guiComponent = new GuiComponent
                {
                    RootWindow = Game.ContentManager.Load<UIElement>(_entry.FilePath)
                };

                var scene = new Scene();

                var entity = new Entity();
                entity.Components.Add(guiComponent);
                scene.Entities.Add(entity);

                Game.Scene = scene;
            }

            base.OnSizeChanged(e);
        }
    }
}
