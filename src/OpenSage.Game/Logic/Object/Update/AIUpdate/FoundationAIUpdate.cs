using System.Linq;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class FoundationAIUpdate : AIUpdate
    {
        FoundationAIUpdateModuleData _moduleData;

        internal FoundationAIUpdate(GameObject gameObject, FoundationAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            var collidingObjects = context.GameContext.Quadtree.FindIntersecting(GameObject);

            foreach (var collidingObject in collidingObjects)
            {
                if (collidingObject.Definition.KindOf.Get(ObjectKinds.Structure))
                {
                    GameObject.IsSelectable = false;
                    GameObject.Hidden = true;
                    return;
                }
            }

            GameObject.IsSelectable = true;
            GameObject.Hidden = false;
        }
    }


    public sealed class FoundationAIUpdateModuleData : AIUpdateModuleData
    {
        internal new static FoundationAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<FoundationAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<FoundationAIUpdateModuleData>
            {
                { "BuildVariation", (parser, x) => x.BuildVariation = parser.ParseInteger() },
            });

        [AddedIn(SageGame.Bfme2)]
        public int BuildVariation { get; private set; }

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new FoundationAIUpdate(gameObject, this);
        }
    }
}
