#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class SetupBossArena
{
    [MenuItem("Tools/Setup Boss Arena")]
    public static void Setup()
    {
        // 1. Limpiar instancias previas
        var oldArena = GameObject.Find("BossArena");
        if (oldArena != null) Object.DestroyImmediate(oldArena);
        var oldBoss = GameObject.Find("SmokeBoss");
        if (oldBoss != null) Object.DestroyImmediate(oldBoss);

        // 2. Mapear sprites del Jefe de Humo
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Bosses/Jefe de Humo (2).png");
        var spriteDict = new Dictionary<string, Sprite>();
        foreach (var s in sprites)
        {
            if (s is Sprite sp) spriteDict[sp.name] = sp;
        }

        Sprite[] GetFrames(string[] names)
        {
            var list = new List<Sprite>();
            foreach (var n in names)
            {
                if (spriteDict.ContainsKey(n)) list.Add(spriteDict[n]);
            }
            return list.ToArray();
        }

        var idle = GetFrames(new string[] { "Jefe de Humo (2)_1", "Jefe de Humo (2)_2", "Jefe de Humo (2)_5", "Jefe de Humo (2)_9", "Jefe de Humo (2)_10", "Jefe de Humo (2)_13", "Jefe de Humo (2)_14", "Jefe de Humo (2)_15" });
        var charge = GetFrames(new string[] { "Jefe de Humo (2)_54", "Jefe de Humo (2)_55", "Jefe de Humo (2)_58", "Jefe de Humo (2)_80", "Jefe de Humo (2)_85" });
        var attack = GetFrames(new string[] { "Jefe de Humo (2)_120", "Jefe de Humo (2)_139", "Jefe de Humo (2)_142" });
        var hurt = GetFrames(new string[] { "Jefe de Humo (2)_162", "Jefe de Humo (2)_169", "Jefe de Humo (2)_178" });

        var projPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/BossProjectile.prefab");

        // 3. Configurar Barra de Vida MORADA en el Canvas "UI" (Activa y visible arriba en la pantalla)
        BossHealthBar bossHealthBarComp = null;
        var canvas = GameObject.Find("UI");
        if (canvas != null)
        {
            var oldBar = canvas.transform.Find("BossHealthBar");
            if (oldBar != null) Object.DestroyImmediate(oldBar.gameObject);

            var barGo = new GameObject("BossHealthBar", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(BossHealthBar));
            barGo.transform.SetParent(canvas.transform, false);

            var rt = barGo.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -70f); // Ubicada en la parte superior del Game View
            rt.sizeDelta = new Vector2(850f, 36f);

            // Fondo de la barra: Púrpura oscuro obsidiana
            var bgImg = barGo.GetComponent<Image>();
            bgImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            bgImg.color = new Color(0.08f, 0.04f, 0.14f, 0.94f);

            // Borde exterior / Resplandor morado neón
            var outline = barGo.AddComponent<Outline>();
            outline.effectColor = new Color(0.85f, 0.35f, 1.0f, 0.95f);
            outline.effectDistance = new Vector2(3f, 3f);

            // Fill Bar (Relleno Morado Vibrante)
            var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fillGo.transform.SetParent(barGo.transform, false);
            var fillRt = fillGo.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = Vector2.one;
            fillRt.offsetMin = new Vector2(4f, 4f);
            fillRt.offsetMax = new Vector2(-4f, -4f);

            var fillImg = fillGo.GetComponent<Image>();
            fillImg.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            fillImg.type = Image.Type.Filled;
            fillImg.fillMethod = Image.FillMethod.Horizontal;
            fillImg.fillOrigin = 0;
            fillImg.fillAmount = 1f;
            fillImg.color = new Color(0.72f, 0.18f, 0.98f, 1f); // Morado vibrante neón

            // Título del Jefe
            var textGo = new GameObject("BossTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGo.transform.SetParent(barGo.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = new Vector2(0.5f, 1f);
            textRt.anchorMax = new Vector2(0.5f, 1f);
            textRt.pivot = new Vector2(0.5f, 0f);
            textRt.anchoredPosition = new Vector2(0f, 6f);
            textRt.sizeDelta = new Vector2(850f, 30f);

            var tmp = textGo.GetComponent<TextMeshProUGUI>();
            tmp.text = "TITÁN DE HUMO PRIMORDIAL";
            tmp.fontSize = 22f;
            tmp.fontStyle = FontStyles.Bold;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.85f, 1.0f, 1f); // Lavanda luminosa

            var cg = barGo.GetComponent<CanvasGroup>();
            cg.alpha = 0f; // Inicialmente oculta hasta acercarse al jefe

            // Serializar en BossHealthBar
            bossHealthBarComp = barGo.GetComponent<BossHealthBar>();
            var barSo = new SerializedObject(bossHealthBarComp);
            barSo.FindProperty("fillImage").objectReferenceValue = fillImg;
            barSo.FindProperty("bossNameText").objectReferenceValue = tmp;
            barSo.FindProperty("canvasGroup").objectReferenceValue = cg;
            barSo.FindProperty("healthColor").colorValue = new Color(0.72f, 0.18f, 0.98f, 1f);
            barSo.ApplyModifiedProperties();

            // Dejar ACTIVA la barra en el hierarchy de UI
            barGo.SetActive(true);
        }

        // 4. Crear el Jefe (SmokeBoss)
        var bossGo = new GameObject("SmokeBoss");
        bossGo.transform.position = new Vector3(118f, -4.5f, 0f);
        bossGo.transform.localScale = new Vector3(1.35f, 1.35f, 1f);

        var bSr = bossGo.AddComponent<SpriteRenderer>();
        bSr.sprite = idle.Length > 0 ? idle[0] : null;
        bSr.sortingLayerName = "Player";
        bSr.sortingOrder = 5;

        var bRb = bossGo.AddComponent<Rigidbody2D>();
        bRb.gravityScale = 0f;
        bRb.freezeRotation = true;
        bRb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var bCol = bossGo.AddComponent<CapsuleCollider2D>();
        bCol.size = new Vector2(1.6f, 2.2f);
        bCol.offset = new Vector2(0f, 0f);

        // FirePoint
        var fpGo = new GameObject("FirePoint");
        fpGo.transform.SetParent(bossGo.transform, false);
        fpGo.transform.localPosition = new Vector3(-0.6f, 0.1f, 0f);

        // HeadWeakness
        var headGo = new GameObject("HeadWeakness");
        headGo.transform.SetParent(bossGo.transform, false);
        headGo.transform.localPosition = new Vector3(0f, 1.25f, 0f);
        var headCol = headGo.AddComponent<BoxCollider2D>();
        headCol.isTrigger = true;
        headCol.size = new Vector2(2.0f, 0.7f);
        headGo.AddComponent<BossHeadWeakness>();

        // BossController
        var bossCtrl = bossGo.AddComponent<BossController>();
        var bSo = new SerializedObject(bossCtrl);
        bSo.FindProperty("maxHealth").intValue = 5;
        bSo.FindProperty("projectilePrefab").objectReferenceValue = projPrefab;
        bSo.FindProperty("firePoint").objectReferenceValue = fpGo.transform;
        bSo.FindProperty("spriteRenderer").objectReferenceValue = bSr;
        bSo.FindProperty("healthBar").objectReferenceValue = bossHealthBarComp;
        bSo.FindProperty("arenaMinX").floatValue = 95f;
        bSo.FindProperty("arenaMaxX").floatValue = 132f;
        bSo.FindProperty("groundY").floatValue = -7.5f;
        bSo.FindProperty("hoverY").floatValue = -4.5f;

        void PopulateArray(string propName, Sprite[] arr)
        {
            var p = bSo.FindProperty(propName);
            p.arraySize = arr.Length;
            for (int i = 0; i < arr.Length; i++)
            {
                p.GetArrayElementAtIndex(i).objectReferenceValue = arr[i];
            }
        }
        PopulateArray("idleFrames", idle);
        PopulateArray("chargeFrames", charge);
        PopulateArray("attackFrames", attack);
        PopulateArray("hurtFrames", hurt);
        bSo.ApplyModifiedProperties();

        // 5. Crear BossArena
        var arenaGo = new GameObject("BossArena");
        arenaGo.transform.position = Vector3.zero;

        var aCol = arenaGo.AddComponent<BoxCollider2D>();
        aCol.isTrigger = true;
        aCol.offset = new Vector2(96.5f, -5f);
        aCol.size = new Vector2(4f, 16f);

        // Barrera de entrada
        var enterBar = new GameObject("EntranceBarrier");
        enterBar.transform.SetParent(arenaGo.transform, false);
        enterBar.transform.position = new Vector3(94.2f, -6f, 0f);
        var ebCol = enterBar.AddComponent<BoxCollider2D>();
        ebCol.size = new Vector2(0.8f, 10f);
        var ebSr = enterBar.AddComponent<SpriteRenderer>();
        ebSr.sprite = bSr.sprite;
        ebSr.color = new Color(0.8f, 0.2f, 0.95f, 0.8f);
        ebSr.drawMode = SpriteDrawMode.Tiled;
        ebSr.size = new Vector2(0.8f, 10f);
        ebSr.sortingLayerName = "Player";
        ebSr.sortingOrder = 8;
        enterBar.SetActive(false);

        // Barrera de salida
        var exitBar = new GameObject("ExitBarrier");
        exitBar.transform.SetParent(arenaGo.transform, false);
        exitBar.transform.position = new Vector3(133.5f, -6f, 0f);
        var exCol = exitBar.AddComponent<BoxCollider2D>();
        exCol.size = new Vector2(0.8f, 10f);
        var exSr = exitBar.AddComponent<SpriteRenderer>();
        exSr.sprite = bSr.sprite;
        exSr.color = new Color(0.8f, 0.2f, 0.95f, 0.8f);
        exSr.drawMode = SpriteDrawMode.Tiled;
        exSr.size = new Vector2(0.8f, 10f);
        exSr.sortingLayerName = "Player";
        exSr.sortingOrder = 8;
        exitBar.SetActive(false);

        // Puerta de victoria
        var doorPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Prefabs/Door.prefab");
        var victoryDoor = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab, arenaGo.transform);
        victoryDoor.name = "VictoryDoor";
        victoryDoor.transform.position = new Vector3(131.2f, -8.6f, 0f);
        victoryDoor.SetActive(false);

        // BossArenaController
        var arenaCtrl = arenaGo.AddComponent<BossArenaController>();
        var aSo = new SerializedObject(arenaCtrl);
        aSo.FindProperty("entranceBarrier").objectReferenceValue = enterBar;
        aSo.FindProperty("exitBarrier").objectReferenceValue = exitBar;
        aSo.FindProperty("victoryDoor").objectReferenceValue = victoryDoor;
        aSo.FindProperty("boss").objectReferenceValue = bossCtrl;
        aSo.ApplyModifiedProperties();

        // 6. Guardar Escena
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        Debug.Log(">>> [SetupBossArena] Barra de vida MORADA superior visible en UI y Arena configuradas con éxito!");
    }
}
#endif
