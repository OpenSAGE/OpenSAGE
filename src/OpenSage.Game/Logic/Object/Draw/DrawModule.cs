﻿#nullable enable

using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object;

public abstract class DrawModule : ModuleBase
{
    public Drawable? Drawable { get; protected set; }
    public GameObject? GameObject => Drawable?.GameObject;
    public abstract IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; }
    public virtual BoundingSphere? BoundingSphere { get; }

    /// <summary>
    /// For limiting tree sway, etc to visible objects.
    /// TODO(Port): Implement this for W3dModelDraw.
    /// </summary>
    public virtual bool IsVisible => true;

    // TODO: Probably shouldn't have this here.
    internal abstract string GetWeaponFireFXBone(WeaponSlot slot);
    internal abstract string GetWeaponLaunchBone(WeaponSlot slot);

    public virtual void UpdateConditionState(BitArray<ModelConditionFlag> flags, IRandom random)
    {

    }

    public virtual void SetAnimationDuration(LogicFrameSpan frames) { }

    public virtual void SetSupplyBoxesRemaining(float boxPercentage) { }

    public virtual void SetTerrainDecal(ObjectDecalType decalType)
    {
        // TODO(Port): Implement this for subclasses.
    }

    internal abstract void Update(in TimeInterval time);

    internal abstract void SetWorldMatrix(in Matrix4x4 worldMatrix);

    internal abstract void BuildRenderList(
        RenderList renderList,
        Camera camera,
        bool castsShadow,
        MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS,
        Dictionary<string, bool>? shownSubObjects = null,
        Dictionary<string, bool>? hiddenSubObjects = null);

    internal abstract (ModelInstance, ModelBone) FindBone(string boneName);

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");

        // The following version number is probably from extra base class in the inheritance hierarchy.
        // Since we don't have that at the moment, just read it here.
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.EndObject();
    }
}

public abstract class DrawModuleData : ModuleData
{
    public override ModuleKinds ModuleKinds => ModuleKinds.Draw;

    internal static ModuleDataContainer ParseDrawModule(IniParser parser, ModuleInheritanceMode inheritanceMode) => ParseModule(parser, DrawModuleParseTable, inheritanceMode);

    internal static readonly Dictionary<string, Func<IniParser, DrawModuleData>> DrawModuleParseTable = new Dictionary<string, Func<IniParser, DrawModuleData>>
    {
        { "W3DBuffDraw", W3dBuffDrawModuleData.Parse },
        { "W3DDebrisDraw", W3dDebrisDrawModuleData.Parse },
        { "W3DDefaultDraw", W3dDefaultDrawModuleData.Parse },
        { "W3DDependencyModelDraw", W3dDependencyModelDrawModuleData.Parse },
        { "W3DFloorDraw", W3dFloorDrawModuleData.Parse },
        { "W3DHordeModelDraw", W3dHordeModelDrawModuleData.Parse },
        { "W3DLaserDraw", W3dLaserDrawModuleData.Parse },
        { "W3DLightDraw", W3dLightDrawModuleData.Parse },
        { "W3DModelDraw", W3dScriptedModelDrawModuleData.ParseModel },
        { "W3DOverlordAircraftDraw", W3dOverlordAircraftDraw.Parse },
        { "W3DOverlordTankDraw", W3dOverlordTankDrawModuleData.Parse },
        { "W3DOverlordTruckDraw", W3dOverlordTruckDrawModuleData.Parse },
        { "W3DPoliceCarDraw", W3dPoliceCarDrawModuleData.Parse },
        { "W3DProjectileStreamDraw", W3dProjectileStreamDrawModuleData.Parse },
        { "W3DPropDraw", W3dPropDrawModuleData.Parse },
        { "W3DQuadrupedDraw", W3dQuadrupedDrawModuleData.Parse },
        { "W3DRopeDraw", W3dRopeDrawModuleData.Parse },
        { "W3DSailModelDraw", W3dSailModelDrawModuleData.Parse },
        { "W3DScienceModelDraw", W3dScienceModelDrawModuleData.Parse },
        { "W3DScriptedModelDraw", W3dScriptedModelDrawModuleData.Parse },
        { "W3DStreakDraw", W3dStreakDrawModuleData.Parse },
        { "W3DSupplyDraw", W3dSupplyDrawModuleData.Parse },
        { "W3DTankDraw", W3dTankDrawModuleData.Parse },
        { "W3DTankTruckDraw", W3dTankTruckDrawModuleData.Parse },
        { "W3DTornadoDraw", W3dTornadoDrawModuleData.Parse },
        { "W3DTracerDraw", W3dTracerDrawModuleData.Parse },
        { "W3DTreeDraw", W3dTreeDrawModuleData.Parse },
        { "W3DTruckDraw", W3dTruckDrawModuleData.Parse },
    };

    internal virtual DrawModule? CreateDrawModule(Drawable drawable, IGameEngine gameEngine) => null; // TODO: Make this abstract.
}
