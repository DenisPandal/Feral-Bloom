using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChainBall : MonoBehaviour
{
    [SerializeField] float angleStart = -90;
    [SerializeField] float angleEnd = 90;
    float angleCurrent;
    [SerializeField] float delayAtStart = 0.2f;
    float timeElapsed;
    bool swingRight;
    [SerializeField] float swingSpeed = 90f;

    void Start()
    {
        if (angleStart > angleEnd)
        {
            swingRight = true;
        }
        angleCurrent = transform.rotation.z;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed < delayAtStart)
        {
            return;
        }

        if (swingRight)
        {
            angleCurrent += swingSpeed * Time.deltaTime;
            if (angleCurrent >= angleEnd)
            {
                swingRight = false;
            }
        }
        if (!swingRight)
        {
            angleCurrent -= swingSpeed * Time.deltaTime;
            if (angleCurrent <= angleStart)
            {
                swingRight = true;
            }
            
        }
        transform.rotation = Quaternion.Euler(0, 0, angleCurrent);
    }
}
