using UnityEngine;

public class ChainBallRotate : MonoBehaviour
{
    [SerializeField] bool clockWise;
    float angleCurrent;
    [SerializeField] float delayAtStart = 0.2f;
    float timeElapsed;
    [SerializeField] float swingSpeed = 90f;

    void Start()
    {
        angleCurrent = transform.rotation.z;
    }

    void Update()
    {
        timeElapsed += Time.deltaTime;
        if (timeElapsed < delayAtStart)
        {
            return;
        }

        if (clockWise)
        {
            angleCurrent -= swingSpeed * Time.deltaTime;
        }
        if (!clockWise)
        {
            angleCurrent += swingSpeed * Time.deltaTime;
        }
        transform.rotation = Quaternion.Euler(0, 0, angleCurrent);
    }
}
