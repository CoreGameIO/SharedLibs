#nullable enable
using System;
using MemoryPack;
using MessagePack;

namespace CoreGame.FixedPoint
{
    /// <summary>
    /// Deterministic fixed-point type Q48.16 backed by long.
    /// 48-bit integer part, 16-bit fractional part.
    /// Range: ~+-140 trillion. Precision: 1/65536 ~ 0.000015.
    /// All operations use integer arithmetic only — no floats.
    /// </summary>
    [MemoryPackable]
    [MessagePackObject]
    public readonly partial struct Fp : IEquatable<Fp>, IComparable<Fp>
    {
        [MemoryPackOrder(0)]
        [Key(0)]
        public readonly long RawValue;

        public const int SHIFT = 16;
        public const long ONE = 1L << SHIFT;       // 65536
        public const long HALF = 1L << (SHIFT - 1); // 32768

        #region Constructors

        public Fp(long rawValue)
        {
            RawValue = rawValue;
        }

        #endregion

        #region Constants

        public static readonly Fp Zero = new(0);
        public static readonly Fp One = new(ONE);
        public static readonly Fp MinusOne = new(-ONE);
        public static readonly Fp Half = new(HALF);
        public static readonly Fp Epsilon = new(1);
        public static readonly Fp MaxValue = new(long.MaxValue);
        public static readonly Fp MinValue = new(long.MinValue);

        #endregion

        #region Factory Methods

        /// <summary>Create fp from an integer.</summary>
        public static Fp FromInt(int value) => new((long)value << SHIFT);

        /// <summary>Create fp from a long.</summary>
        public static Fp FromLong(long value) => new(value << SHIFT);

        /// <summary>Create fp from a raw value (no shift applied).</summary>
        public static Fp FromRaw(long rawValue) => new(rawValue);

        /// <summary>
        /// Create fp from float. FOR CONFIGURATION/INITIALIZATION ONLY!
        /// Do not use in runtime logic — float is non-deterministic.
        /// </summary>
        public static Fp FromFloat(float value) => new((long)(value * ONE));

        /// <summary>
        /// Create fp from double. FOR CONFIGURATION/INITIALIZATION ONLY!
        /// Do not use in runtime logic — double is non-deterministic.
        /// </summary>
        public static Fp FromDouble(double value) => new((long)(value * ONE));

        /// <summary>
        /// Create fp from decimal. Safe for configuration values.
        /// </summary>
        public static Fp FromDecimal(decimal value) => new((long)(value * ONE));

        #endregion

        #region Conversion

        /// <summary>Integer part (discards fractional).</summary>
        public int ToInt() => (int)(RawValue >> SHIFT);

        /// <summary>Integer part as long.</summary>
        public long ToLong() => RawValue >> SHIFT;

        /// <summary>
        /// Convert to float. FOR DISPLAY/UI ONLY!
        /// Do not use for calculations.
        /// </summary>
        public float ToFloat() => (float)RawValue / ONE;

        /// <summary>
        /// Convert to double. FOR DISPLAY/UI ONLY!
        /// </summary>
        public double ToDouble() => (double)RawValue / ONE;

        /// <summary>Ceiling to nearest integer.</summary>
        public int CeilToInt() => (int)((RawValue + ONE - 1) >> SHIFT);

        /// <summary>Floor to nearest integer.</summary>
        public int FloorToInt() => (int)(RawValue >> SHIFT);

        /// <summary>Round to nearest integer.</summary>
        public int RoundToInt() => (int)((RawValue + HALF) >> SHIFT);

        #endregion

        #region Arithmetic Operators

        public static Fp operator +(Fp a, Fp b) => new(a.RawValue + b.RawValue);
        public static Fp operator -(Fp a, Fp b) => new(a.RawValue - b.RawValue);
        public static Fp operator -(Fp a) => new(-a.RawValue);
        public static Fp operator +(Fp a) => a;

        /// <summary>
        /// Multiplication via 128-bit intermediate value.
        /// Splits each long into two 32-bit components.
        /// </summary>
        public static Fp operator *(Fp a, Fp b)
        {
            long al = a.RawValue & 0xFFFFFFFFL;
            long ah = a.RawValue >> 32;
            long bl = b.RawValue & 0xFFFFFFFFL;
            long bh = b.RawValue >> 32;

            long ll = al * bl;
            long lh = al * bh;
            long hl = ah * bl;
            long hh = ah * bh;

            long result = (ll >> SHIFT)
                        + (lh << (32 - SHIFT))
                        + (hl << (32 - SHIFT))
                        + (hh << (64 - SHIFT));

            return new Fp(result);
        }

        /// <summary>
        /// Division: shifts numerator left by SHIFT, then divides.
        /// Uses half-shift approach for large values to avoid overflow.
        /// </summary>
        public static Fp operator /(Fp a, Fp b)
        {
            if (b.RawValue == 0)
                throw new DivideByZeroException("fp division by zero");

            long al = a.RawValue;
            long bl = b.RawValue;

            // Direct path if value fits after shift
            if (al >= -(1L << 47) && al <= (1L << 47))
            {
                return new Fp((al << SHIFT) / bl);
            }

            // Half-shift approach for large values to avoid overflow
            const int halfShift = SHIFT / 2;
            long shifted = (al >> halfShift) * (ONE << halfShift);
            return new Fp(shifted / bl);
        }

        /// <summary>Multiply fp by int (fast path).</summary>
        public static Fp operator *(Fp a, int b) => new(a.RawValue * b);
        public static Fp operator *(int a, Fp b) => new((long)a * b.RawValue);

        /// <summary>Divide fp by int (fast path).</summary>
        public static Fp operator /(Fp a, int b) => new(a.RawValue / b);

        /// <summary>Modulo.</summary>
        public static Fp operator %(Fp a, Fp b) => new(a.RawValue % b.RawValue);

        #endregion

        #region Comparison Operators

        public static bool operator ==(Fp a, Fp b) => a.RawValue == b.RawValue;
        public static bool operator !=(Fp a, Fp b) => a.RawValue != b.RawValue;
        public static bool operator <(Fp a, Fp b) => a.RawValue < b.RawValue;
        public static bool operator >(Fp a, Fp b) => a.RawValue > b.RawValue;
        public static bool operator <=(Fp a, Fp b) => a.RawValue <= b.RawValue;
        public static bool operator >=(Fp a, Fp b) => a.RawValue >= b.RawValue;

        // Comparison with int
        public static bool operator ==(Fp a, int b) => a.RawValue == ((long)b << SHIFT);
        public static bool operator !=(Fp a, int b) => a.RawValue != ((long)b << SHIFT);
        public static bool operator <(Fp a, int b) => a.RawValue < ((long)b << SHIFT);
        public static bool operator >(Fp a, int b) => a.RawValue > ((long)b << SHIFT);
        public static bool operator <=(Fp a, int b) => a.RawValue <= ((long)b << SHIFT);
        public static bool operator >=(Fp a, int b) => a.RawValue >= ((long)b << SHIFT);

        #endregion

        #region Implicit/Explicit Conversions

        /// <summary>Implicit conversion from int.</summary>
        public static implicit operator Fp(int value) => FromInt(value);

        /// <summary>Explicit conversion to int (discards fractional part).</summary>
        public static explicit operator int(Fp value) => value.ToInt();

        /// <summary>Explicit conversion to float (for UI display).</summary>
        public static explicit operator float(Fp value) => value.ToFloat();

        #endregion

        #region IEquatable / IComparable

        public bool Equals(Fp other) => RawValue == other.RawValue;
        public override bool Equals(object? obj) => obj is Fp other && Equals(other);
        public override int GetHashCode() => RawValue.GetHashCode();
        public int CompareTo(Fp other) => RawValue.CompareTo(other.RawValue);

        #endregion

        #region ToString

        public override string ToString()
        {
            long intPart = RawValue >> SHIFT;
            long fracPart = RawValue & (ONE - 1);
            if (RawValue < 0 && fracPart != 0)
            {
                intPart -= 1;
                fracPart = ONE - fracPart;
            }
            long frac4 = (fracPart * 10000) >> SHIFT;
            if (fracPart == 0)
                return intPart.ToString();
            return $"{intPart}.{frac4:D4}";
        }

        public string ToString(string format)
        {
            return ToDouble().ToString(format);
        }

        #endregion
    }
}
