using UnityEngine;

public class BossHeadWeakness : MonoBehaviour
{
    private BossController boss;

    void Awake()
    {
        boss = GetComponentInParent<BossController>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        TryDamage(collision);
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        TryDamage(collision);
    }

    private void TryDamage(Collider2D collision)
    {
        if (boss == null) return;

        // Comprobar si el collider pertenece al jugador (directo o en jerarquía)
        var playerControl = collision.GetComponent<PlayerControl>() 
                         ?? collision.GetComponentInParent<PlayerControl>()
                         ?? collision.transform.root.GetComponent<PlayerControl>();

        if (playerControl != null)
        {
            var playerRb = playerControl.GetComponent<Rigidbody2D>();
            // Permitir pisotón si el jugador no está saliendo despedido hacia arriba a alta velocidad
            if (playerRb == null || playerRb.linearVelocityY <= 2.5f)
            {
                boss.TakeStompDamage(playerControl);
            }
        }
    }
}
