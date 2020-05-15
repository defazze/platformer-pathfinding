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

    public static Dictionary<int, List<Edge>> GetGraph(IEnumerable<Node> nodes, float jumpRadius, float padding = 0)
    {
        var result = new Dictionary<int, List<Edge>>();

        var nativeNodes = new NativeArray<NodeStruct>(nodes.Select(n => new NodeStruct
        {
            start = n.start,
            end = n.end,
            id = n.id
        }).ToArray(), Allocator.TempJob);

        var map = new NativeMultiHashMap<int, EdgeStruct>(nodes.Count(), Allocator.TempJob);

        var job = new CalculateEdgeJob
        {
            map = map,
            nodes = nativeNodes,
            jumpRadius = jumpRadius,
            padding = padding
        };

        job.Run();

        var keys = map.GetKeyArray(Allocator.Temp).Distinct().ToArray();
        var c = 0;
        for (int i = 0; i < keys.Length; i++)
        {
            var edges = new List<Edge>();
            foreach (var item in map.GetValuesForKey(keys[i]))
            {
                var edge = new Edge { start = item.start, end = item.end };
                edge.node = nodes.Single(n => n.id == item.nodeId);
                edges.Add(edge);
                c++;
            }
            result.Add(keys[i], edges);
        }


        nativeNodes.Dispose();
        map.Dispose();

        return result;
    }

    [BurstCompile]
    private struct CalculateEdgeJob : IJob
    {
        public NativeMultiHashMap<int, EdgeStruct> map;
        public NativeArray<NodeStruct> nodes;
        public float jumpRadius;
        public float padding;

        public void Execute()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                for (int j = 0; j < nodes.Length; j++)
                {
                    var startNode = nodes[i];
                    var endNode = nodes[j];

                    if (startNode.id != endNode.id)
                    {
                        var edged = false;
                        var startPoint = float2.zero;
                        var endPoint = float2.zero;

                        //Конечная нода справа от начальной, нет пересечения по x
                        if (endNode.start.x >= startNode.end.x)
                        {
                            if (math.distance(startNode.end, endNode.start) <= jumpRadius)
                            {
                                startPoint = ApplyPadding(startNode.end, startNode.start);
                                endPoint = ApplyPadding(endNode.start, endNode.end);
                                edged = true;
                            }
                        }
                        //Конечная нода слева от начальной, нет пересечения по x
                        else if (endNode.end.x <= startNode.start.x)
                        {
                            if (math.distance(startNode.start, endNode.end) <= jumpRadius)
                            {
                                startPoint = ApplyPadding(startNode.start, startNode.end);
                                endPoint = ApplyPadding(endNode.end, endNode.start);
                                edged = true;
                            }
                        }

                        if (edged)
                        {
                            var edge = new EdgeStruct();
                            edge.start = startPoint;
                            edge.end = endPoint;
                            edge.nodeId = endNode.id;

                            map.Add(startNode.id, edge);
                        }
                    }
                }
            }
        }

        private float2 ApplyPadding(float2 point, float2 paddingVector)
        {
            var result = math.normalize(paddingVector - point) * padding + point;
            return result;
        }
    }
}