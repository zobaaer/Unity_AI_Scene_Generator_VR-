using System;
using System.IO;
using UnityEngine;
public static class TexturePaths
{
    public static string LastGeneratedPath { get; private set; }
    
    public static string GenerateNewPath(string prompt)
    {
        string safePrompt = string.Join("_", prompt.Split(Path.GetInvalidFileNameChars()));
        safePrompt = safePrompt.Length > 50 ? safePrompt.Substring(0, 50) : safePrompt;
        
        LastGeneratedPath = Path.Combine(
            Application.persistentDataPath,
            "GeneratedImages",
            $"{safePrompt}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
        );
        
        Directory.CreateDirectory(Path.GetDirectoryName(LastGeneratedPath));
        return LastGeneratedPath;
    }
}