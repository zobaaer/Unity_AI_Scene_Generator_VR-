using UnityEngine;
using UnityEngine.XR;

public class InventoryToggle : MonoBehaviour
{
    public GameObject inventoryObject; // Assign your Inventory root here
    public XRNode controllerNode = XRNode.LeftHand;

    private InputDevice device;
    private bool previousButtonState = false;

    private void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
    }

    private void Update()
    {
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(controllerNode);
            return;
        }

        if (device.TryGetFeatureValue(CommonUsages.primaryButton, out bool isPressed))
        {
            if (isPressed && !previousButtonState)
            {
                // Toggle visibility
                bool currentState = inventoryObject.activeSelf;
                inventoryObject.SetActive(!currentState);
                Debug.Log("Inventory toggled: " + (!currentState));
            }

            previousButtonState = isPressed;
        }
    }
}
