using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class TextureInventory : MonoBehaviour
{
    public GameObject textureSlotPrefab;
    public Transform gridParent; // Parent object with GridLayoutGroup
    public Transform rightController; // Assign this in the Inspector to the right controller's transform

    private Dictionary<GameObject, Texture2D> textureSlots = new Dictionary<GameObject, Texture2D>();
    private GameObject anchoredTexture; // The currently anchored texture
    private HashSet<string> loadedFiles = new HashSet<string>(); // Track already loaded files

    void Start()
    {
        // Ensure the gridParent has a GridLayoutGroup
        GridLayoutGroup grid = gridParent.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = gridParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        // Configure the GridLayoutGroup
        grid.cellSize = new Vector2(75, 75); // Fixed width and height for each slot
        grid.spacing = new Vector2(10, 10); // Add spacing between slots
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 4; // Adjust the number of columns as needed

        // Load textures and create the grid
        LoadTextures();
    }

    void Update()
    {
        if (anchoredTexture != null)
        {
            // Check for hover and apply texture
            Texture2D texture = textureSlots[anchoredTexture];
            ApplyTextureOnHover(anchoredTexture, texture);
        }
    }

    public void LoadTextures()
    {
        // Use persistentDataPath/GeneratedImages for runtime and build
        string fullPath = Path.Combine(Application.persistentDataPath, "GeneratedImages");

        if (!Directory.Exists(fullPath))
        {
            Debug.LogWarning($"Texture folder not found at path: {fullPath}");
            return;
        }

        string[] imageFiles = Directory.GetFiles(fullPath, "*.*", SearchOption.TopDirectoryOnly)
                                       .Where(file => file.EndsWith(".png") || file.EndsWith(".jpg"))
                                       .ToArray();

        Debug.Log($"Found {imageFiles.Length} image files in folder: {fullPath}");

        foreach (string filePath in imageFiles)
        {
            // Skip files that have already been loaded
            if (loadedFiles.Contains(filePath))
            {
                continue;
            }

            try
            {
                byte[] fileData = File.ReadAllBytes(filePath);
                Texture2D texture = new Texture2D(2, 2);
                if (texture.LoadImage(fileData))
                {
                    texture.name = Path.GetFileNameWithoutExtension(filePath);
                    Debug.Log($"Loaded texture from file: {Path.GetFileName(filePath)}");
                    CreateTextureSlot(texture);

                    // Add the file to the loaded files set
                    loadedFiles.Add(filePath);
                }
                else
                {
                    Debug.LogError($"Failed to load texture from file: {Path.GetFileName(filePath)}");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error loading texture from file: {filePath}. Exception: {ex.Message}");
            }
        }
    }

    void CreateTextureSlot(Texture2D texture)
    {
        // Instantiate the texture slot prefab
        GameObject slot = Instantiate(textureSlotPrefab, gridParent);

        // Ensure the slot has a RectTransform and set its size
        RectTransform rectTransform = slot.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            rectTransform = slot.AddComponent<RectTransform>();
        }
        rectTransform.sizeDelta = new Vector2(75, 75); // Fixed width and height

        // Ensure the slot has a RawImage component
        RawImage rawImage = slot.GetComponent<RawImage>();
        if (rawImage == null)
        {
            rawImage = slot.AddComponent<RawImage>();
        }

        // Set the texture to the RawImage
        rawImage.texture = texture;
        rawImage.color = Color.white; // Ensure fully visible

        // Ensure the RawImage is a raycast target for interaction
        rawImage.raycastTarget = true;

        // Add a BoxCollider to the slot
        BoxCollider collider = slot.GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = slot.AddComponent<BoxCollider>();
        }
        collider.size = new Vector3(0.1f, 0.1f, 0); // Match the size of the slot

        // Add a Button component for click interaction
        Button button = slot.GetComponent<Button>();
        if (button == null)
        {
            button = slot.AddComponent<Button>();
        }

        // Add click event to anchor the texture to the controller
        button.onClick.AddListener(() =>
        {
            // Create a copy of the slot
            GameObject slotCopy = Instantiate(slot);

            // Anchor the copy to the controller
            AnchorToController(slotCopy, texture);
            // Add the slot to the dictionary for tracking
            textureSlots.Add(slotCopy, texture);
        });

        Debug.Log($"Created texture slot for: {texture.name}");
    }

    void AnchorToController(GameObject slot, Texture2D texture)
    {
        if (rightController == null)
        {
            Debug.LogError("Right controller not assigned!");
            return;
        }

        // Destroy any previously anchored slot
        if (anchoredTexture != null)
        {
            Destroy(anchoredTexture);
            textureSlots.Remove(anchoredTexture);
            anchoredTexture = null;
        }
        // Anchor the slot to the right controller
        slot.transform.SetParent(rightController);
        slot.transform.localPosition = Vector3.zero;
        slot.transform.localRotation = Quaternion.identity;

        // Make the slot small and manageable
        slot.transform.localScale = Vector3.one * 1f; // Adjust as needed

        // If using RectTransform (UI), set sizeDelta
        RectTransform rectTransform = slot.GetComponent<RectTransform>();
        if (rectTransform != null)
            rectTransform.sizeDelta = new Vector2(0.1f, 0.1f); // For UI, but scale is usually enough

        anchoredTexture = slot;

        Debug.Log($"Anchored texture slot to controller: {slot.name}");
    }

    void ApplyTextureOnHover(GameObject slot, Texture2D texture)
    {
        Ray ray = new Ray(slot.transform.position, slot.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Check if the object has the tag "Editable" or "Wall"
            if (hit.collider.CompareTag("Editable") || hit.collider.CompareTag("Wall"))
            {
                Renderer renderer = hit.collider.GetComponent<Renderer>();
                if (renderer != null)
                {
                    Material unlitMaterial = new Material(Shader.Find("Unlit/Texture"));
                    unlitMaterial.mainTexture = texture;
                    renderer.material = unlitMaterial;
                    Debug.Log($"Applied texture to: {hit.collider.name}");

                    // Destroy the slot after applying the texture
                    Destroy(slot);

                    // Also destroy and clear the anchoredTexture reference if it's still set
                    if (anchoredTexture != null)
                    {
                        Destroy(anchoredTexture);
                        anchoredTexture = null;
                    }
                }
                else
                {
                    Debug.Log($"Raycast hit object without a Renderer: {hit.collider.name}");
                }
            }
            
        }
    }
}