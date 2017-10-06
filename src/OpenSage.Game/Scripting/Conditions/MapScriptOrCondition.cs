namespace OpenSage.Scripting.Conditions
{
    public sealed class MapScriptOrCondition
    {
        private readonly MapScriptCondition[] _andConditions;

        public MapScriptOrCondition(MapScriptCondition[] andConditions)
        {
            _andConditions = andConditions;
        }

        public bool Evaluate(ScriptExecutionContext context)
        {
            var result = false;

            foreach (var andCondition in _andConditions)
            {
                var conditionValue = andCondition.Evaluate(context);

                if (conditionValue)
                {
                    result = true;
                }
                else
                {
                    return false;
                }
            }

            return result;
        }
    }
}
