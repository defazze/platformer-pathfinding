using Unity.Mathematics;
public struct PathPoint
{
    public float2 position;
    public PathPointType type;
}

/// <summary>
/// Тип движения, которое будет выполнять персонаж в этой точке
/// </summary>
public enum PathPointType
{
    Walk = 0,
    Jump = 1,
    MovementEnd = 2
}