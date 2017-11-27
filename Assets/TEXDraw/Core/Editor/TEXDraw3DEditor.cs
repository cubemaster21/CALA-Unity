using UnityEngine;
using UnityEditor;
using TexDrawLib;

[CustomEditor(typeof(TEXDraw3D))]
[CanEditMultipleObjects]
public class TEXDraw3DEditor : Editor
{
    SerializedProperty m_Text;
	SerializedProperty m_FontIndex;
    SerializedProperty m_Size;
    SerializedProperty m_FontSize;
    SerializedProperty m_SpaceSize;
    SerializedProperty m_Align;
    SerializedProperty m_Color;
    SerializedProperty m_Material;
    SerializedProperty m_Fitting;
	SerializedProperty m_Wrapping;
	SerializedProperty m_Filling;
	
    SerializedProperty m_debugReport;
    //static bool foldExpand = false;

    // Use this for initialization
    void OnEnable()
    {
        m_Text = serializedObject.FindProperty("m_Text");
		m_FontIndex = serializedObject.FindProperty("m_FontIndex");
        m_Size = serializedObject.FindProperty("m_Size");
        m_FontSize = serializedObject.FindProperty("m_FontSize");
        m_Align = serializedObject.FindProperty("m_Align");
        m_SpaceSize = serializedObject.FindProperty("m_SpaceSize");
        m_Color = serializedObject.FindProperty("m_Color");
        m_Material = serializedObject.FindProperty("m_Material");
        m_Fitting = serializedObject.FindProperty("m_AutoFit");
        m_Wrapping = serializedObject.FindProperty("m_AutoWrap");
        m_Filling = serializedObject.FindProperty("m_AutoFill");
        m_debugReport = serializedObject.FindProperty("debugReport");		
	    Undo.undoRedoPerformed += Redraw;
    }
	
	void OnDisable()
	{
		Undo.undoRedoPerformed -= Redraw;
	}
	
    // Update is called once per frame
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUI.BeginChangeCheck();
		
		TEXBoxHighlighting.DrawText(m_Text);
        
        if (serializedObject.targetObjects.Length == 1)
        {
            if (m_debugReport.stringValue != System.String.Empty)
                EditorGUILayout.HelpBox(m_debugReport.stringValue, MessageType.Warning);
        }
		
        EditorGUILayout.PropertyField(m_Size);
        {
            EditorGUI.indentLevel++;
            
            EditorGUILayout.PropertyField(m_FontSize);
            TEXSharedEditor.DoFontIndexSelection(m_FontIndex);
            TEXSharedEditor.DoTextAligmentControl(EditorGUILayout.GetControlRect(), m_Align);
            EditorGUILayout.PropertyField(m_SpaceSize);
            EditorGUILayout.PropertyField(m_Color);
            TEXSharedEditor.DoMaterialGUI(m_Material, (ITEXDraw)target);
            EditorGUILayout.PropertyField(m_Fitting);     
            if (!m_Fitting.hasMultipleDifferentValues && m_Fitting.enumValueIndex > 0 && !((TEXDraw3D)target).GetComponent<RectTransform>())
                EditorGUILayout.HelpBox("Fitting is useful when RectTransform is attached in this GameObject", MessageType.Info); 
            EditorGUILayout.PropertyField(m_Wrapping);      
            EditorGUILayout.PropertyField(m_Filling);      
            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();

        if (EditorGUI.EndChangeCheck())
            Redraw();
        
    }
	
    public void Redraw()
    { 
        foreach (TEXDraw3D i in (serializedObject.targetObjects))
        {
            i.Redraw();   
            i.Repaint();
        }
    }
	

    [MenuItem("GameObject/3D Object/TEXDraw 3D", false, 3300)]
    static void CreateCustomGameObject(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("TEXDraw 3D");
        go.AddComponent<TEXDraw3D>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }

}