using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using GLTFast;
using UnityEngine.XR;
using UnityEngine.SpatialTracking;
using UnityEngine.XR.Interaction.Toolkit;

public class MeshyAPIHandler : MonoBehaviour
{
    [Header("API Configuration")]
    // msy_dummy_api_key_for_test_mode_12345678
    //"msy_qvr9wI4DsFM6H4z7kW5MgzleZq41AX30xFwi";
    public string apiKey = "msy_dummy_api_key_for_test_mode_12345678"; 
    private string apiUrl = "https://api.meshy.ai/openapi/v2/text-to-3d";

    [Header("UI Elements")]
    public TMP_InputField promptInputField;
    public TMP_InputField texturePromptInput;
    public Button generateButton;
    public TMP_Text resultText;
    public RawImage thumbnailDisplay;

    [Header("Scene Placement")]
    public float modelScale = 0.5f;

    [Header("Save Settings")]
    public string saveFolder;


    private string currentTaskId;

    void Start()
    {
        saveFolder = Path.Combine(Application.persistentDataPath, "MeshyAssets");
        generateButton.onClick.AddListener(OnGenerateButtonClick);
        
        // Initialize thumbnail display
        if (thumbnailDisplay != null)
            thumbnailDisplay.gameObject.SetActive(false);
    }

    public void OnGenerateButtonClick()
    {
        if (string.IsNullOrEmpty(promptInputField.text))
        {
            resultText.text = "Please enter a prompt";
            return;
        }

        generateButton.interactable = false;
        resultText.text = "Starting generation...";

        // Call async method
        GenerateModelAsync();
    }

    private async void GenerateModelAsync()
    {
        try
        {
            await CreatePreviewTaskAsync();
            if (string.IsNullOrEmpty(currentTaskId)) return;

            await WaitForTaskCompletionAsync();
            await CreateRefineTaskAsync();
            if (string.IsNullOrEmpty(currentTaskId)) return;

            await WaitForTaskCompletionAsync();
            await DownloadAndPlaceModelAsync();
        }
        catch (Exception ex)
        {
            resultText.text = $"Error: {ex.Message}";
        }
        finally
        {
            generateButton.interactable = true;
        }
    }

    private async Task CreatePreviewTaskAsync()
    {
        var requestData = new PreviewRequest
        {
            mode = "preview",
            prompt = promptInputField.text,
            art_style = "realistic",
            topology = "triangle",
            target_polycount = 10000,
            should_remesh = true,
            ai_model = "meshy-4"
        };

        UnityWebRequest request = CreateAPIRequest(apiUrl, JsonUtility.ToJson(requestData));
        var response = await SendWebRequestAsync(request);
        
        if (response.result == UnityWebRequest.Result.Success)
        {
            var taskResponse = JsonUtility.FromJson<TaskResponse>(response.downloadHandler.text);
            currentTaskId = taskResponse.result;
            resultText.text = $"Preview task created: {currentTaskId}";
        }
        else
        {
            HandleError(response, "Preview creation failed");
        }
    }

    private async Task CreateRefineTaskAsync()
    {
        var requestData = new RefineRequest
        {
            mode = "refine",
            preview_task_id = currentTaskId,
            texture_prompt = texturePromptInput.text,
            ai_model = "meshy-4"
        };

        UnityWebRequest request = CreateAPIRequest(apiUrl, JsonUtility.ToJson(requestData));
        var response = await SendWebRequestAsync(request);

        if (response.result == UnityWebRequest.Result.Success)
        {
            var taskResponse = JsonUtility.FromJson<TaskResponse>(response.downloadHandler.text);
            currentTaskId = taskResponse.result;
            resultText.text = $"Refine task created: {currentTaskId}";
        }
        else
        {
            HandleError(response, "Refine creation failed");
        }
    }

    private async Task WaitForTaskCompletionAsync()
    {
        float timeout = 300f; // 5 minutes
        float startTime = Time.time;
        bool isComplete = false;

        while (!isComplete && Time.time - startTime < timeout)
        {
            UnityWebRequest request = UnityWebRequest.Get($"{apiUrl}/{currentTaskId}");
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            var response = await SendWebRequestAsync(request);

            if (response.result == UnityWebRequest.Result.Success)
            {
                var task = JsonUtility.FromJson<MeshyTask>(response.downloadHandler.text);
                
                if (task.status == "SUCCEEDED")
                {
                    isComplete = true;
                    if (!string.IsNullOrEmpty(task.thumbnail_url))
                        await LoadThumbnailAsync(task.thumbnail_url);
                }
                else if (task.status == "FAILED")
                {
                    resultText.text = $"Task failed: {task.task_error.message}";
                    return;
                }

                resultText.text = $"Status: {task.status} | Progress: {task.progress}%";
            }
            else
            {
                HandleError(response, "Status check failed");
                return;
            }

            await Task.Delay(5000);  // Wait for 5 seconds before checking again
        }

        if (!isComplete)
            resultText.text = "Task timed out";
    }

    private async Task DownloadAndPlaceModelAsync()
    {
        UnityWebRequest request = UnityWebRequest.Get($"{apiUrl}/{currentTaskId}");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        var response = await SendWebRequestAsync(request);

        if (response.result == UnityWebRequest.Result.Success)
        {
            var task = JsonUtility.FromJson<MeshyTask>(response.downloadHandler.text);
            
            if (task.model_urls != null && !string.IsNullOrEmpty(task.model_urls.glb))
            {
                await DownloadGLTFModelAsync(task.model_urls.glb);
            }
            else
            {
                resultText.text = "GLTF URL not available";
            }
        }
    }

    private async Task DownloadGLTFModelAsync(string modelUrl)
    {
        UnityWebRequest request = UnityWebRequest.Get(modelUrl);
        var response = await SendWebRequestAsync(request);

        if (response.result == UnityWebRequest.Result.Success)
        {
            // Ensure directory exists
            if (!Directory.Exists(saveFolder))
                Directory.CreateDirectory(saveFolder);

            string fileName = $"model_{promptInputField.text}.glb";
            string fullPath = Path.Combine(saveFolder, fileName);
            File.WriteAllBytes(fullPath, response.downloadHandler.data);
            Debug.Log("GLB File saved at: " + fullPath);

            // Place in scene
            
            GltfImport gltf = new GltfImport();
            bool success = await gltf.Load(fullPath);

            if (success)
            {
                GameObject model = new GameObject("GLB_Model");
                await gltf.InstantiateMainSceneAsync(model.transform);

                // Spawn the model at the thumbnail's position
                if (thumbnailDisplay != null)
                {
                    model.transform.localScale = Vector3.one * modelScale;
                    model.transform.position = thumbnailDisplay.rectTransform.position;
                }

                // Add Rigid Body
                var rigidbody = model.GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = model.AddComponent<Rigidbody>();
                    rigidbody.useGravity = false;
                    rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous; // Better collision
                }
                // Add Collider
                if (!model.TryGetComponent<Collider>(out _))
                {
                    // First try to find a mesh for MeshCollider
                    var meshFilter = model.GetComponentInChildren<MeshFilter>();
                    if (meshFilter != null)
                    {
                        var meshCollider = model.AddComponent<MeshCollider>();
                        meshCollider.sharedMesh = meshFilter.sharedMesh;
                        meshCollider.convex = true;
                    }
                    else // Fallback to BoxCollider
                    {
                        var boxCollider = model.AddComponent<BoxCollider>();
                        // Auto-size the collider
                        var bounds = CalculateBounds(model);
                        boxCollider.center = bounds.center;
                        boxCollider.size = bounds.size;
                    }
                }

                // Add Grab interaction
                var grabbable = model.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
                grabbable.movementType = UnityEngine.XR.Interaction.Toolkit.Interactables.XRBaseInteractable.MovementType.VelocityTracking;
                grabbable.throwOnDetach = true;

                // Add Editable Tag
                model.tag = "Editable";

                resultText.text = "Model loaded in scene.";

                // Add Mesh Renderer

                // Check if the model has a MeshRenderer in itself or in children
                // Search for any MeshRenderer in children
                var childMeshRenderer = model.GetComponentInChildren<MeshRenderer>();
                var childMeshFilter = model.GetComponentInChildren<MeshFilter>();

                if (childMeshRenderer != null && childMeshFilter != null)
                {
                    // Add MeshFilter and MeshRenderer to root model
                    var meshFilter = model.AddComponent<MeshFilter>();
                    var meshRenderer = model.AddComponent<MeshRenderer>();

                    meshFilter.sharedMesh = childMeshFilter.sharedMesh;
                    meshRenderer.sharedMaterials = childMeshRenderer.sharedMaterials;
                }

                // Add No fall component
                model.AddComponent<ObjectFallReset>();

                grabbable.forceGravityOnDetach = true; // Enable gravity when detached
                
            }
            else
            {
                resultText.text = "Failed to load model.";
            }

        }
        else
        {
            HandleError(response, "Model download failed");
        }
    }


    Bounds CalculateBounds(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(obj.transform.position, Vector3.one);
        
        Bounds bounds = renderers[0].bounds;
        foreach (var renderer in renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }
        return bounds;
    }

    public System.Action<Texture2D> OnThumbnailReady;

    private async Task LoadThumbnailAsync(string url)
    {
        if (thumbnailDisplay == null) return;

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        var response = await SendWebRequestAsync(request);

        if (response.result == UnityWebRequest.Result.Success)
        {
            Texture2D tex = DownloadHandlerTexture.GetContent(response);
            thumbnailDisplay.texture = tex;
            thumbnailDisplay.gameObject.SetActive(true);

            // Invoke the callback to notify that the thumbnail is ready
            OnThumbnailReady?.Invoke(tex);
        }
    }

    private UnityWebRequest CreateAPIRequest(string url, string jsonData)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
        return request;
    }

    private async Task<UnityWebRequest> SendWebRequestAsync(UnityWebRequest request)
    {
        var operation = request.SendWebRequest();
        while (!operation.isDone)
        {
            await Task.Yield();
        }
        return request;
    }


    private void HandleError(UnityWebRequest request, string context)
    {
        Debug.LogError($"{context}: {request.error}");
        resultText.text = $"{context}: {request.error}";
    }

    [System.Serializable]
    private class PreviewRequest
    {
        public string mode;
        public string prompt;
        public string art_style;
        public string topology;
        public int target_polycount;
        public bool should_remesh;
        public string ai_model;
    }

    [System.Serializable]
    private class RefineRequest
    {
        public string mode;
        public string preview_task_id;
        public string texture_prompt;
        public string ai_model;
    }

    [System.Serializable]
    private class TaskResponse
    {
        public string result;
    }

    [System.Serializable]
    private class MeshyTask
    {
        public string id;
        public ModelUrls model_urls;
        public string thumbnail_url;
        public string status;
        public int progress;
        public TaskError task_error;

        [System.Serializable]
        public class ModelUrls
        {
            public string fbx;
            public string glb;
        }

        [System.Serializable]
        public class TaskError
        {
            public string message;
        }
    }
}
