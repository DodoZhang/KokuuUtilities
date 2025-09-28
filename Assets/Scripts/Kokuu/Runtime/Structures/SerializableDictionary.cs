using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

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
}