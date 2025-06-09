using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using TMPro;
using System.IO;
using System.Collections.Generic;

public class EditObjectMenu : MonoBehaviour
{
    [Header("XR Settings")]
    public XRNode controllerNode = XRNode.LeftHand;
    public float raycastDistance = 5f;
    public LayerMask interactableLayer;

    [Header("UI References")]
    public GameObject editMenuCanvas;
    public Toggle gravityToggle;
    public Slider massSlider;
    public Slider scaleSlider;
    public TMP_Dropdown movementDropdown;
    public Button closeButton;
    public TMP_Text massValueText;
    public TMP_Text scaleValueText;
    public TMP_Text output;

    [Header("Texture Editing")]
    public RawImage currentTexturePreview;
    public RawImage newTexturePreview;
    public Button applyTextureButton;
    public Button revertTextureButton;

    [Header("Movement Parameters")]
    public TMP_InputField linearFrequencyInput;
    public TMP_InputField linearAmplitudeInput;
    public TMP_InputField circularRadiusInput;
    public TMP_InputField circularSpeedInput;
    public Button addLinearMovementButton;
    public Button addCircularMovementButton;
    public Button startMovementButton;
    public Button stopMovementButton;

    private bool isMoving = false;
    private float linearFrequency = 0f;
    private float linearAmplitude = 0f;
    private float circularRadius = 0f;
    private float circularSpeed = 0f;
    private Vector3 circularCenter;

    private InputDevice device;
    private bool previousButtonState;
    private GameObject currentTarget;
    private Rigidbody targetRb;
    private Renderer targetRenderer;
    private Material originalMaterial;
    private Vector3 originalScale;

    private Dictionary<GameObject, MovementData> objectMovements = new Dictionary<GameObject, MovementData>();

    [System.Serializable]
    private class MovementData
    {
        public bool isMoving = false;
        public float linearFrequency = 0f;
        public float linearAmplitude = 0f;
        public float circularRadius = 0f;
        public float circularSpeed = 0f;
        public Vector3 circularCenter;
    }

    private MovementData movementData = new MovementData();

    private void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);

        // UI Events
        closeButton.onClick.AddListener(CloseMenu);
        applyTextureButton.onClick.AddListener(ApplyTexture);
        revertTextureButton.onClick.AddListener(RevertTexture);
        addLinearMovementButton.onClick.AddListener(AddLinearMovement);
        addCircularMovementButton.onClick.AddListener(AddCircularMovement);
        startMovementButton.onClick.AddListener(StartMovement);
        stopMovementButton.onClick.AddListener(StopMovement);

        editMenuCanvas.SetActive(false);
    }

    private void Update()
    {
        if (!device.isValid)
        {
            device = InputDevices.GetDeviceAtXRNode(controllerNode);
            return;
        }

        if (device.TryGetFeatureValue(CommonUsages.secondaryButton, out bool isPressed))
        {
            if (isPressed && !previousButtonState) TryShowEditMenu();
            previousButtonState = isPressed;
        }

        foreach (var kvp in objectMovements)
        {
            ApplyMovements(kvp.Key, kvp.Value);
        }

        UpdateTextFields();
    }

    private void TryShowEditMenu()
    {
        if (editMenuCanvas.activeSelf)
        {
            // If the menu is already open, close it
            CloseMenu();
            return;
        }

        if (Physics.Raycast(new Ray(transform.position, transform.forward),
            out RaycastHit hit, raycastDistance, interactableLayer))
        {
            if (hit.collider.CompareTag("Editable"))
            {
                currentTarget = hit.collider.gameObject;
                targetRb = currentTarget.GetComponent<Rigidbody>();
                targetRenderer = currentTarget.GetComponent<Renderer>();

                if (targetRb == null || targetRenderer == null) return;

                // Store original state
                originalMaterial = new Material(targetRenderer.material);
                originalScale = currentTarget.transform.localScale;
                currentTexturePreview.texture = originalMaterial.mainTexture;

                // Initialize UI with current values
                gravityToggle.SetIsOnWithoutNotify(targetRb.useGravity);
                massSlider.SetValueWithoutNotify(targetRb.mass);
                scaleSlider.SetValueWithoutNotify(originalScale.x);
                movementDropdown.SetValueWithoutNotify(targetRb.isKinematic ? 2 :
                    (targetRb.collisionDetectionMode == CollisionDetectionMode.ContinuousDynamic ? 1 : 0));

                // Set up event listeners AFTER initializing values
                gravityToggle.onValueChanged.AddListener(SetGravity);
                massSlider.onValueChanged.AddListener(SetMass);
                scaleSlider.onValueChanged.AddListener(SetScale);
                movementDropdown.onValueChanged.AddListener(SetMovementType);

                // Check for new texture
                if (File.Exists(TexturePaths.LastGeneratedPath))
                {
                    newTexturePreview.texture = LoadTexture(TexturePaths.LastGeneratedPath);
                    applyTextureButton.interactable = true;
                }

                editMenuCanvas.SetActive(true);
            }
        }
    }

    private Texture2D LoadTexture(string path)
    {
        byte[] fileData = File.ReadAllBytes(path);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return texture;
    }

    private void ApplyTexture()
    {
        if (!File.Exists(TexturePaths.LastGeneratedPath)) return;

        Texture2D texture = LoadTexture(TexturePaths.LastGeneratedPath);
        if (texture != null && targetRenderer != null)
        {
            Material unlitMaterial = new Material(Shader.Find("Unlit/Texture"));
            unlitMaterial.mainTexture = texture;            // Use unlit, the texture is dark otherwise
            targetRenderer.material = unlitMaterial;
            revertTextureButton.interactable = true;
            output.text = "<color=green>Texture applied!</color>";
        }
    }

    private void RevertTexture()
    {
        if (targetRenderer != null && originalMaterial != null)
        {
            targetRenderer.material = originalMaterial;
            revertTextureButton.interactable = false;
            output.text = "<color=yellow>Texture reverted</color>";
            UpdateTextFields();
        }
    }

    private void UpdateTextFields()
    {
        if (targetRb != null)
            massValueText.text = $"Mass: {targetRb.mass:F2}kg";

        if (currentTarget != null)
            scaleValueText.text = $"Scale: {currentTarget.transform.localScale.x:F2}";
    }

    private void SetGravity(bool isOn)
    {
        if (targetRb != null)
        {
            targetRb.useGravity = isOn;
            Debug.Log($"Gravity set to: {isOn}");
        }
    }

    private void SetMass(float value)
    {
        if (targetRb != null)
        {
            targetRb.mass = value;
            Debug.Log($"Mass set to: {value}");
            UpdateTextFields();
        }
    }

    private void SetScale(float value)
    {
        if (currentTarget != null)
        {
            currentTarget.transform.localScale = Vector3.one * value;
            Debug.Log($"Scale set to: {value}");
            UpdateTextFields();
        }
    }

    private void SetMovementType(int index)
    {
        if (targetRb == null) return;

        targetRb.isKinematic = (index == 2);
        targetRb.collisionDetectionMode = (index == 1) ?
            CollisionDetectionMode.ContinuousDynamic :
            CollisionDetectionMode.Discrete;
        Debug.Log($"Movement type set to: {index}");
    }

    private void AddLinearMovement()
    {
        if (currentTarget == null) return;

        if (float.TryParse(linearFrequencyInput.text, out float frequency) &&
            float.TryParse(linearAmplitudeInput.text, out float amplitude))
        {
            if (!objectMovements.ContainsKey(currentTarget))
            {
                objectMovements[currentTarget] = new MovementData();
            }

            objectMovements[currentTarget].linearFrequency = frequency;
            objectMovements[currentTarget].linearAmplitude = amplitude;

            Debug.Log($"Linear movement added: Frequency = {frequency}, Amplitude = {amplitude}");
        }
        else
        {
            Debug.LogError("Invalid linear movement parameters.");
        }
    }

    private void AddCircularMovement()
    {
        if (currentTarget == null) return;

        if (float.TryParse(circularRadiusInput.text, out float radius) &&
            float.TryParse(circularSpeedInput.text, out float speed))
        {
            if (!objectMovements.ContainsKey(currentTarget))
            {
                objectMovements[currentTarget] = new MovementData();
            }

            objectMovements[currentTarget].circularRadius = radius;
            objectMovements[currentTarget].circularSpeed = speed;
            objectMovements[currentTarget].circularCenter = currentTarget.transform.position;

            Debug.Log($"Circular movement added: Radius = {radius}, Speed = {speed}");
        }
        else
        {
            Debug.LogError("Invalid circular movement parameters.");
        }
    }

    private void StartMovement()
    {
        if (currentTarget == null) return;

        if (!objectMovements.ContainsKey(currentTarget))
        {
            objectMovements[currentTarget] = new MovementData();
        }

        objectMovements[currentTarget].isMoving = true;
        Debug.Log("Movement started.");
    }

    private void StopMovement()
    {
        if (currentTarget == null) return;

        if (objectMovements.ContainsKey(currentTarget))
        {
            objectMovements[currentTarget].isMoving = false;
            Debug.Log("Movement stopped.");
        }
    }

    private void ApplyMovements(GameObject obj, MovementData movementData)
    {
        if (movementData.isMoving)
        {
            Vector3 newPosition = obj.transform.position;

            // Apply linear movement
            newPosition += GetLinearMovementOffset(movementData);

            // Apply circular movement
            newPosition += GetCircularMovementOffset(obj, movementData);

            // Update the object's position
            obj.transform.position = newPosition;
        }
    }

    private void CloseMenu()
    {
        // Clean up event listeners
        gravityToggle.onValueChanged.RemoveAllListeners();
        massSlider.onValueChanged.RemoveAllListeners();
        scaleSlider.onValueChanged.RemoveAllListeners();
        movementDropdown.onValueChanged.RemoveAllListeners();

        editMenuCanvas.SetActive(false);
        currentTarget = null;
        targetRb = null;
        targetRenderer = null;
        output.text = "Ready";
    }

    private Vector3 GetLinearMovementOffset(MovementData movementData)
    {
        if (movementData.linearFrequency > 0f && movementData.linearAmplitude > 0f)
        {
            float oscillation = Mathf.Sin(Time.time * movementData.linearFrequency) * movementData.linearAmplitude;
            return new Vector3(oscillation, 0f, 0f);
        }

        return Vector3.zero;
    }

    private Vector3 GetCircularMovementOffset(GameObject obj, MovementData movementData)
    {
        if (movementData.circularRadius > 0f && movementData.circularSpeed > 0f)
        {
            float angle = Time.time * movementData.circularSpeed;
            float x = movementData.circularCenter.x + Mathf.Cos(angle) * movementData.circularRadius;
            float z = movementData.circularCenter.z + Mathf.Sin(angle) * movementData.circularRadius;
            return new Vector3(x - obj.transform.position.x, 0f, z - obj.transform.position.z);
        }

        return Vector3.zero;
    }

    private void PrintMovementData()
    {
        Debug.Log("Printing movement data:");

        foreach (var kvp in objectMovements)
        {
            GameObject obj = kvp.Key;
            MovementData data = kvp.Value;

            Debug.Log($"Object: {obj.name}");
            Debug.Log($"  Is Moving: {data.isMoving}");
            Debug.Log($"  Linear Frequency: {data.linearFrequency}");
            Debug.Log($"  Linear Amplitude: {data.linearAmplitude}");
            Debug.Log($"  Circular Radius: {data.circularRadius}");
            Debug.Log($"  Circular Speed: {data.circularSpeed}");
            Debug.Log($"  Circular Center: {data.circularCenter}");
        }
    }
}