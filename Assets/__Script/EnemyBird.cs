using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBird : Enemy
{
    [SerializeField] LayerMask platformMask;

    [SerializeField] List<Transform> navTransforms = new List<Transform>();
    [SerializeField] List<float> navDuration = new List<float>();
    [SerializeField] float delay;
    
    List<Vector3> navPoints = new List<Vector3>();
    int navCount;


    protected override void Start()
    {
        base.Start();
        navPoints.Clear();
        for (int i = 0; i < navTransforms.Count; i++)
        {
            navPoints.Add(navTransforms[i].position);
        }
        navCount = navPoints.Count;
        
        animator.SetBool("isAlive", true);
        StartCoroutine(EnemyMove());
    }

    bool isFacingRightPrev;

    IEnumerator EnemyMove()
    {
        float eplasedTime;
        int next;

        while (isAlive)
        {
            for (int i = 0; i < navCount; i++)
            {
                eplasedTime = 0;
                next = i + 1;
                if (next == navCount)
                {
                    next = 0;
                }

                isFacingRightPrev = isFacingRight;
                isFacingRight = (navPoints[i].x < navPoints[next].x) ? true : false;
                if (isFacingRightPrev != isFacingRight)
                {
                    DirectionChange();
                }

                while (eplasedTime < navDuration[i] && isAlive)
                {
                    eplasedTime += Time.deltaTime;
                    float t = eplasedTime / navDuration[i];
                    float x = Mathf.Lerp(navPoints[i].x, navPoints[next].x, t);
                    float y = Mathf.Lerp(navPoints[i].y, navPoints[next].y, t);
                    transform.position = new Vector3(x, y, 0);

                    yield return null;
                }
                if (isAlive)
                {
                    yield return new WaitForSeconds(delay);
                }
            }
        }
        animator.SetBool("isAlive", false);
    }

    void DirectionChange()
    {
        transform.Rotate(new Vector3(0, 180, 0));
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = UnityEngine.Color.red;
        headPosition = head.transform.position;
        Gizmos.DrawWireCube(headPosition, headSize);
    }
}
