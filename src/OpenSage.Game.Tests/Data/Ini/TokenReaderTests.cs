using OpenSage.Data.Ini.Parser;
using Xunit;

namespace OpenSage.Tests.Data.Ini
{
    public class TokenReaderTests
    {
        private static readonly char[] Spaces = {' '};
        private static readonly char[] Semicolons = {';'};

        [Fact]
        public void TwoPeeksReturnSameInstance()
        {
            var reader = new TokenReader("TokenA TokenB", "test.ini");
            reader.GoToNextLine();

            var tokenA = reader.PeekToken(Spaces);
            var tokenB = reader.PeekToken(Spaces);
            Assert.StrictEqual(tokenA, tokenB);
        }

        [Fact]
        public void PeekMaintainsCurrentPosition()
        {
            var reader = new TokenReader("TokenA TokenB", "test.ini");
            reader.GoToNextLine();

            Assert.Equal(1, reader.CurrentPosition.Character);
            var next = reader.PeekToken(Spaces);
            Assert.Equal("TokenA", next.Value.Text);
            Assert.Equal(1, reader.CurrentPosition.Character);
        }

        [Fact]
        public void NextTokenAdvancesPosition()
        {
            var reader = new TokenReader("TokenA TokenB", "test.ini");
            reader.GoToNextLine();

            Assert.Equal(1, reader.CurrentPosition.Character);
            reader.NextToken(Spaces);
            Assert.NotEqual(1, reader.CurrentPosition.Character);
        }

        [Fact]
        public void PeekWithDifferentSeparatorsInvalidatesBufferedToken()
        {
            var reader = new TokenReader("TokenA TokenB;TokenC TokenD", "test.ini");
            reader.GoToNextLine();

            var untilSpace = reader.PeekToken(Spaces);
            var untilSemicolon = reader.PeekToken(Semicolons);

            Assert.Equal("TokenA", untilSpace.Value.Text);
            Assert.Equal("TokenA TokenB", untilSemicolon.Value.Text);
        }

        [Fact]
        public void NextWithDifferentSeparatorsInvalidatesBufferedToken()
        {
            var reader = new TokenReader("TokenA TokenB;TokenC TokenD", "test.ini");
            reader.GoToNextLine();

            var untilSpace = reader.PeekToken(Spaces);
            var untilSemicolon = reader.NextToken(Semicolons);

            Assert.Equal("TokenA", untilSpace.Value.Text);
            Assert.Equal("TokenA TokenB", untilSemicolon.Value.Text);
        }
    }
}
