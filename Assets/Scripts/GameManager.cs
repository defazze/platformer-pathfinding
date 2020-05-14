using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Unity.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private float _maxJumpRadius = 2f;
    public List<Node> nodes = new List<Node>();
    public List<Edge> edges = new List<Edge>();
    public Dictionary<int, BoxCollider2D> nodeColliders = new Dictionary<int, BoxCollider2D>();

    public static GameManager Instance;
    public GameManager()
    {
        Instance = this;
    }

    void Start()
    {
        var colliders = FindObjectsOfType<BoxCollider2D>();

        var index = 0;
        foreach (var collider in colliders)
        {
            var width = collider.size.x * collider.transform.localScale.x;
            var height = collider.size.y * collider.transform.localScale.y;

            var position = collider.transform.position;
            var rotation = collider.transform.rotation;

            var leftCorner = rotation * (new Vector2(-width / 2, height / 2)) + position;
            var rightCorner = rotation * (new Vector2(width / 2, height / 2)) + position;

            nodes.Add(new Node { start = (Vector2)leftCorner, end = (Vector2)rightCorner, id = index });
            nodeColliders.Add(index, collider);
            index++;
        }

        var points = nodes.SelectMany(n => new[] { new NodePoint { position = n.start, node = n }, new NodePoint { position = n.end, node = n } }).ToArray();
        var nativePoints = new NativeArray<NodePoint>(points, Allocator.Persistent);

        var map = EdgeCalculator.Calculate(nativePoints, _maxJumpRadius);

        var keys = map.GetKeyArray(Allocator.Temp).Distinct().ToArray();

        for (int i = 0; i < keys.Length; i++)
        {
            var edges = map.GetValuesForKey(keys[i]);

            foreach (var edge in edges)
            {
                var tmp = edge;
                tmp.cost = 1;
                this.edges.Add(tmp);
            }

        }

        map.Dispose();
        nativePoints.Dispose();
    }

    void Update()
    {

    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        foreach (var node in nodes)
        {
            Gizmos.DrawLine((Vector2)node.start, (Vector2)node.end);
        }

        Gizmos.color = Color.red;
        foreach (var edge in edges)
        {
            Gizmos.DrawLine((Vector2)edge.start, (Vector2)edge.end);
        }
    }
}
