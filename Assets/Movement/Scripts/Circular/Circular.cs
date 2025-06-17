using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CircularUI : MonoBehaviour, IMovementUI
{
    public TMP_InputField radiusInput;
    public TMP_InputField angularSpeedInput;
    public Button deleteButton;

    private MovementManager manager;
    private GameObject uiObject;

    public CircularMovement GetMovement()
    {
        return new CircularMovement
        {
            radius = float.Parse(radiusInput.text),
            angularSpeed = float.Parse(angularSpeedInput.text)
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