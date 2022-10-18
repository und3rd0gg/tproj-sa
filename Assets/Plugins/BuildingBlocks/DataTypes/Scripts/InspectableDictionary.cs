using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BuildingBlocks.DataTypes {

    /// <summary>
    /// Same usage as generic dictionary. But will create serialized fields in the inspector for this dictionary.
    /// <br> Will synchronize dictionary and inspector during play mode for inspection. No manual adding in editor during play mode.</br>
    /// </summary>
    [Serializable]
    public class InspectableDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IEnumerable {

        // Implicit Operators
        public static implicit operator InspectableDictionary<TKey, TValue>(Dictionary<TKey, TValue> dictionary) {
            return new InspectableDictionary<TKey, TValue>(dictionary);
        }
        public static implicit operator Dictionary<TKey, TValue>(InspectableDictionary<TKey, TValue> inspectable) {
            return new Dictionary<TKey, TValue>(inspectable);
        }

        // Constructors
        public InspectableDictionary() { }
        public InspectableDictionary(Dictionary<TKey, TValue> dictionary) {
            foreach (var pair in dictionary) {
                lookup[pair.Key] = pair.Value;
            }
        }

        // Data Class
        /// <summary>
        /// Allows control over access and serialization. Is class vs KeyValuePair struct.
        /// </summary>
        /// <typeparam name="T2Key">Dictionary Generic Key Type</typeparam>
        /// <typeparam name="T2Value">Dictionary Generic Value Type</typeparam>
        [Serializable]
        private class KVP<T2Key, T2Value> {
            // Constructor
            public KVP(T2Key key, T2Value value) {
                Key = key;
                Value = value;
            }
            // Properties
            [field: SerializeField] public T2Key Key { get; set; }
            [field: SerializeField] public T2Value Value { get; set; }
            // Deconstructor
            public void Deconstruct(out T2Key key, out T2Value value) { key = this.Key; value = this.Value; }
        }

        //---
        // Inspectable List

        [SerializeField, Tooltip("Items can be added in the inspector, only prior to play mode not at runtime, and will be exposed through a dictionary.")]
        private List<KVP<TKey, TValue>> items = new List<KVP<TKey, TValue>>();

        //---
        // Internal Use

        private bool inspectableSync;
        private Dictionary<TKey, TValue> lookup = new Dictionary<TKey, TValue>();

        public TValue this[TKey key] {
            get {
                InspectorToDictionary();
                return lookup[key];
            }
            set {
                InspectorToDictionary();
                lookup[key] = value;
                DictionaryToInspectorAdditive(key, value);
            }
        }

        public TValue this[int index] {
            get {
                InspectorToDictionary();
                return items[index].Value;
            }
            set {
                InspectorToDictionary();
                var c = items[index];
                lookup[c.Key] = value;
                DictionaryToInspectorAdditive(c.Key, value);
            }
        }

        //---
        // Synchronization Methods

        /// <summary>Copies in full the dictionary as lists to the inspector lists.</summary>
        private void DictionaryToInspector() {
            items.Clear();
            var length = lookup.Count;
            var keys = lookup.Keys.ToList();
            for (int i = length - 1; i >= 0; i--) {
                var key = keys[i];
                var value = lookup[key];
                items.Add(new KVP<TKey, TValue>(key, value));
            }
        }

        /// <summary>Adds a Key-Value pair to the inspector list.</summary>
        private void DictionaryToInspectorAdditive(TKey key, TValue value) {
            var existing = items.FirstOrDefault((k) => k.Key.Equals(key));
            if (existing != null) {
                existing.Value = value; 
            }
            else {
                items.Add(new KVP<TKey, TValue>(key, value));
            }
        }

        /// <summary>Removes a Key-Value pair to the inspector list.</summary>
        private void DictionaryToInspectorNegative(TKey key) {
            var count = items.Count;
            for (int i = count - 1; i >= 0; i--) {
                if (items[i].Key.Equals(key)) {
                     items.RemoveAt(i);
                }
            }
        }

        /// <summary>Clears dictionary, rebuilds from serialized inspector list. Removes duplicates by copying back to list.</summary>
        /// <remarks>Only happens the first time dictionary is accessed.</remarks>
        private void InspectorToDictionary() {
            if (inspectableSync is false) {
                lookup.Clear();
                for (int i = items.Count - 1; i >= 0; i--) {
                    var item = items[i].Key;
                    var value = items[i].Value;
                    if (item != null && value != null) {
                        lookup[item] = value;
                    }
                    else {
                        items.RemoveAt(i); items.RemoveAt(i);
                    }
                }
                DictionaryToInspector();
                inspectableSync = true;
            }
        }

        //---
        // IDictionary, ICollection, IEnumerable

        public bool IsFixedSize => false;

        public bool IsReadOnly => false;

        public ICollection<TKey> Keys {
            get {
                InspectorToDictionary();
                return lookup.Keys;
            }
        }

        public ICollection<TValue> Values {
            get {
                InspectorToDictionary();
                return lookup.Values;
            }
        }

        public int Count => lookup.Count;

        public void Add(TKey key, TValue value) {
            InspectorToDictionary();
            lookup.Add(key, value);
            DictionaryToInspectorAdditive(key, value);
        }

        public void Clear() {
            lookup.Clear();
            items.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) {
            InspectorToDictionary();
            return lookup.Contains(item);
        }

        public bool ContainsKey(TKey key) {
            InspectorToDictionary();
            return lookup.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value) {
            InspectorToDictionary();
            var exists = lookup.TryGetValue(key, out TValue backer);
            value = backer;
            return exists;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
            InspectorToDictionary();
            return lookup.GetEnumerator();
        }

        public bool Remove(TKey key) {
            var success = lookup.Remove(key);
            DictionaryToInspectorNegative(key);
            return success;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            InspectorToDictionary();
            return lookup.GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item) {
            lookup.Add(item.Key, item.Value);
            DictionaryToInspectorAdditive(item.Key, item.Value);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) {
            InspectorToDictionary();
            foreach (var kvp in lookup) {
                array[arrayIndex++] = kvp;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item) {
            var success = lookup.Remove(item.Key);
            DictionaryToInspectorNegative(item.Key);
            return success;
        }
    }
}
