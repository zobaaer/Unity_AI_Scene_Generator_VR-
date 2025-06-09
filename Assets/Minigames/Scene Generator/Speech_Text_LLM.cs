using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class InputToLLMManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField userTextInput;
    public TMP_InputField maxItemsInput;
    public Button generateFromTextButton;
    public Button startRecordingButton;
    public Button stopRecordingButton;
    public TMP_Text llmPromptsText;
    public TMP_Text speechText;
    public Transform thumbnailsParent; // Parent for thumbnails

    [Header("Meshy")]
    public MeshyAPIHandler meshyHandlerPrefab;
    public Transform spawnParent;

    [Header("LLM API")]
    public string huggingFaceApiKey = "YOUR_API_KEY";
  
    private bool llmRequestInProgress = false;

    private string GetPromptTemplate()
    {
        string maxItems = maxItemsInput.text;

        return $@"You are a scene creation assistant. Based on the user's request, output a list of up to {maxItems} objects necessary to build that scene. 
    Each item should include:
    - `name`: short name
    - `prompt`: 3D model generation prompt for Meshy API (detailed)
    - `texture_prompt`: optional texture description (realistic if not given)

    Respond in pure JSON format in the following structure:

    {{
    ""objects"": [
        {{
        ""name"": ""tent"",
        ""prompt"": ""A medieval canvas tent with ropes and wooden poles"",
        ""texture_prompt"": ""canvas texture, slightly dirty"",
        }},
        ...
    ]
    }}
    
    Now, the scene I want to create is:
    ";
    }

    private AudioClip clip;
    private byte[] bytes;
    private bool recording;
    private List<string> allPrompts = new List<string>();

    void Start()
    {
        generateFromTextButton.onClick.AddListener(OnGenerateFromText);
        startRecordingButton.onClick.AddListener(StartRecording);
        stopRecordingButton.onClick.AddListener(StopRecording);
        UpdatePromptDisplay();
    }

    void Update()
    {
        if (recording && clip != null && Microphone.GetPosition(null) >= clip.samples)
        {
            StopRecording();
        }
    }

    void OnGenerateFromText()
    {
        if (llmRequestInProgress) 
        {
            Debug.LogWarning("LLM request already in progress.");
            return;
        }

        string input = userTextInput.text.Trim();
        Debug.Log($"Generate button pressed. User input: {input}");

        if (!string.IsNullOrEmpty(input))
        {
            llmRequestInProgress = true;
            generateFromTextButton.interactable = false;

            string prompt = GetPromptTemplate() + "\n" + input;
            Debug.Log($"Generated prompt: {prompt}");

            Debug.Log("Starting LLM request...");
            StartCoroutine(SendToLLMWithRetry(prompt, (items) => {
                Debug.Log("LLM response received. Parsing items...");
                ParseAndSendToMeshy(items);
                llmRequestInProgress = false;
                generateFromTextButton.interactable = true;
            }));
        }
        else
        {
            Debug.LogWarning("User input is empty. Cannot send request.");
        }
    }

    void StartRecording()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogError("No microphone found.");
            return;
        }

        clip = Microphone.Start(null, false, 10, 44100);
        if (clip == null)
        {
            Debug.LogError("Failed to start recording.");
            return;
        }

        Debug.Log("Recording...");
        recording = true;
        speechText.text = "Listening...";
    }

    void StopRecording()
    {
        if (clip == null)
        {
            Debug.LogError("AudioClip is null. Did you start recording?");
            return;
        }

        int position = Microphone.GetPosition(null);
        if (position <= 0)
        {
            Debug.LogError("Invalid microphone position.");
            return;
        }

        Microphone.End(null);
        float[] samples = new float[position * clip.channels];
        clip.GetData(samples, 0);
        bytes = EncodeAsWAV(samples, clip.frequency, clip.channels);
        recording = false;

        Debug.Log("Stopped Recording");
        speechText.text = "Processing speech...";
        StartCoroutine(SendAudioToSpeechToText());
    }

    IEnumerator SendAudioToSpeechToText()
    {
        string apiUrl = "https://api-inference.huggingface.co/models/openai/whisper-large-v3";
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bytes);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Authorization", $"Bearer {huggingFaceApiKey}");
            request.SetRequestHeader("Content-Type", "audio/wav");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                string recognizedText = ParseSpeechToTextResponse(response);
                speechText.text = "You: " + recognizedText;
                if (!string.IsNullOrEmpty(recognizedText))
                {
                    StartCoroutine(SendToLLMWithRetry(GetPromptTemplate() + "\n" + recognizedText, ParseAndSendToMeshy));
                }
            }
            else
            {
                speechText.text = "Speech recognition failed: " + request.error;
                Debug.LogError(request.error);
            }
        }
    }

    string ParseSpeechToTextResponse(string json)
    {
        int idx = json.IndexOf("\"text\":\"");
        if (idx >= 0)
        {
            int start = idx + 8;
            int end = json.IndexOf("\"", start);
            if (end > start)
                return json.Substring(start, end - start);
        }
        return "";
    }

    IEnumerator SendToLLMWithRetry(string prompt, System.Action<List<LLMItem>> callback)
    {
        // Only try once, no retry
        yield return StartCoroutine(SendToLLM(prompt, (json) =>
        {
            List<LLMItem> items = TryParseLLMItems(json);
            if (items != null)
            {
                callback(items);
            }
            else
            {
                llmPromptsText.text = "Failed to get valid JSON from LLM. Please try again or edit your prompt.";
            }
        }));
    }

    IEnumerator SendToLLM(string prompt, System.Action<string> callback)
    {
        string jsonPayload = $@"
        {{
            ""messages"": [
                {{
                    ""role"": ""user"",
                    ""content"": ""{EscapeJson(prompt)}""
                }}
            ],
            ""model"": ""meta-llama/Llama-3.1-8B-Instruct""
        }}";

        using (UnityWebRequest request = new UnityWebRequest(
            "https://router.huggingface.co/hf-inference/models/meta-llama/Llama-3.1-8B-Instruct/v1/chat/completions", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", $"Bearer {huggingFaceApiKey}");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseText = request.downloadHandler.text;
                Debug.Log("LLM RAW RESPONSE:\n" + responseText); // <-- Add this line
                string content = ParseLLMContent(responseText);
                callback?.Invoke(content);
            }
            else
            {
                Debug.LogError("LLM request failed: " + request.error + "\n" + request.downloadHandler.text);
                callback?.Invoke(null);
            }
        }
    }

    string ParseLLMContent(string json)
    {
        // Extract the content string from the LLM response
        var match = Regex.Match(json, @"""content""\s*:\s*""((?:[^""\\]|\\.)*)""");
        if (match.Success)
        {
            // Unescape JSON string (handles \r, \n, etc.)
            string content = match.Groups[1].Value;
            content = content.Replace("\\r", "\r").Replace("\\n", "\n").Replace("\\\"", "\"");
            return content;
        }
        return "";
    }

    List<LLMItem> TryParseLLMItems(string llmResponse)
    {
        if (string.IsNullOrEmpty(llmResponse)) return null;

        // Try to extract the "objects" array from the JSON object
        var match = Regex.Match(llmResponse, @"""objects""\s*:\s*(\[[^\]]*\])", RegexOptions.Singleline);
        if (!match.Success)
        {
            Debug.LogWarning("LLM response did not contain an 'objects' JSON array. Raw response:\n" + llmResponse);
            return null;
        }

        string jsonArray = match.Groups[1].Value;
        // Fix common LLM JSON mistakes
        jsonArray = jsonArray.Replace(";", ",");
        jsonArray = Regex.Replace(jsonArray, @",\s*([\}\]])", "$1");
        try
        {
            LLMItemList wrapper = new LLMItemList { items = JsonHelper.FromJson<LLMItem>(jsonArray) };
            return new List<LLMItem>(wrapper.items);
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning("LLM response contained an 'objects' JSON-like array but parsing failed.\nExtracted JSON:\n" + jsonArray + "\nException: " + ex.Message);
            return null;
        }
    }

    void ParseAndSendToMeshy(List<LLMItem> items)
    {
        Debug.Log($"ParseAndSendToMeshy: items.Count = {(items != null ? items.Count : 0)}");

        if (items == null || items.Count == 0)
        {
            llmPromptsText.text = "No valid prompts found.";
            return;
        }

        allPrompts.Clear();
        foreach (var item in items)
        {
            allPrompts.Add(item.prompt);
        }
        UpdatePromptDisplay();

        // Remove old Meshy handlers
        foreach (Transform child in spawnParent)
            Destroy(child.gameObject);

        // Remove old thumbnails
        foreach (Transform child in thumbnailsParent)
            Destroy(child.gameObject);

        // Send all prompts to Meshy in parallel
        foreach (var item in items)
        {
            Debug.Log($"Creating handler for item: {item.prompt}");

            // Instantiate a new MeshyAPIHandler clone
            var handler = Instantiate(meshyHandlerPrefab, spawnParent);

            // Dynamically create and assign the prompt input field
            GameObject promptInputObj = new GameObject("PromptInputField");
            var promptInputField = promptInputObj.AddComponent<TMP_InputField>();
            promptInputObj.transform.SetParent(handler.transform, false);
            handler.promptInputField = promptInputField; // Assign to handler
            handler.promptInputField.text = item.prompt;

            // Dynamically create and assign the texture prompt input field
            GameObject textureInputObj = new GameObject("TextureInputField");
            var texturePromptInput = textureInputObj.AddComponent<TMP_InputField>();
            textureInputObj.transform.SetParent(handler.transform, false);
            handler.texturePromptInput = texturePromptInput; // Assign to handler
            handler.texturePromptInput.text = item.texture_prompt;

            // Dynamically create and assign the generate button
            GameObject generateButtonObj = new GameObject("GenerateButton");
            var generateButton = generateButtonObj.AddComponent<Button>();
            generateButtonObj.transform.SetParent(handler.transform, false);
            handler.generateButton = generateButton; // Assign to handler

            // Dynamically create a new thumbnail RawImage for this handler
            GameObject thumbObj = new GameObject("Thumbnail");
            thumbObj.transform.SetParent(thumbnailsParent, false);
            var rawImage = thumbObj.AddComponent<RawImage>();
            rawImage.rectTransform.sizeDelta = new Vector2(100, 100); // Adjust size as needed
            rawImage.texture = null; // Start with no texture
            handler.thumbnailDisplay = rawImage; // Assign to handler

            // Dynamically create and assign the result text
            GameObject resultTextObj = new GameObject("ResultText");
            var resultText = resultTextObj.AddComponent<TextMeshProUGUI>();
            resultTextObj.transform.SetParent(handler.transform, false);
            resultText.text = "Waiting for generation..."; // Initial text
            handler.resultText = resultText; // Assign to handler

            // Subscribe to the thumbnail ready event
            handler.OnThumbnailReady = (texture) =>
            {
                rawImage.texture = texture; // Update the RawImage texture
            };

            // Start the generation process AFTER assigning prompts
            handler.generateButton.onClick.AddListener(handler.OnGenerateButtonClick); // Add listener dynamically
            handler.generateButton.onClick.Invoke(); // Simulate button click to start generation
        }
    }


    void UpdatePromptDisplay()
    {
        if (llmPromptsText != null)
        {
            llmPromptsText.text = "LLM Prompts:\n";
            foreach (var prompt in allPrompts)
            {
                llmPromptsText.text += $"- {prompt}\n";
            }
        }
    }

    byte[] EncodeAsWAV(float[] samples, int frequency, int channels)
    {
        using (var memoryStream = new MemoryStream(44 + samples.Length * 2))
        {
            using (var writer = new BinaryWriter(memoryStream))
            {
                writer.Write("RIFF".ToCharArray());
                writer.Write(36 + samples.Length * 2);
                writer.Write("WAVE".ToCharArray());
                writer.Write("fmt ".ToCharArray());
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)channels);
                writer.Write(frequency);
                writer.Write(frequency * channels * 2);
                writer.Write((ushort)(channels * 2));
                writer.Write((ushort)16);
                writer.Write("data".ToCharArray());
                writer.Write(samples.Length * 2);

                foreach (var sample in samples)
                {
                    writer.Write((short)(sample * short.MaxValue));
                }
            }
            return memoryStream.ToArray();
        }
    }

    // Helper for escaping/unescaping JSON strings
    string EscapeJson(string s)
    {
        if (string.IsNullOrEmpty(s)) return "";
        return s.Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
    }
    string UnescapeJson(string s) => s.Replace("\\\"", "\"").Replace("\\\\", "\\");

    [System.Serializable]
    public class LLMItem
    {
        public string prompt;
        public string texture_prompt;
    }

    [System.Serializable]
    public class LLMItemList
    {
        public LLMItem[] items;
    }

    // Helper for parsing JSON arrays
    public static class JsonHelper
    {
        public static T[] FromJson<T>(string json)
        {
            string newJson = "{\"array\":" + json + "}";
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
            return wrapper.array;
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
