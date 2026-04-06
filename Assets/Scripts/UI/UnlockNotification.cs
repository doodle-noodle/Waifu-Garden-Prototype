// =============================================================================
// UnlockNotification.cs  |  Scripts/UI
// WaifuGarden — Phase 3
// Modal notification shown when a new shop item unlocks mid-session.
// Shows the item's icon and "Unlocked <item name>".
// Click anywhere on the panel to dismiss.
// Design the panel visually in the Inspector.
// Place this as a Canvas child (near the top of the draw order for visibility).
// Set the root GameObject ACTIVE in the Hierarchy (Awake hides it).
// =============================================================================

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class UnlockNotification : MonoBehaviour, IPointerClickHandler
{
    public static UnlockNotification Instance { get; private set; }

    [Header("References")]
    [Tooltip("Root panel. Shown/hidden by this script.")]
    public GameObject NotificationPanel;

    [Tooltip("Image showing the unlocked item's icon.")]
    public Image ItemIcon;

    [Tooltip("Text showing 'Unlocked <item name>'.")]
    public TextMeshProUGUI UnlockText;

    // -------------------------------------------------------------------------
    // Queue so rapid unlocks don't overwrite each other.
    // -------------------------------------------------------------------------
    private readonly System.Collections.Generic.Queue<ShopItemData> _queue
        = new System.Collections.Generic.Queue<ShopItemData>();

    private bool _isShowing = false;

    // -------------------------------------------------------------------------

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Hide();
    }

    // -------------------------------------------------------------------------

    /// <summary>Queues a notification for the given item.</summary>
    public void Show(ShopItemData item)
    {
        if (item == null) return;
        _queue.Enqueue(item);
        if (!_isShowing) ShowNext();
    }

    private void ShowNext()
    {
        if (_queue.Count == 0) { _isShowing = false; return; }

        _isShowing = true;
        ShopItemData item = _queue.Dequeue();

        if (ItemIcon   != null) { ItemIcon.sprite = item.ItemIcon; ItemIcon.enabled = item.ItemIcon != null; }
        if (UnlockText != null) UnlockText.text = $"Unlocked\n{item.ItemName}";

        if (NotificationPanel != null) NotificationPanel.SetActive(true);
        AudioManager.Instance?.PlaySFX("evolution"); // reuse a positive SFX for now
    }

    private void Hide()
    {
        if (NotificationPanel != null) NotificationPanel.SetActive(false);
        _isShowing = false;
    }

    // -------------------------------------------------------------------------
    // Click anywhere on the panel to dismiss and show the next queued item.
    // -------------------------------------------------------------------------

    public void OnPointerClick(PointerEventData eventData)
    {
        Hide();
        ShowNext();
    }
}
