using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

public static class EdgeCalculator
{
    public static NativeMultiHashMap<int, Edge> Calculate(NativeArray<NodePoint> points, float maxJumpRadius)
    {
        var map = new NativeMultiHashMap<int, Edge>(points.Length / 2, Allocator.Persistent);


        var job = new CalculateEdgeJob
        {
            map = map,
            points = points,
            maxJumpRadius = maxJumpRadius
        };

        job.Schedule().Complete();
        return map;
    }

    [BurstCompile]
    private struct CalculateEdgeJob : IJob
    {
        public NativeMultiHashMap<int, Edge> map;
        public NativeArray<NodePoint> points;
        public float maxJumpRadius;
        public void Execute()
        {
            for (int i = 0; i < points.Length; i++)
            {
                for (int j = 0; j < points.Length; j++)
                {
                    if (points[i].node.id != points[j].node.id)
                    {
                        if (math.distance(points[i].position, points[j].position) <= maxJumpRadius)
                        {
                            var start = points[i].position;
                            var end = points[j].position;

                            var startNode = points[i].node;
                            var endNode = points[j].node;

                            var edge = new Edge { start = start, end = end, node = endNode };
                            map.Add(startNode.id, edge);
                        }
                    }

                }
            }
        }
    }
}