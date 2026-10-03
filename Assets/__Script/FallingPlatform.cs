using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    Rigidbody2D rb;
    SpriteRenderer sr;
    [SerializeField] float delay = 0.5f;
    [SerializeField] int voidLayer = 31;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            StartCoroutine(Fall());
        }
    }

    IEnumerator Fall()
    {
        SoundFXManager.Play("Falling");
        for (int i = 0; i < 3; i++)
        {
            SetAlpha(1.0f);
            yield return new WaitForSeconds(0.1f);
            SetAlpha(0.5f);
            yield return new WaitForSeconds(0.1f);
        }
        yield return new WaitForSeconds(delay);
        rb.bodyType = RigidbodyType2D.Dynamic;
        gameObject.layer = voidLayer;
        Destroy(gameObject, 1);
    }
    void SetAlpha(float alpha)
    {
        Color color = sr.color;
        color.a = alpha;
        sr.color = color;
    }
}
