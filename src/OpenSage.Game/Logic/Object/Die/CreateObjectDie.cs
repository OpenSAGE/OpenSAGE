﻿using System.IO;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

namespace OpenSage.Logic.Object
{
    public sealed class CreateObjectDie : DieModule
    {
        private readonly CreateObjectDieModuleData _moduleData;

        internal CreateObjectDie(CreateObjectDieModuleData moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void OnDie(BehaviorUpdateContext context, DeathType deathType)
        {
            context.GameContext.ObjectCreationLists.Create(_moduleData.CreationList.Value, context);
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
