using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Logic.Object;

namespace OpenSage.Terrain
{
    public sealed class Thing : GraphicsObject
    {
        private readonly List<Drawable> _drawables;

        public Vector3 Position { get; set; }
        public float Angle { get; set; }

        public IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates => _drawables
            .SelectMany(x => x.ModelConditionStates)
            .Distinct(new BitArrayEqualityComparer<ModelConditionFlag>())
            .OrderBy(x => x.NumBitsSet);

        public BitArray<ModelConditionFlag> ModelConditionFlags { get; private set; }

        public void SetModelConditionFlags(BitArray<ModelConditionFlag> flags)
        {
            ModelConditionFlags = flags;

            // TODO: Let each drawable use the appropriate TransitionState between ConditionStates.

            foreach (var drawable in _drawables)
            {
                drawable.UpdateConditionState(flags);
            }
        }

        private BitArray<ModelConditionFlag> _modelCondition;
        public BitArray<ModelConditionFlag> ModelCondition
        {
            get { return _modelCondition; }
            set
            {
                _modelCondition = value;

                foreach (var drawable in _drawables)
                {
                    drawable.UpdateConditionState(value);
                }
            }
        }

        public Thing(
            ObjectDefinition objectDefinition,
            Vector3 position,
            float angle,
            GameContext gameContext,
            ResourceUploadBatch uploadBatch)
        {
            Position = position;
            Angle = angle;

            _drawables = new List<Drawable>();

            foreach (var draw in objectDefinition.Draws)
            {
                switch (draw)
                {
                    case W3dModelDrawModuleData modelDrawData:
                        _drawables.Add(AddDisposable(new W3dModelDraw(
                            modelDrawData,
                            gameContext,
                            uploadBatch)));
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
            Camera camera,
            GameTime gameTime)
        {
            var world = Matrix4x4.CreateRotationZ(Angle) 
                * Matrix4x4.CreateTranslation(Position);

            foreach (var drawable in _drawables)
            {
                drawable.Draw(
                    commandEncoder,
                    meshEffect,
                    camera,
                    ref world,
                    gameTime);
            }
        }
    }
}
