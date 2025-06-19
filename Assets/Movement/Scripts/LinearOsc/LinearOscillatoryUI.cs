using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LinearOscillatoryUI : MonoBehaviour, IMovementUI
{
    public TMP_InputField amplitudeInput;
    public TMP_InputField frequencyInput;
    public TMP_InputField axisXInput;
    public TMP_InputField axisYInput;
    public TMP_InputField axisZInput;
    public Button deleteButton;

    private MovementManager manager;
    private GameObject uiObject;

    public LinearOscillatoryMovement GetMovement()
    {
        return new LinearOscillatoryMovement
        {
            amplitude = float.TryParse(amplitudeInput.text, out var a) ? a : 0f,
            frequency = float.TryParse(frequencyInput.text, out var f) ? f : 0f,
            axisX = float.TryParse(axisXInput.text, out var x) ? x : 1f,
            axisY = float.TryParse(axisYInput.text, out var y) ? y : 0f,
            axisZ = float.TryParse(axisZInput.text, out var z) ? z : 0f
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