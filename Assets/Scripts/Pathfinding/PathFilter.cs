using Unity.Collections;
using Unity.Mathematics;

public static class PathFilter
{
    /// <summary>
    /// Фильтрует путь, оставляя только ключевые для перемещения для игрока точки
    /// </summary>
    /// <param name="initPath"></param>
    /// <param name="vertices"></param>
    public static NativeList<PathPoint> FilterPath(NativeList<Vertex> initPath, NativeArray<Vertex> matrix, int matrixHeight)
    {
        var path = GetJumpVertices(initPath);
        if (initPath.Length > 0)
        {
            initPath.Dispose();
        }
        var resultingPath = RemoveExtraJumps(path, matrix, matrixHeight);
        path.Dispose();

        return resultingPath;
    }

    /// <summary>
    /// Возращает вершины, которые являются ключевыми при прыжках персонажа
    /// </summary>
    private static NativeList<Vertex> GetJumpVertices(NativeList<Vertex> fullPath)
    {
        var filterPath = new NativeList<Vertex>(Allocator.Temp);
        var nextVertex = new Vertex();
        var previousVertex = new Vertex();
        var previousPointType = PointType.Default;

        for (int i = 0; i < fullPath.Length; i++)
        {
            var currentVertex = fullPath[i];

            if (i != fullPath.Length - 1)
            {
                nextVertex = fullPath[i + 1];
            }
            if (i != 0)
            {
                previousVertex = fullPath[i - 1];
            }

            if (currentVertex.JumpLength == 0 && previousVertex.JumpLength != 0)
            {
                if (previousPointType == PointType.JumpPeak)
                {
                    filterPath.Add(currentVertex);
                    previousPointType = PointType.JumpEnd;
                }
                else if (previousPointType == PointType.JumpBeginning)
                {
                    filterPath.RemoveAtSwapBack(filterPath.Length - 1);
                    previousPointType = PointType.Default;
                }

            }

            if ((currentVertex.JumpLength == 0 && nextVertex.JumpLength != 0))
            {
                filterPath.Add(currentVertex);
                previousPointType = PointType.JumpBeginning;
            }

            if (filterPath.Length != 0 && currentVertex.PositionInMatrix.y > nextVertex.PositionInMatrix.y && currentVertex.PositionInMatrix.y > filterPath[filterPath.Length - 1].PositionInMatrix.y)
            {
                filterPath.Add(currentVertex);
                previousPointType = PointType.JumpPeak;
            }
        }

        var count = filterPath.Length % 3;
        if (count != 0)
        {
            for (int i = 1; i <= count; i++)
            {
                filterPath.RemoveAtSwapBack(filterPath.Length - i);
            }
        }
        filterPath.Add(fullPath[fullPath.Length - 1]);
        fullPath.Dispose();
        return filterPath;
    }

    /// <summary>
    /// Удаляет лишние прыжки
    /// </summary>
    private static NativeList<PathPoint> RemoveExtraJumps(NativeList<Vertex> path, NativeArray<Vertex> matrix, int matrixHeight)
    {
        var correctList = new NativeList<PathPoint>(Allocator.Temp);

        int counter = 0;
        var vertex = path[counter];
        int x, y;
        int sign = 1;
        x = vertex.PositionInMatrix.x;
        y = vertex.PositionInMatrix.y;
        int lastX = -1;
        x++;
        int notGroundVertexCount = 0;
        while (counter < path.Length - 1)
        {
            var index = NavMeshHelper.GetIndex(x, y, matrixHeight);
            vertex = matrix[index];

            sign = path[counter + 2].PositionInMatrix.x > path[counter].PositionInMatrix.x ? 1 : -1;
            var height = path[counter + 2].PositionInMatrix.y - path[counter].PositionInMatrix.y;

            if (x == path[counter + 2].PositionInMatrix.x)
            {
                if (notGroundVertexCount == 10)
                {
                    correctList.Add(new PathPoint { position = path[counter].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 1].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 2].Position, type = PathPointType.Jump });
                }
                if (math.abs(y - path[counter + 2].PositionInMatrix.y) > 2)
                {
                    correctList.Add(new PathPoint { position = path[counter].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 1].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 2].Position, type = PathPointType.Jump });
                }
                counter += 3;
                x = path[counter].PositionInMatrix.x;
                y = path[counter].PositionInMatrix.y;
                continue;
            }

            if (height > 0)
            {
                if (!NavMeshHelper.OnGround(matrix, matrixHeight, x, y))
                {
                    notGroundVertexCount++;
                }
                else
                {
                    notGroundVertexCount = 0;
                }
                if (notGroundVertexCount > 5)
                {
                    correctList.Add(new PathPoint { position = path[counter].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 1].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 2].Position, type = PathPointType.Jump });
                    counter += 3;
                    x = path[counter].PositionInMatrix.x;
                    y = path[counter].PositionInMatrix.y;
                    notGroundVertexCount = 0;
                    continue;
                }

                if (!vertex.IsAvailable)
                {
                    if (x == lastX)
                    {
                        correctList.Add(new PathPoint { position = path[counter].Position, type = PathPointType.Jump });
                        correctList.Add(new PathPoint { position = path[counter + 1].Position, type = PathPointType.Jump });
                        correctList.Add(new PathPoint { position = path[counter + 2].Position, type = PathPointType.Jump });
                        counter += 3;
                        x = path[counter].PositionInMatrix.x;
                        y = path[counter].PositionInMatrix.y;
                        continue;
                    }

                    lastX = x;
                    y++;
                    continue;
                }
                else
                {
                    x += sign;
                }
            }
            else if (height == 0)
            {
                if (!NavMeshHelper.OnGround(matrix, matrixHeight, x, y))
                {
                    notGroundVertexCount++;
                }
                else
                {
                    notGroundVertexCount = 0;
                }
                x += sign;
                if (notGroundVertexCount == 2)
                {
                    correctList.Add(new PathPoint { position = path[counter].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 1].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 2].Position, type = PathPointType.Jump });
                    counter += 3;
                    x = path[counter].PositionInMatrix.x;
                    y = path[counter].PositionInMatrix.y;
                    continue;
                }
            }
            else
            {
                if (y < path[counter + 2].PositionInMatrix.y)
                {
                    x += sign;
                    notGroundVertexCount++;
                }
                else
                {
                    if (vertex.IsAvailable)
                    {
                        y--;
                    }
                    else
                    {
                        x += sign;
                        y++;
                    }
                }

                if (notGroundVertexCount > 10)
                {
                    correctList.Add(new PathPoint { position = path[counter].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 1].Position, type = PathPointType.Jump });
                    correctList.Add(new PathPoint { position = path[counter + 2].Position, type = PathPointType.Jump });
                    counter += 3;
                    x = path[counter].PositionInMatrix.x;
                    y = path[counter].PositionInMatrix.y;
                    notGroundVertexCount = 0;
                    continue;
                }
            }
        }

        correctList.Add(new PathPoint { position = path[path.Length - 1].Position, type = PathPointType.MovementEnd });

        return correctList;
    }
}

