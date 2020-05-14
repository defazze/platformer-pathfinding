using Unity.Mathematics;

public struct NodePoint
{
    public float2 position;
    public int node;
}

public struct EdgeStruct
{
    public float2 start;
    public float2 end;
    public int nodeId;
}

public struct NodeStruct
{
    public float2 start;
    public float2 end;
    public int id;
}