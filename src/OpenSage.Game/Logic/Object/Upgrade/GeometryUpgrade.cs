using System.Collections.Generic;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    internal class GeometryUpgrade : UpgradeModule
    {
        private readonly GeometryUpgradeModuleData _moduleData;

        internal GeometryUpgrade(GameObject gameObject, GeometryUpgradeModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        protected override void OnUpgrade()
        {
            if (_moduleData.ShowGeometry != null)
            {
                foreach (var showGeometry in _moduleData.ShowGeometry)
                {
                    _gameObject.ShowCollider(showGeometry);
                }
            }

            if (_moduleData.HideGeometry != null)
            {
                foreach (var hideGeometry in _moduleData.HideGeometry)
                {
                    _gameObject.HideCollider(hideGeometry);
                }
            }

            //TODO: WallBoundsMesh, RampMesh1, RampMesh2
        }
    }


    public sealed class GeometryUpgradeModuleData : UpgradeModuleData
    {
        internal static GeometryUpgradeModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<GeometryUpgradeModuleData> FieldParseTable = UpgradeModuleData.FieldParseTable
            .Concat(new IniParseTable<GeometryUpgradeModuleData>
            {
                { "ShowGeometry", (parser, x) => x.ShowGeometry = parser.ParseAssetReferenceArray() },
                { "HideGeometry", (parser, x) => x.HideGeometry = parser.ParseAssetReferenceArray() },
                { "WallBoundsMesh", (parser,x) => x.WallBoundsMesh = parser.ParseAssetReference() },
                { "RampMesh1", (parser, x) => x.RampMesh1 = parser.ParseAssetReference() },
                { "RampMesh2", (parser, x) => x.RampMesh2 = parser.ParseAssetReference() },
            });

        public string[] ShowGeometry { get; private set; }
        public string[] HideGeometry { get; private set; }
        public string WallBoundsMesh { get; private set; } // e.g. P4 where is that defined?
        public string RampMesh1 { get; private set; } // e.g. P2 where is that defined?
        public string RampMesh2 { get; private set; } // e.g. P3 where is that defined?

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new GeometryUpgrade(gameObject, this);
        }
    }
}
