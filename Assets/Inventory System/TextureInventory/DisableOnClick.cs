using UnityEngine;
using UnityEngine.UI;

public class ToggleObjectsOnClick : MonoBehaviour
{
    [Header("UI References")]
    public Button toggleButton;  // Assign your UI button in Inspector
    
    [Header("GameObjects to Control")]
    public GameObject objectToDisable;  // GameObject that will be disabled
    public GameObject objectToEnable;   // GameObject that will be enabled

    void Start()
    {
        // Link the button click to the ToggleObjects method
        toggleButton.onClick.AddListener(ToggleObjects);
    }

    void ToggleObjects()
    {
        // Disable the first object
        if(objectToDisable != null)
            objectToDisable.SetActive(false);
        
        // Enable the second object
        if(objectToEnable != null)
            objectToEnable.SetActive(true);
    }

    // Optional: Reset references (for safety)
    private void OnValidate()
    {
        if (toggleButton == null)
            toggleButton = GetComponent<Button>();
    }
}