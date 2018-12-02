using System.Collections.Generic;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class ObjectFilter
    {
        internal static ObjectFilter Parse(IniParser parser)
        {
            var result = new ObjectFilter();
            result.Target = ObjectFilterTarget.All;

            IniToken? token;
            if ((token = parser.GetNextTokenOptional()) != null)
            {
                var stringValue = token.Value.Text.ToUpperInvariant();
                switch (stringValue)
                {
                    case "ALL":
                        result.Rule = ObjectFilterRule.All;
                        break;

                    case "NONE":
                        result.Rule = ObjectFilterRule.None;
                        break;

                    case "ANY":
                        result.Rule = ObjectFilterRule.Any;
                        break;

                    default:
                        throw new IniParseException($"Expected one of ALL, NONE, or ANY", token.Value.Position);
                }
            }

            var stringToValueMap = IniParser.GetEnumMap<ObjectKinds>();

            var includeThings = new List<string>();
            var excludeThings = new List<string>();

            while ((token = parser.GetNextTokenOptional()) != null)
            {
                var stringValue = token.Value.Text.ToUpperInvariant();

                switch (stringValue)
                {
                    case "ENEMIES":
                        result.Target = ObjectFilterTarget.Enemies;
                        continue;
                    case "ALLIES":
                        result.Target = ObjectFilterTarget.Allies;
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
                        throw new IniParseException($"Expected value to have a + or - prefix", token.Value.Position);
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

        public ObjectFilterRule Rule { get; private set; }
        public ObjectFilterTarget Target { get; private set; }
        public BitArray<ObjectKinds> Include { get; } = new BitArray<ObjectKinds>();
        public BitArray<ObjectKinds> Exclude { get; } = new BitArray<ObjectKinds>();
        public IReadOnlyList<string> IncludeThings { get; private set; }
        public IReadOnlyList<string> ExcludeThings { get; private set; }
    }

    public enum ObjectFilterRule
    {
        All,
        Any,
        None
    }

    public enum ObjectFilterTarget
    {
        All,
        Enemies,
        Allies
    }
}
