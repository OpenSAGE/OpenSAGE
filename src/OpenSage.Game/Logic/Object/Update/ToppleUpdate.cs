using System.Numerics;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.FX;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ToppleUpdate : UpdateModule
    {
        private readonly ToppleUpdateModuleData _moduleData;

        private ToppleState _toppleState;
        private float _toppleAcceleration;
        private float _toppleSpeed;
        private Vector3 _toppleDirection;
        private float _toppleAngle;
        private float _unknownFloat;
        private uint _stumpId;

        internal ToppleUpdate(ToppleUpdateModuleData moduleData)
        {
            _moduleData = moduleData;

            _toppleState = ToppleState.NotToppled;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            switch (_toppleState)
            {
                case ToppleState.Toppling:
                    {
                        // TODO: InitialAccelPercent
                        var deltaAngle = 0.01f;
                        _toppleAngle += deltaAngle;
                        context.GameObject.SetRotation(context.GameObject.Rotation * Quaternion.CreateFromYawPitchRoll(deltaAngle, 0, 0));
                        if (_toppleAngle > MathUtility.PiOver2)
                        {
                            _toppleAngle = MathUtility.PiOver2;
                            if (_moduleData.BounceVelocityPercent.IsZero)
                            {
                                _toppleState = ToppleState.Toppled;
                                KillObject(context);
                            }
                            else
                            {
                                _moduleData.BounceFX?.Value.Execute(context);
                                //_toppleState = ToppleState.Bouncing1Up;
                                // TODO
                            }
                        }
                        // TODO
                        break;
                    }
            }
        }

        internal override void OnCollide(BehaviorUpdateContext context, GameObject collidingObject)
        {
            // If we've already started toppling, don't do anything.
            if (_toppleState != ToppleState.NotToppled)
            {
                return;
            }

            // Only things with a CrusherLevel greater than our CrushableLevel, can topple us.
            if (collidingObject.Definition.CrusherLevel <= context.GameObject.Definition.CrushableLevel)
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

            _toppleState = ToppleState.Toppling;

            // TODO: Is this the right time to do this?
            context.GameObject.ModelConditionFlags.Set(ModelConditionFlag.Toppled, true);
        }

        private void CreateStump(BehaviorUpdateContext context)
        {
            if (_moduleData.StumpName == null)
            {
                return;
            }

            var stump = context.GameContext.GameObjects.Add(_moduleData.StumpName.Value, null);
            stump.UpdateTransform(context.GameObject.Translation, context.GameObject.Rotation);
            _stumpId = stump.ID;
        }

        private void KillObject(BehaviorUpdateContext context)
        {
            context.GameObject.Kill(DeathType.Toppled);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.PersistSingle(ref _toppleSpeed);
            reader.PersistSingle(ref _toppleAcceleration);
            reader.PersistVector3(ref _toppleDirection);
            reader.PersistEnum(ref _toppleState);
            reader.PersistSingle(ref _toppleAngle);
            reader.PersistSingle(ref _unknownFloat);

            reader.SkipUnknownBytes(9);

            reader.PersistUInt32(ref _stumpId);
        }

        private enum ToppleState
        {
            NotToppled = 0,
            Toppling   = 1,
            Toppled    = 2,
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
        public Percentage BounceVelocityPercent { get; private set; } = new Percentage(0.3f);
        public Percentage InitialAccelPercent { get; private set; } = new Percentage(0.01f);
        public LazyAssetReference<ObjectDefinition> StumpName { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new ToppleUpdate(this);
        }
    }
}
