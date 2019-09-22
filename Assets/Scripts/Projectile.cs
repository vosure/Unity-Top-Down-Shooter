using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    float speed = 10.0f;

    public void setSpeed(float speed)
    {
        this.speed = speed;
    }

    void Update()
    {
        transform.Translate(Vector3.forward * Time.deltaTime * speed);     
    }
}
