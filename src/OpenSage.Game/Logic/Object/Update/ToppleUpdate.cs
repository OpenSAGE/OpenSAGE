using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ToppleUpdate : UpdateModule
    {
        private readonly ToppleUpdateModuleData _moduleData;

        private ToppleState _state;
        private float _toppleAngle;

        internal ToppleUpdate(ToppleUpdateModuleData moduleData)
        {
            _moduleData = moduleData;

            _state = ToppleState.NotToppled;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            switch (_state)
            {
                case ToppleState.Toppling:
                    _toppleAngle += 0.01f * (float)context.Time.DeltaTime.TotalSeconds;
                    if (_toppleAngle > MathUtility.PiOver2)
                    {
                        // TODO: Bouncing
                        _state = ToppleState.Toppled;
                        KillObject(context);
                    }
                    // TODO
                    break;

                case ToppleState.Bouncing:
                    // TODO
                    // TODO: BounceFX
                    // TODO: BounceVelocityPercent
                    break;
            }

            // TODO: InitialAccelPercent
        }

        internal override void OnCollide(BehaviorUpdateContext context, GameObject collidingObject)
        {
            // If we've already started toppling, don't do anything.
            if (_state != ToppleState.NotToppled)
            {
                return;
            }

            // Only vehicles can topple things.
            // TODO: Is this right?
            if (!collidingObject.Definition.KindOf.Get(ObjectKinds.Vehicle))
            {
                return;
            }

            // TODO: Use colliding object's position (and velocity?) to decide topple direction.
            // TODO: ToppleLeftOrRightOnly
            StartTopple(context);
        }

        private void StartTopple(BehaviorUpdateContext context)
        {
            _moduleData.ToppleFX?.Value.Execute(context);

            CreateStump(context);

            if (_moduleData.KillWhenStartToppling)
            {
                KillObject(context);
                return;
            }

            _state = ToppleState.Toppling;
        }

        private void CreateStump(BehaviorUpdateContext context)
        {
            if (_moduleData.StumpName == null)
            {
                return;
            }

            var stump = context.GameContext.GameObjects.Add(_moduleData.StumpName.Value);

            stump.Transform.Translation = context.GameObject.Transform.Translation;
            stump.Transform.Rotation = context.GameObject.Transform.Rotation;
        }

        private void KillObject(BehaviorUpdateContext context)
        {
            context.GameObject.Kill(DeathType.Toppled, context.Time);
        }

        private enum ToppleState
        {
            NotToppled,
            Toppling,
            Bouncing,
            Toppled,
        }
    }

    public sealed class ToppleUpdateModuleData : UpdateModuleData
    {
        internal static ToppleUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ToppleUpdateModuleData> FieldParseTable = new IniParseTable<ToppleUpdateModuleData>
        {
            { "ToppleFX", (parser, x) => x.ToppleFX = parser.ParseFXListReference() },
            { "BounceFX", (parser, x) => x.BounceFX = parser.ParseFXListReference() },
            { "KillWhenStartToppling", (parser, x) => x.KillWhenStartToppling = parser.ParseBoolean() },
            { "ToppleLeftOrRightOnly", (parser, x) => x.ToppleLeftOrRightOnly = parser.ParseBoolean() },
            { "ReorientToppledRubble", (parser, x) => x.ReorientToppledRubble = parser.ParseBoolean() },
            { "BounceVelocityPercent", (parser, x) => x.BounceVelocityPercent = parser.ParsePercentage() },
            { "InitialAccelPercent", (parser, x) => x.InitialAccelPercent = parser.ParsePercentage() },
            { "StumpName", (parser, x) => x.StumpName = parser.ParseObjectReference() },
        };

        public LazyAssetReference<FXList> ToppleFX { get; private set; }
        public LazyAssetReference<FXList> BounceFX { get; private set; }
        public bool KillWhenStartToppling { get; private set; }
        public bool ToppleLeftOrRightOnly { get; private set; }
        public bool ReorientToppledRubble { get; private set; }
        public Percentage BounceVelocityPercent { get; private set; } = new Percentage(0.2f);
        public Percentage InitialAccelPercent { get; private set; } = new Percentage(0.01f);
        public LazyAssetReference<ObjectDefinition> StumpName { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ToppleUpdate(this);
        }
    }
}
