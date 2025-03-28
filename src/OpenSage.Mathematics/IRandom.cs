using System;
using System.Runtime.CompilerServices;

namespace OpenSage.Mathematics;

public interface IRandom
{
    uint Seed { get; }

    void Initialize(uint seed);

    /// <summary>
    /// Returns a random integer that is within a specified range.
    /// </summary>
    /// <param name="lo">The inclusive lower bound of the random number returned.</param>
    /// <param name="hi">The inclusive upper bound of the random number returned.
    /// <paramref name="hi"/> must be greater than or equal to
    /// <paramref name="lo"/>.</param>
    /// <returns>
    /// A 32-bit signed integer greater than or equal to <paramref name="lo"/>
    /// and less than or equal to <paramref name="hi"/>; that is, the range of
    /// return values includes both <paramref name="lo"/> and <paramref name="hi"/>.
    /// If <paramref name="lo"/> equals <paramref name="hi"/>, <paramref name="lo"/>
    /// is returned.
    /// </returns>
    int Next(int lo, int hi);

    /// <summary>
    /// Returns a random floating-point number that is within a specified range.
    /// </summary>
    /// <param name="lo">The inclusive lower bound of the random number returned.</param>
    /// <param name="hi">The inclusive upper bound of the random number returned.
    /// <paramref name="hi"/> must be greater than or equal to
    /// <paramref name="lo"/>.</param>
    /// <returns>
    /// A 32-bit floating-point number greater than or equal to <paramref name="lo"/>
    /// and less than or equal to <paramref name="hi"/>; that is, the range of
    /// return values includes both <paramref name="lo"/> and <paramref name="hi"/>.
    /// If <paramref name="lo"/> equals <paramref name="hi"/>, <paramref name="lo"/>
    /// is returned.
    /// </returns>
    float NextSingle(float lo, float hi);
}

public sealed class SystemRandom : IRandom
{
    private Random _random;

    public uint Seed => throw new NotImplementedException();

    public SystemRandom()
    {
        _random = CreateRandom(0);
    }

    public void Initialize(uint seed)
    {
        _random = CreateRandom((int)seed);
    }

    private static Random CreateRandom(int seed) => new(seed);

    public int Next(int lo, int hi)
    {
        return _random.Next(lo, hi + 1);
    }

    public float NextSingle(float lo, float hi)
    {
        return lo + (hi - lo) * _random.NextSingle();
    }
}

public sealed class SageRandom : IRandom
{
    private static readonly float MultiplicationFactor = 1.0f / (MathF.Pow(2, 8 * sizeof(uint)) - 1.0f);

    private readonly uint[] _seed;
    private uint _baseSeed;

    public uint Seed => _baseSeed;

    public SageRandom()
    {
        _seed = new uint[6];

        Initialize(0);
    }

    public void Initialize(uint seed)
    {
        _baseSeed = seed;

        var ax = seed;
        ax += 0xf22d0e56;
        _seed[0] = ax;
        ax += unchecked(0x883126e9 - 0xf22d0e56);
        _seed[1] = ax;
        ax += 0xc624dd2f - 0x883126e9;
        _seed[2] = ax;
        ax += unchecked(0x0702c49c - 0xc624dd2f);
        _seed[3] = ax;
        ax += 0x9e353f7d - 0x0702c49c;
        _seed[4] = ax;
        ax += unchecked(0x6fdf3b64 - 0x9e353f7d);
        _seed[5] = ax;
    }

    public int Next(int lo, int hi)
    {
        var delta = (uint)(hi - lo + 1);

        if (delta == 0)
        {
            return hi;
        }

        return (int)((GetRandomValue() % delta) + lo);
    }

    public float NextSingle(float lo, float hi)
    {
        var delta = hi - lo;

        if (delta <= 0)
        {
            return hi;
        }

        return ((GetRandomValue() * MultiplicationFactor) * delta) + lo;
    }

    private uint GetRandomValue()
    {
        uint ax;
        uint c = 0;

        Adc(out ax, _seed[5], _seed[4], ref c);
        _seed[4] = ax;

        Adc(out ax, ax, _seed[3], ref c);
        _seed[3] = ax;

        Adc(out ax, ax, _seed[2], ref c);
        _seed[2] = ax;

        Adc(out ax, ax, _seed[1], ref c);
        _seed[1] = ax;

        Adc(out ax, ax, _seed[0], ref c);
        _seed[0] = ax;

        // Increment seed array, bubbling up the carries.
        if (++_seed[5] == 0
            && ++_seed[4] == 0
            && ++_seed[3] == 0
            && ++_seed[2] == 0
            && ++_seed[1] == 0)
        {
            ++_seed[0];
            ++ax;
        }

        return ax;
    }

    /// <summary>
    /// Add with carry. <paramref name="sum"/> is replaced with <paramref name="a"/> +
    /// <paramref name="b"/> + <paramref name="c"/>. <paramref name="c"/> is replaced with
    /// 1 if there was a carry, 0 if there wasn't. A carry occurred if the sum is less
    /// than one of the inputs. This is addition, so carry can never be more than 1.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void Adc(out uint sum, uint a, uint b, ref uint c)
    {
        sum = a + b + c;
        c = (sum < a || sum < b)
            ? 1u
            : 0u;
    }
}
