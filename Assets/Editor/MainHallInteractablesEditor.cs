using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MainHallInteractables))]
public class MainHallInteractablesEditor : Editor
{
    private SerializedProperty typeProp;
    private SerializedProperty canvasToOpenProp;
    private SerializedProperty sceneToOpenProp;
    private SerializedProperty isAutomaticProp;
    private SerializedProperty interactionHintProp;
    private SerializedProperty upgradePropsContainerProp;

    private void OnEnable()
    {
        typeProp = serializedObject.FindProperty("type");
        sceneToOpenProp = serializedObject.FindProperty("sceneToOpen");
        canvasToOpenProp = serializedObject.FindProperty("canvasToOpen");
        isAutomaticProp = serializedObject.FindProperty("isAutomatic");
        interactionHintProp = serializedObject.FindProperty("interactionHint");
        upgradePropsContainerProp = serializedObject.FindProperty("upgradePropsContainer");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(typeProp);
        EditorGUILayout.PropertyField(canvasToOpenProp);
        

        // Get current enum value
        MainHallInteractables.InteractionType currentType =
            (MainHallInteractables.InteractionType)typeProp.enumValueIndex;

        if (currentType == MainHallInteractables.InteractionType.Leave)
        {
            EditorGUILayout.PropertyField(sceneToOpenProp);
        }

        if (currentType == MainHallInteractables.InteractionType.Upgrade)
        {
            EditorGUILayout.PropertyField(upgradePropsContainerProp);
        }

        EditorGUILayout.PropertyField(isAutomaticProp);
        EditorGUILayout.PropertyField(interactionHintProp);

        serializedObject.ApplyModifiedProperties();
    }
}