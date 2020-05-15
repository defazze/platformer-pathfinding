using System;
using Unity.Collections;
using Unity.Mathematics;

public class Node
{
    public int id;
    public float2 start;
    public float2 end;
    public int distanceFromStart;
    public int cost;
    public bool visited;
    public Node nearestToStart;
}
