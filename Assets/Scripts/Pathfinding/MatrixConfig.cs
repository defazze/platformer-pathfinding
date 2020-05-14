using Unity.Mathematics;

public struct MatrixConfig
{
    /// <summary>
    /// Размер ячейки в юнитах
    /// </summary>
    public float cellSize;

    /// <summary>
    /// Ширина матрицы (в ячейках)
    /// </summary>
    public int width;

    /// <summary>
    /// Высота матрицы (в ячейках)
    /// </summary>
    public int height;
    public int2 offset;

    /// <summary>
    /// Смещение начала координат матрицы (левый нижний угол) относительно центра мира (в юнитах)
    /// </summary>
    /// <value></value>
    public float2 Transform
    {
        get
        {
            var center = new float2(offset.x * cellSize, offset.y * cellSize);
            var transform = new float2(center.x - width * cellSize / 2, center.y - height * cellSize / 2);

            return transform;
        }
    }

    /// <summary>
    /// Перевод расстояния из ячеек в юниты
    /// </summary>
    /// <param name="cellsCount">Количество ячеек</param>
    /// <returns></returns>
    public float ToUnit(int cellsCount)
    {
        return cellsCount * cellSize;
    }

    /// <summary>
    /// Перевод расстояния из юнитов в ячейки
    /// </summary>
    /// <param name="units">Количество юнитов</param>
    /// <returns></returns>
    public int ToCells(float units)
    {
        return (int)math.round(units / cellSize);
    }

    /// <summary>
    /// Перевод координат матрицы в мировые координаты
    /// </summary>
    /// <param name="matrixCell">Координаты в матрице</param>
    /// <returns></returns>
    public float2 ToUnit(int2 matrixCell)
    {
        var units = new float2((matrixCell.x - 0.5f) * cellSize, (matrixCell.y - 0.5f) * cellSize);
        return units + Transform;
    }

    /// <summary>
    /// Перевод мировых координат в координаты матрицы
    /// </summary>
    /// <param name="units">Мировые координаты</param>
    /// <returns></returns>
    public int2 ToCells(float2 units)
    {
        var coords = units - Transform;
        var matrixCoords = new int2((int)(math.round(coords.x / cellSize + 0.5f)), (int)math.round(coords.y / cellSize + 0.5f));

        return matrixCoords;
    }
}