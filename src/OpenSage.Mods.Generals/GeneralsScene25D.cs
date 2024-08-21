using System;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Content.Translation;
using OpenSage.Graphics.Cameras;
using OpenSage.Gui;
using OpenSage.Logic.Object;
using OpenSage.Mathematics;
using SixLabors.Fonts;

namespace OpenSage.Mods.Generals;

public class GeneralsScene25D(Scene3D scene3D, AssetStore assetStore) : Scene25D(scene3D, assetStore)
{
    private const string VeterancyPipPrefix = "SCVETER";
    private const string EmptyContainerPipName = "SCPPipEmpty";
    private const string FullContainerPipName = "SCPPipFull";
    private const string EmptyAmmoPipName = "SCPAmmoEmpty";
    private const string FullAmmoPipName = "SCPAmmoFull";

    private readonly LazyAssetReference<MappedImage> _veteranPip = assetStore.MappedImages.GetLazyAssetReferenceByName($"{VeterancyPipPrefix}1");
    private readonly LazyAssetReference<MappedImage> _elitePip = assetStore.MappedImages.GetLazyAssetReferenceByName($"{VeterancyPipPrefix}2");
    private readonly LazyAssetReference<MappedImage> _heroicPip = assetStore.MappedImages.GetLazyAssetReferenceByName($"{VeterancyPipPrefix}3");

    private readonly LazyAssetReference<MappedImage> _emptyContainerPip = assetStore.MappedImages.GetLazyAssetReferenceByName(EmptyContainerPipName);
    private readonly LazyAssetReference<MappedImage> _fullContainerPip = assetStore.MappedImages.GetLazyAssetReferenceByName(FullContainerPipName);

    // while there are single-image animations for this in the generals game files, they don't appear to ever be used in the game
    private readonly LazyAssetReference<MappedImage> _emptyAmmoPip = assetStore.MappedImages.GetLazyAssetReferenceByName(EmptyAmmoPipName);
    private readonly LazyAssetReference<MappedImage> _fullAmmoPip = assetStore.MappedImages.GetLazyAssetReferenceByName(FullAmmoPipName);

    private readonly Font _defaultFont = scene3D.Game.ContentManager.FontManager.GetOrCreateFont(14, FontWeight.Normal);
    private readonly AssetStore _assetStore = assetStore;
    private AnimationTemplate LevelUpAnimation => _assetStore.Animations.GetLazyAssetReferenceByName(GameData.LevelGainAnimationName).Value;

    /// <summary>
    /// Draws veterancy, container, and ammo pips specific to Generals/Zero Hour
    /// </summary>
    protected override void DrawPips(DrawingContext2D drawingContext, GameObject obj, bool focused)
    {
        if (focused)
        {
            var containerBehavior = obj.FindBehavior<OpenContainModule>();
            if (containerBehavior is { DrawPips: true } && containerBehavior.ContainedObjectIds.Count > 0)
            {
                DrawContainerPips(drawingContext, obj, containerBehavior);
            }

            foreach (var weapon in obj.ActiveWeaponSet.Weapons)
            {
                if (weapon?.Template.ShowsAmmoPips == true)
                {
                    DrawAmmoPips(drawingContext, obj, weapon.CurrentRounds, weapon.Template.ClipSize);
                }
            }
        }

        if (obj.Rank > 0)
        {
            DrawRank(drawingContext, obj);
        }

        base.DrawPips(drawingContext, obj, focused);
    }

    protected override void EnqueueTransientAnimations(GameObject gameObject, uint currentFrame)
    {
        if (gameObject.ActiveCashEvent.HasValue)
        {
            var cashEvent = gameObject.ActiveCashEvent.Value;
            // as far as I can tell, there aren't any localized strings for this;
            TransientAnimations.Enqueue(new CashAnimation(Camera, currentFrame, _defaultFont,
                gameObject.Transform.Translation + cashEvent.Offset, cashEvent.Amount, cashEvent.Color.ToColorRgbaF()));

            gameObject.ActiveCashEvent = null;
        }

        if (gameObject.VeterancyHelper.ShowRankUpAnimation)
        {
            TransientAnimations.Enqueue(new RankUpAnimation(Camera, currentFrame, GameData, new Animation(LevelUpAnimation), gameObject.Translation with { Z = gameObject.Translation.Z + 20 }));

            gameObject.VeterancyHelper.ShowRankUpAnimation = false;
        }
    }

    private void DrawRank(DrawingContext2D drawingContext, GameObject gameObject)
    {
        var mappedImage = gameObject.Rank switch
        {
            1 => _veteranPip,
            2 => _elitePip,
            3 => _heroicPip,
            _ => throw new ArgumentOutOfRangeException(nameof(gameObject.Rank), gameObject.Rank, "Rank not supported"),
        };

        var boundingSphere = GetBoundingSphere(gameObject);

        var xOffset = Camera.GetScreenSize(boundingSphere) / 1.5f; // 1.5 seems to give us a good offset from where the health bar would be

        var rankWorldSpacePos = gameObject.Translation with
        {
            Z = gameObject.Translation.Z + gameObject.Definition.Geometry.Shapes[0].Height,
        };

        var rankRect = Camera.WorldToScreenRectangle(
            rankWorldSpacePos,
            mappedImage.Value.Coords.Size.ToSizeF());

        if (rankRect.HasValue)
        {
            var rect = rankRect.Value;
            drawingContext.DrawMappedImage(mappedImage.Value, rect.WithX(rect.X + xOffset));
        }
    }

    private void DrawContainerPips(DrawingContext2D drawingContext, GameObject gameObject, OpenContainModule containerBehavior)
    {
        var totalPips = containerBehavior.TotalSlots;
        var infantryPips = 0; // infantry pips render first
        var vehiclePips = 0;
        foreach (var unitId in containerBehavior.ContainedObjectIds)
        {
            var definition = GameObjects.GetObjectById(unitId).Definition;
            var toAdd = definition.TransportSlotCount;
            if (definition.KindOf.Get(ObjectKinds.Infantry))
            {
                infantryPips += toAdd;
            }
            else
            {
                vehiclePips += toAdd;
            }
        }

        var pipSize = _fullContainerPip.Value.Coords.Size.ToSizeF() * GameData.ContainerPipScaleFactor;

        var pipWidth = pipSize.Width;

        var boundingSphere = GetBoundingSphere(gameObject);
        var xOffset = Camera.GetScreenSize(boundingSphere) / -2; // these just start where the health bar starts

        var pipWorldSpacePos = gameObject.Translation + GameData.ContainerPipWorldOffset;

        var pipRect = Camera.WorldToScreenRectangle(
            pipWorldSpacePos,
            _fullContainerPip.Value.Coords.Size.ToSizeF());

        if (!pipRect.HasValue)
        {
            return;
        }

        for (var i = 0; i < infantryPips; i++)
        {
            var rect = pipRect.Value;
            drawingContext.DrawMappedImage(_fullContainerPip.Value, rect.WithX(rect.X + xOffset), ColorRgbaF.Green);
            xOffset += pipWidth;
        }

        for (var i = 0; i < vehiclePips; i++)
        {
            var rect = pipRect.Value;
            drawingContext.DrawMappedImage(_fullContainerPip.Value, rect.WithX(rect.X + xOffset), ColorRgbaF.Blue);
            xOffset += pipWidth;
        }

        for (var i = vehiclePips + infantryPips; i < totalPips; i++)
        {
            var rect = pipRect.Value;
            drawingContext.DrawMappedImage(_emptyContainerPip.Value, rect.WithX(rect.X + xOffset));
            xOffset += pipWidth;
        }
    }

    private void DrawAmmoPips(DrawingContext2D drawingContext, GameObject gameObject, int currentRounds, int clipSize)
    {
        var emptyRoundsToDraw = clipSize - currentRounds;

        var pipSize = _fullAmmoPip.Value.Coords.Size.ToSizeF() * GameData.AmmoPipScaleFactor;

        var pipWidth = pipSize.Width;

        var boundingSphere = GetBoundingSphere(gameObject);
        var xOffset = Camera.GetScreenSize(boundingSphere) / -2; // these just start where the health bar starts - same position as garrison pips (guess a unit shouldn't have both?)

        var pipWorldSpacePos = gameObject.Translation + GameData.AmmoPipWorldOffset;

        var pipRect = Camera.WorldToScreenRectangle(
            pipWorldSpacePos,
            _fullAmmoPip.Value.Coords.Size.ToSizeF());

        if (!pipRect.HasValue)
        {
            return;
        }

        // todo: ammopipscreenoffset?

        for (var i = 0; i < currentRounds; i++)
        {
            var rect = pipRect.Value;
            drawingContext.DrawMappedImage(_fullAmmoPip.Value, rect.WithX(rect.X + xOffset));
            xOffset += pipWidth;
        }

        for (var i = 0; i < emptyRoundsToDraw; i++)
        {
            var rect = pipRect.Value;
            drawingContext.DrawMappedImage(_emptyAmmoPip.Value, rect.WithX(rect.X + xOffset));
            xOffset += pipWidth;
        }
    }
}

// shows text on-screen with the included offset, slowly moving up and fading out over 2.75 seconds
internal class CashAnimation(Camera camera, uint currentFrame, Font font, in Vector3 baseLocation, int amount, in ColorRgbaF baseColor) : TransientAnimation(camera, currentFrame)
{
    protected override uint FrameLength => (uint)(Game.LogicFramesPerSecond * 2.75f);

    private readonly Vector3 _baseLocation = baseLocation;
    private readonly string _text = $"{(amount < 0 ? "-" : string.Empty)}{MoneySymbol.Localize()}{amount}";
    private readonly ColorRgbaF _baseColor = baseColor;

    // this stuff seems to be hardcoded in the engine
    private static readonly LocalizedString MoneySymbol = new("GUI:MoneySymbol");
    private const int MoneyRiseHeight = 30; // unable to find this parameterized anywhere

    public override void DrawForFrame(DrawingContext2D drawingContext, uint currentFrame)
    {
        var progress = Progress(currentFrame);
        var zOffset = MoneyRiseHeight * progress;
        var opacity = progress < 0.75 ? 1 : (1 - progress) * 4; // there seems to be a falloff where we don't adjust opacity for the first bit

        var worldRectangle = Camera.WorldToScreenRectangle(_baseLocation with { Z = _baseLocation.Z + zOffset }, new SizeF(100)); // unsure what the correct size should be

        if (worldRectangle.HasValue)
        {
            var rect = worldRectangle.Value;
            drawingContext.DrawText(_text, font, OpenSage.Gui.TextAlignment.Center,new ColorRgbaF(0,0,0, opacity), rect.WithX(rect.X + 1).WithY(rect.Y + 1)); // drop shadow
            drawingContext.DrawText(_text, font, OpenSage.Gui.TextAlignment.Center,_baseColor.WithA(opacity), rect);
        }

        base.DrawForFrame(drawingContext, currentFrame);
    }
}

// Shows the rank-up animation rising above the character
internal class RankUpAnimation(Camera camera, uint currentFrame, GameData gameData, Animation animation, in Vector3 baseLocation) : TransientAnimation(camera, currentFrame)
{
    protected override uint FrameLength { get; } = (uint)(Game.LogicFramesPerSecond * gameData.LevelGainAnimationTime);
    private readonly float _zRise = gameData.LevelGainAnimationZRise;

    private readonly Vector3 _baseLocation = baseLocation;

    public override void DrawForFrame(DrawingContext2D drawingContext, uint currentFrame)
    {
        animation.SetFrame(currentFrame);
        var currentImage = animation.Current;

        var progress = Progress(currentFrame);
        var zOffset = _zRise * progress * 5; // multiplying by 5 feels better - not sure why

        var worldRectangle = Camera.WorldToScreenRectangle(_baseLocation with { Z = _baseLocation.Z + zOffset }, currentImage.Coords.Size.ToSizeF());

        if (worldRectangle.HasValue)
        {
            var rect = worldRectangle.Value;
            drawingContext.DrawMappedImage(currentImage, rect);
        }

        base.DrawForFrame(drawingContext, currentFrame);
    }
}
