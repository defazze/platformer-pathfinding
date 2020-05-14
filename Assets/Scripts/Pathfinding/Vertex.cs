using System;
using Unity.Mathematics;
public struct Vertex : IEquatable<Vertex>
{
    public SurfaceType SurfaceType { get; set; }
    public float2 Position { get; set; }
    public int2 PositionInMatrix { get; set; }
    public int2 ParentMatrixPosition { get; set; }
    public float DistanceFromStart { get; set; }
    public float DistanceToFinish { get; set; }
    public float Score { get; set; }
    public bool IsNode { get; set; }
    public bool IsAvailable { get; set; }
    public int ParentZ { get; set; }
    public int JumpLength { get; set; }
    public int Z { get; set; }

    public bool Equals(Vertex other)
    {
        return PositionInMatrix.x == other.PositionInMatrix.x &&
            PositionInMatrix.y == other.PositionInMatrix.y;
    }
    public override int GetHashCode()
    {
        var positionHash = PositionInMatrix.GetHashCode();
        return positionHash;
    }
}