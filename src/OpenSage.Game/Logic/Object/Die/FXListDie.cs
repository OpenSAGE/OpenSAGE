using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;
using OpenSage.FX;

namespace OpenSage.Logic.Object
{
    public sealed class FXListDie : DieModule
    {
        private readonly FXListDieModuleData _moduleData;

        internal FXListDie(FXListDieModuleData moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void OnDie(BehaviorUpdateContext context, DeathType deathType)
        {
            _moduleData.DeathFX.Value.Execute(new FXListExecutionContext(
                context.GameObject.Transform.Rotation,
                context.GameObject.Transform.Translation,
                context.GameContext));
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

    public sealed class FXListDieModuleData : DieModuleData
    {
        internal static FXListDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FXListDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<FXListDieModuleData>
            {
                { "DeathFX", (parser, x) => x.DeathFX = parser.ParseFXListReference() },
                { "OrientToObject", (parser, x) => x.OrientToObject = parser.ParseBoolean() },
                { "StartsActive", (parser, x) => x.StartsActive = parser.ParseBoolean() },
                { "ConflictsWith", (parser, x) => x.ConflictsWith = parser.ParseAssetReferenceArray() },
                { "TriggeredBy", (parser, x) => x.TriggeredBy = parser.ParseAssetReferenceArray() }
            });

        public LazyAssetReference<FXList> DeathFX { get; private set; }
        public bool OrientToObject { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool StartsActive { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string[] ConflictsWith { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public string[] TriggeredBy { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new FXListDie(this);
        }
    }
}
