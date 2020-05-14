using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private float _maxJumpRadius = 2f;
    public List<Node> nodes = new List<Node>();
    public Dictionary<int, List<Edge>> graph;
    public static GameManager Instance;
    public GameManager()
    {
        Instance = this;
    }

    void Start()
    {
        var colliders = FindObjectsOfType<BoxCollider2D>();

        nodes = GraphBuilder.GetNodes(colliders);
        graph = GraphBuilder.GetGraph(nodes, _maxJumpRadius);
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

        if (graph != null)
        {
            Gizmos.color = Color.red;
            var edges = graph.Values.Aggregate(new List<Edge>(), (total, next) => { total.AddRange(next); return total; });
            foreach (var edge in edges)
            {
                Gizmos.DrawLine((Vector2)edge.start, (Vector2)edge.end);
            }
        }
    }
}
