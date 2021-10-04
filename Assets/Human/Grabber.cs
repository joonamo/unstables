using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grabber : MonoBehaviour
{
    public string button;
    public HingeJoint2D joint;
    public float strength = 1.0f;
    public bool grabbed = false;
    Human human;
    public Grabber other;
    GameManager gm;

    // Start is called before the first frame update
    void Start()
    {
        human = GetComponentInParent<Human>();
        gm = GameManager.GetGameManager();
    }

    public void Ungrab() {
        joint.enabled = false;
        grabbed = false;
    }

    Vector2 getAnchorPoint() {
        var p3 = transform.TransformPoint(joint.anchor);
        return new Vector2(p3.x, p3.y);
    }

    // Update is called once per frame
    void Update()
    {
        bool tryingToGrab =
            human.stamina > 0.0f && (
                Input.GetButtonDown(button) ||
                (!other.grabbed && Input.GetButton(button))
            );

        if (!grabbed && tryingToGrab) {
            var anchor = getAnchorPoint();
            foreach (var part in human.horseParts) {
                var closestPoint = part.ClosestPoint(anchor);
                var distance = (closestPoint - anchor).SqrMagnitude();
                if (distance < 0.001f) {
                    grabbed = true;
                    joint.enabled = true;
                    joint.connectedBody = part;

                    var horse = part.GetComponentInParent<Horse>();
                    if (!horse.grabSound.isPlaying && (Time.timeScale < 0.1 || Random.Range(0.0f, 1.0f) < 0.3)) {
                        horse.grabSound.pitch = Random.Range(0.90f, 1.1f);
                        horse.grabSound.Play();
                    }
                    break;
                }
            }
        } else if (Input.GetButtonUp(button)) {
            Ungrab();
        }
    }
}
