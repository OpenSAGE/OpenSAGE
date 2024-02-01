﻿using OpenSage.Content;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class CreateObjectDie : DieModule
    {
        private new CreateObjectDieModuleData ModuleData { get; }

        internal CreateObjectDie(CreateObjectDieModuleData moduleData) : base(moduleData)
        {
            ModuleData = moduleData;
        }

        private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
        {
            context.GameContext.ObjectCreationLists.Create(ModuleData.CreationList.Value, context);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    public sealed class CreateObjectDieModuleData : DieModuleData
    {
        internal static CreateObjectDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CreateObjectDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<CreateObjectDieModuleData>
            {
                { "CreationList", (parser, x) => x.CreationList = parser.ParseObjectCreationListReference() },
                { "TransferPreviousHealth", (parser, x) => x.TransferPreviousHealth = parser.ParseBoolean() },
                { "DebrisPortionOfSelf", (parser, x) => x.DebrisPortionOfSelf = parser.ParseAssetReference() },
                { "UpgradeRequired", (parser, x) => x.UpgradeRequired = parser.ParseAssetReferenceArray() }
            });

        public LazyAssetReference<ObjectCreationList> CreationList { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public bool TransferPreviousHealth { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string DebrisPortionOfSelf { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string[] UpgradeRequired { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new CreateObjectDie(this);
        }
    }
}
