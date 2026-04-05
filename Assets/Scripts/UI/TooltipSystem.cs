// =============================================================================
// TooltipSystem.cs  |  Scripts/UI
// WaifuGarden — Pre-Phase 2 Fixes
// Switched from legacy Input.mousePosition to UnityEngine.InputSystem.Mouse.
// =============================================================================

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }

    [Header("Tooltip Panel Root")]
    public GameObject TooltipPanel;

    [Header("Text Fields")]
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI TypeText;
    public TextMeshProUGUI DescriptionText;

    [Header("Offset from cursor (pixels)")]
    public Vector2 Offset = new Vector2(16f, -16f);

    private RectTransform _rect;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (TooltipPanel != null) _rect = TooltipPanel.GetComponent<RectTransform>();
        Hide();
    }

    // -------------------------------------------------------------------------

    public void Show(string itemName, string itemType, string description)
    {
        if (TooltipPanel == null) return;
        if (NameText        != null) NameText.text        = itemName;
        if (TypeText        != null) TypeText.text        = itemType;
        if (DescriptionText != null) DescriptionText.text = description;
        TooltipPanel.SetActive(true);
    }

    public void Hide()
    {
        if (TooltipPanel != null) TooltipPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------

    private void Update()
    {
        if (_rect == null || TooltipPanel == null || !TooltipPanel.activeSelf) return;

        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 mousePos = mouse.position.ReadValue();
        PositionNearMouse(mousePos);
    }

    private void PositionNearMouse(Vector2 mousePos)
    {
        float width  = _rect.rect.width;
        float height = _rect.rect.height;

        float x = mousePos.x + Offset.x;
        float y = mousePos.y + Offset.y;

        // Flip left if right edge goes off screen.
        if (x + width > Screen.width)
            x = mousePos.x - width - Mathf.Abs(Offset.x);

        // Flip up if bottom edge goes off screen.
        if (y - height < 0)
            y = mousePos.y + Mathf.Abs(Offset.y) + height;

        // Final clamp as safety net.
        x = Mathf.Clamp(x, 0, Screen.width  - width);
        y = Mathf.Clamp(y, height, Screen.height);

        _rect.position = new Vector3(x, y, 0f);
    }
}
