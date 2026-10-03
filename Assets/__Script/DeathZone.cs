using UnityEngine;
using UnityEngine.Events;

public class DeathZone : MonoBehaviour
{
    public static UnityAction<int, Vector3, bool> OnPlayerHit;


    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Vector3 position = transform.position;
            OnPlayerHit?.Invoke(5, position, true);
        }
    }

}
