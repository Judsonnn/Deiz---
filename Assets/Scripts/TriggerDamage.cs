using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerDamage : MonoBehaviour
{
    public HeartSystem heart;
    private void OnCollisionEnter2D(Collision2d colision)
    {
        if (collision.gameObject.tag == "Player")
            heart.vida--;
    }
}
