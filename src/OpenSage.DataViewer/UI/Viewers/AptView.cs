using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenSage.Data;
using OpenSage.DataViewer.Controls;
using OpenSage.Gui.Apt;

namespace OpenSage.DataViewer.UI.Viewers
{
    public sealed class AptView : GameControl
    {
        public AptView(FileSystemEntry entry, Game game)
        {
            var scene = new Scene();
            var entity = new Entity();

            var guiComponent = game.ContentManager.Load<AptComponent>(entry.FilePath);
            entity.Components.Add(guiComponent);
            scene.Entities.Add(entity);

            game.Scene = scene;
            Game = game;
        }
    }
}
