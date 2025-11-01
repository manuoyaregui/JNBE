using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Editor for PlayerPhysicsController that shows module properties conditionally
/// based on whether each module is enabled.
/// </summary>
[CustomEditor(typeof(PlayerPhysicsController))]
[CanEditMultipleObjects]
public class PlayerPhysicsControllerEditor : Editor
{
    private SerializedProperty enableGravity;
    private SerializedProperty enableInertia;
    private SerializedProperty enableJump;
    private SerializedProperty enableDash;
    private SerializedProperty enablePogo;
    private SerializedProperty enableClimb;
    private SerializedProperty enableMovement;
    private SerializedProperty enableCamera;
    private SerializedProperty enableExternalForce;
    
    private SerializedProperty gravitySettings;
    private SerializedProperty inertiaSettings;
    private SerializedProperty jumpSettings;
    private SerializedProperty dashSettings;
    private SerializedProperty pogoSettings;
    private SerializedProperty climbSettings;
    private SerializedProperty movementSettings;
    private SerializedProperty cameraSettings;
    private SerializedProperty externalForceSettings;
    
    private void OnEnable()
    {
        // Find enable/disable properties
        enableGravity = serializedObject.FindProperty("enableGravity");
        enableInertia = serializedObject.FindProperty("enableInertia");
        enableJump = serializedObject.FindProperty("enableJump");
        enableDash = serializedObject.FindProperty("enableDash");
        enablePogo = serializedObject.FindProperty("enablePogo");
        enableClimb = serializedObject.FindProperty("enableClimb");
        enableMovement = serializedObject.FindProperty("enableMovement");
        enableCamera = serializedObject.FindProperty("enableCamera");
        enableExternalForce = serializedObject.FindProperty("enableExternalForce");
        
        // Find module settings properties
        gravitySettings = serializedObject.FindProperty("gravitySettings");
        inertiaSettings = serializedObject.FindProperty("inertiaSettings");
        jumpSettings = serializedObject.FindProperty("jumpSettings");
        dashSettings = serializedObject.FindProperty("dashSettings");
        pogoSettings = serializedObject.FindProperty("pogoSettings");
        climbSettings = serializedObject.FindProperty("climbSettings");
        movementSettings = serializedObject.FindProperty("movementSettings");
        cameraSettings = serializedObject.FindProperty("cameraSettings");
        externalForceSettings = serializedObject.FindProperty("externalForceSettings");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        // Draw script reference (disabled)
        EditorGUI.BeginDisabledGroup(true);
        SerializedProperty scriptProp = serializedObject.FindProperty("m_Script");
        EditorGUILayout.PropertyField(scriptProp);
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space(5);
        
        // Draw core systems (always visible) - these are shown normally
        DrawCoreSystems();
        
        EditorGUILayout.Space(10);
        
        // Module Toggles Section
        EditorGUILayout.LabelField("Module Toggles", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        enableGravity.boolValue = EditorGUILayout.Toggle("Enable Gravity", enableGravity.boolValue);
        enableInertia.boolValue = EditorGUILayout.Toggle("Enable Inertia", enableInertia.boolValue);
        enableJump.boolValue = EditorGUILayout.Toggle("Enable Jump", enableJump.boolValue);
        enableDash.boolValue = EditorGUILayout.Toggle("Enable Dash", enableDash.boolValue);
        enablePogo.boolValue = EditorGUILayout.Toggle("Enable Pogo", enablePogo.boolValue);
        enableClimb.boolValue = EditorGUILayout.Toggle("Enable Climb", enableClimb.boolValue);
        enableMovement.boolValue = EditorGUILayout.Toggle("Enable Movement", enableMovement.boolValue);
        enableCamera.boolValue = EditorGUILayout.Toggle("Enable Camera", enableCamera.boolValue);
        enableExternalForce.boolValue = EditorGUILayout.Toggle("Enable External Force", enableExternalForce.boolValue);
        EditorGUI.indentLevel--;
        
        EditorGUILayout.Space(10);
        
        // Physics Modules Section
        EditorGUILayout.LabelField("Physics Modules", EditorStyles.boldLabel);
        
        if (enableGravity.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(gravitySettings, new GUIContent("Gravity Settings"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        if (enableInertia.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(inertiaSettings, new GUIContent("Inertia Settings"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        if (enableExternalForce.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(externalForceSettings, new GUIContent("External Force Settings"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.Space(5);
        
        // Movement Modules Section
        EditorGUILayout.LabelField("Movement Modules", EditorStyles.boldLabel);
        
        if (enableJump.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(jumpSettings, new GUIContent("Jump Settings"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        if (enableDash.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(dashSettings, new GUIContent("Dash Settings"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        if (enablePogo.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(pogoSettings, new GUIContent("Pogo Settings"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        if (enableClimb.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(climbSettings, new GUIContent("Climb Settings"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        if (enableMovement.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(movementSettings, new GUIContent("Movement Settings"), true);
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(5);
        }
        
        EditorGUILayout.Space(5);
        
        // Camera Module Section
        EditorGUILayout.LabelField("Camera Module", EditorStyles.boldLabel);
        
        if (enableCamera.boolValue)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(cameraSettings, new GUIContent("Camera Settings"), true);
            EditorGUI.indentLevel--;
        }
        
        // Draw legacy public access fields
        SerializedProperty resetCameraProp = serializedObject.FindProperty("resetCamera");
        if (resetCameraProp != null)
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(resetCameraProp);
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawCoreSystems()
    {
        // Draw Core Systems section
        SerializedProperty collisionLayers = serializedObject.FindProperty("collisionLayers");
        SerializedProperty floorAngleThreshold = serializedObject.FindProperty("floorAngleThreshold");
        SerializedProperty wallAngleThreshold = serializedObject.FindProperty("wallAngleThreshold");
        SerializedProperty groundCheckRadius = serializedObject.FindProperty("groundCheckRadius");
        SerializedProperty wallCheckDistance = serializedObject.FindProperty("wallCheckDistance");
        
        SerializedProperty gunCamera = serializedObject.FindProperty("GunCamera");
        SerializedProperty footPoint = serializedObject.FindProperty("footPoint");
        SerializedProperty wallPointL = serializedObject.FindProperty("wallPointL");
        SerializedProperty wallPointR = serializedObject.FindProperty("wallPointR");
        SerializedProperty camera = serializedObject.FindProperty("camera");
        
        if (collisionLayers != null)
        {
            EditorGUILayout.LabelField("Core Systems (Always Active)", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(collisionLayers);
            EditorGUILayout.PropertyField(floorAngleThreshold);
            EditorGUILayout.PropertyField(wallAngleThreshold);
            EditorGUILayout.PropertyField(groundCheckRadius);
            EditorGUILayout.PropertyField(wallCheckDistance);
            EditorGUI.indentLevel--;
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(gunCamera);
            EditorGUILayout.PropertyField(footPoint);
            EditorGUILayout.PropertyField(wallPointL);
            EditorGUILayout.PropertyField(wallPointR);
            EditorGUILayout.PropertyField(camera);
            EditorGUI.indentLevel--;
        }
    }
}

