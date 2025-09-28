using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Kokuu.Structures
{
    [Serializable]
    public struct SerializableKeyValuePair<TKey, TValue>
    {
        public TKey key;
        public TValue value;

        public SerializableKeyValuePair(TKey key, TValue value)
        {
            this.key = key;
            this.value = value;
        }
    }
    
#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializableKeyValuePair<,>))]
    internal class SerializableKeyValuePairDrawer : PropertyDrawer
    {
        private const string KeyPropertyName = nameof(SerializableKeyValuePair<string, string>.key);
        private const string ValuePropertyName = nameof(SerializableKeyValuePair<string, string>.value);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty keyProperty = property.FindPropertyRelative(KeyPropertyName);
            SerializedProperty valueProperty = property.FindPropertyRelative(ValuePropertyName);
            return Mathf.Max(EditorGUI.GetPropertyHeight(keyProperty), EditorGUI.GetPropertyHeight(valueProperty));
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty keyProperty = property.FindPropertyRelative(KeyPropertyName);
            SerializedProperty valueProperty = property.FindPropertyRelative(ValuePropertyName);

            float labelWidth = EditorGUIUtility.labelWidth;
            float space = EditorGUIUtility.standardVerticalSpacing;
            
            label = EditorGUI.BeginProperty(position, label, property);
            
            position = EditorGUI.PrefixLabel(position, label);
            
            Rect keyPosition = position;
            keyPosition.width = labelWidth - space;
            keyPosition.height = EditorGUI.GetPropertyHeight(keyProperty);
            
            Rect valuePosition = position;
            valuePosition.x += labelWidth;
            valuePosition.width -= labelWidth;
            valuePosition.height = EditorGUI.GetPropertyHeight(valueProperty);
            
            EditorGUI.PropertyField(keyPosition, keyProperty, GUIContent.none);
            EditorGUI.PropertyField(valuePosition, valueProperty, GUIContent.none);
            
            EditorGUI.EndProperty();
        }
    }
#endif
}