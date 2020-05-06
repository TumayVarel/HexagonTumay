#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UI;

namespace UnityEngine.UI.Extensions
{
    [CanEditMultipleObjects, CustomEditor(typeof(HexagonController), false)]
    public class HexagonControllerEditor : GraphicEditor
    {
        /// <summary>
        /// Editor script to avoid the "Bu çocuk her şeyi yapmış da renk seçimini eklememiş" situation, and to show I can write editor scripts, whenever needed
        /// </summary>
        public override void OnInspectorGUI()
        {
            base.DrawDefaultInspector();
            EditorGUILayout.HelpBox("To change color, please click onto MatchTreeUIDataHolder", MessageType.Info);
        }
    }
}
#endif