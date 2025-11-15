using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private float damage;
    [SerializeField] private float fireRate;
    [SerializeField] private int magSize;
    [SerializeField] private bool automatic;

    private PlayerInputHandler playerInputHandler = PlayerInputHandler.instance;
    private int bulletsInMag;
    private float lastFire = 0;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (playerInputHandler.isUsing && Time.time >= lastFire + fireRate && bulletsInMag > 0)
        {
            lastFire = Time.time;
            bulletsInMag--;
            Debug.Log("Pew");
            if (!automatic)
            {
                playerInputHandler.isUsing = false;
            }
        }
    }
}
