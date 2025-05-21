using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(PipeContainer)), CanEditMultipleObjects]
public class NPipeContainerEditor : Editor
{
    private IPipeImportable editingImportable = null;
    private static IPipeImportable lastEditingImportable = null;
    private int selectedAppendIndex = 0;
    private bool confirmDeletion = false;
    private bool isMultiInstance = false;
    private GUIStyle normalStyle;
    private GUIStyle boldStyle;
    private Color thisContainerColor;
    private Color thisContainerMultiColor;

    public override void OnInspectorGUI()
    {
        // try to re-open similar importable to the last one
        if (editingImportable == null && lastEditingImportable != null)
        {
            string warningMessage = "";
            IPipeImportable[] imp = Utils.GetSimiliarPipes(new UnityEngine.Object[] { this.target }, this.target as PipeContainer, lastEditingImportable, out warningMessage);
            if (imp.Length != 1)
            {
                lastEditingImportable = null;
            }
            else
            {
                editingImportable = imp[0];
            }
        }

        isMultiInstance = targets.Length > 1;

        // main inspectorr
        DrawPipelineElements();

    }
    protected bool DrawPipelineElements()
    {
        //==================================================================================================================================
        // setup colors
        //==================================================================================================================================
        normalStyle = new GUIStyle((GUIStyle)"helpbox");
        normalStyle.normal.textColor = Color.black;
        normalStyle.fontStyle = FontStyle.Normal;
        boldStyle = new GUIStyle((GUIStyle)"helpbox");
        boldStyle.normal.textColor = Color.black;
        boldStyle.fontStyle = FontStyle.Bold;
        thisContainerColor = new Color(0.8f, 1.0f, 0.6f);
        thisContainerMultiColor = new Color(0.8f, 0.6f, 1.0f);

        string assetPath = AssetDatabase.GetAssetPath(target);
        IPipeImportable[] allImportables = Utils.GetByType<IPipeImportable>(target);
        IPipeImportable[] outputPipes = Utils.FindOutputPipes(allImportables);

        //==================================================================================================================================
        // Tool Buttons (Select Me, Invalidation)
        //==================================================================================================================================

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Select Me"))
        {
            Selection.objects = this.targets;
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Invalidate & Reimport All"))
        {
            Utils.InvalidateAndReimportAll(targets);
        }
        if (GUILayout.Button("Invalidate & Reimport All Deep"))
        {
            Utils.InvalidateAndReimportAllDeep(targets);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndHorizontal();

        //==================================================================================================================================
        // Single Instance Mode Only for Lazyness Tool Buttons (Deleta all Payload, Instantiate)
        //==================================================================================================================================

        if (!this.isMultiInstance)
        {
            if (GUILayout.Button("Delete all Payload"))
            {
                UnityEngine.Object[] allObjects = AssetDatabase.LoadAllAssetsAtPath(assetPath);
                foreach (UnityEngine.Object obj in allObjects)
                {
                    if (!(obj is IPipeImportable) && !(obj is PipeContainer))
                    {
                        DestroyImmediate(obj, true);
                    }
                }
                EditorUtility.SetDirty(target);
            }

            foreach (IPipeImportable imp in allImportables)
            {
                if (imp is IPipeInstantiable && GUILayout.Button("Instantiate " + imp.GetInstanceName() + " (" + imp.GetTypeName() + ")"))
                {
                    ((IPipeInstantiable)imp).Instatiate();
                }
            }
        }

        //==================================================================================================================================
        // Draw Pipelines
        //==================================================================================================================================
        {
            GUILayout.Space(10f);

            // headline
            GUILayout.Label(string.Format("Pipelines", allImportables.Length), EditorStyles.boldLabel);

            // pipelines
            GUILayout.BeginHorizontal();
            HashSet<IPipeImportable> visited = new HashSet<IPipeImportable>();
            foreach (IPipeImportable iimp in outputPipes)
            {
                DrawPipelineElements(assetPath, iimp, visited, false);
            }

            GUI.backgroundColor = isMultiInstance ? thisContainerMultiColor : thisContainerColor;

            if (!isMultiInstance)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Add: ");
                {
                    GUILayout.BeginVertical();
                    foreach (Type factoryType in NPipeReflectionUtil.GetAllTypesWithAttribute(typeof(PipeStartingAttribute)))
                    {
                        PipeStartingAttribute attr = (PipeStartingAttribute)factoryType.GetCustomAttributes(typeof(PipeStartingAttribute), true)[0];
                        if (attr.attached && GUILayout.Button(attr.name))
                        {
                            Utils.CreateAttachedPipe(assetPath, factoryType);
                        }
                    }

                    List<System.Type> allTypes = new List<System.Type>(NPipeReflectionUtil.GetAllTypesWithAttribute(typeof(PipeAppendableAttribute)));
                    List<string> allLabels = new List<string>();
                    allLabels.Add("Other ...");
                    foreach (Type factoryType in allTypes)
                    {
                        PipeAppendableAttribute attr = (PipeAppendableAttribute)factoryType.GetCustomAttributes(typeof(PipeAppendableAttribute), true)[0];
                        allLabels.Add(attr.name);
                    }
                    int selection = EditorGUILayout.Popup(0, allLabels.ToArray());
                    if (selection > 0)
                    {
                        Utils.CreateAttachedPipe(assetPath, allTypes[selection - 1]);
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndHorizontal();
        }

        drawPipeEditor(assetPath);

        return false;
    }


    protected string DrawPipelineElements(string assetPath, IPipeImportable importable, HashSet<IPipeImportable> visited, bool hasNext, string nextAssetPath = "")
    {
        string thisAssetPath = AssetDatabase.GetAssetPath(importable as UnityEngine.Object);
        bool thisIsInContainer = thisAssetPath == assetPath;
        bool nextIsInContainer = nextAssetPath == assetPath;

        //====================================================================================================================
        // Recursion for parent elements
        //====================================================================================================================

        GUILayout.BeginVertical();
        IPipeComposite composite = importable as IPipeComposite;
        bool isSource = true;
        bool parentIsInDifferentAsset = false;
        if (composite != null)
        {
            IPipeImportable[] sources = composite.GetAllInputs();
            GUILayout.BeginHorizontal();
            if (sources != null)
            {
                foreach (IPipeImportable source in sources)
                {
                    isSource = false;
                    if (source != null)
                    {
                        if (thisAssetPath != DrawPipelineElements(assetPath, source, visited, true, thisAssetPath))
                        {
                            parentIsInDifferentAsset = true;
                        }
                    }
                    else
                    {
                        GUILayout.Label("NULL !");
                    }
                }
            }

            GUILayout.EndHorizontal();
        }

        //====================================================================================================================
        // Background Color
        //====================================================================================================================

        if ((!thisIsInContainer && parentIsInDifferentAsset) || (isSource && !thisIsInContainer && thisAssetPath != null))
        {
            GUILayout.BeginHorizontal();
            GUI.backgroundColor = GetColorForAssetPath(thisAssetPath);
            EditorGUILayout.ObjectField(AssetDatabase.LoadMainAssetAtPath(thisAssetPath), typeof(PipeContainer), false);
            GUI.backgroundColor = Color.white;
            GUILayout.EndHorizontal();
        }

        GUILayout.BeginHorizontal();

        if (visited.Contains(importable))
        {
            GUILayout.Label(importable.GetTypeName());
        }
        else
        {
            visited.Add(importable);

            //====================================================================================================================
            // Action Buttons
            //====================================================================================================================

            if (!thisIsInContainer)
            {
                GUI.backgroundColor = GetColorForAssetPath(thisAssetPath);
            }
            else
            {
                GUI.backgroundColor = isMultiInstance ? thisContainerMultiColor : thisContainerColor;
            }

            //====================================================================================================================
            // Delete Editiable
            //====================================================================================================================

            if (editingImportable == importable && this.targets.Length < 2)
            {
                if (this.targets.Length < 2)
                {
                    if (!confirmDeletion && GUILayout.Button("Delete"))
                    {
                        confirmDeletion = true;
                    }
                    else if (confirmDeletion && GUILayout.Button("Sure?"))
                    {
                        editingImportable = null;
                        lastEditingImportable = null;
                        Delete(assetPath, importable);
                        AssetDatabase.SaveAssets();
                    }
                }
            }

            //====================================================================================================================
            // Edit Editiable
            //====================================================================================================================

            if (editingImportable == importable)
            {
                if (GUILayout.Button("Close", GUILayout.Width(40)))
                {
                    editingImportable = null;
                    lastEditingImportable = null;
                }
            }
            else
            {
                if (thisIsInContainer && GUILayout.Button("Edit", GUILayout.Width(40)))
                {
                    editingImportable = importable;
                    lastEditingImportable = importable;
                    confirmDeletion = false;
                }
            }

            //====================================================================================================================
            // Editable Label
            //====================================================================================================================

            GUIStyle style = normalStyle;
            if (editingImportable == importable)
            {
                style = boldStyle;
            }
            string n = ((PipeContainer)target).GetDisplayName(importable);
            GUILayout.Label(n, style);
        }
        GUILayout.EndHorizontal();

        if (hasNext)
        {
            if (thisIsInContainer || nextIsInContainer)
            {
                DrawArrow(GUI.backgroundColor = isMultiInstance ? thisContainerMultiColor : thisContainerColor);
            }
            else if (nextAssetPath != thisAssetPath)
            {
                DrawArrow(GetColorForAssetPath(nextAssetPath));
            }
        }

        GUI.backgroundColor = Color.white;

        GUILayout.EndVertical();

        return thisAssetPath;
    }

    private void DrawArrow(Color color)
    {
        Rect rect = GUILayoutUtility.GetLastRect();

        // Draw the lines
        Handles.BeginGUI();

        Handles.color = color;

        float width1 = 2.0f;
        float width2 = 10.0f;
        float height1 = 10.0f;
        float height2 = 12.0f;
        Vector2 anchor = new Vector2(rect.xMin + (rect.xMax - rect.xMin) / 2f, rect.yMax + 4f);

        Vector2 p1 = anchor + Vector2.left * width1;
        Vector2 p2 = anchor + Vector2.right * width1;
        Vector2 p3 = p1 + Vector2.up * height1;
        Vector2 p4 = p2 + Vector2.up * height1;
        Handles.DrawAAConvexPolygon(new Vector3[] { p1, p3, p4, p2 });

        Vector2 q1 = anchor + Vector2.left * width2 + Vector2.up * height1;
        Vector2 q2 = anchor + Vector2.right * width2 + Vector2.up * height1;
        Vector2 q3 = anchor + Vector2.up * (height1 + height2);
        Handles.DrawAAConvexPolygon(new Vector3[] { q1, q2, q3 });

        Handles.EndGUI();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
    }

    private static Dictionary<string, Color> assetPathColors = new Dictionary<string, Color>();

    private Color GetColorForAssetPath(string path)
    {
        if (!assetPathColors.ContainsKey(path))
        {
            assetPathColors.Add(path, UnityEngine.Random.ColorHSV(0.0f, 1.0f, 0.2f, 0.2f, 0.7f, 0.7f, 1, 1));
        }
        return assetPathColors[path];
    }

    protected void Delete(string path, IPipeImportable importable)
    {
        editingImportable = null;
        lastEditingImportable = null;

        importable.Destroy();
        Undo.DestroyObjectImmediate(importable as UnityEngine.Object);

        AssetDatabase.Refresh();
    }

    protected void Delete(string path, IPipeImportable[] importable)
    {
        Debug.LogError("Not Yet Supported");
    }

    private void drawPipeEditor(string assetPath)
    {
        IPipeImportable importable = editingImportable as IPipeImportable;
        if (importable == null)
        {
            return;
        }

        //====================================================================================================================
        // Selected Importable(s) Label
        //====================================================================================================================

        IPipeImportable[] multiInstanceEditingImportables = null;

        if (isMultiInstance)
        {
            string warningMessage = "";
            multiInstanceEditingImportables = Utils.GetSimiliarPipes(this.targets, this.target as PipeContainer, this.editingImportable as IPipeImportable, out warningMessage);

            GUILayout.Space(10f);
            GUILayout.Label(string.Format("Selected: {0} ( {1} instances )", editingImportable.GetTypeName(), multiInstanceEditingImportables.Length), EditorStyles.boldLabel);

            if (warningMessage.Length > 0)
            {
                //                GUI.backgroundColor = Color.yellow;
                GUILayout.Label("WARNING: " + warningMessage);
            }
        }
        else
        {
            GUILayout.Space(10f);
            GUILayout.Label("Selected: " + editingImportable.GetTypeName(), EditorStyles.boldLabel);
        }

        //====================================================================================================================
        // Selected Importable(s) Editor Inspectors
        //====================================================================================================================

        IPipeEditable editable = editingImportable as IPipeEditable;
        if (editable != null)
        {
            GUILayout.Label("Edit:");
            if (isMultiInstance)
            {
                editable.DrawMultiInstanceEditor(~EditFlags.TOOLS, Utils.GetUntypedFactories<IPipeImportable>(multiInstanceEditingImportables));
            }
            else
            {
                editable.DrawInspector(~EditFlags.TOOLS);
            }
        }

        GUILayout.Space(10f);
        GUILayout.BeginVertical();
        GUILayout.Space(10f);

        //====================================================================================================================
        // append other pipes 
        //====================================================================================================================

        if (!isMultiInstance)
        {
            GUILayout.BeginHorizontal();
            List<System.Type> allTypes = new List<System.Type>();
            List<PipeAppendableAttribute> allAttrsa = new List<PipeAppendableAttribute>();
            List<string> allLabels = new List<string>();
            foreach (Type factoryType in NPipeReflectionUtil.GetAllTypesWithAttribute(typeof(PipeAppendableAttribute)))
            {
                PipeAppendableAttribute attr = (PipeAppendableAttribute)factoryType.GetCustomAttributes(typeof(PipeAppendableAttribute), true)[0];
                if (!attr.sourceType.IsAssignableFrom(importable.GetType()))
                {
                    continue;
                }
                allTypes.Add(factoryType);
                allAttrsa.Add(attr);
                allLabels.Add(attr.name);
            }
            if (allTypes.Count > 0)
            {
                GUILayout.Label("Append: ");
                selectedAppendIndex = EditorGUILayout.Popup(selectedAppendIndex, allLabels.ToArray());
                if (selectedAppendIndex >= 0 && selectedAppendIndex < allTypes.Count)
                {
                    PipeAppendableAttribute pipe = allAttrsa[selectedAppendIndex];
                    IPipeComposite newImportable = null;

                    if (pipe.attached && GUILayout.Button("This Container"))
                    {
                        newImportable = Utils.CreateAttachedPipe(assetPath, allTypes[selectedAppendIndex], importable) as IPipeComposite;
                        editingImportable = newImportable;
                        lastEditingImportable = newImportable;
                        confirmDeletion = false;
                    }
                    if (pipe.separate && GUILayout.Button("New Container"))
                    {
                        newImportable = Utils.CreateSeparatedPipe(assetPath, allTypes[selectedAppendIndex], importable) as IPipeComposite;
                    }

                    if (newImportable != null)
                    {
                        AssetDatabase.SaveAssets();
                        UnityEditor.Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GetAssetPath(newImportable as UnityEngine.Object));
                    }
                }
                else
                {
                    selectedAppendIndex = 0;
                }
            }
            GUILayout.EndHorizontal();
        }


        GUILayout.EndVertical();
    }
}