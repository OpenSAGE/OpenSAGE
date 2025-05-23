﻿using System.Numerics;
using OpenSage.Graphics.Rendering;
using OpenSage.Gui;
using OpenSage.Mathematics;
using Veldrid;

namespace OpenSage.Scripting;

internal sealed class CameraFadeOverlay : DisposableBase, IPersistableObject
{
    private readonly IGame _game;
    private readonly DrawingContext2D _drawingContext;

    public CameraFadeType FadeType;

    public float From;
    public float To;

    public float CurrentValue;
    public uint CurrentFrame;

    public uint FramesIncrease;
    public uint FramesHold;
    public uint FramesDecrease;

    public CameraFadeOverlay(IGame game)
    {
        _game = game;

        _drawingContext = AddDisposable(new DrawingContext2D(
            game.ContentManager,
            game.GraphicsLoadContext,
            BlendStateDescription.SingleAdditiveBlend,
            RenderPipeline.GameOutputDescription));
    }

    public void Persist(StatePersister reader)
    {
        reader.PersistEnum(ref FadeType);

        reader.PersistSingle(ref From);
        reader.PersistSingle(ref To);

        reader.PersistSingle(ref CurrentValue);
        reader.PersistUInt32(ref CurrentFrame);

        reader.PersistUInt32(ref FramesIncrease);
        reader.PersistUInt32(ref FramesHold);
        reader.PersistUInt32(ref FramesDecrease);
    }

    public void Update()
    {
        if (FadeType == CameraFadeType.NotFading)
        {
            return;
        }

        if (CurrentFrame < FramesIncrease)
        {
            var s = CurrentFrame / (float)FramesIncrease;
            CurrentValue = MathUtility.Lerp(From, To, s);
        }
        else if (CurrentFrame < FramesIncrease + FramesHold)
        {
            CurrentValue = To;
        }
        else if (CurrentFrame < FramesIncrease + FramesHold + FramesDecrease)
        {
            var s = (CurrentFrame - FramesIncrease - FramesHold) / (float)FramesDecrease;
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
            outputSize,
            // TODO: Pass correct time here
            TimeInterval.Zero);

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
