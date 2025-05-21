using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NPVoxUtils : MonoBehaviour
{
#if UNITY_EDITOR
    private static PipeContainer templateImportable = null;

    public static void LoadTemplateMetadata(out PipeContainer metadata, out bool unavailable)
    {
        if (templateImportable != null)
        {
            metadata = templateImportable;
            unavailable = false;
            return;
        }
        string metadataTemplatePath = AssetDatabase.GUIDToAssetPath(NPVoxConstants.PIPELINE_TEMPLATE);
        if (metadataTemplatePath == null)
        {
            UnityEngine.Debug.LogWarning(
                "NPVox: Could not find the Metadata Template with GUID '" + NPVoxConstants.PIPELINE_TEMPLATE +
                "' (" + metadataTemplatePath + "), if you removed the Asset, please create a new Asset and set it's GUID in Preferences -> NPVox"
            );
            unavailable = true;
            metadata = templateImportable;
            return;
        }
        unavailable = false;
        metadata = templateImportable = (PipeContainer)AssetDatabase.LoadAssetAtPath(metadataTemplatePath, typeof(PipeContainer));
    }

    public static PipeContainer GetTemplatePipeline()
    {
        if (templateImportable == null)
        {
            PipeContainer template;
            bool unavailable;
            LoadTemplateMetadata(out template, out unavailable);
        }
        return templateImportable;
    }

    public static T GetTemplateForType<T>() where T : UnityEngine.Object
    {
        string metadataTemplatePath = AssetDatabase.GUIDToAssetPath(NPVoxConstants.PIPELINE_TEMPLATE);
        if (metadataTemplatePath == null)
        {
            UnityEngine.Debug.LogWarning(
                "NPVox: Could not find the Metadata Template with GUID '" + NPVoxConstants.PIPELINE_TEMPLATE +
                "' (" + metadataTemplatePath + "), if you removed the Asset, please create a new Asset and set it's GUID in Preferences -> NPVox"
            );
            return null;
        }
        return (T)AssetDatabase.LoadAssetAtPath(metadataTemplatePath, typeof(T));
    }
#endif
}