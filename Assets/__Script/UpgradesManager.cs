using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class UpgradesManager : MonoBehaviour
{
    private UIDocument uiDocument;
    private VisualElement rootElement;
    private Button closeBtn;
    public static bool isUpgradesOpen = false;

    void Awake()
    {
        uiDocument = GetComponent<UIDocument>();
    }

    void Start()
    {
        if (uiDocument != null && uiDocument.rootVisualElement != null)
        {
            rootElement = uiDocument.rootVisualElement;
            rootElement.style.display = DisplayStyle.None;
            isUpgradesOpen = false;

            closeBtn = rootElement.Q<Button>("CloseUpgradesBtn");
            if (closeBtn != null)
            {
                closeBtn.clicked += CloseUpgrades;
            }
        }
    }

    void OnDisable()
    {
        if (closeBtn != null)
        {
            closeBtn.clicked -= CloseUpgrades;
        }
        isUpgradesOpen = false;
    }

    void Update()
    {
        // Toggle con tecla K
        if (Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
        {
            ToggleUpgrades();
            return;
        }

        // Si está abierto y presiona Escape, también puede cerrarlo
        if (isUpgradesOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseUpgrades();
        }
    }

    public void ToggleUpgrades()
    {
        if (isUpgradesOpen)
        {
            CloseUpgrades();
        }
        else
        {
            OpenUpgrades();
        }
    }

    public void OpenUpgrades()
    {
        if (!GameManager.isGameOn) return;

        isUpgradesOpen = true;
        Time.timeScale = 0f;

        if (rootElement != null)
        {
            rootElement.style.display = DisplayStyle.Flex;

            if (GameManager.hasDash)
            {
                var slot2 = rootElement.Q<VisualElement>("Slot2");
                if (slot2 != null && slot2.ClassListContains("slot-locked"))
                {
                    slot2.RemoveFromClassList("slot-locked");
                    slot2.AddToClassList("slot-unlocked");
                    
                    var mysteryContainer = slot2.Q<VisualElement>(null, "mystery-container");
                    if (mysteryContainer != null) 
                    {
                        mysteryContainer.Clear();
                        mysteryContainer.RemoveFromClassList("mystery-container");
                        mysteryContainer.AddToClassList("power-art-container");
                        
                        var dashIcon = new VisualElement();
                        dashIcon.style.width = 60;
                        dashIcon.style.height = 60;
                        Texture2D tex = Resources.Load<Texture2D>("Dash");
                        if (tex != null) dashIcon.style.backgroundImage = new StyleBackground(tex);
                        dashIcon.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
                        mysteryContainer.Add(dashIcon);
                    }
                    
                    var leaf = slot2.Q<Label>(null, "leaf-locked");
                    if (leaf != null) 
                    {
                        leaf.text = "🌿";
                        leaf.RemoveFromClassList("leaf-locked");
                        leaf.AddToClassList("leaf-unlocked");
                    }
                    
                    var nameLbl = slot2.Q<Label>(null, "slot-name-locked");
                    if (nameLbl != null) 
                    {
                        nameLbl.text = "IMPULSO DASH";
                        nameLbl.RemoveFromClassList("slot-name-locked");
                        nameLbl.AddToClassList("slot-name-unlocked");
                    }
                }
            }
        }
    }

    public void CloseUpgrades()
    {
        isUpgradesOpen = false;
        Time.timeScale = 1f;

        if (rootElement != null)
        {
            rootElement.style.display = DisplayStyle.None;
        }
    }
}
