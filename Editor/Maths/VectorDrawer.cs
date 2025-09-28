using UnityEditor;
using UnityEngine;

namespace Kokuu.Maths
{
    [CustomPropertyDrawer(typeof(Vector))]
    [CustomPropertyDrawer(typeof(VectorC))]
    [CustomPropertyDrawer(typeof(VectorF))]
    public class VectorDrawer : PropertyDrawer
    {
        private const string ValuesPropertyName = "_val";
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative(ValuesPropertyName), label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.PropertyField(position, property.FindPropertyRelative(ValuesPropertyName), label);
        }
    }
}