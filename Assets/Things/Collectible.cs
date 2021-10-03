using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public int score = 1;
    public bool active = true;
    public Vector3 targetPosition;
    public float flyTime = 3.0f;
    public float flyTimeDeviation = 1.0f;
    public float expireTime = 40.0f;

    public float floatDev = 0.1f;
    public float floatSpeed = 2.0f;

    float spawnTime;
    bool flying = true;
    Vector3 startPosition;
    float flyHeight;

    // Start is called before the first frame update
    void Start()
    {
        spawnTime = Time.time;
        startPosition = transform.position;
        flyTime = flyTime + Random.Range(flyTimeDeviation, flyTimeDeviation);
        flyHeight = Info.screenHeight - targetPosition.y;
        flyHeight = Random.Range(flyHeight * 0.5f, flyHeight);
    }

    void Update() {
        if (flying) {
            var phase = Mathf.Clamp01((Time.time - spawnTime) / flyTime);
            var height = flyHeight * Mathf.Sin(Mathf.PI * phase);
            var pos = Vector3.Lerp(startPosition, targetPosition, phase);
            transform.position = new Vector3(pos.x, pos.y + height, pos.z);
            if (phase >= 1.0f) {
                flying = false;
            }
        } else {
            var floatTimer = Time.time - spawnTime - flyTime;
            if (floatTimer > expireTime) {
                Destroy(this.gameObject);
            } else {
                transform.position = new Vector3(
                    targetPosition.x,
                    targetPosition.y + Mathf.Sin(-floatTimer * floatSpeed) * floatDev,
                    targetPosition.z);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (active && other.gameObject.tag == "Human") {
            GameManager.GetGameManager().ReportScore(score);
            active = false;
            Destroy(this.gameObject);
        }
    }
}
