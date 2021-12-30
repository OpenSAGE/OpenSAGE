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
    public sealed class W3dProjectileStreamDraw : DrawModule
    {
        public override IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; } = Array.Empty<BitArray<ModelConditionFlag>>();

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

            base.Load(reader);
        }
    }

    /// <summary>
    /// Requires object to have KindOf = INERT.
    /// </summary>
    public sealed class W3dProjectileStreamDrawModuleData : DrawModuleData
    {
        internal static W3dProjectileStreamDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dProjectileStreamDrawModuleData> FieldParseTable = new IniParseTable<W3dProjectileStreamDrawModuleData>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "Width", (parser, x) => x.Width = parser.ParseFloat() },
            { "TileFactor", (parser, x) => x.TileFactor = parser.ParseFloat() },
            { "ScrollRate", (parser, x) => x.ScrollRate = parser.ParseFloat() },
            { "MaxSegments", (parser, x) => x.MaxSegments = parser.ParseInteger() },
        };

        public string Texture { get; private set; }
        public float Width { get; private set; }
        public float TileFactor { get; private set; }
        public float ScrollRate { get; private set; }
        public int MaxSegments { get; private set; }

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dProjectileStreamDraw();
        }
    }
}
