using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Lavel : PropertyAttribute
{
    public string label;
    public Lavel(string label)
    {
        this.label = label;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Lavel))]
public class NamingDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var newLabel = new GUIContent((attribute as Lavel).label);
        EditorGUI.PropertyField(position, property, newLabel);
    }
}
#endif