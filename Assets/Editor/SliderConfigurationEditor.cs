using UnityEditor;

[CustomEditor(typeof(SliderConfiguration))]
public class SliderConfigurationEditor : Editor
{
    private SerializedProperty sliderTypeProp;
    private SerializedProperty sliderProp;
    private SerializedProperty globalVolumeProp;

    private void OnEnable()
    {
        sliderTypeProp = serializedObject.FindProperty("sliderType");
        sliderProp = serializedObject.FindProperty("slider");
        globalVolumeProp = serializedObject.FindProperty("globalVolume");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(sliderTypeProp);
        EditorGUILayout.PropertyField(sliderProp);

        // Get current enum value
        SliderConfiguration.ConfigurationSliderType currentType =
            (SliderConfiguration.ConfigurationSliderType)sliderTypeProp.enumValueIndex;

        if (currentType == SliderConfiguration.ConfigurationSliderType.Brightness)
        {
            EditorGUILayout.PropertyField(globalVolumeProp);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
