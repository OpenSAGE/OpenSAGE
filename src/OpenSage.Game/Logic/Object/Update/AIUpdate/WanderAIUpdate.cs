using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class WanderAIUpdate : AIUpdate
    {
        internal WanderAIUpdate(GameObject gameObject, WanderAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
        }

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);
        }
    }

    /// <summary>
    /// Allows this object to move randomly about its point of origin using a SET_WANDER locomotor.
    /// </summary>
    public sealed class WanderAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static WanderAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<WanderAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<WanderAIUpdateModuleData>());

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new WanderAIUpdate(gameObject, this);
        }
    }
}
