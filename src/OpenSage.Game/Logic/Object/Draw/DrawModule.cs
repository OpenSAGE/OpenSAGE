using System;
using System.Collections.Generic;
using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.ParticleSystems;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Logic.Object
{
    public abstract class DrawModule : DisposableBase
    {
        public abstract IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; }

        internal virtual IEnumerable<AttachedParticleSystem> GetAllAttachedParticleSystems()
        {
            yield break;
        }

        public virtual void UpdateConditionState(BitArray<ModelConditionFlag> flags)
        {

        }

        internal abstract void Update(GameTime gameTime);

        internal abstract void SetWorldMatrix(in Matrix4x4 worldMatrix);

        internal abstract void BuildRenderList(RenderList renderList, CameraComponent camera);
    }

    public abstract class DrawModuleData : ModuleData
    {
        internal static DrawModuleData ParseDrawModule(IniParser parser) => ParseModule(parser, DrawModuleParseTable);

        internal static readonly Dictionary<string, Func<IniParser, DrawModuleData>> DrawModuleParseTable = new Dictionary<string, Func<IniParser, DrawModuleData>>
        {
            { "W3DDebrisDraw", W3dDebrisDrawModuleData.Parse },
            { "W3DDefaultDraw", W3dDefaultDrawModuleData.Parse },
            { "W3DDependencyModelDraw", W3dDependencyModelDrawModuleData.Parse },
            { "W3DLaserDraw", W3dLaserDrawModuleData.Parse },
            { "W3DLightDraw", W3dLightDrawModuleData.Parse },
            { "W3DModelDraw", W3dModelDrawModuleData.ParseModel },
            { "W3DOverlordAircraftDraw", W3dOverlordAircraftDraw.Parse },
            { "W3DOverlordTankDraw", W3dOverlordTankDrawModuleData.Parse },
            { "W3DOverlordTruckDraw", W3dOverlordTruckDrawModuleData.Parse },
            { "W3DPoliceCarDraw", W3dPoliceCarDrawModuleData.Parse },
            { "W3DProjectileStreamDraw", W3dProjectileStreamDrawModuleData.Parse },
            { "W3DRopeDraw", W3dRopeDrawModuleData.Parse },
            { "W3DScienceModelDraw", W3dScienceModelDrawModuleData.Parse },
            { "W3DScriptedModelDraw", W3dScriptedModelDrawModuleData.Parse },
            { "W3DSupplyDraw", W3dSupplyDrawModuleData.Parse },
            { "W3DTankDraw", W3dTankDrawModuleData.Parse },
            { "W3DTankTruckDraw", W3dTankTruckDrawModuleData.Parse },
            { "W3DTracerDraw", W3dTracerDrawModuleData.Parse },
            { "W3DTreeDraw", W3dTreeDrawModuleData.Parse },
            { "W3DTruckDraw", W3dTruckDrawModuleData.Parse },
        };

        internal virtual DrawModule CreateDrawModule(ContentManager contentManager) => null; // TODO: Make this abstract.
    }
}
