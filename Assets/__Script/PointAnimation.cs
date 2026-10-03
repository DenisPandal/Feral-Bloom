using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PointAnimation : MonoBehaviour
{
    public AnimationCurve curve;
    [SerializeField] float duration = 1.5f;
    float elapsedTime;
    RectTransform rt;
    TMP_Text textMeshPro;
    public int point;
    public Material[] materials;
    public float moveAmount = 20f;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        textMeshPro = GetComponent<TMP_Text>();
        textMeshPro.text = point.ToString();
        StartCoroutine(Step1());
        StartCoroutine(Step2());

    }

    IEnumerator Step1()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + new Vector3(0, moveAmount, 0);

        float elapsedTime = 0f;

        while (elapsedTime < duration + 0.1f)
        {
            rt.anchoredPosition = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / duration));
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    IEnumerator Step2()
    {
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            textMeshPro.fontMaterial = materials[0];
            yield return new WaitForSeconds(0.25f);
            textMeshPro.fontMaterial = materials[1];
            yield return new WaitForSeconds(0.25f);
        }
    }
}
