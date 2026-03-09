using CoreGame.FixedPoint;
using MemoryPack;
using MessagePack;
using Xunit;

namespace CoreGame.FixedPoint.Tests;

public class SerializationTests
{
    [Fact]
    public void MemoryPack_Roundtrip()
    {
        Fp original = Fp.FromDecimal(3.14159m);
        byte[] bytes = MemoryPackSerializer.Serialize(original);
        Fp deserialized = MemoryPackSerializer.Deserialize<Fp>(bytes);
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void MemoryPack_Zero()
    {
        byte[] bytes = MemoryPackSerializer.Serialize(Fp.Zero);
        Fp deserialized = MemoryPackSerializer.Deserialize<Fp>(bytes);
        Assert.Equal(Fp.Zero, deserialized);
    }

    [Fact]
    public void MemoryPack_Negative()
    {
        Fp original = Fp.FromDecimal(-42.5m);
        byte[] bytes = MemoryPackSerializer.Serialize(original);
        Fp deserialized = MemoryPackSerializer.Deserialize<Fp>(bytes);
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void MemoryPack_Array()
    {
        Fp[] original = { Fp.FromInt(1), Fp.FromDecimal(2.5m), Fp.FromInt(-3) };
        byte[] bytes = MemoryPackSerializer.Serialize(original);
        Fp[]? deserialized = MemoryPackSerializer.Deserialize<Fp[]>(bytes);
        Assert.NotNull(deserialized);
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void MessagePack_Roundtrip()
    {
        Fp original = Fp.FromDecimal(3.14159m);
        byte[] bytes = MessagePackSerializer.Serialize(original);
        Fp deserialized = MessagePackSerializer.Deserialize<Fp>(bytes);
        Assert.Equal(original, deserialized);
    }

    [Fact]
    public void MessagePack_Zero()
    {
        byte[] bytes = MessagePackSerializer.Serialize(Fp.Zero);
        Fp deserialized = MessagePackSerializer.Deserialize<Fp>(bytes);
        Assert.Equal(Fp.Zero, deserialized);
    }

    [Fact]
    public void MessagePack_Array()
    {
        Fp[] original = { Fp.FromInt(1), Fp.FromDecimal(2.5m), Fp.FromInt(-3) };
        byte[] bytes = MessagePackSerializer.Serialize(original);
        Fp[]? deserialized = MessagePackSerializer.Deserialize<Fp[]>(bytes);
        Assert.NotNull(deserialized);
        Assert.Equal(original, deserialized);
    }
}
