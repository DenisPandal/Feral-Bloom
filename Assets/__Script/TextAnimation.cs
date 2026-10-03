using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextAnimation : MonoBehaviour
{
    public AnimationCurve curve1;
    public AnimationCurve curve2;
    [SerializeField] float duration = 1.5f;
    float elapsedTime;
    RectTransform rt;
    TMP_Text textMeshPro;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        textMeshPro = GetComponent<TMP_Text>();
        StartCoroutine(Step1());
    }

    IEnumerator Step1()
    {
        Vector3 scaleOriginal = rt.localScale;

        elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = curve1.Evaluate(elapsedTime / duration);
            rt.localScale = scaleOriginal * scale;
            yield return null;
        }
        StartCoroutine(Step2());
    }

    IEnumerator Step2()
    {
        Vector3 scaleOriginal = rt.localScale;

        yield return new WaitForSeconds(1f);
        elapsedTime = 0;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float scale = curve2.Evaluate(elapsedTime / duration);
            rt.localScale = scaleOriginal * scale;
            yield return null;
        }
    }
}
