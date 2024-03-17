using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SerializedItemCollectionDict : MonoBehaviour
{
    [Serializable]
    public class ItemCollection
    {
        public List<GameObject> Collections;
    }

    [SerializedDictionary("Case", "ItemCollection")]
    public SerializedDictionary<int, ItemCollection> ItemCollectionDict;

    public bool TryGetValue(int key, out ItemCollection item)
    {
        return ItemCollectionDict.TryGetValue(key, out item);
    }
}
