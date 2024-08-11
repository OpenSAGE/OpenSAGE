using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using OpenSage.Client;
using OpenSage.Data.Ini;
using OpenSage.Graphics;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;
using OpenSage.Graphics.Shaders;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class W3dRopeDraw : DrawModule
    {
        private float _unknownFloat1;
        private float _unknownFloat2;
        private float _width;
        private float _wobbleLength;
        private float _wobbleAmplitude;
        private float _unknownFloat3;
        private float _unknownFloat4;

        // TODO
        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates => Enumerable.Empty<BitArray<ModelConditionFlag>>();

        internal override void BuildRenderList(RenderList renderList, Camera camera, bool castsShadow, MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS, Dictionary<string, bool> shownSubObjects = null, Dictionary<string, bool> hiddenSubObjects = null)
        {
            // TODO
        }

        internal override (ModelInstance, ModelBone) FindBone(string boneName)
        {
            throw new System.NotImplementedException();
        }

        internal override string GetWeaponFireFXBone(WeaponSlot slot)
        {
            throw new System.NotImplementedException();
        }

        internal override string GetWeaponLaunchBone(WeaponSlot slot)
        {
            throw new System.NotImplementedException();
        }

        internal override void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            // TODO
        }

        internal override void Update(in TimeInterval time)
        {
            // TODO
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistSingle(ref _unknownFloat1);
            reader.PersistSingle(ref _unknownFloat2);
            reader.PersistSingle(ref _width);

            reader.SkipUnknownBytes(24);

            reader.PersistSingle(ref _wobbleLength);
            reader.PersistSingle(ref _wobbleAmplitude);
            reader.PersistSingle(ref _unknownFloat3);
            reader.PersistSingle(ref _unknownFloat4);

            reader.SkipUnknownBytes(4);
        }
    }

    /// <summary>
    /// Requires object to be KindOf = DRAWABLE_ONLY.
    /// </summary>
    public sealed class W3dRopeDrawModuleData : DrawModuleData
    {
        internal static W3dRopeDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dRopeDrawModuleData> FieldParseTable = new IniParseTable<W3dRopeDrawModuleData>();

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dRopeDraw();
        }
    }
}
