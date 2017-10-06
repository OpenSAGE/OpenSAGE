namespace OpenSage.Scripting.Conditions
{
    public sealed class MapScriptConditions
    {
        private readonly MapScriptOrCondition[] _orConditions;

        public MapScriptConditions(MapScriptOrCondition[] orConditions)
        {
            _orConditions = orConditions;
        }

        public bool Evaluate(ScriptExecutionContext context)
        {
            foreach (var orCondition in _orConditions)
            {
                var conditionValue = orCondition.Evaluate(context);

                if (conditionValue)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
