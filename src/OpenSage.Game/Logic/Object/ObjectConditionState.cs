using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class ObjectConditionState : ObjectDrawState
    {
        internal static ObjectConditionState ParseDefault(IniParser parser)
        {
            var result = parser.ParseBlock(FieldParseTable);

            result.ConditionFlags = new BitArray<ModelConditionFlag>(); // "NONE"

            return result;
        }

        internal static ObjectConditionState Parse(IniParser parser)
        {
            var conditionFlags = parser.ParseEnumBitArray<ModelConditionFlag>();

            var result = parser.ParseBlock(FieldParseTable);

            result.ConditionFlags = conditionFlags;

            return result;
        }

        private static readonly IniParseTable<ObjectConditionState> FieldParseTable = new IniParseTable<ObjectConditionState>
        {
            { "TransitionKey", (parser, x) => x.TransitionKey = parser.ParseIdentifier() },
        }.Concat<ObjectConditionState, ObjectDrawState>(BaseFieldParseTable);

        public BitArray<ModelConditionFlag> ConditionFlags { get; private set; }

        public string TransitionKey { get; private set; }

        public ObjectConditionState Clone(BitArray<ModelConditionFlag> conditionFlags)
        {
            return new ObjectConditionState
            {
                ConditionFlags = conditionFlags,

                Model = Model,

                WeaponRecoilBones = WeaponRecoilBones,
                WeaponFireFXBones = WeaponFireFXBones,
                WeaponMuzzleFlashes = WeaponMuzzleFlashes,
                WeaponLaunchBones = WeaponLaunchBones,

                Animations = Animations,
                AnimationMode = AnimationMode,
                AnimationSpeedFactorRange = AnimationSpeedFactorRange,
                IdleAnimations = IdleAnimations,
                TransitionKey = TransitionKey,
                Flags = Flags,

                Turret = Turret,
                TurretArtAngle = TurretArtAngle,

                HideSubObject = HideSubObject,
                ShowSubObject = ShowSubObject,
                ParticleSysBones = ParticleSysBones,
            };
        }
    }
}
