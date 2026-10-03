using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Trap : MonoBehaviour
{
    public int damage = 1;
    public static UnityAction<int, Vector3, bool> OnPlayerHit;
    Vector3 position;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            position = transform.position;
            OnPlayerHit?.Invoke(damage, position, false);
        }
    }
}
