using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Melee : MonoBehaviour
{
    [SerializeField] private float range;
    [SerializeField] private float damage;
    [SerializeField] private float attackspeed;
    [SerializeField] private LayerMask mask;
    
    private float lastAttack;
    private Camera mainCam;
    private PlayerInputHandler playerInputHandler;

    // Start is called before the first frame update
    void Start()
    {
        mainCam = Camera.main;
        playerInputHandler = PlayerInputHandler.instance;
    }

    // Update is called once per frame
    void Update()
    {
        if(playerInputHandler.isUsing && Time.time >= lastAttack + attackspeed)
        {
            Attack();
        }
    }

    private void Attack()
    {
        RaycastHit hit;
        if(Physics.Raycast(mainCam.transform.position, mainCam.transform.forward, out hit, range, mask))
        {
            Debug.Log(hit.collider.name);
        }
        lastAttack = Time.time;
    }
}
