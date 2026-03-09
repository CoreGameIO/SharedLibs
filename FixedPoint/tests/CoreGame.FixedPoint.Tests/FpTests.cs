using CoreGame.FixedPoint;
using Xunit;

namespace CoreGame.FixedPoint.Tests;

public class FpArithmeticTests
{
    [Fact]
    public void Addition()
    {
        Fp a = Fp.FromInt(3);
        Fp b = Fp.FromInt(5);
        Assert.Equal(Fp.FromInt(8), a + b);
    }

    [Fact]
    public void Subtraction()
    {
        Fp a = Fp.FromInt(10);
        Fp b = Fp.FromInt(4);
        Assert.Equal(Fp.FromInt(6), a - b);
    }

    [Fact]
    public void UnaryNegation()
    {
        Fp a = Fp.FromInt(7);
        Assert.Equal(Fp.FromInt(-7), -a);
    }

    [Fact]
    public void Multiplication()
    {
        Fp a = Fp.FromInt(3);
        Fp b = Fp.FromInt(4);
        Assert.Equal(Fp.FromInt(12), a * b);
    }

    [Fact]
    public void Multiplication_Fractional()
    {
        Fp a = Fp.FromDecimal(2.5m);
        Fp b = Fp.FromDecimal(4.0m);
        Assert.Equal(Fp.FromInt(10), a * b);
    }

    [Fact]
    public void Multiplication_LargeValues()
    {
        // Test overflow-safe 128-bit intermediate
        Fp a = Fp.FromInt(100_000);
        Fp b = Fp.FromInt(100_000);
        Assert.Equal(Fp.FromLong(10_000_000_000L), a * b);
    }

    [Fact]
    public void Multiplication_ByInt()
    {
        Fp a = Fp.FromDecimal(2.5m);
        Assert.Equal(Fp.FromDecimal(7.5m), a * 3);
        Assert.Equal(Fp.FromDecimal(7.5m), 3 * a);
    }

    [Fact]
    public void Division()
    {
        Fp a = Fp.FromInt(10);
        Fp b = Fp.FromInt(4);
        Assert.Equal(Fp.FromDecimal(2.5m), a / b);
    }

    [Fact]
    public void Division_ByInt()
    {
        Fp a = Fp.FromInt(15);
        Assert.Equal(Fp.FromInt(5), a / 3);
    }

    [Fact]
    public void Division_ByZero_Throws()
    {
        Fp a = Fp.FromInt(1);
        Assert.Throws<DivideByZeroException>(() => a / Fp.Zero);
    }

    [Fact]
    public void Modulo()
    {
        Fp a = Fp.FromInt(7);
        Fp b = Fp.FromInt(3);
        Assert.Equal(Fp.FromInt(1), a % b);
    }
}

public class FpConversionTests
{
    [Fact]
    public void FromInt_ToInt_Roundtrip()
    {
        Assert.Equal(42, Fp.FromInt(42).ToInt());
        Assert.Equal(-5, Fp.FromInt(-5).ToInt());
        Assert.Equal(0, Fp.FromInt(0).ToInt());
    }

    [Fact]
    public void FromDecimal()
    {
        Fp a = Fp.FromDecimal(3.25m);
        Assert.Equal(3, a.ToInt());
        Assert.InRange(a.ToFloat(), 3.24f, 3.26f);
    }

    [Fact]
    public void FromFloat()
    {
        Fp a = Fp.FromFloat(1.5f);
        Assert.Equal(1, a.ToInt());
        Assert.InRange(a.ToFloat(), 1.49f, 1.51f);
    }

    [Fact]
    public void CeilToInt()
    {
        Assert.Equal(4, Fp.FromDecimal(3.1m).CeilToInt());
        Assert.Equal(3, Fp.FromInt(3).CeilToInt());
    }

    [Fact]
    public void FloorToInt()
    {
        Assert.Equal(3, Fp.FromDecimal(3.9m).FloorToInt());
        Assert.Equal(3, Fp.FromInt(3).FloorToInt());
    }

    [Fact]
    public void RoundToInt()
    {
        Assert.Equal(4, Fp.FromDecimal(3.6m).RoundToInt());
        Assert.Equal(3, Fp.FromDecimal(3.4m).RoundToInt());
        Assert.Equal(4, Fp.FromDecimal(3.5m).RoundToInt());
    }

    [Fact]
    public void ImplicitConversion_FromInt()
    {
        Fp a = 5;
        Assert.Equal(Fp.FromInt(5), a);
    }

    [Fact]
    public void ExplicitConversion_ToInt()
    {
        Fp a = Fp.FromDecimal(3.7m);
        Assert.Equal(3, (int)a);
    }

    [Fact]
    public void ExplicitConversion_ToFloat()
    {
        Fp a = Fp.FromInt(5);
        Assert.Equal(5.0f, (float)a);
    }
}

public class FpComparisonTests
{
    [Fact]
    public void Equality()
    {
        Assert.True(Fp.FromInt(3) == Fp.FromInt(3));
        Assert.False(Fp.FromInt(3) == Fp.FromInt(4));
        Assert.True(Fp.FromInt(3) != Fp.FromInt(4));
    }

    [Fact]
    public void LessThan_GreaterThan()
    {
        Assert.True(Fp.FromInt(2) < Fp.FromInt(3));
        Assert.True(Fp.FromInt(3) > Fp.FromInt(2));
        Assert.True(Fp.FromInt(2) <= Fp.FromInt(2));
        Assert.True(Fp.FromInt(2) >= Fp.FromInt(2));
    }

    [Fact]
    public void Comparison_WithInt()
    {
        Fp a = Fp.FromInt(5);
        Assert.True(a == 5);
        Assert.True(a != 4);
        Assert.True(a > 4);
        Assert.True(a < 6);
        Assert.True(a >= 5);
        Assert.True(a <= 5);
    }

    [Fact]
    public void CompareTo()
    {
        Assert.True(Fp.FromInt(3).CompareTo(Fp.FromInt(2)) > 0);
        Assert.True(Fp.FromInt(2).CompareTo(Fp.FromInt(3)) < 0);
        Assert.Equal(0, Fp.FromInt(5).CompareTo(Fp.FromInt(5)));
    }

    [Fact]
    public void Equals_And_HashCode()
    {
        Fp a = Fp.FromInt(7);
        Fp b = Fp.FromInt(7);
        Assert.True(a.Equals(b));
        Assert.True(a.Equals((object)b));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
        Assert.False(a.Equals(null));
    }
}

public class FpToStringTests
{
    [Fact]
    public void Integer_NoFraction()
    {
        Assert.Equal("5", Fp.FromInt(5).ToString());
        Assert.Equal("0", Fp.Zero.ToString());
    }

    [Fact]
    public void WithFraction()
    {
        string s = Fp.FromDecimal(3.5m).ToString();
        Assert.Contains("3.", s);
    }

    [Fact]
    public void Negative()
    {
        Assert.Equal("-5", Fp.FromInt(-5).ToString());
    }

    [Fact]
    public void FormatString()
    {
        string s = Fp.FromDecimal(3.14159m).ToString("F2");
        Assert.StartsWith("3.1", s);
    }
}

public class FpConstantsTests
{
    [Fact]
    public void Constants()
    {
        Assert.Equal(0, Fp.Zero.ToInt());
        Assert.Equal(1, Fp.One.ToInt());
        Assert.Equal(-1, Fp.MinusOne.ToInt());
        Assert.InRange(Fp.Half.ToFloat(), 0.49f, 0.51f);
        Assert.Equal(1, Fp.Epsilon.RawValue);
    }
}
