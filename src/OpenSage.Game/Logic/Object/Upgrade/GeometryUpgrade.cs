using System.Collections.Generic;
using System.Linq;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    internal class GeometryUpgrade : UpgradeModule
    {
        private readonly GeometryUpgradeModuleData _moduleData;
        private readonly List<Geometry> _allGeometries;

        internal GeometryUpgrade(GameObject gameObject, GeometryUpgradeModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;

            _allGeometries = new List<Geometry>
            {
                _gameObject.Definition.Geometry
            };
            _allGeometries.AddRange(_gameObject.Definition.AdditionalGeometries);
            _allGeometries.AddRange(_gameObject.Definition.OtherGeometries);
        }

        internal override void OnTrigger(BehaviorUpdateContext context, bool triggered)
        {
            if (_moduleData.ShowGeometry != null)
            {
                foreach (var showGeometry in _moduleData.ShowGeometry)
                {
                    ShowCollider(context, showGeometry);
                }
            }
            
            if (_moduleData.HideGeometry != null)
            {
                foreach (var hideGeometry in _moduleData.HideGeometry)
                {
                    HideCollider(context, hideGeometry);
                }
            }

            //TODO: WallBoundsMesh, RampMesh1, RampMesh2
        }

        private void ShowCollider(BehaviorUpdateContext context, string name)
        {
            if (_gameObject.Colliders.Any(x => x.Name.Equals(name)))
            {
                return;
            }

            var newColliders = new List<Collider>();
            foreach (var geometry in _allGeometries)
            {
                if (geometry.Name.Equals(name))
                {
                    newColliders.Add(Collider.Create(geometry, _gameObject.Transform));
                }
            }

            if (_gameObject.AffectsAreaPassability)
            {
                foreach (var collider in newColliders)
                {
                    context.GameContext.Navigation.UpdateAreaPassability(collider, false);
                }
            }
            _gameObject.Colliders.AddRange(newColliders);
            _gameObject.RoughCollider = Collider.Create(_gameObject.Colliders);
            context.GameContext.Quadtree.Update(_gameObject);
        }

        private void HideCollider(BehaviorUpdateContext context, string name)
        {
            if (!_gameObject.Colliders.Any(x => x.Name.Equals(name)))
            {
                return;
            }
            
            for (var i = _gameObject.Colliders.Count - 1; i >= 0; i--)
            {
                var collider = _gameObject.Colliders[i];
                if (!collider.Name.Equals(name))
                {
                    continue;
                }
                if (_gameObject.AffectsAreaPassability)
                {
                    context.GameContext.Navigation.UpdateAreaPassability(collider, true);
                }
                _gameObject.Colliders.RemoveAt(i);
            }
            _gameObject.RoughCollider = Collider.Create(_gameObject.Colliders);
            context.GameContext.Quadtree.Update(_gameObject);
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
