using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Vector2 _pivotOffset;
    private float _buttomOffset;
    private bool _isMove;
    private List<Node> _path;
    private Vector2 _targetPoint;

    void Start()
    {

        var collider = GetComponent<CapsuleCollider2D>();
        var scale = GetComponent<Transform>().localScale;
        var pivotPosition = GetComponent<Transform>().GetChild(0).localPosition;
        _pivotOffset = new Vector2(pivotPosition.x * scale.x, pivotPosition.y * scale.y);

        var height = scale.y * collider.size.y;
        _buttomOffset = height / 2 + _pivotOffset.y + 0.05f;

    }

    void Update()
    {
        if (_isMove)
        {
            var point = (Vector2)transform.position + _pivotOffset;
            var currentNodeId = NodeCalculator.GetCurrentNode(GameManager.Instance.nodes, point, _buttomOffset, out var closestPoint);

            if (currentNodeId == -1)
            {
                //мы в прыжке

            }
            else
            {
                //мы где-то стоим
                var node = GameManager.Instance.nodes.Single(n => n.id == currentNodeId);
                var nodeIndex = _path.IndexOf(node);

                var targetPoint = _targetPoint;
                if (nodeIndex < (_path.Count - 1))
                {
                    var nextNode = _path[nodeIndex + 1];
                    var possibleEdges = GameManager.Instance.graph[node.id];
                    var edge = possibleEdges.Single(e => e.node.id == nextNode.id);
                    targetPoint = edge.start;

                    //Debug.Log($"current node: {currentNodeId}, nextNode: {nextNode.id}, target point: {targetPoint}");
                }

                var body = GetComponent<Rigidbody2D>();
                var distance = Vector2.Distance(point, targetPoint);
                if (distance > _buttomOffset)
                {
                    var velocity = targetPoint - (Vector2)closestPoint;
                    velocity.Normalize();
                    velocity *= GameManager.Instance._speed;

                    body.velocity = velocity;
                }
                else
                {
                    //Debug.Log("Point!");
                    body.velocity = Vector2.zero;
                }
            }
        }
        //Debug.Log(currentNode);
    }

    public void OnMove(InputValue value)
    {
        /*
        var val = value.Get<Vector2>();
        var delta = (int)val.x;

        var body = GetComponent<Rigidbody2D>();
        if (val.x > 0 && val.y > 0)
        {
            body.AddForce(new Vector2(2, 2), ForceMode2D.Impulse);
        }
        else if (delta != 0)
        {
            var velocity = body.velocity;
            velocity.x = delta * 3;
            body.velocity = velocity;
        }
        */
    }

    public void OnClick(InputValue value)
    {
        var val = value.Get<float>();
        if (val == 1)
        {
            Vector2 pos = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            RaycastHit2D hit = Physics2D.Raycast(pos, -Vector2.up);

            if (hit.collider != null && hit.distance < 2f)
            {
                var nodes = GameManager.Instance.nodes;

                foreach (var node in nodes)
                {
                    var inSegment = MathHelper.IsPointInSegment(node.start, node.end, hit.point);

                    if (inSegment)
                    {
                        var currentNodeId = NodeCalculator.GetCurrentNode(GameManager.Instance.nodes, (Vector2)transform.position, 1f, out var _);
                        if (currentNodeId != -1)
                        {
                            var targetNode = node;
                            var currentNode = nodes.Single(n => n.id == currentNodeId);
                            _path = PathBuilder.Search(GameManager.Instance.graph, nodes, currentNode, targetNode);
                            _isMove = true;
                            _targetPoint = hit.point;

                            /*
                            foreach (var pathNode in _path)
                            {
                                Debug.Log(pathNode.id);
                            }*/
                        }

                        break;
                    }
                }
            }
        }
    }
}
