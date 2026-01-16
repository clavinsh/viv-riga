using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.Rendering;

public class TextureAssignment: Editor
{
    static EWScriptableObject eWSettings;

    [MenuItem("Assets/Easy Way Tools/Texture Assignment Tool")]
    private static void TextureAssignmentTool()
    {
        string[] projectTextures = AssetDatabase.FindAssets("t:Texture2D", new [] { "Assets" });
        var selected = Selection.objects;

        // Pre-compute all filenames and paths once to avoid repeated GUIDToAssetPath calls
        Dictionary<string, string> textureFileNames = new Dictionary<string, string>();
        Dictionary<string, string> texturePaths = new Dictionary<string, string>();
        foreach (string guid in projectTextures)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            texturePaths[guid] = path;
            textureFileNames[guid] = Path.GetFileNameWithoutExtension(path);
        }

        GetEWScriptableObject();

        // Cache render pipeline once - it doesn't change during execution
        string renderPipeline = GetRenderPipeline();

        // Build profile lookup dictionary for O(1) access by shader name
        Dictionary<string, EWScriptableObject.AssignmentProfile> profilesByShader = new Dictionary<string, EWScriptableObject.AssignmentProfile>();
        foreach (EWScriptableObject.AssignmentProfile assignmentProfile in eWSettings.assignmentProfilesList)
        {
            profilesByShader[assignmentProfile.shaderName] = assignmentProfile;
        }

        // Pre-cache split and trimmed suffixes for each profile item
        Dictionary<EWScriptableObject.AssignmentProfile.AssignmentProfileItem, string[]> cachedSuffixes =
            new Dictionary<EWScriptableObject.AssignmentProfile.AssignmentProfileItem, string[]>();
        foreach (EWScriptableObject.AssignmentProfile profile in eWSettings.assignmentProfilesList)
        {
            foreach (EWScriptableObject.AssignmentProfile.AssignmentProfileItem profileItem in profile.assignmentProfileItems)
            {
                string[] rawSuffixes = profileItem.textureName.Split(',');
                List<string> trimmedSuffixes = new List<string>();
                foreach (string suf in rawSuffixes)
                {
                    string trimmed = suf.Trim(' ');
                    if (trimmed.Length > 0)
                        trimmedSuffixes.Add(trimmed);
                }
                cachedSuffixes[profileItem] = trimmedSuffixes.ToArray();
            }
        }

        // Collect materials to process
        List<Material> materialsToProcess = new List<Material>();
        foreach (var o in selected)
        {
            if (o.GetType() == typeof(Material))
            {
                materialsToProcess.Add((Material)o);
            }
        }

        // Track materials that were modified for batched import
        List<string> modifiedMaterialPaths = new List<string>();

        // Batch asset operations for better performance
        AssetDatabase.StartAssetEditing();
        try
        {
            for (int i = 0; i < materialsToProcess.Count; i++)
            {
                Material material = materialsToProcess[i];

                // Show progress bar for large batches
                if (materialsToProcess.Count > 5)
                {
                    EditorUtility.DisplayProgressBar("Assigning Textures",
                        $"Processing {material.name} ({i + 1}/{materialsToProcess.Count})",
                        (float)i / materialsToProcess.Count);
                }

                string shaderName = material.shader.name;

                // O(1) profile lookup instead of iterating
                if (!profilesByShader.TryGetValue(shaderName, out EWScriptableObject.AssignmentProfile matchedProfile))
                {
                    Debug.Log("Profile for Material " + material.name + " not found");
                    continue;
                }

                if (matchedProfile.profileName == null)
                {
                    Debug.Log("Profile for Material " + material.name + " not found");
                    continue;
                }

                //Filter Textures contain/start with Material Name
                string materialName = material.name;

                List<string> matMatchedTextures = new List<string>();

                foreach (string projectTexture in projectTextures)
                {
                    string textureName = textureFileNames[projectTexture];
                    if ((textureName.StartsWith(materialName) && eWSettings.assignmentMethod == 0) || (textureName.Contains(materialName) && eWSettings.assignmentMethod == 1))
                    {
                        matMatchedTextures.Add(projectTexture);
                    }
                }

                //If was found textures with material name, Try assign it to Material Slots
                if (matMatchedTextures.Count > 0)
                {
                    foreach (EWScriptableObject.AssignmentProfile.AssignmentProfileItem profileItem in matchedProfile.assignmentProfileItems)
                    {
                        // Use pre-cached trimmed suffixes
                        string[] searchingTextureSuf = cachedSuffixes[profileItem];

                        string slotMatchedTexture = "";

                        foreach (var matMatchedTexture in matMatchedTextures)
                        {
                            string textureName = textureFileNames[matMatchedTexture];

                            foreach (var textureSuf in searchingTextureSuf)
                            {
                                if (textureName.EndsWith(textureSuf))
                                    slotMatchedTexture = matMatchedTexture;
                            }
                        }

                        if (slotMatchedTexture.Length > 0)
                        {
                            Texture2D texture = (Texture2D)AssetDatabase.LoadAssetAtPath(texturePaths[slotMatchedTexture], typeof(Texture2D));
                            material.SetTexture(profileItem.materialSlot, texture);

                            if (renderPipeline == "Legacy")
                            {
                                if (profileItem.materialSlot == "_BumpMap")
                                    material.EnableKeyword("_NORMALMAP");

                                if (profileItem.materialSlot == "_MetallicGlossMap")
                                    material.EnableKeyword("_METALLICGLOSSMAP");

                                if (profileItem.materialSlot == "_SpecGlossMap")
                                    material.EnableKeyword("_SPECGLOSSMAP");

                                if (profileItem.materialSlot == "_EmissionMap")
                                {
                                    MaterialEditor.FixupEmissiveFlag(material);
                                    if (material.GetColor("_EmissionColor") == Color.black)
                                        material.SetColor("_EmissionColor", Color.white);
                                    material.EnableKeyword("_EMISSION");
                                    material.globalIlluminationFlags = 0;
                                }
                            }

                            if (renderPipeline == "URP")
                            {
                                if (profileItem.materialSlot == "_BumpMap")
                                    material.EnableKeyword("_NORMALMAP");

                                if (profileItem.materialSlot == "_MetallicGlossMap")
                                {
                                    material.DisableKeyword("_SPECULAR_SETUP");
                                    material.EnableKeyword("_METALLICSPECGLOSSMAP");
                                    material.SetFloat("_WorkflowMode", 1);
                                    material.SetFloat("_Smoothness", 1);
                                }

                                if (profileItem.materialSlot == "_SpecGlossMap")
                                {
                                    material.EnableKeyword("_SPECULAR_SETUP");
                                    material.SetFloat("_WorkflowMode", 0);
                                    material.SetFloat("_Smoothness", 1);
                                    material.EnableKeyword("_METALLICSPECGLOSSMAP");
                                }

                                if (profileItem.materialSlot == "_EmissionMap")
                                {
                                    MaterialEditor.FixupEmissiveFlag(material);
                                    if (material.GetColor("_EmissionColor") == Color.black)
                                        material.SetColor("_EmissionColor", Color.white);
                                    material.EnableKeyword("_EMISSION");
                                    material.globalIlluminationFlags = 0;
                                }
                            }

                            if (renderPipeline == "HDRP")
                            {
                                if (profileItem.materialSlot == "_NormalMap")
                                    material.EnableKeyword("_NORMALMAP");

                                if (profileItem.materialSlot == "_MaskMap")
                                    material.EnableKeyword("_MASKMAP");

                                if (profileItem.materialSlot == "_SpecularColorMap")
                                {
                                    material.EnableKeyword("_MATERIAL_FEATURE_SPECULAR_COLOR");
                                    material.SetFloat("_MaterialID", 4f);
                                }

                                if (profileItem.materialSlot == "_EmissiveColorMap")
                                {
                                    if (material.GetColor("_EmissiveColor") == Color.black)
                                        material.SetColor("_EmissiveColor", Color.white);
                                    material.EnableKeyword("_EMISSIVE_COLOR_MAP");
                                }
                            }

                        }
                    }

                    // Collect path for batched import instead of importing immediately
                    modifiedMaterialPaths.Add(AssetDatabase.GetAssetPath(material));
                }
            }

            // Batched import of all modified materials
            foreach (string materialPath in modifiedMaterialPaths)
            {
                AssetDatabase.ImportAsset(materialPath, ImportAssetOptions.ForceUpdate);
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            EditorUtility.ClearProgressBar();
        }

        // Single refresh after all materials are processed
        AssetDatabase.Refresh();
    }
    static void GetEWScriptableObject()
    {
        string eWScriptObjPath = "Assets/Editor/EasyWayTools/EWSettings.asset";
        eWSettings = (EWScriptableObject)AssetDatabase.LoadAssetAtPath(eWScriptObjPath, typeof(EWScriptableObject));
        if (eWSettings == null)
        {
            eWSettings = ScriptableObject.CreateInstance<EWScriptableObject>();
            AssetDatabase.CreateAsset(eWSettings, eWScriptObjPath);
            AssetDatabase.Refresh();
        }

        if (eWSettings.assignmentProfilesList.Count < 1)
        {
            eWSettings.InitDefaultAssignmentProfiles();
            SaveSettings();
        }
    }

    static void SaveSettings()
    {
        EditorUtility.SetDirty(eWSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    static string GetFileName(string fileName)
    {
        return Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(fileName));
    }

    static string GetRenderPipeline()
    {
        string renderPipeline = "";

        if (GraphicsSettings.defaultRenderPipeline == null)
        {
            renderPipeline = "Legacy";
        }
        else if (GraphicsSettings.defaultRenderPipeline.GetType().Name.Contains("HDRender"))
        {
            renderPipeline = "HDRP";
        }
        else if (GraphicsSettings.defaultRenderPipeline.GetType().Name.Contains("UniversalRender"))
        {
            renderPipeline = "URP";
        }

        return renderPipeline;
    }
}