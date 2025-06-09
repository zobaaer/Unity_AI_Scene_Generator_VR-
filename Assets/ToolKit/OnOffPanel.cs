using UnityEngine;
using UnityEngine.UI;

public class UIButtonToggleManager : MonoBehaviour
{
    // List of objects to deactivate when a button is pressed
    public GameObject[] objectsToDeactivate;

    // The object to activate when the button is pressed
    public GameObject objectToActivate;

    // Reference to UI buttons
    public Button toggleButton;

    private void Start()
    {
        // Make sure to set up the button listener
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(OnButtonPressed);
        }
    }

    // This method will be called when the button is pressed
    private void OnButtonPressed()
    {
        // Activate the designated object
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
            Debug.Log($"{objectToActivate.name} activated.");
        }

        // Deactivate all other referenced objects
        foreach (GameObject obj in objectsToDeactivate)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Debug.Log($"{obj.name} deactivated.");
            }
        }
    }
}
