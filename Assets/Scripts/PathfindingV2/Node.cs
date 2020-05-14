using System;
using Unity.Collections;
using Unity.Mathematics;

public struct Node : IEquatable<Node>
{
    public int id;
    public float2 start;
    public float2 end;
    public int distanceFromStart;
    public int cost;
    public bool visited;
    public bool Equals(Node other)
    {
        return start.x == other.start.x && start.y == other.start.y && end.x == other.end.x && end.y == other.end.y && id == other.id;
    }
}