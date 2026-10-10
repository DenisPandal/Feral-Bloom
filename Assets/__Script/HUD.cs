using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.PlayerLoop.EarlyUpdate;

public class HUD : MonoBehaviour
{
    [SerializeField] Image heartPrefab;
    [SerializeField] TextMeshProUGUI scoreText;
    [SerializeField] GameObject pointText;
    [SerializeField] RectTransform uiCanvas;
    List<Image> hearts = new List<Image>();
    RectTransform rt;
    int healthPrevValue = 0;

    [Header("--- Frame Settings ---")]
    [Tooltip("Ajusta estos valores para cambiar el tamaño/posición del Marco de Vida. Ej: X=-40, Y=-30")]
    [SerializeField] Vector2 framePaddingMin = new Vector2(-30, -20);
    [SerializeField] Vector2 framePaddingMax = new Vector2(30, 20);

    [Header("--- Leaves Positioning ---")]
    [Tooltip("Controla la distancia entre cada hoja de vida.")]
    [SerializeField] Vector2 leafSpacing = new Vector2(60, 0);
    [Tooltip("Posición inicial de la primera hoja.")]
    [SerializeField] Vector2 startOffset = new Vector2(250, 0);


    void Awake()
    {
        // Forzamos el offset para que el Inspector no lo sobreescriba con valores antiguos
        startOffset = new Vector2(250, 0);

        rt = GetComponentInParent<RectTransform>();
        PlayerHealth.OnHealthChanged += HealthUpdate;
        GameManager.OnScoreChanged += ScoreUpdate;
        Enemy.OnGettingPoint += PointShow;
        Item.OnGettingPoint += PointShow;
    }

    void Start()
    {
        ScoreUpdate();
        
        // Add Marco de vida background
        GameObject frameObj = new GameObject("MarcoVida", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        frameObj.transform.SetParent(transform, false);
        frameObj.transform.SetAsFirstSibling(); // Render behind the hearts
        
        var layout = frameObj.GetComponent<LayoutElement>();
        layout.ignoreLayout = true; // Don't let HorizontalLayoutGroup squash it
        
        var img = frameObj.GetComponent<Image>();
        Sprite marcoSprite = Resources.Load<Sprite>("Marco de vida");
        if (marcoSprite != null) 
        {
            img.sprite = marcoSprite;
            // You can change preserveAspect to true if the frame looks stretched
            img.preserveAspect = false; 
        }
        
        var frameRt = frameObj.GetComponent<RectTransform>();
        frameRt.anchorMin = Vector2.zero;
        frameRt.anchorMax = Vector2.one;
        // Padding around the hearts so the frame is larger than the container
        frameRt.offsetMin = framePaddingMin;
        frameRt.offsetMax = framePaddingMax;
    }

    void OnDisable()
    {
        PlayerHealth.OnHealthChanged -= HealthUpdate;
        GameManager.OnScoreChanged -= ScoreUpdate;
        Enemy.OnGettingPoint -= PointShow;
        Item.OnGettingPoint -= PointShow;
    }

    void PointShow(Vector3 pos, int point)
    {

        GameObject floatingText = Instantiate(pointText);
        floatingText.transform.position = pos;
        PointAnimation pointAnimation = floatingText.GetComponent<PointAnimation>();
        pointAnimation.point = point;
    }

    void ScoreUpdate()
    {
        scoreText.text = GameManager.score.ToString("N0");
    }

    void HealthUpdate(int health)
    {
        if (healthPrevValue > health)
        {
            healthDelete(health);
        }
        if (healthPrevValue < health)
        {
            healthAdd(health);
        }
        healthPrevValue = health;
    }

    void healthAdd(int health)
    {
        for (int i = healthPrevValue; i < health; i++)
        {
            Image newHeart = Instantiate(heartPrefab, transform);
            
            // Remove animator and use custom sprite
            Animator anim = newHeart.GetComponent<Animator>();
            if (anim != null) Destroy(anim);
            
            Sprite vidaSprite = Resources.Load<Sprite>("vida");
            if (vidaSprite != null) 
            {
                newHeart.sprite = vidaSprite;
                newHeart.preserveAspect = true;
            }
            
            // Manually position each leaf
            RectTransform heartRt = newHeart.GetComponent<RectTransform>();
            heartRt.anchorMin = new Vector2(0, 0.5f);
            heartRt.anchorMax = new Vector2(0, 0.5f);
            heartRt.pivot = new Vector2(0, 0.5f);
            heartRt.anchoredPosition = startOffset + (leafSpacing * i);

            hearts.Add(newHeart);
        }
    }    
    void healthDelete(int health)
    {
        // Remove from the end to avoid shifting issues
        for (int i = healthPrevValue - 1; i >= health; i--)
        {
            if (i >= 0 && i < hearts.Count)
            {
                if (hearts[i] != null && hearts[i].gameObject != null)
                {
                    Destroy(hearts[i].gameObject);
                }
                hearts.RemoveAt(i);
            }
        }
    }
}
