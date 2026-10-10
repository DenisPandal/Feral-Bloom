using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealthBar : MonoBehaviour
{
    public static BossHealthBar Instance { get; private set; }

    [Header("--- UI References ---")]
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI bossNameText;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("--- Colors ---")]
    [SerializeField] private Color healthColor = new Color(0.72f, 0.18f, 0.98f, 1f); // Morado vibrante neón

    private float targetFill = 1f;

    void Awake()
    {
        Instance = this;
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (fillImage != null)
        {
            fillImage.color = healthColor;
        }
        Hide();
    }

    void OnEnable()
    {
        Instance = this;
        if (fillImage != null)
        {
            fillImage.color = healthColor;
        }
    }

    void Update()
    {
        if (fillImage != null)
        {
            fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * smoothSpeed);
            if (Mathf.Abs(fillImage.fillAmount - targetFill) < 0.005f)
            {
                fillImage.fillAmount = targetFill;
            }
        }
    }

    public void Show(string bossName, int currentHealth, int maxHealth)
    {
        gameObject.SetActive(true);
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (fillImage != null) fillImage.color = healthColor;
        if (bossNameText != null) bossNameText.text = bossName;
        targetFill = Mathf.Clamp01((float)currentHealth / Mathf.Max(1, maxHealth));
        if (fillImage != null) fillImage.fillAmount = targetFill;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        targetFill = Mathf.Clamp01((float)currentHealth / Mathf.Max(1, maxHealth));
    }

    public void Hide()
    {
        StartCoroutine(HideRoutine());
    }

    private System.Collections.IEnumerator HideRoutine()
    {
        // Wait until the bar has visually emptied (or close enough) before hiding
        if (targetFill <= 0.01f)
        {
            while (fillImage != null && fillImage.fillAmount > 0.05f)
            {
                yield return null;
            }
            yield return new WaitForSeconds(0.5f); // Pequena pausa dramatica
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
