using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;


public static class PathFinder
{
    private const int INITIAL_JUMP_FORCE = 3;
    private const int MULTIPLIER_JUMP = 2;
    private const int INCREMENT_JUMP_UP = 2;
    private const int INCREMENT_JUMP_SIDEWAYS = 1;
    private const int MAX_FALL_HEIGTH = 20;

    public static NativeList<PathPoint> FindPath(PathFinderData pathFinderData, MatrixConfig matrixConfig)
    {
        var layerCount = -1;

        var matrix = pathFinderData.vertices;
        NativeList<Vertex> nodes = new NativeList<Vertex>(Allocator.Temp);
        var maxJumpLength = pathFinderData.maxCharacterJumpHeight * MULTIPLIER_JUMP;

        var startVertex = GetMatrixVertex(matrix, matrixConfig.height, pathFinderData.startPoint, matrixConfig);
        var targetVertex = GetMatrixVertex(matrix, matrixConfig.height, pathFinderData.targetPoint, matrixConfig);

        var isAvailable = TryGetTargetPosition(matrix, matrixConfig.height, ref targetVertex);


        SetStartPosition(matrix, matrixConfig.height, ref startVertex, pathFinderData.characterWidth);

        var index = NavMeshHelper.GetIndex(startVertex.PositionInMatrix, matrixConfig.height);
        var currentVertex = matrix[index];

        currentVertex.JumpLength = GetStartJumpValue(matrix, matrixConfig.height, startVertex.PositionInMatrix, maxJumpLength, pathFinderData.characterWidth);

        AddNodesLayer(nodes, matrixConfig.height, matrixConfig.width, ref layerCount);
        SetNodeVertex(currentVertex, currentVertex.PositionInMatrix.x, currentVertex.PositionInMatrix.y, 0, nodes, matrixConfig.height, matrixConfig.width, layerCount);

        var lastGroundVertex = currentVertex;
        var waitingList = new NativeList<Vertex>(Allocator.Temp);
        Vertex leastScoreVertex = new Vertex { Score = float.MaxValue };

        while (!currentVertex.Equals(targetVertex))
        {
            index = NavMeshHelper.GetIndex(currentVertex.PositionInMatrix.x, currentVertex.PositionInMatrix.y - 1, matrixConfig.height);
            var vertex = matrix[index];

            if (!vertex.IsAvailable)
            {
                lastGroundVertex = vertex;
            }

            var neighbors = GetNeighbors(matrix, matrixConfig.height, matrixConfig.width, currentVertex, lastGroundVertex, targetVertex,
                pathFinderData.characterWidth, pathFinderData.characterHeight, maxJumpLength, nodes, ref layerCount);
            waitingList.AddRange(neighbors);

            index = waitingList.IndexOf(currentVertex);
            if (index != -1)
            {
                waitingList.RemoveAtSwapBack(index);
            }

            if (neighbors.Length == 0)
            {
                if (waitingList.Length == 0)
                {
                    currentVertex = leastScoreVertex;
                    break;
                }
                currentVertex = GetBestVertex(waitingList);
            }
            else
            {
                currentVertex = GetBestVertex(neighbors);
            }

            if (currentVertex.Score < leastScoreVertex.Score && NavMeshHelper.OnGround(matrix, matrixConfig.height, currentVertex.PositionInMatrix))
            {
                leastScoreVertex = currentVertex;
            }

            neighbors.Dispose();
        }

        if (currentVertex.Position.Equals(float2.zero) && currentVertex.PositionInMatrix.Equals(int2.zero))
        {
            currentVertex = targetVertex;
            var point = TryGetTargetPosition(matrix, matrixConfig.height, ref currentVertex);
            var points = new NativeList<PathPoint>(Allocator.Temp);
            points.Add(new PathPoint { position = currentVertex.Position, type = PathPointType.Walk });

            return points;

        }
        else
        {
            if (!NavMeshHelper.OnGround(matrix, matrixConfig.height, targetVertex.PositionInMatrix) && !currentVertex.Equals(targetVertex))
            {
                currentVertex = leastScoreVertex;
            }

            waitingList.Dispose();

            var vertexPath = GetPath(currentVertex, startVertex, nodes, matrixConfig.height, matrixConfig.width, layerCount);

            var path = PathFilter.FilterPath(vertexPath, matrix, matrixConfig.height);
            if (vertexPath.Length > 0)
            {
                vertexPath.Dispose();
            }

            return path;
        }
    }

    /// <summary>
    /// Возращает ячейку матрицы 
    /// </summary>
    private static Vertex GetMatrixVertex(NativeArray<Vertex> vertices, int matrixHeight, float2 point, MatrixConfig matrixConfig)
    {
        var matrixCoords = matrixConfig.ToCells(point);
        var index = NavMeshHelper.GetIndex(matrixCoords, matrixHeight);
        var vertex = vertices[index];

        return vertex;
    }

    /// <summary>
    /// Возращает значение прыжка начальной точки, 0 - если стоит на земле, максимальное - если в воздухе
    /// </summary>
    private static int GetStartJumpValue(NativeArray<Vertex> matrix, int matrixHeight, int2 position, int maxJumpLength, int characterWidth)
    {
        var jumpValue = maxJumpLength;

        for (int x = position.x - characterWidth / 2; x < position.x + characterWidth / 2; x++)
        {
            if (NavMeshHelper.OnGround(matrix, matrixHeight, x, position.y))
            {
                jumpValue = 0;
                break;
            }
        }

        return jumpValue;
    }

    /// <summary>
    /// Отпускает перпендикуляр вниз из заданной точки, и возращает ближайшую к ней, под которой точка недоступна
    /// </summary>
    private static bool TryGetTargetPosition(NativeArray<Vertex> matrix, int matrixHeight, ref Vertex targetVertex)
    {
        var index = NavMeshHelper.GetIndex(targetVertex.PositionInMatrix.x, targetVertex.PositionInMatrix.y - 1, matrixHeight);
        var newVertex = matrix[index];
        while (newVertex.IsAvailable)
        {
            index = NavMeshHelper.GetIndex(newVertex.PositionInMatrix.x, newVertex.PositionInMatrix.y - 1, matrixHeight);
            newVertex = matrix[index];
            if (newVertex.PositionInMatrix.y == 1)
            {
                return false;
            }
        }

        index = NavMeshHelper.GetIndex(newVertex.PositionInMatrix.x, newVertex.PositionInMatrix.y + 1, matrixHeight);
        targetVertex = matrix[index];

        return true;
    }

    private static void SetStartPosition(NativeArray<Vertex> matrix, int matrixHeight, ref Vertex startVertex, int characterWidht)
    {
        var vertex = startVertex;
        int index;
        while (!vertex.IsAvailable)
        {
            index = NavMeshHelper.GetIndex(vertex.PositionInMatrix.x, vertex.PositionInMatrix.y + 1, matrixHeight);
            vertex = matrix[index];
        }

        index = NavMeshHelper.GetIndex(vertex.PositionInMatrix.x, vertex.PositionInMatrix.y - 1, matrixHeight);
        var groundVertex = matrix[index];

        if (groundVertex.IsAvailable)
        {
            for (int i = -characterWidht / 2; i < characterWidht / 2; i++)
            {
                index = NavMeshHelper.GetIndex(vertex.PositionInMatrix.x + i, vertex.PositionInMatrix.y - 1, matrixHeight);
                var vert = matrix[index];

                if (!vert.IsAvailable)
                {
                    vertex = vert;
                    while (!vertex.IsAvailable)
                    {
                        index = NavMeshHelper.GetIndex(vertex.PositionInMatrix.x, vertex.PositionInMatrix.y + 1, matrixHeight);
                        vertex = matrix[index];
                        break;
                    }
                    break;
                }
            }
        }
        startVertex = vertex;
    }

    /// <summary>
    /// Возращает весь полученный путь
    /// </summary>
    private static NativeList<Vertex> GetPath(Vertex targetVertex, Vertex startVertex, NativeList<Vertex> nodes, int matrixHeight, int matrixWidth, int layerCount)
    {
        var path = new NativeList<Vertex>(Allocator.Temp);
        var vertex = targetVertex;
        path.Add(targetVertex);
        while (!vertex.Equals(startVertex))
        {
            path.Add(vertex);
            vertex = GetVertexParent(nodes, vertex, matrixHeight, matrixWidth, layerCount);
        }
        path.Add(startVertex);

        var reversePath = new NativeList<Vertex>(Allocator.Temp);

        for (int i = path.Length - 1; i >= 0; i--)
        {
            reversePath.Add(path[i]);
        }
        path.Dispose();

        return reversePath;
    }

    /// <summary>
    /// Возвращает список соседних вершин, отсеивая просмотренные и непроходимые
    /// </summary>
    /// <param name="parentVertex">Вершина</param>
    /// <returns>Список соседних вершин</returns>
    private static NativeList<Vertex> GetNeighbors(NativeArray<Vertex> matrix, int matrixHeight, int matrixWidth, Vertex parentVertex, Vertex lastGroundVertex,
        Vertex targetVertex, int characterWidth, int characterHeight, int maxJumpLength, NativeList<Vertex> nodes, ref int layerCount)
    {
        var neighbors = new NativeList<Vertex>(Allocator.Temp);
        var parentPosition = parentVertex.PositionInMatrix;

        var directions = GetDirections();

        for (int k = 0; k < directions.Length; k++)
        {
            var i = directions[k].x;
            var j = directions[k].y;

            if (parentPosition.x + i >= matrixWidth ||
                parentPosition.x + i < 0 ||
                parentPosition.y + j >= matrixHeight ||
                parentPosition.y + j < 0)
            {
                continue;
            }

            var index = NavMeshHelper.GetIndex(parentPosition.x + i, parentPosition.y + j, matrixHeight);
            var currentVertex = matrix[index];
            var position = currentVertex.PositionInMatrix;

            var isMaxFallHeight = CheckMaxFallHeight(lastGroundVertex.PositionInMatrix.y, position.y);
            if (isMaxFallHeight)
            {
                continue;
            }

            bool isAvailable = true;

            var onGround = false;
            var atCeiling = false;

            for (var w = -characterWidth / 2; w < characterWidth / 2; w++)
            {
                if (position.x + w < 0)
                {
                    isAvailable = false;
                    break;
                }

                index = NavMeshHelper.GetIndex(position, matrixHeight);
                if (!matrix[index].IsAvailable)
                {
                    isAvailable = false;
                }

                if (position.y - 1 < 0)
                {
                    onGround = false;
                }
                else if (NavMeshHelper.OnGround(matrix, matrixHeight, position))
                {
                    onGround = true;
                }
                index = NavMeshHelper.GetIndex(position.x + w, position.y + characterHeight, matrixHeight);
                if (!matrix[index].IsAvailable)
                {
                    atCeiling = true;
                }
            }

            if (!isAvailable)
            {
                continue;
            }

            isAvailable = CheckLocation(currentVertex, matrix, matrixHeight, characterWidth, characterHeight, 1) && CheckLocation(currentVertex, matrix, matrixHeight, characterWidth, characterHeight, -1);

            if (!isAvailable)
            {
                continue;
            }

            if (onGround && parentVertex.JumpLength > 4 && parentVertex.PositionInMatrix.y == position.y)
            {
                continue;
            }

            var jumpLength = GetNodeVertex(parentPosition.x, parentPosition.y, parentVertex.Z, nodes, matrixHeight, matrixWidth, layerCount).JumpLength;

            var newJumpLength = GetNeighbourJumpLength(onGround, atCeiling, position, parentPosition, jumpLength, maxJumpLength);

            if (jumpLength >= 0 && jumpLength % 2 != 0 && position.x != parentPosition.x)
            {
                continue;
            }

            if (jumpLength >= maxJumpLength && position.y > parentPosition.y)
            {
                continue;
            }

            if (newJumpLength >= maxJumpLength + 6 && position.x != parentPosition.x && (newJumpLength - (maxJumpLength + 6)) % 8 != 3)
                continue;


            currentVertex.JumpLength = newJumpLength;
            var length = GetNodeVertexLength(position.x, currentVertex.PositionInMatrix.y, nodes, matrixHeight, matrixWidth, layerCount);

            if (length > 0)
            {
                int lowestJump = int.MaxValue;

                for (int l = 0; l < length; l++)
                {
                    var jump = GetNodeVertex(position.x, position.y, l, nodes, matrixHeight, matrixWidth, layerCount).JumpLength;

                    if (jump < lowestJump)
                        lowestJump = jump;
                }

                if (lowestJump <= newJumpLength)
                    continue;
            }

            currentVertex.Z = length;

            currentVertex.DistanceFromStart = math.distance(position,
                   targetVertex.PositionInMatrix);
            currentVertex.DistanceToFinish = math.distance(position,
                targetVertex.PositionInMatrix);
            currentVertex.Score = currentVertex.DistanceFromStart + currentVertex.DistanceToFinish;
            currentVertex.ParentMatrixPosition = parentPosition;
            currentVertex.ParentZ = parentVertex.Z;
            neighbors.Add(currentVertex);

            if (length > layerCount)
            {
                AddNodesLayer(nodes, matrixHeight, matrixWidth, ref layerCount);
            }

            SetNodeVertex(currentVertex, currentVertex.PositionInMatrix.x, currentVertex.PositionInMatrix.y, length, nodes, matrixHeight, matrixWidth, layerCount);
            //nodes.Add(currentVertex);
        }

        return neighbors;
    }

    private static int GetNeighbourJumpLength(bool onGround, bool atCeiling, int2 neighbourPosition, int2 parentPosition, int parentJumpLength, int maxJumpLength)
    {
        var jumpLength = parentJumpLength;

        if (onGround)
        {
            jumpLength = 0;
        }
        else if (atCeiling)
        {
            if (neighbourPosition.x != parentPosition.x)
                jumpLength = math.max(maxJumpLength + 1, jumpLength + INCREMENT_JUMP_SIDEWAYS);
            else
                jumpLength = math.max(maxJumpLength, jumpLength + INCREMENT_JUMP_UP);
        }
        else if (neighbourPosition.y > parentPosition.y)
        {
            if (parentJumpLength < 2)
            {
                jumpLength = INITIAL_JUMP_FORCE;
            }
            else if (parentJumpLength % 2 == 0)
            {
                jumpLength = parentJumpLength + INCREMENT_JUMP_UP;
            }
            else
            {
                jumpLength = parentJumpLength + INCREMENT_JUMP_SIDEWAYS;
            }
        }
        else if (neighbourPosition.y < parentPosition.y)
        {
            if (parentJumpLength % 2 == 0)
            {
                jumpLength = math.max(maxJumpLength, parentJumpLength + INCREMENT_JUMP_UP);
            }
            else
            {
                jumpLength = math.max(maxJumpLength, parentJumpLength + INCREMENT_JUMP_SIDEWAYS);
            }
        }
        else if (neighbourPosition.x != parentPosition.x)
        {
            jumpLength = parentJumpLength + INCREMENT_JUMP_SIDEWAYS;
        }

        return jumpLength;
    }

    /// <summary>
    /// Возвращает вершину в коллекции c наименьшим значением Score
    /// </summary>
    private static Vertex GetBestVertex(NativeList<Vertex> list)
    {
        var bestVertex = list[0];

        for (int i = 1; i < list.Length; i++)
        {
            if (list[i].Score < bestVertex.Score)
            {
                bestVertex = list[i];
            }
        }

        return bestVertex;
    }

    /// <summary>
    /// Возвращает родителя вершины
    /// </summary>
    /// <param name="childVertex"></param>
    private static Vertex GetVertexParent(NativeList<Vertex> nodes, Vertex childVertex, int matrixHeight, int matrixWidth, int layerCount)
    {
        var parentPosition = childVertex.ParentMatrixPosition;
        var parentZ = childVertex.ParentZ;

        var parent = GetNodeVertex(parentPosition.x, parentPosition.y, parentZ, nodes, matrixHeight, matrixWidth, layerCount);

        return parent;
    }

    /// <summary>
    /// Проверяет, не больше ли расстояние от начальной точки до текущей по оси Y максимально возможного
    /// </summary>
    private static bool CheckMaxFallHeight(float startY, float currentY)
    {
        var distance = math.abs(startY - currentY);
        return distance >= MAX_FALL_HEIGTH;
    }

    /// <summary>
    /// Проверяет доступность вершины для персонажа
    /// </summary>
    /// <param name="vertex">Вершина для проверки</param>
    /// <param name="matrix">Матрица вершин</param>
    /// <param name="characterWidth">Ширина персонажа</param>
    /// <param name="characterHeight">Высота персонажа</param>
    /// <param name="characterSide">Сторона, с которой идет проверка</param>
    /// <returns></returns>
    internal static bool CheckLocation(Vertex vertex, NativeArray<Vertex> matrix, int matrixHeight, int characterWidth, int characterHeight, int characterSide)
    {
        var nextVertex = vertex;
        var y = nextVertex.PositionInMatrix.y;
        var pivotPosition = vertex.PositionInMatrix;
        var x = nextVertex.PositionInMatrix.x;
        var lastX = x + characterSide * characterWidth / 2;
        while (math.abs(lastX - x) > 0)
        {
            var index = NavMeshHelper.GetIndex(x, y, matrixHeight);
            nextVertex = matrix[index];

            if (!NavMeshHelper.CheckVerticalVertices(matrix, matrixHeight, nextVertex.PositionInMatrix, pivotPosition.y + characterHeight - y, 1))
            {
                return false;
            }
            if (!nextVertex.IsAvailable)
            {
                y++;
            }
            x += characterSide;
        }


        return true;
    }

    private static Vertex GetNodeVertex(int x, int y, int z, NativeList<Vertex> nodes, int matrixHeight, int matrixWidth, int layerCount)
    {
        var index = y + x * matrixHeight + z * matrixHeight * matrixWidth;
        var vertex = nodes[index];

        return vertex;
    }

    private static void SetNodeVertex(Vertex vertex, int x, int y, int z, NativeList<Vertex> nodes, int matrixHeight, int matrixWidth, int layerCount)
    {
        var index = y + x * matrixHeight + z * matrixHeight * matrixWidth;
        vertex.IsNode = true;
        nodes[index] = vertex;
    }

    private static short GetNodeVertexLength(int x, int y, NativeList<Vertex> nodes, int matrixHeight, int matrixWidth, int layerCount)
    {
        if (layerCount == -1)
        {
            return 0;
        }
        short lenght = 0;
        for (int i = 0; i <= layerCount; i++)
        {
            var vertex = GetNodeVertex(x, y, i, nodes, matrixHeight, matrixWidth, layerCount);

            if (vertex.IsNode)
            {
                lenght++;
            }

        }

        return lenght;
    }

    private static void AddNodesLayer(NativeList<Vertex> nodes, int matrixHeight, int matrixWidth, ref int layerCount)
    {
        for (int i = 0; i < matrixHeight * matrixWidth; i++)
        {
            nodes.Add(new Vertex());
        }

        layerCount++;
    }

    private static NativeArray<int2> GetDirections()
    {
        var directions = new NativeArray<int2>(4, Allocator.Temp);
        directions[0] = new int2(0, -1);
        directions[1] = new int2(1, 0);
        directions[2] = new int2(0, 1);
        directions[3] = new int2(-1, 0);

        return directions;
    }
}

