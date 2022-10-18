using System;
using UnityEngine;

namespace BuildingBlocks.DataTypes {

    // Unity serialization requires a set method. Private or not. Init is a a C# 9.0 feature coming.

    [Serializable]
    public class SerializedDataObj<T1, T2> {
        // Constructor
        public SerializedDataObj(T1 item1, T2 item2) => (Item1, Item2) = (item1, item2);
        // Serialized Public Properties
        [field: SerializeField] public T1 Item1 { get; private set; }
        [field: SerializeField] public T2 Item2 { get; private set; }
        // Deconstructor
        public void Destructor(out T1 item1, out T2 item2) => (item1, item2) = (Item1, Item2); 
    }

    [Serializable]
    public class SerializedDataObj<T1, T2, T3> {
        // Constructor
        public SerializedDataObj(T1 item1, T2 item2, T3 item3) => (Item1, Item2, Item3) = (item1, item2, item3);
        // Serialized Public Properties
        [field: SerializeField] public T1 Item1 { get; private set; }
        [field: SerializeField] public T2 Item2 { get; private set; }
        [field: SerializeField] public T3 Item3 { get; private set; }
        // Deconstructor
        public void Destructor(out T1 item1, out T2 item2, out T3 item3) => (item1, item2, item3) = (Item1, Item2, Item3);
    }

    [Serializable]
    public class SerializedDataObj<T1, T2, T3, T4> {
        // Constructor
        public SerializedDataObj(T1 item1, T2 item2, T3 item3, T4 item4) => (Item1, Item2, Item3, Item4) = (item1, item2, item3, item4);
        // Serialized Public Properties
        [field: SerializeField] public T1 Item1 { get; private set; }
        [field: SerializeField] public T2 Item2 { get; private set; }
        [field: SerializeField] public T3 Item3 { get; private set; }
        [field: SerializeField] public T4 Item4 { get; private set; }
        // Deconstructor
        public void Destructor(out T1 item1, out T2 item2, out T3 item3, out T4 item4) => (item1, item2, item3, item4) = (Item1, Item2, Item3, Item4);
    }
}
