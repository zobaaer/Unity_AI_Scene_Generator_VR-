using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using TMPro;
using System.IO;

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

    private InputDevice device;
    private bool previousButtonState;
    private GameObject currentTarget;
    private Rigidbody targetRb;
    private Renderer targetRenderer;
    private Material originalMaterial;
    private Vector3 originalScale;

    private void Start()
    {
        device = InputDevices.GetDeviceAtXRNode(controllerNode);
        
        // UI Events
        closeButton.onClick.AddListener(CloseMenu);
        applyTextureButton.onClick.AddListener(ApplyTexture);
        revertTextureButton.onClick.AddListener(RevertTexture);
        
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
}