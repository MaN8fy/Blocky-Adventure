using UnityEngine;

//creating blocks as scriptable objects
[CreateAssetMenu(fileName = "New Block", menuName = "Create New Block/New Block", order = 0)]
public class ScriptableBlock : ScriptableObject {
    public string blockName;
    public int destroyDifficulty;
    public int stack;
    public Material material;
    
}