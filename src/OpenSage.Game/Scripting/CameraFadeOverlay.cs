using System.IO;
using System.Numerics;
using OpenSage.Data.Sav;
using OpenSage.FileFormats;
using OpenSage.Graphics.Rendering;
using OpenSage.Gui;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Scripting
{
    internal sealed class CameraFadeOverlay : DisposableBase
    {
        private readonly Game _game;
        private readonly DrawingContext2D _drawingContext;

        public CameraFadeType FadeType;

        public float From;
        public float To;

        public float CurrentValue;
        public uint CurrentFrame;

        public uint FramesIncrease;
        public uint FramesHold;
        public uint FramesDecrease;

        public CameraFadeOverlay(Game game)
        {
            _game = game;

            _drawingContext = AddDisposable(new DrawingContext2D(
                game.ContentManager,
                game.GraphicsLoadContext,
                BlendStateDescription.SingleAdditiveBlend,
                RenderPipeline.GameOutputDescription));
        }

        internal void Load(SaveFileReader reader)
        {
            FadeType = reader.ReadEnum<CameraFadeType>();

            From = reader.ReadSingle();
            To = reader.ReadSingle();

            CurrentValue = reader.ReadSingle();
            CurrentFrame = reader.ReadUInt32();

            FramesIncrease = reader.ReadUInt32();
            FramesHold = reader.ReadUInt32();
            FramesDecrease = reader.ReadUInt32();
        }

        public void Update()
        {
            if (FadeType == CameraFadeType.NotFading)
            {
                return;
            }

            if (CurrentFrame < FramesIncrease)
            {
                var s = CurrentFrame / (float) FramesIncrease;
                CurrentValue = MathUtility.Lerp(From, To, s);
            }
            else if (CurrentFrame < FramesIncrease + FramesHold)
            {
                CurrentValue = To;
            }
            else if (CurrentFrame < FramesIncrease + FramesHold + FramesDecrease)
            {
                var s = (CurrentFrame - FramesIncrease - FramesHold) / (float) FramesDecrease;
                CurrentValue = MathUtility.Lerp(To, From, s);
            }
            else
            {
                CurrentValue = From;
            }

            CurrentFrame += 1;
        }

        public void Render(CommandList commandList, SizeF outputSize)
        {
            if (FadeType == CameraFadeType.NotFading)
            {
                return;
            }

            commandList.PushDebugGroup("Camera fade");

            _drawingContext.Begin(
                commandList,
                _game.AssetStore.LoadContext.StandardGraphicsResources.LinearClampSampler,
                outputSize);

            switch (FadeType)
            {
                case CameraFadeType.AdditiveBlendToWhite:
                    _drawingContext.FillRectangle(
                        new RectangleF(Vector2.Zero, outputSize),
                        new ColorRgbaF(CurrentValue, CurrentValue, CurrentValue, 1));
                    break;
            }

            _drawingContext.End();

            commandList.PopDebugGroup();
        }
    }

    internal enum CameraFadeType
    {
        NotFading = 0,
        SubtractiveBlendToBlack = 1,
        AdditiveBlendToWhite = 2,
        SaturateBlend = 3,
        MultiplyBlendToBlack = 4,
    }
}
