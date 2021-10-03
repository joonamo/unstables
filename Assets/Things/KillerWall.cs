using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KillerWall : MonoBehaviour
{

    GameManager gm;

    void Start() {
        gm = GameManager.GetGameManager();
    }

    void OnCollisionEnter2D(Collision2D other) {
        if (other.gameObject.tag == "Human" && !gm.human.anyGrab) {
            gm.ReportDeath();
        }
    }

}
