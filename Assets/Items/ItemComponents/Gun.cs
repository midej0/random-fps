using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float fireRate;
    [SerializeField] private int magSize;
    [SerializeField] private bool automatic;
    [SerializeField] private float headshotMultiplier;
    [SerializeField] private LayerMask mask;

    private PlayerInputHandler playerInputHandler = PlayerInputHandler.instance;
    public int bulletsInMag;
    private Camera mainCam;
    private float lastFire = 0;

    void Start()
    {
        mainCam = Camera.main;
        bulletsInMag = magSize;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInputHandler.isUsing && Time.time >= lastFire + fireRate && bulletsInMag > 0)
        {
            Shoot();
        }

        if(playerInputHandler.reloadTriggered && bulletsInMag < magSize)
        {
            Reload();
        }
    }

    private void Shoot()
    {
        RaycastHit hit;
            if(Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, 1000, mask))
            {
                Debug.Log(hit.collider.name);
            }
            lastFire = Time.time;
            bulletsInMag--;
            Debug.Log(name);
            if (!automatic)
            {
                playerInputHandler.isUsing = false;
            }
    }

    private void Reload()
    {
        bulletsInMag = magSize;
        playerInputHandler.reloadTriggered = false;
    }
}
