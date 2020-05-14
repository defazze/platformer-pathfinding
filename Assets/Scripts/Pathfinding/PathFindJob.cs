using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct PathFindJob : IJob
{
    public PathFinderData data;
    public NativeList<PathPoint> path;
    public MatrixConfig matrixConfig;

    public void Execute()
    {
        var findingPath = PathFinder.FindPath(data, matrixConfig);

        for (int i = 0; i < findingPath.Length; i++)
        {
            path.Add(findingPath[i]);
        }
    }
}
