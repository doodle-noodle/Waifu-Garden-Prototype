// =============================================================================
// SlotLabelUI.cs  |  Scripts/UI
// WaifuGarden — Phase 2
// A small label that appears directly above a hovered grid slot.
// Shows plant name and status (grow time, "Ready for harvest!", "Wants to evolve!")
// Attach to a root-level Canvas child named "SlotLabel".
// Design the visual in the Inspector — this script only sets text and position.
// =============================================================================

using UnityEngine;
using TMPro;

public class SlotLabelUI : MonoBehaviour
{
    public static SlotLabelUI Instance { get; private set; }

    // -------------------------------------------------------------------------
    [Header("References")]
    [Tooltip("Root GameObject of the label panel. Shown/hidden by this script.")]
    public GameObject LabelPanel;

    [Tooltip("TextMeshProUGUI showing the plant name and status.")]
    public TextMeshProUGUI StatusText;

    [Header("Position")]
    [Tooltip("How many pixels above the slot centre the label appears.")]
    public float VerticalOffset = 70f;

    // -------------------------------------------------------------------------
    private RectTransform _rect;
    private SlotController _currentSlot;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (LabelPanel != null) _rect = LabelPanel.GetComponent<RectTransform>();
        Hide();
    }

    // -------------------------------------------------------------------------
    // Public API — called by SlotController.OnPointerEnter / OnPointerExit
    // -------------------------------------------------------------------------

    public void Show(SlotController slot)
    {
        if (LabelPanel == null || slot == null || slot.OccupyingPlant == null) return;
        _currentSlot = slot;
        LabelPanel.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        _currentSlot = null;
        if (LabelPanel != null) LabelPanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Update: keep text current (remaining time changes every frame) and
    // keep position locked above the slot.
    // -------------------------------------------------------------------------

    private void Update()
    {
        if (LabelPanel == null || !LabelPanel.activeSelf) return;
        if (_currentSlot == null || _currentSlot.OccupyingPlant == null) { Hide(); return; }
        Refresh();
        PositionAboveSlot();
    }

    private void Refresh()
    {
        if (StatusText == null || _currentSlot?.OccupyingPlant == null) return;
        StatusText.text = _currentSlot.OccupyingPlant.GetStatusText();
    }

    private void PositionAboveSlot()
    {
        if (_rect == null || _currentSlot == null) return;

        // Get the slot's screen position and offset upward.
        RectTransform slotRT = _currentSlot.GetComponent<RectTransform>();
        if (slotRT == null) return;

        // Use the slot's world position (works for any Canvas mode).
        Vector3 slotScreenPos = RectTransformUtility.WorldToScreenPoint(null, slotRT.position);
        slotScreenPos.y += VerticalOffset;

        // Clamp horizontally so the label never goes off screen.
        float halfWidth = _rect.rect.width * 0.5f;
        slotScreenPos.x = Mathf.Clamp(slotScreenPos.x, halfWidth, Screen.width - halfWidth);

        _rect.position = slotScreenPos;
    }
}
