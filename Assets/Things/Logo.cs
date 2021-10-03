using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logo : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        var phase = Mathf.Sin(Time.unscaledTime) * 0.5f + 0.5f;
        transform.eulerAngles = new Vector3(0.0f, 0.0f, Mathf.Lerp(-7.0f, 7.0f, phase));
    }
}
