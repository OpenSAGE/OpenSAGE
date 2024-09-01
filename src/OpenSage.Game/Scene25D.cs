using System;
using System.Collections.Generic;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Logic;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;

namespace OpenSage;

public class Scene25D(Scene3D scene3D, AssetStore assetStore)
{
    protected IGameObjectCollection GameObjects => scene3D.GameObjects;
    protected Camera Camera => scene3D.Camera;
    protected GameData GameData => assetStore.GameData.Current;

    private Player LocalPlayer => scene3D.LocalPlayer;

    private readonly LocalizedString _buildProgressString = new("CONTROLBAR:UnderConstructionDesc");

    /// <summary>
    /// Animations which are not persisted across saves
    /// </summary>
    protected Queue<TransientAnimation> TransientAnimations { get; } = [];

    public void Draw(DrawingContext2D drawingContext)
    {
        // The AssetViewer has no LocalPlayer
        if (LocalPlayer != null)
        {
            HashSet<uint> propagandizedUnits = [];

            foreach (var obj in GameObjects.Objects)
            {
                if (obj.FindBehavior<PropagandaTowerBehavior>() is { } behavior)
                {
                    foreach (var unitId in behavior.ObjectsInRange)
                    {
                        propagandizedUnits.Add(unitId);
                    }
                }
            }

            foreach (var obj in GameObjects.Objects)
            {
                if (obj.Hidden)
                {
                    continue;
                }

                var focused = obj.IsSelected || LocalPlayer.HoveredUnit == obj;

                if (obj.IsSelected || LocalPlayer.HoveredUnit == obj)
                {
                    DrawHealthBox(drawingContext, obj);
                }

                if (obj.IsBeingConstructed())
                {
                    DrawBuildProgress(drawingContext, obj);
                }

                DrawPips(drawingContext, obj, focused);

                // todo: break out animations?
                if (propagandizedUnits.Contains(obj.ID))
                {
                    // todo: not sure how this visually stacks with other items
                    // todo: subliminal vs enthusiastic
                    AddAnimationToDrawable(obj, AnimationType.Enthusiastic);
                }
                else
                {
                    RemoveAnimationFromDrawable(obj, AnimationType.Enthusiastic);
                }

                var healType = obj.IsKindOf(ObjectKinds.Structure) ? AnimationType.StructureHeal :
                    obj.IsKindOf(ObjectKinds.Vehicle) ? AnimationType.VehicleHeal : AnimationType.DefaultHeal;
                if (!obj.IsKindOf(ObjectKinds.NoHealIcon) && obj.HealedByObjectId > 0)
                {
                    // todo: how to tell if a unit is being healed by itself?
                    // a unit can be healing itself (usually from an upgrade like junk repair or veterancy, where healedbyobjectid is 0),
                    // or can be healed by another object (ambulance, hospital, prop tower, etc)
                    // animation DefaultHeal
                    AddAnimationToDrawable(obj, healType);
                }
                else
                {
                    RemoveAnimationFromDrawable(obj, healType);
                }

                // todo: animations
                // MoneyPickUp,
                // LevelGainedAnimation, not an object animation?
                // GetHealedAnimation, not an object animation?
                // BombTimed,
                // BombRemote,
                // CarBomb,
                // Disabled,
                // AmmoFull, UNUSED?
                // AmmoEmpty, UNUSED?

                DrawAnimations(drawingContext, obj, scene3D.GameContext.GameLogic.CurrentFrame.Value);
                // todo: transient animations need to be pulled from a game object, but processed separately
                EnqueueTransientAnimations(obj, scene3D.GameContext.GameLogic.CurrentFrame.Value);
            }

            // transient animations are not tied to a specific object
            DrawTransientAnimations(drawingContext, scene3D.GameContext.GameLogic.CurrentFrame.Value);
        }
    }

    protected virtual void DrawPips(DrawingContext2D drawingContext, GameObject gameObject, bool focused) { }

    /// <summary>
    /// Draws animations which are not persisted across saves.
    /// </summary>
    protected virtual void EnqueueTransientAnimations(GameObject gameObject, uint currentFrame) { }

    protected static BoundingSphere GetBoundingSphere(GameObject gameObject)
    {
        var geometrySize = gameObject.Definition.Geometry.Shapes[0].MajorRadius;

        // Not sure if this is what IsSmall is actually for.
        if (gameObject.Definition.Geometry.IsSmall)
        {
            geometrySize = Math.Max(geometrySize, 15);
        }

        return new BoundingSphere(gameObject.Translation, geometrySize);
    }

    private void DrawHealthBox(DrawingContext2D drawingContext, GameObject gameObject)
    {
        if (gameObject.Definition.KindOf.Get(ObjectKinds.Horde))
        {
            return;
        }

        var boundingSphere = GetBoundingSphere(gameObject);

        var healthBoxSize = Camera.GetScreenSize(boundingSphere);

        // todo: there should be some additional height being added here, but it's unclear what the logic should be
        var healthBoxWorldSpacePos = gameObject.Translation.WithZ(gameObject.Translation.Z + gameObject.Definition.Geometry.Shapes[0].Height);
        var healthBoxRect = Camera.WorldToScreenRectangle(
            healthBoxWorldSpacePos,
            new SizeF(healthBoxSize, 3));

        if (healthBoxRect == null)
        {
            return;
        }

        void DrawBar(in RectangleF rect, in ColorRgbaF color, float value)
        {
            var actualRect = rect.WithWidth(rect.Width * value);
            drawingContext.FillRectangle(actualRect, color);

            var borderColor = color.WithRGB(color.R / 2.0f, color.G / 2.0f, color.B / 2.0f);
            drawingContext.DrawRectangle(rect, borderColor, 1);
        }

        // TODO: Not sure what to draw for InactiveBody?
        if (gameObject.HasActiveBody())
        {
            var red = 0f;
            float green;
            var blue = 0f;

            if (gameObject.IsBeingConstructed())
            {
                green = (float)gameObject.HealthPercentage;
                blue = 1;
            }
            else
            {
                red = Math.Clamp((1 - (float)gameObject.HealthPercentage) * 2, 0, 1);
                green = Math.Clamp((float)gameObject.HealthPercentage * 2, 0, 1);
            }

            DrawBar(
                healthBoxRect.Value,
                new ColorRgbaF(red, green, blue, 1),
                (float)gameObject.HealthPercentage);
        }

        var yOffset = 0;
        if (gameObject.ProductionUpdate != null && !gameObject.IsBeingConstructed())
        {
            yOffset += 4;
            var productionBoxRect = healthBoxRect.Value.WithY(healthBoxRect.Value.Y + yOffset);
            var productionBoxValue = gameObject.ProductionUpdate.IsProducing
                ? gameObject.ProductionUpdate.ProductionQueue[0].Progress
                : 0;

            DrawBar(
                productionBoxRect,
                new ColorRgba(172, 255, 254, 255).ToColorRgbaF(),
                productionBoxValue);
        }

        var gainsExperience = gameObject.FindBehavior<ExperienceUpdate>()?.ObjectGainsExperience ?? false;
        if (gainsExperience)
        {
            yOffset += 4;
            var experienceBoxRect = healthBoxRect.Value.WithY(healthBoxRect.Value.Y + yOffset);
            DrawBar(
                experienceBoxRect,
                new ColorRgba(255, 255, 0, 255).ToColorRgbaF(),
                gameObject.ExperienceValue / (float)gameObject.ExperienceRequiredForNextLevel);
        }
    }

    private void DrawBuildProgress(DrawingContext2D drawingContext, GameObject gameObject)
    {
        if (gameObject.Definition.KindOf.Get(ObjectKinds.Horde))
        {
            return;
        }

        var boundingSphere = GetBoundingSphere(gameObject);

        var buildProgressSize = Camera.GetScreenSize(boundingSphere);

        var buildProgressWorldSpacePos = gameObject.Translation;
        var buildProgressRect = Camera.WorldToScreenRectangle(
            buildProgressWorldSpacePos,
            new SizeF(buildProgressSize * 10, 40)); // these numbers feel right, but are just a guess

        if (buildProgressRect == null)
        {
            return;
        }

        var text = _buildProgressString.Localize(gameObject.BuildProgress * 100);
        // todo: is this the correct font?
        drawingContext.DrawText(text, gameObject.GameContext.Game.ContentManager.FontManager.GetOrCreateFont(assetStore.InGameUI.Current.MessageFont, 26, FontWeight.Normal), TextAlignment.Center, ColorRgbaF.White, buildProgressRect.Value);
    }

    private void DrawBottomLeftImage(DrawingContext2D drawingContext, GameObject gameObject, MappedImage image)
    {
        var boundingSphere = GetBoundingSphere(gameObject);

        var xOffset = Camera.GetScreenSize(boundingSphere) / -2; // these just start where the health bar starts

        var rankWorldSpacePos = gameObject.Translation with
        {
            Z = gameObject.Translation.Z + gameObject.Definition.Geometry.Shapes[0].Height - image.Coords.Height / 8f,
        };

        var size = image.Coords.Size.ToSizeF();
        var propRect = Camera.WorldToScreenRectangle(
            rankWorldSpacePos,
            new SizeF(size.Width / 2, size.Height / 2)); // for some reason this seems to be half size

        if (propRect.HasValue)
        {
            var rect = propRect.Value;
            xOffset += rect.Width / 2;
            drawingContext.DrawMappedImage(image, rect.WithX(rect.X + xOffset));
        }
    }

    private void DrawTopCenteredImage(DrawingContext2D drawingContext, GameObject gameObject, MappedImage image)
    {
        var rankWorldSpacePos = gameObject.Translation with
        {
            Z = gameObject.Translation.Z + gameObject.Definition.Geometry.Shapes[0].Height + image.Coords.Height / 2f,
        };

        var propRect = Camera.WorldToScreenRectangle(
            rankWorldSpacePos,
            image.Coords.Size.ToSizeF());

        if (propRect.HasValue)
        {
            var rect = propRect.Value;
            drawingContext.DrawMappedImage(image, rect);
        }
    }

    private static void AddAnimationToDrawable(GameObject gameObject, AnimationType animationType)
    {
        gameObject.Drawable.AddAnimation(animationType);
    }

    private static void RemoveAnimationFromDrawable(GameObject gameObject, AnimationType animationType)
    {
        gameObject.Drawable.RemoveAnimation(animationType);
    }

    private readonly List<AnimationType> _animationsToRemove = []; // instantiating this here instead of in-scope prevents allocations

    private void DrawAnimations(DrawingContext2D context, GameObject gameObject, uint currentFrame)
    {
        _animationsToRemove.Clear();
        foreach (var animation in gameObject.Drawable.Animations)
        {
            if (animation.SetFrame(currentFrame))
            {
                var image = animation.Current;

                switch (animation.AnimationType)
                {
                    case AnimationType.DefaultHeal:
                    case AnimationType.StructureHeal:
                    case AnimationType.VehicleHeal:
                    case AnimationType.Disabled:
                    case AnimationType.CarBomb:
                        DrawTopCenteredImage(context, gameObject, image);
                        break;
                    case AnimationType.Enthusiastic:
                    case AnimationType.Subliminal:
                        DrawBottomLeftImage(context, gameObject, image);
                        break;
                    case AnimationType.BombTimed:
                    case AnimationType.BombRemote:
                    // todo: the above animations appear centered above the health bar for vehicles, and offset to the side for structures
                    case AnimationType.MoneyPickUp: // unknown how this animation works yet
                        throw new NotImplementedException("animation not yet implemented");
                    case AnimationType.AmmoFull:
                    case AnimationType.AmmoEmpty:
                        throw new NotSupportedException("animation not supported by game engine");
                    case AnimationType.LevelGainedAnimation:
                    case AnimationType.GetHealedAnimation:
                        throw new NotSupportedException("animation not object-based"); // potentially remove these from AnimationType in the future?
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                _animationsToRemove.Add(animation.AnimationType);
            }
        }

        foreach (var animation in _animationsToRemove)
        {
            RemoveAnimationFromDrawable(gameObject, animation);
        }
    }

    protected void DrawTransientAnimations(DrawingContext2D drawingContext, uint currentFrame)
    {
        var animationsToProcess = TransientAnimations.Count;
        for (var i = 0; i < animationsToProcess; i++)
        {
            var animation = TransientAnimations.Dequeue();

            animation.DrawForFrame(drawingContext, currentFrame);

            if (!animation.Complete)
            {
                TransientAnimations.Enqueue(animation);
            }
        }
    }
}

public abstract class TransientAnimation(Camera camera, uint startFrame)
{
    protected abstract uint FrameLength { get; }
    protected Camera Camera { get; } = camera;
    private uint EndFrame => startFrame + FrameLength;

    public bool Complete { get; protected set; }

    public virtual void DrawForFrame(DrawingContext2D drawingContext, uint currentFrame)
    {
        Complete = currentFrame >= EndFrame;
    }

    protected float Progress(uint currentFrame)
    {
        return (float)(currentFrame - startFrame) / (EndFrame - startFrame);
    }
}
