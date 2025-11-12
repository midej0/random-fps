using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraEffects : MonoBehaviour
{
    [HideInInspector] public float cameraTilt = 0;

    private Camera mainCam;

    void Awake()
    {
        mainCam = Camera.main;
    }

    public IEnumerator TiltCam(float startAngle, float endAngle, float duration)
    {
        float counter = 0f;

        while(counter < duration)
        {
            counter += Time.unscaledDeltaTime;
            cameraTilt = Mathf.Lerp(startAngle, endAngle, counter / duration);
            mainCam.transform.localRotation = Quaternion.Euler(0,0, cameraTilt);
            yield return null;
        }
    }
}
