using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public class FoundationAIUpdate : AIUpdate
    {
        private readonly FoundationAIUpdateModuleData _moduleData;
        private LogicFrame _waitUntil;
        private LogicFrameSpan _updateInterval;

        //TODO: rather notify this when the corresponding order is processed and update again when the object is dead/destroyed
        internal FoundationAIUpdate(GameObject gameObject, FoundationAIUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
            _updateInterval = new LogicFrameSpan((uint)MathF.Ceiling(Game.LogicFramesPerSecond / 2)); // 0.5s, we do not have to check every frame
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            CheckForStructure(context, GameObject, ref _waitUntil, _updateInterval);
        }

        internal static void CheckForStructure(BehaviorUpdateContext context, GameObject obj, ref LogicFrame waitUntil, LogicFrameSpan interval)
        {
            if (context.LogicFrame < waitUntil)
            {
                return;
            }

            waitUntil = context.LogicFrame + interval;

            var collidingObjects = context.GameContext.Quadtree.FindNearby(obj, obj.Transform, obj.RoughCollider.WorldBounds.Radius);

            foreach (var collidingObject in collidingObjects)
            {
                if (collidingObject.Definition.KindOf.Get(ObjectKinds.Structure))
                {
                    obj.IsSelectable = false;
                    obj.Hidden = true;
                    return;
                }
            }

            obj.IsSelectable = true;
            obj.Hidden = false;
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
