using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PipeSelectorAttribute))]
public class NPipePropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (Selection.gameObjects.Length > 1)
        {
            EditorGUI.LabelField(position, label.text + ": Multiple object editing not yet supported, sorry");
            return;
        }

        PipeSelectorAttribute selector = attribute as PipeSelectorAttribute;
        UnityEngine.Object val = (UnityEngine.Object)typeof(Utils)
            .GetMethod("DrawSourcePropertySelector")
            .MakeGenericMethod(selector.Type)
            .Invoke(null, new object[] {
                label, position, property.objectReferenceValue, null
            });

        property.objectReferenceValue = val;
    }
}