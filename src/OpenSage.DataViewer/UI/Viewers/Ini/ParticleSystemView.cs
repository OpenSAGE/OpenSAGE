using OpenSage.Data.Ini;
using OpenSage.DataViewer.Controls;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.DataViewer.UI.Viewers.Ini
{
    public sealed class ParticleSystemView : GameControl
    {
        public ParticleSystemView(Game game, ParticleSystemDefinition particleSystemDefinition)
        {
            var scene = new Scene();

            var particleSystemEntity = new Entity();
            particleSystemEntity.Components.Add(new ParticleSystem(particleSystemDefinition));
            scene.Entities.Add(particleSystemEntity);

            game.Scene = scene;

            Game = game;
        }
    }
}
