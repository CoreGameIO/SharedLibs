namespace CoreGame.FixedPoint
{
    /// <summary>
    /// Math utilities for the fp type.
    /// All operations are deterministic (integer arithmetic only).
    /// </summary>
    public static class FpMath
    {
        public static Fp Max(Fp a, Fp b) => a.RawValue >= b.RawValue ? a : b;
        public static Fp Min(Fp a, Fp b) => a.RawValue <= b.RawValue ? a : b;

        public static Fp Abs(Fp a) => a.RawValue >= 0 ? a : new Fp(-a.RawValue);

        public static Fp Sign(Fp a)
        {
            if (a.RawValue > 0) return Fp.One;
            if (a.RawValue < 0) return Fp.MinusOne;
            return Fp.Zero;
        }

        public static Fp Clamp(Fp value, Fp min, Fp max)
        {
            if (value.RawValue < min.RawValue) return min;
            if (value.RawValue > max.RawValue) return max;
            return value;
        }

        /// <summary>
        /// Linear interpolation: a + (b - a) * t
        /// </summary>
        public static Fp Lerp(Fp a, Fp b, Fp t)
        {
            return a + (b - a) * t;
        }

        /// <summary>
        /// Linear interpolation with t clamped to [0, 1].
        /// </summary>
        public static Fp LerpClamped(Fp a, Fp b, Fp t)
        {
            t = Clamp(t, Fp.Zero, Fp.One);
            return a + (b - a) * t;
        }

        /// <summary>Ceiling to integer.</summary>
        public static int CeilToInt(Fp a) => a.CeilToInt();

        /// <summary>Floor to integer.</summary>
        public static int FloorToInt(Fp a) => a.FloorToInt();

        /// <summary>Round to nearest integer.</summary>
        public static int RoundToInt(Fp a) => a.RoundToInt();

        /// <summary>Fractional part.</summary>
        public static Fp Frac(Fp a) => new Fp(a.RawValue & (Fp.ONE - 1));

        /// <summary>Integer part as fp (no fractional).</summary>
        public static Fp Floor(Fp a) => new Fp(a.RawValue & ~(Fp.ONE - 1));

        /// <summary>
        /// Integer square root (approximate, via Newton's method iterations).
        /// </summary>
        public static Fp Sqrt(Fp a)
        {
            if (a.RawValue <= 0) return Fp.Zero;

            // Initial guess via bit shift
            long val = a.RawValue;
            long guess = val;

            // Find highest bit for initial approximation
            int bits = 0;
            long temp = val;
            while (temp > 0)
            {
                bits++;
                temp >>= 1;
            }

            // sqrt approximation: 2^(bits/2), adjusted for Q48.16
            guess = 1L << ((bits + Fp.SHIFT) >> 1);

            // 6 Newton iterations: x_{n+1} = (x_n + val/x_n) / 2
            for (int i = 0; i < 6; i++)
            {
                if (guess == 0) break;
                long div = (val << Fp.SHIFT) / guess;
                guess = (guess + div) >> 1;
            }

            return new Fp(guess);
        }
    }
}
