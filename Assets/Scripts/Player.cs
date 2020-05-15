using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Vector2 _pivotOffset;
    private float _buttomOffset;
    private List<Node> _path;
    private Vector2? _targetPoint;

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
        if (_targetPoint.HasValue)
        {
            var point = (Vector2)transform.position + _pivotOffset;
            var currentNodeId = NodeCalculator.GetCurrentNode(GameManager.Instance.nodes, point, _buttomOffset, out var closestPoint);

            if (currentNodeId == -1)
            {
                //мы в прыжке, пока ничего делать не надо

            }
            else
            {
                //мы на поверхности, надо куда то двигаться
                var node = GameManager.Instance.nodes.Single(n => n.id == currentNodeId);
                var nodeIndex = _path.IndexOf(node);

                var targetPoint = _targetPoint.Value;
                var nextPoint = _targetPoint.Value;

                float jumpAngle = 0;

                if (nodeIndex < (_path.Count - 1))
                {
                    var nextNode = _path[nodeIndex + 1];
                    var possibleEdges = GameManager.Instance.graph[node.id];
                    var edge = possibleEdges.Single(e => e.node.id == nextNode.id);
                    targetPoint = edge.start;
                    nextPoint = edge.end;
                    jumpAngle = edge.jumpAngle;
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
                    if (nextPoint == _targetPoint)
                    {
                        Debug.Log("Target!");
                        body.velocity = Vector2.zero;
                        _targetPoint = null;
                    }
                    else
                    {
                        //мы достигли края одного узла, прыгаем на другой
                        var jumpPoint = closestPoint;

                        float gravity = Physics2D.gravity.magnitude;

                        float angle = jumpAngle;
                        var jumpDistance = Mathf.Abs(jumpPoint.x - nextPoint.x);
                        var jumpHeight = jumpPoint.y - nextPoint.y;

                        float initialVelocity = (1 / Mathf.Cos(angle)) * Mathf.Sqrt((0.5f * gravity * Mathf.Pow(jumpDistance, 2)) / (jumpDistance * Mathf.Tan(angle) + jumpHeight));
                        Vector2 velocity = new Vector2(initialVelocity * Mathf.Cos(angle), initialVelocity * Mathf.Sin(angle));

                        if (nextPoint.x - jumpPoint.x < 0)
                        {
                            velocity = -Vector2.Reflect(velocity, Vector2.up);
                        }

                        body.velocity = velocity;
                    }
                }
            }
        }
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
                        var currentNodeId = NodeCalculator.GetCurrentNode(nodes, (Vector2)transform.position, 1f, out var _);
                        if (currentNodeId != -1)
                        {
                            var targetNode = node;
                            var currentNode = nodes.Single(n => n.id == currentNodeId);
                            _path = PathBuilder.Search(GameManager.Instance.graph, nodes, currentNode, targetNode, out var pathExist);

                            if (pathExist)
                            {
                                _targetPoint = hit.point;

                                /*
                                                                Debug.Log($"Current node: {currentNode.id}, target node: {targetNode.id}");
                                                                foreach (var pathNode in _path)
                                                                {
                                                                    Debug.Log(pathNode.id);
                                                                }*/
                            }
                            else
                            {
                                _targetPoint = null;
                                Debug.Log("No way :(");
                            }
                        }

                        break;
                    }
                }
            }
        }
    }
}
