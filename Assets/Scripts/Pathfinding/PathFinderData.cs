using Unity.Mathematics;
using Unity.Collections;

public struct PathFinderData
{
    /// <summary>
    /// Матрица вершин матрицы
    /// </summary>
    public NativeArray<Vertex> vertices;
    /// <summary>
    /// Начальная точка
    /// </summary>
    public float2 startPoint;
    /// <summary>
    /// Конечная точка
    /// </summary>
    public float2 targetPoint;
    /// <summary>
    /// Максимальная высота прыжка персонажа в количестве ячеек
    /// </summary>
    public int maxCharacterJumpHeight;
    /// <summary>
    /// Высота персонажа в количестве ячеек
    /// </summary>
    public int characterHeight;
    /// <summary>
    /// Ширина персонажа в количестве ячеек
    /// </summary>
    public int characterWidth;
}