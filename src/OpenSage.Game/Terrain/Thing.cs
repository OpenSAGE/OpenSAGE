using System.Collections.Generic;
using System.Numerics;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.Data.Map;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Logic.Object;
using OpenSage.Terrain.Util;

namespace OpenSage.Terrain
{
    public sealed class Thing : GraphicsObject
    {
        private readonly List<Drawable> _drawables;

        public Vector3 Position { get; set; }
        public float Angle { get; set; }

        private BitArray<ModelConditionFlag> _modelCondition;
        public BitArray<ModelConditionFlag> ModelCondition
        {
            get { return _modelCondition; }
            set
            {
                _modelCondition = value;

                foreach (var drawable in _drawables)
                {
                    drawable.OnModelConditionStateChanged(value);
                }
            }
        }

        public Thing(
            MapObject mapObject,
            HeightMap heightMap,
            ObjectDefinition objectDefinition,
            FileSystem fileSystem,
            ContentManager contentManager,
            ResourceUploadBatch uploadBatch,
            GraphicsDevice graphicsDevice,
            IniDataContext iniDataContext,
            ParticleSystemManager particleSystemManager)
        {
            var position = mapObject.Position.ToVector3();
            position.Z = heightMap.GetHeight(mapObject.Position.X, mapObject.Position.Y);
            Position = position;

            Angle = mapObject.Angle;

            _drawables = new List<Drawable>();

            foreach (var draw in objectDefinition.Draws)
            {
                switch (draw)
                {
                    case W3dModelDrawModuleData modelDrawData:
                        _drawables.Add(AddDisposable(new W3dModelDraw(
                            modelDrawData,
                            fileSystem,
                            contentManager,
                            uploadBatch,
                            iniDataContext,
                            particleSystemManager)));
                        break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            foreach (var drawable in _drawables)
            {
                drawable.Update(gameTime);
            }
        }

        public void Draw(
            CommandEncoder commandEncoder,
            MeshEffect meshEffect,
            Camera camera)
        {
            var world = Matrix4x4.CreateRotationZ(Angle) 
                * Matrix4x4.CreateTranslation(Position);

            foreach (var drawable in _drawables)
            {
                drawable.Draw(
                    commandEncoder,
                    meshEffect,
                    camera,
                    ref world);
            }
        }
    }
}
