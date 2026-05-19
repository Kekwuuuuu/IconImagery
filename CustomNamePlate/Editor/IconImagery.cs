using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

public class IconImagery : EditorWindow
{
    private Texture2D sourceTexture;
    private int outputSize = 512;
    private static readonly string saveFolder = "Assets/CustomNamePlate/Icons";

    [MenuItem("Window/IconImagery")]
    public static void ShowWindow()
    {
        GetWindow<IconImagery>("IconImagery");
    }

    private void OnGUI()
    {
        GUILayout.Label("Settings", EditorStyles.boldLabel);

        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Icon Photo", sourceTexture, typeof(Texture2D), false);

        outputSize = EditorGUILayout.IntField("Output Size (px)", outputSize);
        outputSize = Mathf.Clamp(outputSize, 32, 2048);

        if (sourceTexture == null)
        {
            EditorGUILayout.HelpBox("Assign a PNG or JPG texture above.", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox($"Source: {sourceTexture.name}\nSize: {sourceTexture.width}×{sourceTexture.height}", MessageType.None);
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox($"Save folder:\n{saveFolder}", MessageType.Info);

        GUI.enabled = sourceTexture != null;
        if (GUILayout.Button("Crop to Circle & Save", GUILayout.Height(40)))
        {
            CropToCircleAndSave();
        }
        GUI.enabled = true;
    }

    private void CropToCircleAndSave()
    {
        if (sourceTexture == null)
        {
            EditorUtility.DisplayDialog("Error", "Please assign a source texture.", "OK");
            return;
        }

        // Ensure save folder exists
        string fullFolder = Path.Combine(Application.dataPath, "CustomNamePlate", "Icons");
        if (!Directory.Exists(fullFolder))
            Directory.CreateDirectory(fullFolder);

        // Sanitise filename from original asset name
        string safeName = SanitizeFileName(sourceTexture.name);
        string basePath = Path.Combine(fullFolder, safeName);
        string finalPath = basePath + ".png";
        int counter = 1;
        while (File.Exists(finalPath))
        {
            finalPath = basePath + $"_{counter++}.png";
        }

        // Create circular texture
        Texture2D circularTex = MakeCircularTexture(sourceTexture, outputSize);
        if (circularTex == null)
        {
            EditorUtility.DisplayDialog("Error", "Failed to create circular texture.", "OK");
            return;
        }

        // Encode and save
        byte[] pngData = circularTex.EncodeToPNG();
        File.WriteAllBytes(finalPath, pngData);
        DestroyImmediate(circularTex); // clean up temporary texture
        AssetDatabase.Refresh();

        // Convert to asset path and set up as Sprite
        string assetPath = "Assets" + finalPath.Substring(Application.dataPath.Length);
        TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100;
            importer.SaveAndReimport();
        }

        Object createdAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        EditorGUIUtility.PingObject(createdAsset);
        EditorUtility.DisplayDialog("Success", $"Circular icon saved to:\n{assetPath}", "OK");
    }

    private Texture2D MakeCircularTexture(Texture2D source, int targetSize)
    {
        // Prepare temporary render texture
        RenderTexture rt = RenderTexture.GetTemporary(targetSize, targetSize, 24, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;

        // Clear with transparent black
        GL.Clear(true, true, new Color(0, 0, 0, 0));

        // Use a material that draws a circle with alpha = 1 inside, 0 outside
        // But easier: create a new Texture2D and manually set pixels in a circle
        // However, we also need to respect source image content (scale to fit)
        // We'll use Graphics.Blit with a custom shader or manual pixel loop.
        // Manual pixel loop is simpler and doesn't require a separate shader file.

        // First, sample the source texture scaled to target size (stretch to square)
        Texture2D scaledTex = ScaleTexture(source, targetSize, targetSize);
        Color[] srcPixels = scaledTex.GetPixels(0, 0, targetSize, targetSize);

        // Create new texture
        Texture2D result = new Texture2D(targetSize, targetSize, TextureFormat.ARGB32, false);
        Color[] destPixels = new Color[targetSize * targetSize];

        float radius = targetSize / 2f;
        Vector2 center = new Vector2(radius, radius);

        for (int y = 0; y < targetSize; y++)
        {
            for (int x = 0; x < targetSize; x++)
            {
                float dx = x - center.x;
                float dy = y - center.y;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= radius)
                {
                    // Inside circle: keep source color, fully opaque
                    Color col = srcPixels[y * targetSize + x];
                    col.a = 1f;
                    destPixels[y * targetSize + x] = col;
                }
                else
                {
                    // Outside: fully transparent
                    destPixels[y * targetSize + x] = Color.clear;
                }
            }
        }

        result.SetPixels(destPixels);
        result.Apply();

        // Cleanup
        DestroyImmediate(scaledTex);
        RenderTexture.active = null;
        rt.Release();

        return result;
    }

    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight, 24, RenderTextureFormat.ARGB32);
        RenderTexture.active = rt;

        // Blit source to render texture, scaling to fit
        Graphics.Blit(source, rt);
        Texture2D result = new Texture2D(targetWidth, targetHeight, TextureFormat.ARGB32, false);
        result.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        rt.Release();
        return result;
    }

    private string SanitizeFileName(string name)
    {
        string invalidChars = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
        string sanitized = Regex.Replace(name, $"[{Regex.Escape(invalidChars)}]", "");
        sanitized = sanitized.Replace(' ', '_');
        if (sanitized.Length > 50)
            sanitized = sanitized.Substring(0, 50);
        return sanitized;
    }
}