using System;
using System.Collections.Generic;
using System.Linq;
using OpenSage.Client;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class W3dSupplyDraw : W3dModelDraw
    {
        private readonly Dictionary<string, int> _boxBoneMap;
        private readonly string _bonePrefix;

        internal W3dSupplyDraw(W3dSupplyDrawModuleData data, Drawable drawable, GameContext context)
            : base(data, drawable, context)
        {
            _bonePrefix = data.SupplyBonePrefix;
            _boxBoneMap = string.IsNullOrEmpty(data.SupplyBonePrefix)
                ? new Dictionary<string, int>()
                : ActiveModelInstance.Model.BoneHierarchy.Bones.Where(b => b.Name.StartsWith(data.SupplyBonePrefix))
                    .ToDictionary(b => b.Name, b => b.Index);
        }

        public void SetBoxesRemaining(float boxPercentage)
        {
            var totalBones = _boxBoneMap.Count;

            // use ceiling so we don't hide all the boxes when there are still supplies left
            var boxesRemaining = (int) Math.Ceiling(totalBones * boxPercentage);
            for (var i = totalBones; i > boxesRemaining; i--)
            {
                if (_boxBoneMap.TryGetValue($"{_bonePrefix}{i:00}", out var boneIndex))
                {
                    ActiveModelInstance.BoneVisibilities[boneIndex] = false;
                }
            }
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    public sealed class W3dSupplyDrawModuleData : W3dModelDrawModuleData
    {
        internal static W3dSupplyDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<W3dSupplyDrawModuleData> FieldParseTable = W3dModelDrawModuleData.FieldParseTable
            .Concat(new IniParseTable<W3dSupplyDrawModuleData>
            {
                { "SupplyBonePrefix", (parser, x) => x.SupplyBonePrefix = parser.ParseString() }
            });

        public string SupplyBonePrefix { get; private set; }

        internal override DrawModule CreateDrawModule(Drawable drawable, GameContext context)
        {
            return new W3dSupplyDraw(this, drawable, context);
        }
    }
}
