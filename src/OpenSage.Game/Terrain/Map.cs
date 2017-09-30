using System.Collections.Generic;
using System.Linq;
using LLGfx;
using OpenSage.Data.Map;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Terrain.Util;

namespace OpenSage.Terrain
{
    public sealed class Map : GraphicsObject
    {
        private readonly GameContext _gameContext;

        //private readonly Terrain _terrain;

        private readonly Dictionary<TimeOfDay, Lights> _lightConfigurations;
        private Lights _lights;

        //public Terrain Terrain => _terrain;

        private readonly MeshEffect _meshEffect;

        private readonly List<Thing> _things;
        private readonly List<Road> _roads;

        private TimeOfDay _currentTimeOfDay;
        public TimeOfDay CurrentTimeOfDay
        {
            get { return _currentTimeOfDay; }
            set
            {
                _currentTimeOfDay = value;
                _lights = _lightConfigurations[value];
            }
        }

        public Map(MapFile mapFile, GameContext gameContext)
        {
            _gameContext = gameContext;

            var uploadBatch = new ResourceUploadBatch(gameContext.GraphicsDevice);
            uploadBatch.Begin();

            //_terrain = AddDisposable(new Terrain(mapFile, gameContext));

            uploadBatch.End();

            //_meshEffect = AddDisposable(new MeshEffect(gameContext.GraphicsDevice));

            //_things = new List<Thing>();
            //_roads = new List<Road>();

            //foreach (var mapObject in mapFile.ObjectsList.Objects)
            //{
            //    uploadBatch.Begin();

            //    switch (mapObject.RoadType)
            //    {
            //        case RoadType.None:
            //            var objectDefinition = gameContext.IniDataContext.Objects.FirstOrDefault(x => x.Name == mapObject.TypeName);
            //            if (objectDefinition != null)
            //            {
            //                var position = mapObject.Position.ToVector3();
            //                position.Z = _terrain.HeightMap.GetHeight(position.X, position.Y);

            //                _things.Add(AddDisposable(new Thing(
            //                    objectDefinition,
            //                    position,
            //                    mapObject.Angle,
            //                    gameContext,
            //                    uploadBatch)));
            //            }
            //            break;

            //        default:
            //            _roads.Add(AddDisposable(new Road()));
            //            break;
            //    }

            //    uploadBatch.End();
            //}
        }

        public void Update(GameTime gameTime)
        {
            foreach (var thing in _things)
            {
                thing.Update(gameTime);
            }

            //_gameContext.ParticleSystemManager.Update(gameTime);
        }

        public void Draw(
            CommandEncoder commandEncoder,
            Camera camera,
            GameTime gameTime)
        {
            //Terrain.Draw(
            //    commandEncoder,
            //    camera,
            //    ref _lights);

            _meshEffect.Begin(commandEncoder);

            _meshEffect.SetLights(ref _lights);

            foreach (var thing in _things)
            {
                thing.Draw(
                    commandEncoder,
                    _meshEffect,
                    camera,
                    gameTime);
            }

            //_gameContext.ParticleSystemManager.Draw(
            //    commandEncoder, 
            //    camera);
        }
    }
}
