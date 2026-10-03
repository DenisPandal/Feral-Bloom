using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapScroller : MonoBehaviour
{
    public float scrollSpeed = 1f;
    private Vector3 positionStart;
    [SerializeField] enum eScrollDirection { up, down, left, right }
    [SerializeField] eScrollDirection scrollDirection = eScrollDirection.right;
    float directionX, directionY;

    void Start()
    {
        positionStart = transform.position;
        directionX = 1;
        directionY = 1;
        if (scrollDirection == eScrollDirection.left)
        {
            directionX = -1;
        }
        if (scrollDirection == eScrollDirection.down)
        {
            directionY = -1;
        }

    }

    void Update()
    {
        if (scrollDirection == eScrollDirection.left || scrollDirection == eScrollDirection.right)
        {
            transform.position = positionStart + Vector3.right * Mathf.Repeat(Time.time * scrollSpeed * directionX, transform.localScale.x);
        }
        if (scrollDirection == eScrollDirection.up || scrollDirection == eScrollDirection.down)
        {
            transform.position = positionStart + Vector3.up * Mathf.Repeat(Time.time * scrollSpeed * directionY, transform.localScale.x);
        }
    }
}
