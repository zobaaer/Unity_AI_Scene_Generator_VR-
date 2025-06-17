using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class LinearOscillatoryUI : MonoBehaviour, IMovementUI
{
    public TMP_InputField amplitudeInput;
    public TMP_InputField frequencyInput;
    public Button deleteButton;

    private MovementManager manager;
    private GameObject uiObject;

    public LinearOscillatoryMovement GetMovement()
    {
        return new LinearOscillatoryMovement
        {
            amplitude = float.Parse(amplitudeInput.text),
            frequency = float.Parse(frequencyInput.text)
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