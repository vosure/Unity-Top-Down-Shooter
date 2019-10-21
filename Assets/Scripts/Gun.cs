using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    public Transform muzzle;
    public Projectile projectile;
    public float msBetweenShoots = 100;
    public float muzzleVelocity = 35;

    public Transform shell;
    public Transform shellEjection;

    float nextShotTime;

    Muzzleflash muzzleflash;

    private void Start()
    {
        muzzleflash = GetComponent<Muzzleflash>();
    }

    public void Shoot()
    {
        if (Time.time > nextShotTime)
        {
            nextShotTime = Time.time + msBetweenShoots / 1000.0f;
            Projectile projectile = Instantiate(this.projectile, muzzle.position, muzzle.rotation) as Projectile;
            projectile.setSpeed(muzzleVelocity);

            Instantiate(shell, shellEjection.position, shellEjection.rotation);
            muzzleflash.Activate();
        }
    }
}
