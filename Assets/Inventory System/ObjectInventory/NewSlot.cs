using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

public class InventorySlot : MonoBehaviour
{
    public GameObject storedItem;
    private Vector3 originalScale; // Store original scale locally

    [Header("Highlight Settings (UI Image)")]
    public Image slotImage;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.cyan;

    private void Start()
    {
        if (slotImage != null)
            slotImage.color = defaultColor;
    }

    private void OnTriggerStay(Collider other)
    {
        if (storedItem != null) return;

        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (interactable != null && !interactable.isSelected)
        {
            if (slotImage != null)
                slotImage.color = highlightColor;

            storedItem = other.gameObject;

            // Store the original scale before flattening
            originalScale = storedItem.transform.localScale;

            Debug.Log($"[InventorySlot] Storing item: {storedItem.name}");

            // Move and parent the item
            storedItem.transform.SetParent(transform);
            storedItem.transform.localPosition = Vector3.zero;
            storedItem.transform.localRotation = Quaternion.identity;

            // Flatten Z only
            storedItem.transform.localScale = new Vector3(originalScale.x, originalScale.y, 0.05f);

            // Disable physics
            Rigidbody rb = storedItem.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.useGravity = false;
                rb.isKinematic = true;
            }

            interactable.selectExited.AddListener(OnGrabReleased);

            if (slotImage != null)
                slotImage.color = defaultColor;
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (storedItem != null) return;

        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = other.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (interactable != null && slotImage != null)
        {
            slotImage.color = defaultColor;
        }
    }

    private void OnGrabReleased(SelectExitEventArgs args)
    {
        if (storedItem == null) return;

        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = storedItem.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (interactable != null)
            interactable.selectExited.RemoveListener(OnGrabReleased);

        Debug.Log($"[InventorySlot] Item removed from inventory: {storedItem.name}");

        // Unparent and restore physics
        storedItem.transform.SetParent(null);

        Rigidbody rb = storedItem.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // Restore original scale
        storedItem.transform.localScale = originalScale;

        storedItem = null;
    }
}
