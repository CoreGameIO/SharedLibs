# Changelog

## [0.2.1] - 2026-03-11

### Fixed
- UPM package: added LICENSE.meta for Unity compatibility

## [0.2.0] - 2026-03-11

### Added
- `FpMath.PowInt(Fp base, int exponent)` — integer exponentiation via binary method
- `FpMath.Log2(Fp a)` — log base 2 (bit-scan + repeated squaring), fully deterministic
- LICENSE file (MIT)
- `documentationUrl`, `changelogUrl`, `licensesUrl` in package.json

## [0.1.0] - 2026-03-09

### Added
- `Fp` — deterministic fixed-point struct (Q48.16, long-backed)
- `FpMath` — static math utilities (Min, Max, Abs, Sign, Clamp, Lerp, LerpClamped, Sqrt, Floor, Frac, rounding)
- Overflow-safe multiplication (128-bit intermediate) and division
- Implicit conversion from `int`, explicit to `int`/`float`
- Comparison operators with `Fp` and `int`
- MemoryPack serialization support (when `com.cysharp.memorypack` is installed)
- MessagePack serialization support (when `com.github.messagepack-csharp` is installed)
- Stub attributes for compilation without serializer packages
