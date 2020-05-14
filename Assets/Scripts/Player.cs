using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private int delta;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        var currentNode = NodeCalculator.GetCurrentNode(GameManager.Instance.nodes, (Vector2)transform.position, 1f);
    }

    public void OnMove(InputValue value)
    {
        var val = value.Get<Vector2>();
        delta = (int)val.x;

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

                    //TODO: запуск алгоритма пасфайдинга
                }
            }
        }
    }
}
