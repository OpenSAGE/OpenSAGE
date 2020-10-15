using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class DestroyDie : DieModule
    {
        private readonly GameObject _gameObject;
        private readonly DestroyDieModuleData _moduleData;

        internal DestroyDie(GameObject gameObject, DestroyDieModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void OnDie(BehaviorUpdateContext context, DeathType deathType)
        {
            _gameObject.Destroy();
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

    public sealed class DestroyDieModuleData : DieModuleData
    {
        internal static DestroyDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DestroyDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<DestroyDieModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DestroyDie(gameObject, this);
        }
    }
}
