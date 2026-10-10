using System.Collections;
using UnityEngine;

public class BossArenaController : MonoBehaviour
{
    [Header("--- Arena Barriers ---")]
    [SerializeField] private GameObject entranceBarrier;
    [SerializeField] private GameObject exitBarrier;

    [Header("--- Victory Rewards ---")]
    [SerializeField] private GameObject victoryDoor;

    [Header("--- Boss ---")]
    [SerializeField] private BossController boss;

    private bool fightStarted = false;

    void Start()
    {
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
        if (exitBarrier != null) exitBarrier.SetActive(false);
        if (victoryDoor != null) victoryDoor.SetActive(false);

        BossController.OnBossDefeated += HandleBossDefeated;
    }

    void OnDestroy()
    {
        BossController.OnBossDefeated -= HandleBossDefeated;
    }

    // El jugador debe estar bien dentro de la arena (lejos de la barrera de entrada) para cerrarla
    public const float EntryX = 99f;
    private Transform playerTf;

    void Update()
    {
        if (fightStarted) return;
        if (playerTf == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTf = p.transform;
            return;
        }
        if (playerTf.position.x >= EntryX)
        {
            StartBossFight();
        }
    }

    public void StartBossFight()
    {
        if (fightStarted) return;
        fightStarted = true;

        // Cerrar la arena para que el jugador no pueda escapar
        if (entranceBarrier != null) entranceBarrier.SetActive(true);
        if (exitBarrier != null) exitBarrier.SetActive(true);

        // Desactivar el trigger para evitar re-entradas
        var col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        // Activar al jefe
        if (boss != null)
        {
            boss.ActivateBoss();
        }
    }

    private void HandleBossDefeated()
    {
        StartCoroutine(OpenArenaRoutine());
    }

    private IEnumerator OpenArenaRoutine()
    {
        yield return new WaitForSeconds(1.5f);

        // Desbloquear barreras
        if (entranceBarrier != null) entranceBarrier.SetActive(false);
        if (exitBarrier != null) exitBarrier.SetActive(false);

        // Revelar puerta de salida para completar el nivel
        if (victoryDoor != null)
        {
            victoryDoor.SetActive(true);
            SoundFXManager.Play("LevelProgress");
        }
    }
}
