using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public List<ScriptableBlock> Blocks;
    
    [System.NonSerialized]
    public int currentItem = 0;

    public void switchItem(int i) {
        currentItem += i;
        if (currentItem < 0) {
            currentItem = Blocks.Count - 1; 
        }
        if (currentItem >= Blocks.Count) {
            currentItem = 0;
        }
        Debug.Log(currentItem);
    }
}
