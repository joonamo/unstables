using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : MonoBehaviour
{

    public List<Rigidbody2D> horseParts = new List<Rigidbody2D>();
    public Rigidbody2D torso;
    public float steerAmount = 1.0f;
    public float launchForce = 100.0f;

    public bool anyGrab = false;
    bool prevGrab = false;

    Grabber[] grabbers;

    // Start is called before the first frame update
    void Start()
    {
        grabbers = GameObject.FindObjectsOfType<Grabber>();
    }

    public void UpdateHorseParts(List<Horse> horses) {
        horseParts.Clear();

        foreach (var horse in horses) {
            foreach(var rb in horse.GetComponentsInChildren<Rigidbody2D>()) {
                 if (rb.gameObject.tag == "HorsePart") {
                     horseParts.Add(rb);
                 }
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        prevGrab = anyGrab;
        anyGrab = false;
        foreach (var grabber in grabbers) {
            if (grabber.grabbed) {
                anyGrab = true;
                break;
            }
        }

        Vector2 steerVec = Vector2.zero;
        if (Input.GetButton("steer_right")) {
            steerVec.x = 1.0f;
        } else if (Input.GetButton("steer_left")) {
            steerVec.x = -1.0f;
        } 
        if (Input.GetButton("steer_up")) {
            steerVec.y = 1.0f;
        } else if (Input.GetButton("steer_down")) {
            steerVec.y = -1.0f;
        }

        if (anyGrab) {
            torso.AddForce(steerVec * steerAmount, ForceMode2D.Force);
        } else if (prevGrab) {
            torso.AddForce(steerVec.normalized * launchForce, ForceMode2D.Impulse);
        }
    }
}