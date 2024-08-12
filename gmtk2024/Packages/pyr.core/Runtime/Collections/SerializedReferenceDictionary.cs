using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace pyr.Core.Collections;

[Serializable]
public class SerializedReferenceDictionary<TKey, TValue>
    : Dictionary<TKey, TValue>,
        ISerializationCallbackReceiver
{
    [SerializeField] public List<KVP<TKey, TValue>> _Kvp = new();

    public void OnBeforeSerialize()
    {
        foreach (var kvp in this)
            if (
                _Kvp.FirstOrDefault(r => Comparer.Equals(r.Key, kvp.Key))
                is { } serializedKvp
            )
                serializedKvp.Value = kvp.Value;
            else
                _Kvp.Add(kvp);

        _Kvp.RemoveAll(r => !ContainsKey(r.Key));

        for (var i = 0; i < _Kvp.Count; i++)
            _Kvp[i].index = i;
    }

    public void OnAfterDeserialize()
    {
        Clear();

        _Kvp.RemoveAll(r => r.Key is null);

        foreach (var serializedKvp in _Kvp.Where(serializedKvp =>
                     !(serializedKvp.isKeyRepeated = ContainsKey(serializedKvp.Key))))
            Add(serializedKvp.Key, serializedKvp.Value);
    }

    [Serializable]
    public class KVP<K, V>
    {
        [SerializeField] public K Key;

        [SerializeReference] public V Value;

        public int index;
        public bool isKeyRepeated;

        public KVP(K key, V value)
        {
            Key = key;
            Value = value;
        }

        public static implicit operator KVP<K, V>(KeyValuePair<K, V> kvp)
        {
            return new KVP<K, V>(kvp.Key, kvp.Value);
        }

        public static implicit operator KeyValuePair<K, V>(KVP<K, V> kvp)
        {
            return new KeyValuePair<K, V>(kvp.Key, kvp.Value);
        }
    }
}
