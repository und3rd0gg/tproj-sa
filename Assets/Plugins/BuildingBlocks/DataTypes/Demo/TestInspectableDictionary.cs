using System;
using System.Collections.Generic;
using UnityEngine;

namespace BuildingBlocks.DataTypes {

    public class TestInspectableDictionary : MonoBehaviour {

        // Serialized Fields
        public InspectableDictionary<string, int> Inspectable = new InspectableDictionary<string, int>();

        [SerializeField]
        private InspectableDictionary<string, MyCustomDataClass> myCustomClassDictionary = new InspectableDictionary<string, MyCustomDataClass>();

        // Private Fields
        private const string searchKey = "TestEntry2";
        private const string dictionaryKey = "TestEntry2";

        // Unity Callbacks
        private void Start() {
            // Show what is in dictionary at start
            foreach (var kvp in Inspectable) {
                Debug.Log($"Original Value: {kvp.Key}, {kvp.Value}");
            }

            // Key access
            Inspectable[dictionaryKey] = 2;

            // Index access
            Inspectable[0] = 3;

            var found = Inspectable.TryGetValue(searchKey, out int value) ? $"was found with value: {value}" : "was not found";

            Debug.Log($"Dictionary TryGetValue with key: {searchKey}, {found}.");
        }



        // Here for example purposes with usage.
        [Serializable]
        public class MyCustomDataClass {
            // Constructors
            public MyCustomDataClass() { }
            public MyCustomDataClass(string store, Color color, IEnumerable<string> list) {
                Store = store;
                Color = color;
                List = new List<string>(list);
            }

            // Properties
            [field: SerializeField]
            public string Store { get; private set; }
            [field: SerializeField, ColorUsage(true, true)]
            public Color Color { get; private set; }
            [field: SerializeField]
            public List<string> List { get; private set; }

            // Deconstructors
            public void Deconstruct(out string name, out Color color) {
                name = Store;
                color = Color;
            }

            public void Deconstruct(out string name, out Color color, out IEnumerable<string> list) {
                name = Store;
                color = Color;
                list = List;
            }            
        }
    }
}
