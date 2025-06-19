using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RotationUI : MonoBehaviour, IMovementUI
{
    public TMP_InputField axisXInput;
    public TMP_InputField axisYInput;
    public TMP_InputField axisZInput;
    public TMP_InputField speedInput;
    public Toggle clockwiseToggle;
    public Button deleteButton;

    private MovementManager manager;
    private GameObject uiObject;

    public RotationMovement GetMovement()
    {
        return new RotationMovement
        {
            axisX = float.TryParse(axisXInput.text, out var x) ? x : 0f,
            axisY = float.TryParse(axisYInput.text, out var y) ? y : 0f,
            axisZ = float.TryParse(axisZInput.text, out var z) ? z : 1f,
            speed = float.TryParse(speedInput.text, out var s) ? s : 0f,
            clockwise = clockwiseToggle.isOn
        };
    }

    public void SetManager(MovementManager manager, GameObject uiObject)
    {
        this.manager = manager;
        this.uiObject = uiObject;
        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteSelf);
    }

    private void DeleteSelf()
    {
        if (manager != null && uiObject != null)
            manager.RemoveMovementUI(uiObject);
    }
}