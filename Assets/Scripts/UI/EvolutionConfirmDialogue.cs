// =============================================================================
// EvolutionConfirmDialogue.cs  |  Scripts/UI
// WaifuGarden — Phase 2
// Modal confirmation popup shown when the player clicks a glowing (evolution
// pending) plant slot.
// YES → calls GridManager.ConfirmEvolution (stub in Phase 2, full in Phase 6)
// NO  → dismisses without action
// Design the panel visually in the Inspector. Wire Yes/No buttons via OnClick.
// =============================================================================

using UnityEngine;
using TMPro;

public class EvolutionConfirmDialogue : MonoBehaviour
{
    public static EvolutionConfirmDialogue Instance { get; private set; }

    // -------------------------------------------------------------------------
    [Header("References")]
    [Tooltip("Root panel GameObject. Shown/hidden by this script.")]
    public GameObject DialoguePanel;

    [Tooltip("Text field for the prompt. Format: '{0} wants to evolve. Proceed?'")]
    public TextMeshProUGUI PromptText;

    // Wire Yes and No buttons directly to OnYesClicked() and OnNoClicked()
    // in the Inspector via their OnClick lists.

    // -------------------------------------------------------------------------
    private SlotController _pendingSlot;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Hide();
    }

    // -------------------------------------------------------------------------

    /// <summary>Opens the dialogue for the given slot and plant name.</summary>
    public void Show(SlotController slot, string plantName)
    {
        if (DialoguePanel == null) return;
        _pendingSlot = slot;
        if (PromptText != null)
            PromptText.text = $"{plantName} wants to evolve. Proceed?";
        DialoguePanel.SetActive(true);
        AudioManager.Instance?.PlaySFX("ui_open");
    }

    private void Hide()
    {
        _pendingSlot = null;
        if (DialoguePanel != null) DialoguePanel.SetActive(false);
    }

    // -------------------------------------------------------------------------
    // Wire these to the Yes and No button OnClick lists in the Inspector.
    // -------------------------------------------------------------------------

    public void OnYesClicked()
    {
        if (_pendingSlot != null)
            GridManager.Instance?.ConfirmEvolution(_pendingSlot);
        AudioManager.Instance?.PlaySFX("ui_close");
        Hide();
    }

    public void OnNoClicked()
    {
        AudioManager.Instance?.PlaySFX("ui_close");
        Hide();
    }
}
