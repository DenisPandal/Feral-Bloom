using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavController : MonoBehaviour
{
    [SerializeField] List<Transform> navTransforms = new List<Transform>();
    [SerializeField] List<float> navDuration = new List<float>();
    [SerializeField] float delay;

    List<Vector3> navPoints = new List<Vector3>();
    int navCount;
    Rigidbody2D rb;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        navPoints.Clear();
        for (int i = 0; i < navTransforms.Count; i++)
        {
            navPoints.Add(navTransforms[i].position);
        }
        navCount = navPoints.Count;
        StartCoroutine(Navigate());
    }

    IEnumerator Navigate()
    {
        float eplasedTime;
        int next;
        while (true)
        {
            for (int i = 0; i < navCount; i++)
            {
                eplasedTime = 0;
                next = i + 1;
                if (next == navCount)
                {
                    next = 0;
                }

                while (eplasedTime < navDuration[i])
                {
                    eplasedTime += Time.deltaTime;
                    float t = eplasedTime / navDuration[i];
                    float x = Mathf.Lerp(navPoints[i].x, navPoints[next].x, t);
                    float y = Mathf.Lerp(navPoints[i].y, navPoints[next].y, t);
                    transform.position = new Vector3(x, y, 0);

                    yield return null;
                }
                yield return new WaitForSeconds(delay);
            }
        }
    }
}
