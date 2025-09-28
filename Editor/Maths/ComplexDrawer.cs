using UnityEditor;
using UnityEngine;

namespace Kokuu.Maths
{
    [CustomPropertyDrawer(typeof(Complex))]
    public class ComplexDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
                
            SerializedProperty realProperty = property.FindPropertyRelative(nameof(Complex.real));
            SerializedProperty imagProperty = property.FindPropertyRelative(nameof(Complex.imag));
            string str = new Complex(realProperty.floatValue, imagProperty.floatValue).ToString();
                
            EditorGUI.BeginChangeCheck();
            str = EditorGUI.TextField(position, label, str);
            if (EditorGUI.EndChangeCheck())
            {
                if (Complex.TryParse(str, out Complex c))
                {
                    realProperty.floatValue = c.real;
                    imagProperty.floatValue = c.imag;
                }
            }
                
            EditorGUI.EndProperty();
        }
    }
}