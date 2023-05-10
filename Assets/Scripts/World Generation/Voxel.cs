using UnityEngine;

public class Voxel {
    public ScriptableBlock block;
    public Vector3 chunkOffset;
    public GameObject voxelObject;
    public int x;
    public int y;
    public int z;

    public Voxel(ScriptableBlock blockType) {
        block = blockType;
    }

    public ScriptableBlock GetScriptableBlock() {
        return block;
    }
}