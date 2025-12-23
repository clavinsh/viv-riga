using UnityEngine;
using UnityEditor;
using System.IO;

public class FbxTextures
{
    [MenuItem("Tools/Auto-Assign Textures from FBM Folders")]
    static void AutoAssignTexturesFromFBM()
    {
        string[] materialGuids = AssetDatabase.FindAssets("t:Material");
        int assignedCount = 0;
        
        foreach (string guid in materialGuids)
        {
            string matPath = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
            
            if (mat != null)
            {
                string matName = mat.name;
                
                // Look for matching .fbm folder
                string[] allFolders = AssetDatabase.GetSubFolders("Assets");
                foreach (string folder in GetAllSubFolders("Assets"))
                {
                    string folderName = Path.GetFileName(folder);
                    
                    // Check if folder is like "materialName.fbm"
                    if (folderName == $"{matName}.fbm")
                    {
                        // Get all textures from this folder
                        string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { folder });
                        
                        foreach (string texGuid in textureGuids)
                        {
                            string texPath = AssetDatabase.GUIDToAssetPath(texGuid);
                            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                            
                            if (tex != null)
                            {
                                // Assign based on texture name/type
                                string texName = tex.name.ToLower();
                                
                                // Main/Albedo texture
                                if (texName.Contains("diffuse") || texName.Contains("albedo") || texName.Contains("base") || texName.Contains("color"))
                                {
                                    mat.mainTexture = tex;
                                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                                }
                                // Normal map
                                else if (texName.Contains("normal") || texName.Contains("bump"))
                                {
                                    if (mat.HasProperty("_BumpMap")) mat.SetTexture("_BumpMap", tex);
                                }
                                // Metallic
                                else if (texName.Contains("metallic"))
                                {
                                    if (mat.HasProperty("_MetallicGlossMap")) mat.SetTexture("_MetallicGlossMap", tex);
                                }
                                // If no specific type, assign as main texture
                                else if (mat.mainTexture == null)
                                {
                                    mat.mainTexture = tex;
                                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", tex);
                                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", tex);
                                }
                            }
                        }
                        
                        EditorUtility.SetDirty(mat);
                        assignedCount++;
                        Debug.Log($"Assigned textures from {folderName} to {matName}");
                        break;
                    }
                }
            }
        }
        
        AssetDatabase.SaveAssets();
        Debug.Log($"Done! Assigned textures to {assignedCount} materials.");
    }
    
    static string[] GetAllSubFolders(string rootFolder)
    {
        System.Collections.Generic.List<string> folders = new System.Collections.Generic.List<string>();
        folders.Add(rootFolder);
        
        string[] subFolders = AssetDatabase.GetSubFolders(rootFolder);
        foreach (string folder in subFolders)
        {
            folders.AddRange(GetAllSubFolders(folder));
        }
        
        return folders.ToArray();
    }
}