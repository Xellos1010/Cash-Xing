#if UNITY_EDITOR
using UnityEditor;
internal class BoomSportsEditor : Editor
{
    //public bool enableDefaultInspector = false;
    public override void OnInspectorGUI()
    {
        BoomEditorUtilities.DrawUILine(UnityEngine.Color.white);
        //enableDefaultInspector = EditorGUILayout.Toggle("Toggle to view Inspector raw", enableDefaultInspector);
        //if (enableDefaultInspector)
        base.OnInspectorGUI();
    }
}
#endif