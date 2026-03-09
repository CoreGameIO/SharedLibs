using CoreGame.FixedPoint;
using Xunit;

namespace CoreGame.FixedPoint.Tests;

public class FpMathTests
{
    [Fact]
    public void Max()
    {
        Assert.Equal(Fp.FromInt(5), FpMath.Max(Fp.FromInt(3), Fp.FromInt(5)));
        Assert.Equal(Fp.FromInt(5), FpMath.Max(Fp.FromInt(5), Fp.FromInt(3)));
    }

    [Fact]
    public void Min()
    {
        Assert.Equal(Fp.FromInt(3), FpMath.Min(Fp.FromInt(3), Fp.FromInt(5)));
        Assert.Equal(Fp.FromInt(3), FpMath.Min(Fp.FromInt(5), Fp.FromInt(3)));
    }

    [Fact]
    public void Abs()
    {
        Assert.Equal(Fp.FromInt(5), FpMath.Abs(Fp.FromInt(5)));
        Assert.Equal(Fp.FromInt(5), FpMath.Abs(Fp.FromInt(-5)));
        Assert.Equal(Fp.Zero, FpMath.Abs(Fp.Zero));
    }

    [Fact]
    public void Sign()
    {
        Assert.Equal(Fp.One, FpMath.Sign(Fp.FromInt(10)));
        Assert.Equal(Fp.MinusOne, FpMath.Sign(Fp.FromInt(-3)));
        Assert.Equal(Fp.Zero, FpMath.Sign(Fp.Zero));
    }

    [Fact]
    public void Clamp()
    {
        Fp min = Fp.FromInt(2);
        Fp max = Fp.FromInt(8);

        Assert.Equal(Fp.FromInt(5), FpMath.Clamp(Fp.FromInt(5), min, max));
        Assert.Equal(min, FpMath.Clamp(Fp.FromInt(1), min, max));
        Assert.Equal(max, FpMath.Clamp(Fp.FromInt(10), min, max));
    }

    [Fact]
    public void Lerp()
    {
        Fp a = Fp.FromInt(0);
        Fp b = Fp.FromInt(10);

        Assert.Equal(Fp.FromInt(0), FpMath.Lerp(a, b, Fp.Zero));
        Assert.Equal(Fp.FromInt(10), FpMath.Lerp(a, b, Fp.One));
        Assert.Equal(Fp.FromInt(5), FpMath.Lerp(a, b, Fp.Half));
    }

    [Fact]
    public void LerpClamped()
    {
        Fp a = Fp.FromInt(0);
        Fp b = Fp.FromInt(10);

        // t > 1 should clamp to b
        Assert.Equal(Fp.FromInt(10), FpMath.LerpClamped(a, b, Fp.FromInt(2)));
        // t < 0 should clamp to a
        Assert.Equal(Fp.FromInt(0), FpMath.LerpClamped(a, b, Fp.FromInt(-1)));
    }

    [Fact]
    public void CeilToInt()
    {
        Assert.Equal(4, FpMath.CeilToInt(Fp.FromDecimal(3.1m)));
        Assert.Equal(3, FpMath.CeilToInt(Fp.FromInt(3)));
    }

    [Fact]
    public void FloorToInt()
    {
        Assert.Equal(3, FpMath.FloorToInt(Fp.FromDecimal(3.9m)));
    }

    [Fact]
    public void RoundToInt()
    {
        Assert.Equal(4, FpMath.RoundToInt(Fp.FromDecimal(3.6m)));
        Assert.Equal(3, FpMath.RoundToInt(Fp.FromDecimal(3.4m)));
    }

    [Fact]
    public void Frac()
    {
        Fp val = Fp.FromDecimal(3.75m);
        Fp frac = FpMath.Frac(val);
        Assert.InRange(frac.ToFloat(), 0.74f, 0.76f);
    }

    [Fact]
    public void Floor()
    {
        Fp val = Fp.FromDecimal(3.75m);
        Fp floor = FpMath.Floor(val);
        Assert.Equal(Fp.FromInt(3), floor);
    }

    [Fact]
    public void Sqrt()
    {
        // sqrt(4) = 2
        Fp result4 = FpMath.Sqrt(Fp.FromInt(4));
        Assert.InRange(result4.ToFloat(), 1.99f, 2.01f);

        // sqrt(9) = 3
        Fp result9 = FpMath.Sqrt(Fp.FromInt(9));
        Assert.InRange(result9.ToFloat(), 2.99f, 3.01f);

        // sqrt(2) ~ 1.414
        Fp result2 = FpMath.Sqrt(Fp.FromInt(2));
        Assert.InRange(result2.ToFloat(), 1.41f, 1.42f);

        // sqrt(0) = 0
        Assert.Equal(Fp.Zero, FpMath.Sqrt(Fp.Zero));

        // sqrt(negative) = 0
        Assert.Equal(Fp.Zero, FpMath.Sqrt(Fp.FromInt(-1)));
    }

    [Fact]
    public void Sqrt_LargeValue()
    {
        // sqrt(1000000) = 1000
        Fp result = FpMath.Sqrt(Fp.FromInt(1_000_000));
        Assert.InRange(result.ToFloat(), 999f, 1001f);
    }
}
