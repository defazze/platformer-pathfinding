using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PathBuilder
{
    public static List<Node> Search(Dictionary<int, List<Edge>> map, List<Node> nodes, Node start, Node end)
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

            var edges = map[node.id].OrderBy(e => e.cost);

            foreach (var edge in edges)
            {
                var connectedNode = edge.node;
                if (!connectedNode.visited)
                {
                    if (connectedNode.distanceFromStart == -1 || connectedNode.distanceFromStart > node.distanceFromStart + edge.cost)
                    {
                        connectedNode.distanceFromStart = node.distanceFromStart + edge.cost;
                        connectedNode.nearestToStart = node;

                        if (!opens.Contains(connectedNode))
                        {
                            opens.Add(connectedNode);
                        }
                    }
                }
            }
            node.visited = true;
            if (node.id == end.id)
            {
                break;
            }

        } while (opens.Any());

        var path = new List<Node> { end };
        BuildPath(nodes, path, end);

        path.Reverse();

        return path;
    }

    private static void BuildPath(List<Node> allNodes, List<Node> path, Node node)
    {
        if (node.nearestToStart == null)
        {
            return;
        }

        var nearestNode = node.nearestToStart;
        path.Add(nearestNode);
        BuildPath(allNodes, path, nearestNode);
    }
}
