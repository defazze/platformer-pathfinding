using Unity.Collections;
using Unity.Mathematics;

public static class NavMeshHelper
{
    /// <summary>
    /// Возращает индекс элемента в одномерном массиве
    /// </summary>
    /// <param name="x">Позиция x в двумерном массиве</param>
    /// <param name="y">Позиция y в двумерном масиве </param>
    /// <param name="matrixHeight">Высота двумерного массива</param>
    /// <returns>Индекс</returns>
    public static int GetIndex(int x, int y, int matrixHeight)
    {
        if (x < 0 || y < 0)
        {
            return -1;
        }

        var index = x * matrixHeight + y;

        return index;
    }

    /// <summary>
    /// Возращает индекс элемента в одномерном массиве
    /// </summary>
    /// <param name="position">Позиция в двумерном массиве</param>
    /// <param name="matrixWidth">Высота двумерного массива</param>
    /// <returns>Индекс</returns>
    public static int GetIndex(int2 position, int matrixHeight)
    {
        var index = position.x * matrixHeight + position.y;

        return index;
    }

    /// <summary>
    /// Проверяет вершину под заданной вершиной на непроходимость
    /// </summary>
    public static bool OnGround(NativeArray<Vertex> matrix, int matrixHeight, int2 position)
    {
        var index = GetIndex(position.x, position.y - 1, matrixHeight);
        var vertex = matrix[index];

        return !vertex.IsAvailable;
    }

    /// <summary>
    /// Проверяет вершину под заданной вершиной на непроходимость
    /// </summary>
    public static bool OnGround(NativeArray<Vertex> matrix, int matrixHeight, int positionX, int positionY)
    {
        var index = GetIndex(positionX, positionY - 1, matrixHeight);
        var vertex = matrix[index];

        return !vertex.IsAvailable;
    }

    /// <summary>
    /// Проверяет все вершины матрицы около необходимой на их непроходимость
    /// </summary>
    /// <param name="matrix">Матрица</param>
    /// <param name="position">Позиция вершина, над которой будут проверяться другие вершины</param>
    /// <param name="count">Количество вершин, которые будут проверяться</param>
    /// <returns>Все ли проходимые</returns>
    public static bool CheckVerticalVertices(NativeArray<Vertex> matrix, int matrixHeight, int2 position, int count, int sign)
    {
        for (int i = 1; i <= count; i++)
        {
            var index = GetIndex(position.x, position.y + i * sign, matrixHeight);

            if (index == -1)
            {
                return false;
            }

            if (!matrix[index].IsAvailable)
            {
                return false;
            }
        }

        return true;
    }
}
