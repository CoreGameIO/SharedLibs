# CoreGame FixedPoint

Deterministic fixed-point arithmetic for game logic. Q48.16 format backed by `long` — all operations use integer arithmetic only, no floating-point. Cross-platform deterministic by design.

## Features

- **Q48.16 format** — 48-bit integer part, 16-bit fractional part. Range: ~+/-140 trillion, precision: ~0.000015
- **Integer-only arithmetic** — addition, subtraction, multiplication (128-bit safe), division (overflow-safe), modulo
- **Math utilities** — Min, Max, Abs, Sign, Clamp, Lerp, Sqrt, Floor, Frac, rounding
- **Zero dependencies** — works standalone in Unity and .NET
- **Serialization** — optional MemoryPack and MessagePack support (auto-detected in Unity via asmdef versionDefines)

## Quick Start

```csharp
using CoreGame.FixedPoint;

// From integers (deterministic)
Fp health = Fp.FromInt(100);
Fp damage = Fp.FromDecimal(25.5m);
Fp remaining = health - damage;  // 74.5

// Implicit int conversion
Fp count = 5;
Fp total = count * Fp.FromDecimal(1.5m);  // 7.5

// Math utilities
Fp clamped = FpMath.Clamp(remaining, Fp.Zero, Fp.FromInt(100));
Fp distance = FpMath.Sqrt(Fp.FromInt(50));
Fp interpolated = FpMath.Lerp(Fp.Zero, Fp.FromInt(10), Fp.Half);  // 5.0

// Display (float conversion for UI only)
Debug.Log($"HP: {remaining.ToFloat():F1}");
```

## Installation

### Unity (UPM)

Add via Package Manager using git URL or local path:
```
https://github.com/CoreGameIO/SharedMeta.git?path=com.coregame.fixedpoint
```

No dependencies required. MemoryPack/MessagePack serialization activates automatically when those packages are installed.

### .NET (NuGet)

```
dotnet add package CoreGame.FixedPoint
```

## API Reference

### `Fp` struct

| Method | Description |
|--------|-------------|
| `Fp.FromInt(int)` | Create from integer (deterministic) |
| `Fp.FromLong(long)` | Create from long (deterministic) |
| `Fp.FromDecimal(decimal)` | Create from decimal (safe for config) |
| `Fp.FromFloat(float)` | Create from float (config only, non-deterministic!) |
| `Fp.FromRaw(long)` | Create from raw Q48.16 value |
| `.ToInt()` | Integer part (truncate) |
| `.ToFloat()` | Float approximation (display only!) |
| `.CeilToInt()` | Ceiling to int |
| `.FloorToInt()` | Floor to int |
| `.RoundToInt()` | Round to nearest int |

**Constants:** `Fp.Zero`, `Fp.One`, `Fp.MinusOne`, `Fp.Half`, `Fp.Epsilon`, `Fp.MaxValue`, `Fp.MinValue`

**Operators:** `+`, `-`, `*`, `/`, `%`, unary `-`, comparisons (`==`, `!=`, `<`, `>`, `<=`, `>=`) with both `Fp` and `int`

### `FpMath` utilities

| Method | Description |
|--------|-------------|
| `FpMath.Max(a, b)` | Maximum of two values |
| `FpMath.Min(a, b)` | Minimum of two values |
| `FpMath.Abs(a)` | Absolute value |
| `FpMath.Sign(a)` | Returns 1, -1, or 0 |
| `FpMath.Clamp(val, min, max)` | Clamp value to range |
| `FpMath.Lerp(a, b, t)` | Linear interpolation |
| `FpMath.LerpClamped(a, b, t)` | Lerp with t clamped to [0,1] |
| `FpMath.Sqrt(a)` | Square root (Newton's method) |
| `FpMath.Floor(a)` | Integer part as Fp |
| `FpMath.Frac(a)` | Fractional part |

## Determinism

All arithmetic operations use integer math only. The `Fp` struct is safe for:
- Client-server synchronization (SharedMeta optimistic execution)
- Lockstep multiplayer
- Deterministic replay/rollback

**Do not** use `FromFloat`/`FromDouble` in game logic — float results vary across platforms. Use `FromInt`, `FromDecimal`, or `FromRaw` for deterministic values.

## Serialization

The `Fp` struct supports both MemoryPack and MessagePack:

- **Unity**: Serialization activates automatically when `com.cysharp.memorypack` or `com.github.messagepack-csharp` packages are installed. Without them, stub attributes keep the code compilable.
- **.NET/NuGet**: Both serializers are included as dependencies.

Since `Fp` is an 8-byte unmanaged struct, serialization is zero-copy (direct memory).

## Requirements

- Unity 6000.0+ (UPM) or .NET Standard 2.1+ / .NET 8.0+ (NuGet)
