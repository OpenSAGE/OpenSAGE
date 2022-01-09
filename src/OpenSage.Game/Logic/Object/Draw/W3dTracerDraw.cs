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

namespace OpenSage.Logic.Object
{
    public class W3dTracerDraw : DrawModule
    {
        private readonly W3dTracerDrawModuleData _data;

        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates => Array.Empty<BitArray<ModelConditionFlag>>();

        internal W3dTracerDraw(W3dTracerDrawModuleData data, GameContext context)
        {
            _data = data;
        }

        internal override void Update(in TimeInterval gameTime)
        {
            // TODO: implement
        }

        internal override void BuildRenderList(RenderList renderList, Camera camera, bool castsShadow, MeshShaderResources.RenderItemConstantsPS renderItemConstantsPS, Dictionary<string, bool> shownSubObjects = null, Dictionary<string, bool> hiddenSubObjects = null)
        {
            // TODO: Implement
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }

        internal override string GetWeaponFireFXBone(WeaponSlot slot)
        {
            throw new NotImplementedException();
        }

        internal override string GetWeaponLaunchBone(WeaponSlot slot)
        {
            throw new NotImplementedException();

        }

        internal override void SetWorldMatrix(in Matrix4x4 worldMatrix)
        {
            throw new NotImplementedException();
        }

        internal override (ModelInstance, ModelBone) FindBone(string boneName)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Requires object to be KindOf = DRAWABLE_ONLY.
    /// </summary>
    public sealed class W3dTracerDrawModuleData : DrawModuleData
    {
        internal static W3dTracerDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dTracerDrawModuleData> FieldParseTable = new IniParseTable<W3dTracerDrawModuleData>();

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dTracerDraw(this, context);
        }
    }
}
