using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    public sealed class ObjectFilter
    {
        internal static ObjectFilter Parse(IniParser parser)
        {
            var result = new ObjectFilter();

            IniToken? token;
            var stringToValueMap = IniParser.GetEnumMap<ObjectKinds>();

            var includeThings = new List<string>();
            var excludeThings = new List<string>();

            while ((token = parser.GetNextTokenOptional()) != null)
            {
                var stringValue = token.Value.Text.ToUpperInvariant();

                switch (stringValue)
                {
                    case "ALL":
                        result.Rules.Set(ObjectFilterRule.All, true);
                        continue;

                    case "NONE":
                        result.Rules.Set(ObjectFilterRule.None, true);
                        continue;

                    case "ANY":
                        result.Rules.Set(ObjectFilterRule.Any, true);
                        continue;

                    case "ALLIES":
                        result.Rules.Set(ObjectFilterRule.Allies, true);
                        continue;

                    case "ENEMIES":
                        result.Rules.Set(ObjectFilterRule.Enemies, true);
                        continue;

                    case "NEUTRAL":
                        result.Rules.Set(ObjectFilterRule.Neutrals, true);
                        continue;

                    case "NEUTRALS":
                        result.Rules.Set(ObjectFilterRule.Neutrals, true);
                        continue;

                    case "NOT_SIMILAR":
                        result.Rules.Set(ObjectFilterRule.NotSimilar, true);
                        continue;

                    case "SELF":
                        result.Rules.Set(ObjectFilterRule.Self, true);
                        continue;

                    case "SUICIDE":
                        result.Rules.Set(ObjectFilterRule.Suicide, true);
                        continue;

                    case "NOT_AIRBORNE":
                        result.Rules.Set(ObjectFilterRule.NotAirborne, true);
                        continue;

                    case "SAME_HEIGHT_ONLY":
                        result.Rules.Set(ObjectFilterRule.SameHeightOnly, true);
                        continue;

                    case "MINES":
                        result.Rules.Set(ObjectFilterRule.Mines, true);
                        continue;

                    case "SAME_PLAYER":
                        result.Rules.Set(ObjectFilterRule.SamePlayer, true);
                        continue;
                }

                bool isInclude;
                switch (stringValue[0])
                {
                    case '+':
                        isInclude = true;
                        break;

                    case '-':
                        isInclude = false;
                        break;

                    default:
                        throw new IniParseException($"Expected {stringValue} to have a + or - prefix", token.Value.Position);
                }

                stringValue = stringValue.Substring(1);

                if (stringToValueMap.TryGetValue(stringValue, out var enumValue))
                {
                    var bitArray = isInclude ? result.Include : result.Exclude;
                    bitArray.Set((ObjectKinds) enumValue, true);
                }
                else
                {
                    var list = isInclude ? includeThings : excludeThings;
                    list.Add(stringValue);
                }
            }

            result.IncludeThings = includeThings;
            result.ExcludeThings = excludeThings;

            return result;
        }

        public BitArray<ObjectFilterRule> Rules { get; } = new BitArray<ObjectFilterRule>();
        public BitArray<ObjectKinds> Include { get; } = new BitArray<ObjectKinds>();
        public BitArray<ObjectKinds> Exclude { get; } = new BitArray<ObjectKinds>();
        public IReadOnlyList<string> IncludeThings { get; private set; }
        public IReadOnlyList<string> ExcludeThings { get; private set; }
    }

    public enum ObjectFilterRule
    {
        All,
        Any,
        None,
        Enemies,
        Allies,
        Neutrals,
        SamePlayer,
        NotSimilar,
        Self,
        Suicide,
        NotAirborne,
        SameHeightOnly,
        Mines
    }
}
