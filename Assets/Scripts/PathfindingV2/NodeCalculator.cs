using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Mathematics;

public static class NodeCalculator
{
    public static int GetCurrentNode(List<Node> nodes, float2 position, float radius)
    {
        var nodesArr = new NativeArray<NodeStruct>(nodes.Select(n =>
        new NodeStruct
        {
            start = n.start,
            end = n.end,
            id = n.id
        }).ToArray(), Allocator.Temp);

        for (int i = 0; i < nodesArr.Length; i++)
        {
            var node = nodesArr[i];
            if (node.start.x <= position.x && node.end.x >= position.x)
            {
                var closestPoint = MathHelper.GetClosestPoint(node.start, node.end, position);
                if (math.distance(position, closestPoint) <= radius)
                {
                    return node.id;
                }
            }
        }

        return -1;
    }
}