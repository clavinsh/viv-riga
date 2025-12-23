using UnityEngine;
using UnityEditor;
using System.IO;

public class BatchTextureTool
{
    [MenuItem("Tools/Extract FBX Textures and Set Quality")]
    static void ProcessFBXTextures()
    {
        // Step 1: Set all FBX to extract materials externally
        string[] fbxPaths = AssetDatabase.FindAssets("t:Model");
        int fbxCount = 0;
        
        foreach (string guid in fbxPaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            if (path.EndsWith(".fbx"))
            {
                ModelImporter modelImporter = AssetImporter.GetAtPath(path) as ModelImporter;
                if (modelImporter != null)
                {
                    modelImporter.materialLocation = ModelImporterMaterialLocation.External;
                    modelImporter.SaveAndReimport();
                    fbxCount++;
                }
            }
        }
        
        Debug.Log($"Processed {fbxCount} FBX files for material extraction.");
        AssetDatabase.Refresh();
        
        // Step 2: Set all textures to 4096 with high quality
        string[] texturePaths = AssetDatabase.FindAssets("t:Texture2D");
        int textureCount = 0;
        
        foreach (string guid in texturePaths)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            
            if (textureImporter != null)
            {
                textureImporter.maxTextureSize = 4096;
                textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
                textureImporter.SaveAndReimport();
                textureCount++;
            }
        }
        
        Debug.Log($"Configured {textureCount} textures to 4096 with high quality compression.");
        Debug.Log("All done!");
    }
}