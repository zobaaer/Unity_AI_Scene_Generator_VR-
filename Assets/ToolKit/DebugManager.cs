using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;

public class XRDebugToggleAndScroll : MonoBehaviour
{
    public GameObject debugConsole; // Assign your console panel/canvas
    public ScrollRect scrollRect;
    public XRNode controllerNode = XRNode.RightHand;

    // GameObjects to disable/enable
    public GameObject turnObject;
    public GameObject teleportationObject;
    public GameObject jumpObject;

    public float scrollSpeed = 1.5f;

    private InputDevice device;
    private bool previousPrimaryButtonState = false;
    private bool debugVisible = false;

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

        // Toggle the debug console with the primary button (A)
        if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed))
        {
            if (isPressed && !previousPrimaryButtonState)
            {
                debugVisible = !debugVisible;
                debugConsole.SetActive(debugVisible);
                Debug.Log("Debug console toggled: " + debugVisible);

                // Enable/disable interaction systems
                if (turnObject != null) turnObject.SetActive(!debugVisible);
                if (teleportationObject != null) teleportationObject.SetActive(!debugVisible);
                if (jumpObject != null) jumpObject.SetActive(!debugVisible);
            }

            previousPrimaryButtonState = isPressed;
        }

        // Scroll the console if visible
        if (debugVisible && device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
        {
            float scrollDelta = axis.y * scrollSpeed * Time.deltaTime;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + scrollDelta);
        }
    }
}
