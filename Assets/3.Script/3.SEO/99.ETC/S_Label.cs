using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class S_Lavel : PropertyAttribute
{
    public string label;
    public S_Lavel(string label)
    {
        this.label = label;
    }
}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(S_Lavel))]
public class S_NamingDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var newLabel = new GUIContent((attribute as S_Lavel).label);
        EditorGUI.PropertyField(position, property, newLabel);
    }
}
#endif