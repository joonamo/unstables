using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public GameObject piece1;
    public GameObject piece2;

    public float speed;

    public float extent = 17.8f;

    // Update is called once per frame
    void Update()
    {
        float phase = (Time.time * speed) % 1.0f;
        float phase2 = (phase + 0.5f) % 1.0f;

        var target = piece1.transform.position;

        piece1.transform.position = new Vector3(
            Mathf.Lerp(-extent, extent, phase),
            target.y,
            target.z
        );
        piece2.transform.position = new Vector3(
            Mathf.Lerp(-extent, extent, phase2),
            target.y,
            target.z
        );
    }
}
