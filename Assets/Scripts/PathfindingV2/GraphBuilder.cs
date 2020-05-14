using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class GraphBuilder
{
    public static List<Node> GetNodes(IEnumerable<BoxCollider2D> colliders)
    {
        var result = new List<Node>();

        var index = 0;
        foreach (var collider in colliders)
        {
            var width = collider.size.x * collider.transform.localScale.x;
            var height = collider.size.y * collider.transform.localScale.y;

            var position = collider.transform.position;
            var rotation = collider.transform.rotation;

            var leftCorner = rotation * (new Vector2(-width / 2, height / 2)) + position;
            var rightCorner = rotation * (new Vector2(width / 2, height / 2)) + position;

            result.Add(new Node { start = (Vector2)leftCorner, end = (Vector2)rightCorner, id = index });
            index++;
        }

        return result;
    }

    public static Dictionary<int, List<Edge>> GetGraph(IEnumerable<Node> nodes, float jumpRadius)
    {
        var result = new Dictionary<int, List<Edge>>();

        var points = nodes.SelectMany(n => new[] { new NodePoint { position = n.start, node = n }, new NodePoint { position = n.end, node = n } }).ToArray();
        var nativePoints = new NativeArray<NodePoint>(points, Allocator.TempJob);

        var map = new NativeMultiHashMap<int, Edge>(points.Length / 2, Allocator.TempJob);

        var job = new CalculateEdgeJob
        {
            map = map,
            points = nativePoints,
            maxJumpRadius = jumpRadius
        };

        job.Run();

        var keys = map.GetKeyArray(Allocator.Temp).Distinct().ToArray();

        for (int i = 0; i < keys.Length; i++)
        {
            var edges = new List<Edge>();
            foreach (var edge in map.GetValuesForKey(keys[i]))
            {
                var tmp = edge;
                tmp.cost = 1;
                edges.Add(tmp);
            }
            result.Add(i, edges);
        }

        nativePoints.Dispose();
        map.Dispose();

        return result;
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