using System;
using System.Collections.Generic;
using UnityEngine;

namespace pyr.Core.Collections;

[Serializable]
public class SerializedReferenceList<T> : List<T>, ISerializationCallbackReceiver
{
    [SerializeField] public List<Element> _Values = new();

    public void OnBeforeSerialize()
    {
        _Values.Clear();

        for (var i = 0; i < Count; i++)
            _Values.Add(new Element(this[i]));
    }

    public void OnAfterDeserialize()
    {
        Clear();

        _Values.RemoveAll(r => r.Value is null);

        foreach (var element in _Values)
            if (!(element.Value is null))
                Add(element.Value);
    }

    [Serializable]
    public class Element
    {
        [SerializeReference] public T Value;

        public Element(T value)
        {
            Value = value;
        }
    }
}
