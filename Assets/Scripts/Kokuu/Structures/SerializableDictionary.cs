using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;
#endif

namespace Kokuu.Structures
{
    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] internal List<SerializableKeyValuePair<TKey, TValue>> serialized = new();
        
        [SerializeField] internal bool duplicateKeysExist;
        
        public SerializableDictionary() { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : base(collection) { }
        public SerializableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer) : base(collection, comparer) { }
        public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
        public SerializableDictionary(int capacity) : base(capacity) { }
        public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }
        protected SerializableDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }
        
        public void OnBeforeSerialize()
        {
            if (duplicateKeysExist) return;
            
            serialized.Clear();
            serialized.Capacity = Count;
            foreach (KeyValuePair<TKey, TValue> kvp in this)
                serialized.Add(new SerializableKeyValuePair<TKey, TValue>(kvp.Key, kvp.Value));
        }

        public void OnAfterDeserialize()
        {
            duplicateKeysExist = false;
            
            Clear();
            EnsureCapacity(serialized.Capacity);
            foreach (SerializableKeyValuePair<TKey, TValue> kvp in serialized)
                if (!TryAdd(kvp.key, kvp.value)) duplicateKeysExist = true;
        }
    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    internal class SerializableDictionaryDrawer : PropertyDrawer
    {
        private const string SerializedPropertyName = nameof(SerializableDictionary<string, string>.serialized);
        private const string DuplicateKeysExistPropertyName = nameof(SerializableDictionary<string, string>.duplicateKeysExist);
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            float lineHeight = EditorGUIUtility.singleLineHeight;
            float space = EditorGUIUtility.standardVerticalSpacing;
            
            SerializedProperty serializedProperty = property.FindPropertyRelative(SerializedPropertyName);
            bool duplicateKeysExist = property.FindPropertyRelative(DuplicateKeysExistPropertyName).boolValue;
            
            if (property.isExpanded)
            {
                float listHeight = GetList(serializedProperty).GetHeight();
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
                
                GetList(serializedProperty).DoList(listPosition);
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

        private ReorderableList GetList(SerializedProperty property)
        {
            if (ReorderableListUtility.TryGetList(property, out ReorderableList list)) return list;
            
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
#endif
}