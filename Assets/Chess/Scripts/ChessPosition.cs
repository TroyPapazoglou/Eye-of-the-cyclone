using System;
using UnityEngine;

[Serializable]
public class ChessPosition 
{
    public int x;
    public int z;

    public ChessPosition(int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public bool Equals(ChessPosition other)
    {
        return x == other.x && z == other.z;
    }

    public override bool Equals(object obj)
    {
        return obj is ChessPosition other &&
               Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(x, z);
    }

    public static bool operator ==(
        ChessPosition a,
        ChessPosition b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(
        ChessPosition a,
        ChessPosition b)
    {
        return !a.Equals(b);
    }
}
