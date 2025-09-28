using UnityEditor;
using UnityEngine;

namespace Kokuu.Maths
{
    [CustomPropertyDrawer(typeof(Fractional))]
    public class FractionalDrawer : PropertyDrawer
    {
        private const string NumeratorPropertyName = "_numerator";
        private const string DenominatorPropertyName = "_denominator";
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            label = EditorGUI.BeginProperty(position, label, property);
                
            SerializedProperty numeratorProperty = property.FindPropertyRelative(NumeratorPropertyName);
            SerializedProperty denominatorProperty = property.FindPropertyRelative(DenominatorPropertyName);
            string str = new Fractional(numeratorProperty.intValue, denominatorProperty.intValue).ToString();
                
            EditorGUI.BeginChangeCheck();
            str = EditorGUI.TextField(position, label, str);
            if (EditorGUI.EndChangeCheck())
            {
                if (Fractional.TryParse(str, out Fractional frac))
                {
                    numeratorProperty.intValue = frac.numerator;
                    denominatorProperty.intValue = frac.denominator;
                }
            }
                
            EditorGUI.EndProperty();
        }
    }
}