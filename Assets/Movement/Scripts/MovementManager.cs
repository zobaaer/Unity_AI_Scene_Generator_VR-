using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovementManager : MonoBehaviour
{
    public TMP_Dropdown movementTypeDropdown;
    public Button addButton;
    public Button saveButton;
    public Transform movementListContent;

    public GameObject linearOscillatoryPrefab;
    public GameObject circularPrefab;
    public GameObject rotationPrefab; // Assign in Inspector

    private List<GameObject> spawnedMovementUIs = new List<GameObject>();

    public System.Action OnSaveMovements;

    void Start()
    {
        addButton.onClick.AddListener(AddMovementUI);
        if (saveButton != null)
            saveButton.onClick.AddListener(() => OnSaveMovements?.Invoke());
    }

    void AddMovementUI()
    {
        GameObject prefabToInstantiate = null;

        switch (movementTypeDropdown.value)
        {
            case 0: prefabToInstantiate = linearOscillatoryPrefab; break;
            case 1: prefabToInstantiate = circularPrefab; break;
            case 2: prefabToInstantiate = rotationPrefab; break;
        }

        if (prefabToInstantiate != null)
        {
            GameObject uiInstance = Instantiate(prefabToInstantiate, movementListContent);
            // Set reference to this manager for delete callback
            var deletable = uiInstance.GetComponent<IMovementUI>();
            if (deletable != null)
                deletable.SetManager(this, uiInstance);
            spawnedMovementUIs.Add(uiInstance);
        }
    }

    public List<Movement> GetAllMovements()
    {
        List<Movement> movements = new List<Movement>();

        foreach (GameObject ui in spawnedMovementUIs)
        {
            if (ui == null) continue;
            if (ui.TryGetComponent(out LinearOscillatoryUI linear))
            {
                movements.Add(linear.GetMovement());
            }
            else if (ui.TryGetComponent(out CircularUI circular))
            {
                movements.Add(circular.GetMovement());
            }
            else if (ui.TryGetComponent(out RotationUI rotation))
            {
                movements.Add(rotation.GetMovement());
            }
        }

        return movements;
    }

    public void LoadMovements(List<Movement> movements)
    {
        foreach (var ui in spawnedMovementUIs)
            Destroy(ui);
        spawnedMovementUIs.Clear();

        foreach (var movement in movements)
        {
            GameObject prefab = null;
            if (movement is LinearOscillatoryMovement)
                prefab = linearOscillatoryPrefab;
            else if (movement is CircularMovement)
                prefab = circularPrefab;
            else if (movement is RotationMovement)
                prefab = rotationPrefab;

            if (prefab != null)
            {
                GameObject uiInstance = Instantiate(prefab, movementListContent);
                var deletable = uiInstance.GetComponent<IMovementUI>();
                if (deletable != null)
                    deletable.SetManager(this, uiInstance);

                if (movement is LinearOscillatoryMovement lom)
                {
                    var ui = uiInstance.GetComponent<LinearOscillatoryUI>();
                    ui.amplitudeInput.text = lom.amplitude.ToString();
                    ui.frequencyInput.text = lom.frequency.ToString();
                    ui.axisXInput.text = lom.axisX.ToString();
                    ui.axisYInput.text = lom.axisY.ToString();
                    ui.axisZInput.text = lom.axisZ.ToString();
                }
                else if (movement is CircularMovement cm)
                {
                    var ui = uiInstance.GetComponent<CircularUI>();
                    ui.radiusInput.text = cm.radius.ToString();
                    ui.angularSpeedInput.text = cm.angularSpeed.ToString();
                }
                else if (movement is RotationMovement rm)
                {
                    var ui = uiInstance.GetComponent<RotationUI>();
                    ui.axisXInput.text = rm.axisX.ToString();
                    ui.axisYInput.text = rm.axisY.ToString();
                    ui.axisZInput.text = rm.axisZ.ToString();
                    ui.speedInput.text = rm.speed.ToString();
                    ui.clockwiseToggle.isOn = rm.clockwise;
                }
                spawnedMovementUIs.Add(uiInstance);
            }
        }
    }

    public void RemoveMovementUI(GameObject ui)
    {
        if (spawnedMovementUIs.Contains(ui))
        {
            spawnedMovementUIs.Remove(ui);
            Destroy(ui);
        }
    }
}

// Add this interface for delete callback
public interface IMovementUI
{
    void SetManager(MovementManager manager, GameObject uiObject);
}
