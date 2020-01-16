using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content.Loaders;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class DrawModule : DisposableBase
    {
        public abstract IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; }

        // TODO: Probably shouldn't have this here.
        internal abstract string GetWeaponFireFXBone(WeaponSlot slot);

        internal virtual IEnumerable<AttachedParticleSystem> GetAllAttachedParticleSystems()
        {
            yield break;
        }

        public virtual void UpdateConditionState(BitArray<ModelConditionFlag> flags)
        {

        }

        internal abstract void Update(in TimeInterval time, GameObject gameObject);

        internal abstract void SetWorldMatrix(in Matrix4x4 worldMatrix);

        internal abstract void BuildRenderList(RenderList renderList, Camera camera, bool castsShadow, MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS);

        internal abstract (ModelInstance, ModelBone) FindBone(string boneName);
    }

    public abstract class DrawModuleData : ModuleData
    {
        internal static DrawModuleData ParseDrawModule(IniParser parser) => ParseModule(parser, DrawModuleParseTable);

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
            { "W3DModelDraw", W3dModelDrawModuleData.ParseModel },
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

        internal virtual DrawModule CreateDrawModule(AssetLoadContext loadContext) => null; // TODO: Make this abstract.
    }
}
