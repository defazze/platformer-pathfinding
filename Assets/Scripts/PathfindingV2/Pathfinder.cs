using System.Collections.Generic;
using System.Linq;
using Unity.Collections;

public static class Pathfinder
{
    public static void Search(NativeMultiHashMap<int, Edge> map, List<Node> nodes, Node start, Node end)
    {
        nodes.ForEach(n => n.distanceFromStart = -1);
        start.distanceFromStart = 0;

        var opens = new List<Node>();
        opens.Add(start);

        do
        {
            opens = opens.OrderBy(n => n.distanceFromStart).ToList();
            var node = opens.First();
            opens.Remove(node);

            var edges = GetEdges(map.GetValuesForKey(node.id)).OrderBy(e => e.cost);

            foreach (var edge in edges)
            {
                var connectedNode = edge.node;
                if (!connectedNode.visited)
                {
                    if (connectedNode.distanceFromStart == -1 || connectedNode.distanceFromStart > node.distanceFromStart + edge.cost)
                    {
                        connectedNode.distanceFromStart = node.distanceFromStart + edge.cost;
                    }
                }
            }

        } while (opens.Any());
    }

    private static List<Edge> GetEdges(NativeMultiHashMap<int, Edge>.Enumerator edges)
    {
        var result = new List<Edge>();

        foreach (var edge in edges)
        {
            result.Add(edge);
        }

        return result;
    }
}
