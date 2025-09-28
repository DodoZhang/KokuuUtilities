using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;

namespace Kokuu.Editor
{
    public static partial class EditorGUIExtension
    {
        private const int MaxReorderableListBufferSize = 64;

        private struct ReorderableListBuffer
        {
            public SerializedObject serializedObject;
            public string propertyPath;
            public ReorderableList list;
        }

        private static readonly LinkedList<ReorderableListBuffer> buffers = new();

        public static ReorderableList GetReorderableList(SerializedProperty property)
        {
            TryGetReorderableList(property, out ReorderableList list);
            return list;
        }

        public static bool TryGetReorderableList(SerializedProperty property, out ReorderableList list)
        {
            if (property is null) throw new ArgumentNullException();
            
            SerializedObject serializedObject = property.serializedObject;
            string propertyPath = property.propertyPath;

            for (LinkedListNode<ReorderableListBuffer> node = buffers.First; node != null; node = node.Next)
            {
                if (node.Value.serializedObject == serializedObject && node.Value.propertyPath == propertyPath)
                {
                    buffers.Remove(node);
                    buffers.AddFirst(node);
                    list = node.Value.list;
                    return true;
                }
            }

            ReorderableListBuffer buffer = new()
            {
                serializedObject = serializedObject,
                propertyPath = propertyPath,
                list = new ReorderableList(property.serializedObject, property)
            };
            buffers.AddFirst(buffer);
            
            if (buffers.Count > MaxReorderableListBufferSize) buffers.RemoveLast();
            
            list = buffer.list;
            return false;
        }
    }
}