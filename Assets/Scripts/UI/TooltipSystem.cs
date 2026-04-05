// =============================================================================
// TooltipSystem.cs  |  Scripts/UI
// WaifuGarden — Phase 1 Update 2
// Follows the mouse cursor. Works with Screen Space — Overlay canvas.
// Automatically flips to stay on screen when near edges.
// =============================================================================

using UnityEngine;
using TMPro;

public class TooltipSystem : MonoBehaviour
{
    public static TooltipSystem Instance { get; private set; }

    [Header("Tooltip Panel Root")]
    [Tooltip("The root GameObject of the tooltip box. Shown/hidden by this script.")]
    public GameObject TooltipPanel;

    [Header("Text Fields")]
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI TypeText;
    public TextMeshProUGUI DescriptionText;

    [Header("Offset from cursor (pixels)")]
    [Tooltip("How far right and down from the cursor the tooltip appears by default.")]
    public Vector2 Offset = new Vector2(16f, -16f);

    // -------------------------------------------------------------------------
    private RectTransform _rect;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (TooltipPanel != null)
            _rect = TooltipPanel.GetComponent<RectTransform>();

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
    // Position tooltip every frame while visible.
    // Uses raw screen coordinates — works with Screen Space Overlay with no camera.
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (_rect == null || !TooltipPanel.activeSelf) return;

        Vector2 mouse  = Input.mousePosition;
        float   width  = _rect.rect.width;
        float   height = _rect.rect.height;

        // Start with the default offset (right and below cursor).
        float x = mouse.x + Offset.x;
        float y = mouse.y + Offset.y;

        // Flip left if the right edge would go off screen.
        if (x + width > Screen.width)
            x = mouse.x - width - Mathf.Abs(Offset.x);

        // Flip up if the bottom edge would go off screen.
        if (y - height < 0)
            y = mouse.y + Mathf.Abs(Offset.y) + height;

        // Clamp as a final safety net.
        x = Mathf.Clamp(x, 0, Screen.width  - width);
        y = Mathf.Clamp(y, height, Screen.height);

        // Screen Space Overlay: anchoredPosition is not used.
        // We set the world position directly instead.
        _rect.position = new Vector3(x, y, 0f);
    }
}
