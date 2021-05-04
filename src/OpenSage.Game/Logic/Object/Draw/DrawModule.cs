using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public abstract class DrawModule : ModuleBase
    {
        public Drawable Drawable { get; protected set; }
        public GameObject GameObject => Drawable.GameObject;
        public abstract IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; }

        // TODO: Probably shouldn't have this here.
        internal abstract string GetWeaponFireFXBone(WeaponSlot slot);
        internal abstract string GetWeaponLaunchBone(WeaponSlot slot);

        public virtual void UpdateConditionState(BitArray<ModelConditionFlag> flags, Random random)
        {

        }

        internal abstract void Update(in TimeInterval time);

        internal abstract void SetWorldMatrix(in Matrix4x4 worldMatrix);

        internal abstract void BuildRenderList(
            RenderList renderList,
            Camera camera,
            bool castsShadow,
            MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS,
            Dictionary<string, bool> shownSubObjects = null,
            Dictionary<string, bool> hiddenSubObjects = null);

        internal abstract (ModelInstance, ModelBone) FindBone(string boneName);

        internal virtual void DrawInspector() { }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            // The following version number is probably from extra base class in the inheritance hierarchy.
            // Since we don't have that at the moment, just read it here.

            var extraVersion = reader.ReadVersion();
            if (extraVersion != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
        }
    }

    public abstract class DrawModuleData : ModuleData
    {
        public override ModuleKind ModuleKind => ModuleKind.Draw;

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

        internal virtual DrawModule CreateDrawModule(Drawable drawable, GameContext context) => null; // TODO: Make this abstract.
    }
}
