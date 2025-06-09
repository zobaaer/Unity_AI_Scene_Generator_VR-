using UnityEngine;
using TMPro;
using HuggingFace.API;
using UnityEngine.UI;
using System.IO;

public class AIImageGenerator : MonoBehaviour
{
    public TMP_InputField promptInput;
    public Button generateButton;
    public RawImage previewImage;
    public TMP_Text output;

    private void Start()
    {
        generateButton.onClick.AddListener(GenerateImage);
        output.text = "<color=white>Generating...</color>";
    }

    private void GenerateImage()
    {
        string prompt = promptInput.text;
        if (string.IsNullOrWhiteSpace(prompt)) return;

        generateButton.interactable = false;
        HuggingFaceAPI.TextToImage(prompt, texture =>
        {
            // Use persistentDataPath for Android/Quest
            string saveFolder = Path.Combine(Application.persistentDataPath, "GeneratedImages");
            if (!Directory.Exists(saveFolder))
            {
                Directory.CreateDirectory(saveFolder);
            }

            // Generate a unique file name for the image
            string fileName = $"{prompt.Replace(" ", "_")}_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(saveFolder, fileName);

            // Save the image as a PNG file
            File.WriteAllBytes(filePath, ((Texture2D)texture).EncodeToPNG());
            Debug.Log($"Image saved to: {filePath}");

            // Show preview
            previewImage.texture = texture;
            generateButton.interactable = true;
            output.text = "<color=green>Image Generated and Saved!</color>";
        },
        error =>
        {
            Debug.LogError(error);
            generateButton.interactable = true;
            output.text = "<color=red>Error Generating Image!</color>";
        });
    }
}