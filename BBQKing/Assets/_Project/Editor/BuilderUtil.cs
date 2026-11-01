using UnityEditor;
using UnityEngine;

namespace BBQKing.EditorTools
{
    /// <summary>
    /// Small helpers shared by the scene builders: loading sprites and filling in Inspector fields
    /// from code.
    /// </summary>
    public static class BuilderUtil
    {
        public static Sprite LoadSprite(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) Debug.LogError("BBQ King: missing sprite " + path);
            return s;
        }

        public static void SetString(Object target, string field, string value)
        {
            var so = new SerializedObject(target);
            so.FindProperty(field).stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        // Sets a private [SerializeField] field, exactly as dragging something into the Inspector would.
        public static void SetRef(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogError($"BBQ King: no field '{field}' on {target.GetType().Name}"); return; }
            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        /// <summary>Fills an array field, like dragging several things into an Inspector list.</summary>
        public static void SetRefs(Object target, string field, Object[] values)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null) { Debug.LogError($"BBQ King: no field '{field}' on {target.GetType().Name}"); return; }
            prop.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++) prop.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
