using OpenSage.Scripting;
using Xunit;

namespace OpenSage.Tests.Scripting
{
    public class ScriptConditionsTests : IClassFixture<GameFixture>
    {
        private readonly GameFixture _fixture;

        public ScriptConditionsTests(GameFixture fixture)
        {
            _fixture = fixture;
        }

        [Theory(Skip = "Only works locally")]
        [InlineData(ScriptingComparison.EqualTo, 5, true)]
        [InlineData(ScriptingComparison.EqualTo, 4, false)]
        [InlineData(ScriptingComparison.NotEqual, 5, false)]
        [InlineData(ScriptingComparison.NotEqual, 4, true)]
        [InlineData(ScriptingComparison.LessOrEqual, 5, true)]
        [InlineData(ScriptingComparison.LessOrEqual, 4, false)]
        [InlineData(ScriptingComparison.LessOrEqual, 6, true)]
        [InlineData(ScriptingComparison.LessThan, 5, false)]
        [InlineData(ScriptingComparison.LessThan, 4, false)]
        [InlineData(ScriptingComparison.LessThan, 6, true)]
        [InlineData(ScriptingComparison.GreaterOrEqual, 5, true)]
        [InlineData(ScriptingComparison.GreaterOrEqual, 4, true)]
        [InlineData(ScriptingComparison.GreaterOrEqual, 6, false)]
        [InlineData(ScriptingComparison.GreaterThan, 5, false)]
        [InlineData(ScriptingComparison.GreaterThan, 4, true)]
        [InlineData(ScriptingComparison.GreaterThan, 6, false)]
        public void TestCounter(ScriptingComparison comparison, int compareValue, bool expectedResult)
        {
            var game = _fixture.GetGame(SageGame.CncGenerals);

            game.Scripting.SetCounterValue("MyCounter", 5);

            var condition = new ScriptCondition(
                ScriptConditionType.Counter,
                new ScriptArgument(ScriptArgumentType.CounterName, "MyCounter"),
                new ScriptArgument(ScriptArgumentType.Comparison, (int) comparison),
                new ScriptArgument(ScriptArgumentType.Integer, compareValue));

            var result = game.Scripting.EvaluateScriptCondition(condition);

            Assert.Equal(expectedResult, result);
        }
    }
}
