using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Horse : MonoBehaviour
{
    public float oscillateMin = 0.0f;
    public float oscillateMax = 0.0f;
    public float oscillateSpeed = 1.0f;
    float oscillateOffset = 0.0f;
    public Rigidbody2D rb;

    public List<SpringJoint2D> springs = new List<SpringJoint2D>();

    public Vector2 minBounds;
    public Vector2 maxBounds;

    float moveSpeed = 0.0f;
    public float moveSpeedMax = 2.0f;
    public float moveSpeedMin = 1.0f;
    public float acceleration = 1.0f;

    Vector2 moveVec = Vector2.zero;
    Vector2 targetPos = Vector2.zero;

    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        oscillateSpeed += Random.Range(-0.1f, 0.1f);
        oscillateOffset = Random.Range(0.0f, 5.0f);

        gm = GameManager.GetGameManager();

        ChooseTarget();
    }

    public void ChooseTarget() {
        targetPos = new Vector3(
            Random.Range(minBounds.x, maxBounds.x),
            Random.Range(minBounds.y, maxBounds.y),
            transform.position.z
        );
        moveSpeed = Random.Range(moveSpeedMin, moveSpeedMax);
    }

    // Update is called once per frame
    void Update()
    {
        var phase = (Mathf.Sin((Time.time + oscillateOffset) * oscillateSpeed) + 1.0f) * 0.5f;

        rb.MoveRotation(Mathf.Lerp(oscillateMin, oscillateMax, phase));

        var footPhase = phase;
        foreach (var spring in springs) {
            spring.distance = phase * 0.6f;
            footPhase = (phase + 0.1f) % 1.0f;
        }

        if (gm.phase > GamePhase.oneHorse) {
            var toTarget = targetPos - rb.position;
            if (toTarget.sqrMagnitude < 0.2) {
                ChooseTarget();
            } else {
                moveVec = Vector3.Lerp(moveVec, toTarget.normalized * moveSpeed, acceleration * Time.deltaTime);
                rb.MovePosition(rb.position + moveVec * Time.deltaTime);
            }
        }
    }
}
