using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ObjectMovementHandler : MonoBehaviour
{
    [Header("XR Settings")]
    public XRNode controllerNode = XRNode.LeftHand;
    public float raycastDistance = 5f;
    public LayerMask interactableLayer;

    [Header("UI References")]
    public GameObject movementMenuCanvas;
    public TMP_Dropdown movementTypeDropdown;
    public Transform parameterContainer;
    public GameObject parameterFieldPrefab; // Prefab for parameter input fields
    public Button addMovementButton;
    public Button startMovementsButton;
    public Button stopMovementsButton;
    public Transform movementListContent; // ScrollView Content object
    public GameObject movementEntryPrefab; // Prefab for movement entries

    private InputDevice device;
    private bool previousButtonState;
    private GameObject currentTarget;
    private List<MovementData> currentMovements = new List<MovementData>();
    private bool isMoving = false;

    // Movement data structure
    [System.Serializable]
    public class MovementData
    {
        public MovementType movementType;
        public Dictionary<string, float> parameters = new Dictionary<string, float>();
    }

    private void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);

        // UI Events
        addMovementButton.onClick.AddListener(AddMovement);
        startMovementsButton.onClick.AddListener(StartMovements);
        stopMovementsButton.onClick.AddListener(StopMovements);

        movementMenuCanvas.SetActive(false);
    }

    private void Update()
    {
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(controllerNode);
            return;
        }

        // Handle secondary button press to toggle the movement menu
        if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed))
        {
            if (isPressed && !previousButtonState)
            {
                ToggleMovementMenu();
            }
            previousButtonState = isPressed;
        }

        // Apply movements if active
        if (isMoving && currentTarget != null)
        {
            ApplyMovements();
        }
    }

    private void ToggleMovementMenu()
    {
        // Toggle the movement menu
        if (movementMenuCanvas.activeSelf)
        {
            movementMenuCanvas.SetActive(false);
            currentTarget = null;
            Debug.Log("Movement menu closed.");
        }
        else
        {
            TrySelectObject();
        }
    }

    private void TrySelectObject()
    {
        if (Physics.Raycast(new Ray(transform.position, transform.forward), out RaycastHit hit, raycastDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Editable"))
            {
                currentTarget = hit.collider.gameObject;
                Debug.Log($"Selected object: {currentTarget.name}");
                movementMenuCanvas.SetActive(true);
            }
        }
    }

    private void AddMovement()
    {
        if (currentTarget == null) return;

        // Create a new movement entry
        MovementData newMovement = new MovementData
        {
            movementType = (MovementType)movementTypeDropdown.value,
            parameters = new Dictionary<string, float>()
        };

        // Add default parameters based on movement type
        switch (newMovement.movementType)
        {
            case MovementType.Linear:
                newMovement.parameters["Speed"] = 1f;
                newMovement.parameters["DirectionX"] = 1f;
                newMovement.parameters["DirectionY"] = 0f;
                newMovement.parameters["DirectionZ"] = 0f;
                break;
            case MovementType.Oscillatory:
                newMovement.parameters["Amplitude"] = 1f;
                newMovement.parameters["Frequency"] = 1f;
                newMovement.parameters["AxisX"] = 0f;
                newMovement.parameters["AxisY"] = 1f;
                newMovement.parameters["AxisZ"] = 0f;
                break;
            case MovementType.Circular:
                newMovement.parameters["Radius"] = 1f;
                newMovement.parameters["Speed"] = 1f;
                newMovement.parameters["AxisX"] = 0f;
                newMovement.parameters["AxisY"] = 1f;
                newMovement.parameters["AxisZ"] = 0f;
                break;
        }

        currentMovements.Add(newMovement);
        CreateMovementEntry(newMovement);
    }

    private void CreateMovementEntry(MovementData movementData)
    {
        GameObject entry = Instantiate(movementEntryPrefab, movementListContent);

        var dropdown = entry.GetComponentInChildren<TMP_Dropdown>();
        dropdown.SetValueWithoutNotify((int)movementData.movementType);

        Transform inputContainer = entry.transform.Find("DynamicInputContainer");
        foreach (var param in movementData.parameters)
        {
            CreateParameterField(inputContainer, param.Key, param.Value.ToString(), movementData);
        }

        Button removeButton = entry.transform.Find("RemoveButton").GetComponent<Button>();
        removeButton.onClick.AddListener(() =>
        {
            currentMovements.Remove(movementData);
            Destroy(entry);
        });
    }

    private void CreateParameterField(Transform container, string label, string value, MovementData movementData)
    {
        GameObject paramField = Instantiate(parameterFieldPrefab, container);

        var labelComp = paramField.transform.Find("Label").GetComponent<TMP_Text>();
        var inputField = paramField.transform.Find("InputField").GetComponent<TMP_InputField>();

        labelComp.text = label;
        inputField.text = value;

        inputField.onEndEdit.AddListener(newValue =>
        {
            if (float.TryParse(newValue, out float floatValue))
            {
                movementData.parameters[label] = floatValue;
            }
        });
    }

    private void StartMovements()
    {
        if (currentTarget == null || currentMovements.Count == 0) return;

        isMoving = true;
        Debug.Log("Movements started.");
    }

    private void StopMovements()
    {
        isMoving = false;
        Debug.Log("Movements stopped.");
    }

    private void ApplyMovements()
    {
        foreach (var movement in currentMovements)
        {
            switch (movement.movementType)
            {
                case MovementType.Linear:
                    ApplyLinearMovement(movement);
                    break;
                case MovementType.Oscillatory:
                    ApplyOscillatoryMovement(movement);
                    break;
                case MovementType.Circular:
                    ApplyCircularMovement(movement);
                    break;
            }
        }
    }

    private void ApplyLinearMovement(MovementData movement)
    {
        if (!movement.parameters.TryGetValue("Speed", out float speed) ||
            !movement.parameters.TryGetValue("DirectionX", out float dirX) ||
            !movement.parameters.TryGetValue("DirectionY", out float dirY) ||
            !movement.parameters.TryGetValue("DirectionZ", out float dirZ)) return;

        Vector3 direction = new Vector3(dirX, dirY, dirZ).normalized;
        currentTarget.transform.Translate(direction * speed * Time.deltaTime);
    }

    private void ApplyOscillatoryMovement(MovementData movement)
    {
        if (!movement.parameters.TryGetValue("Amplitude", out float amplitude) ||
            !movement.parameters.TryGetValue("Frequency", out float frequency) ||
            !movement.parameters.TryGetValue("AxisX", out float axisX) ||
            !movement.parameters.TryGetValue("AxisY", out float axisY) ||
            !movement.parameters.TryGetValue("AxisZ", out float axisZ)) return;

        Vector3 axis = new Vector3(axisX, axisY, axisZ).normalized;
        float oscillation = Mathf.Sin(Time.time * frequency) * amplitude;
        currentTarget.transform.Translate(axis * oscillation * Time.deltaTime);
    }

    private void ApplyCircularMovement(MovementData movement)
    {
        if (!movement.parameters.TryGetValue("Radius", out float radius) ||
            !movement.parameters.TryGetValue("Speed", out float speed) ||
            !movement.parameters.TryGetValue("AxisX", out float axisX) ||
            !movement.parameters.TryGetValue("AxisY", out float axisY) ||
            !movement.parameters.TryGetValue("AxisZ", out float axisZ)) return;

        Vector3 axis = new Vector3(axisX, axisY, axisZ).normalized;
        float angle = speed * Time.time;
        Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
        currentTarget.transform.Translate(offset * Time.deltaTime);
    }
}

public enum MovementType
{
    Linear = 0,
    Oscillatory = 1,
    Circular = 2
}