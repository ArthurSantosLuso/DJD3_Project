using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomPropertyDrawer(typeof(SceneDropdownAttribute))]  
public class SceneDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Get all enabled scenes from Build Settings
        var scenes = EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => System.IO.Path.GetFileNameWithoutExtension(s.path))
            .ToArray();

        // Find current index
        int currentIndex = Mathf.Max(0, System.Array.IndexOf(scenes, property.stringValue));

        // Draw popup
        int selectedIndex = EditorGUI.Popup(position, label.text, currentIndex, scenes);

        // Save selected scene name
        if (selectedIndex >= 0 && selectedIndex < scenes.Length)
        {
            property.stringValue = scenes[selectedIndex];
        }
    }
}