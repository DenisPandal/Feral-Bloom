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


    void Awake()
    {
        rt = GetComponentInParent<RectTransform>();
        PlayerHealth.OnHealthChanged += HealthUpdate;
        GameManager.OnScoreChanged += ScoreUpdate;
        Enemy.OnGettingPoint += PointShow;
        Item.OnGettingPoint += PointShow;
    }

    void Start()
    {
        ScoreUpdate();
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
            hearts.Add(newHeart);
        }
    }    
    void healthDelete(int health)
    {
        for (int i = health; i < healthPrevValue; i++)
        {
            if (i < hearts.Count)
            {
                hearts[i].GetComponent<Animator>().SetTrigger("isHit");
                Destroy(hearts[i].gameObject, 1.0f);
            }
        }

    }
}
