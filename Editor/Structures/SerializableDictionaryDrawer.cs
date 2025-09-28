using Kokuu.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Kokuu.Structures
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    internal class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const string SerializedPropertyName = "serialized";
        private const string DuplicateKeysExistPropertyName = "duplicateKeysExist";
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float space = EditorGUIUtility.standardVerticalSpacing;
            
            SerializedProperty serializedProperty = property.FindPropertyRelative(SerializedPropertyName);
            bool duplicateKeysExist = property.FindPropertyRelative(DuplicateKeysExistPropertyName).boolValue;
            
            if (property.isExpanded)
            {
                float listHeight = GetReorderableList(serializedProperty).GetHeight();
                if (duplicateKeysExist) return (lineHeight + space) * 3 + listHeight;
                return lineHeight + space + listHeight;
            }
            
            return lineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float labelWidth = EditorGUIUtility.labelWidth;
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float space = EditorGUIUtility.standardVerticalSpacing;
            
            SerializedProperty serializedProperty = property.FindPropertyRelative(SerializedPropertyName);
            bool duplicateKeysExist = property.FindPropertyRelative(DuplicateKeysExistPropertyName).boolValue;

            Rect headerPosition = position;
            headerPosition.height = lineHeight;
            label = EditorGUI.BeginProperty(headerPosition, label, serializedProperty);
            property.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(headerPosition, property.isExpanded, label);
            EditorGUI.EndProperty();

            if (property.isExpanded)
            {
                Rect listPosition = position;
                listPosition.y += lineHeight + space;
                listPosition.height -= lineHeight + space;
                
                if (duplicateKeysExist)
                {
                    Rect warningPosition = position;
                    warningPosition.y += lineHeight + space;
                    warningPosition.height = lineHeight * 2 + space;
                    EditorGUI.HelpBox(warningPosition, "Serialization Paused: Duplicate Keys Exist", MessageType.Warning);
                    
                    listPosition.y += (lineHeight + space) * 2;
                    listPosition.height -= (lineHeight + space) * 2;
                }
                
                GetReorderableList(serializedProperty).DoList(listPosition);
            }
            else
            {
                if (property.FindPropertyRelative(DuplicateKeysExistPropertyName).boolValue)
                {
                    Rect warningPosition = position;
                    warningPosition.x += labelWidth;
                    warningPosition.width -= labelWidth;
                    EditorGUI.HelpBox(warningPosition, "Duplicate Keys Exist", MessageType.Warning);
                }
            }
            
            EditorGUI.EndFoldoutHeaderGroup();
        }

        private ReorderableList GetReorderableList(SerializedProperty property)
        {
            if (EditorGUIExtension.TryGetReorderableList(property, out ReorderableList list)) return list;
            
            float labelWidth = EditorGUIUtility.labelWidth;
            float space = EditorGUIUtility.standardVerticalSpacing;
            
            list.multiSelect = true;
            list.drawHeaderCallback = headerPosition =>
            {
                Rect keyHeaderPosition = headerPosition;
                keyHeaderPosition.width = labelWidth - space;

                Rect valueHeaderPosition = headerPosition;
                valueHeaderPosition.x += labelWidth;
                valueHeaderPosition.width -= labelWidth;

                EditorGUI.indentLevel++;
                EditorGUI.LabelField(keyHeaderPosition, "Key");
                EditorGUI.LabelField(valueHeaderPosition, "Value");
                EditorGUI.indentLevel--;
            };
            list.drawElementBackgroundCallback = (rect, index, active, focused) =>
            {
                if (active) EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
                if (focused) EditorGUI.DrawRect(rect, new Color(0.17f, 0.36f, 0.53f));
            };
            list.elementHeightCallback = index =>
                EditorGUI.GetPropertyHeight(property.GetArrayElementAtIndex(index));
            list.drawElementCallback = (rect, index, active, focused) =>
                EditorGUI.PropertyField(rect, property.GetArrayElementAtIndex(index), GUIContent.none);
            
            return list;
        }
    }
}