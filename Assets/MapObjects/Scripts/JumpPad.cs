using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    //The speed at which the launchpad launches you, launches in direction of launchpad.
    [SerializeField] private float launchForce;
    //The speed in the y direction.
    [SerializeField] private float launchYForce;

    private void OnTriggerEnter(Collider other)
    {
        Vector3 launchVec = transform.forward * launchForce;
        launchVec += transform.up * launchYForce;
        if (launchVec.magnitude == 0)
        {
            Debug.LogWarning("Launch Velocity Is Zero, Please Increase The Values");
        }

        if (other.CompareTag("player"))
        {
            other.GetComponent<PlayerMovement>().AddVelocity(launchVec);
        }
    }
}
