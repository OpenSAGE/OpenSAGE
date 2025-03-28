using Xunit;

namespace OpenSage.Mathematics.Tests;

public class SageRandomTests
{
    [Fact]
    public void CanGenerateRandomIntegers()
    {
        var random = new SageRandom();

        Assert.Equal(5, random.Next(0, 5));
        Assert.Equal(1, random.Next(0, 5));
        Assert.Equal(4, random.Next(0, 5));
        Assert.Equal(4, random.Next(0, 5));
        Assert.Equal(5, random.Next(0, 5));
        Assert.Equal(2, random.Next(0, 5));
        Assert.Equal(1, random.Next(0, 5));
        Assert.Equal(0, random.Next(0, 5));
        Assert.Equal(2, random.Next(0, 5));
        Assert.Equal(5, random.Next(0, 5));
        Assert.Equal(2, random.Next(0, 5));
        Assert.Equal(0, random.Next(0, 5));
        Assert.Equal(5, random.Next(0, 5));
        Assert.Equal(3, random.Next(0, 5));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-10, 5)]
    public void GeneratesIntegerValuesInRequestedRange(int lo, int hi)
    {
        var random = new SageRandom();
        for (var i = 0; i < 1000; i++)
        {
            var value = random.Next(lo, hi);
            Assert.True(value >= lo && value <= hi);
        }
    }

    [Fact]
    public void CanGenerateRandomFloats()
    {
        var random = new SageRandom();

        Assert.Equal(1.67192996f, random.NextSingle(0, 5));
        Assert.Equal(0.767719746f, random.NextSingle(0, 5));
        Assert.Equal(4.53429937f, random.NextSingle(0, 5));
        Assert.Equal(2.31859875f, random.NextSingle(0, 5));
        Assert.Equal(2.48254776f, random.NextSingle(0, 5));
        Assert.Equal(4.58807659f, random.NextSingle(0, 5));
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-10, 5)]
    [InlineData(-1.5f, 3.5f)]
    public void GeneratesFloatValuesInRequestedRange(int lo, int hi)
    {
        var random = new SageRandom();
        for (var i = 0; i < 1000; i++)
        {
            var value = random.NextSingle(lo, hi);
            Assert.True(value >= lo && value <= hi);
        }
    }
}
