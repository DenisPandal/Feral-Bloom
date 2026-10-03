using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Item : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rb;
    Vector3 positionOrig;
    [SerializeField] int score;
    [SerializeField] float moveDuration = 1.5f;
    [SerializeField] float moveAmount = 1f;
    [SerializeField] string sound;
    public static UnityAction<Vector3, int> OnGettingPoint;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        positionOrig = transform.position;
        StartCoroutine(Modulate());
    }

    IEnumerator Modulate()
    {
        float elaptedTime = 0f;
        float Offset = 0f;
        
        while (true)
        {
            elaptedTime += Time.deltaTime;
            Offset = Mathf.Sin(Time.time / moveDuration * (2 * Mathf.PI)) * moveAmount;
            transform.position = new Vector3(positionOrig.x, positionOrig.y + Offset, positionOrig.z);
            yield return null;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            gameObject.layer = 13;
            SoundFXManager.Play(sound);
            animator.SetTrigger("collected");
            GameManager.ScoreAdd(score);
            OnGettingPoint?.Invoke(transform.position, score);
            Destroy(gameObject, 0.5f);
        }
    }

    void OnDisable()
    {
        StopCoroutine(Modulate());
    }
}
