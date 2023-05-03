using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public List<Voxel> voxels = new List<Voxel>();
    public Vector3 offset;

    public Chunk(Vector3 getOffset) {
        offset = getOffset;
    }

    public Vector3 GetOffset(){
        return offset;
    }

    public Voxel GetVoxel(int x, int y, int z){
        foreach (Voxel voxel in voxels)
        {
            if(voxel.x == x && voxel.y == y && voxel.z == z) {
                return voxel;
            }
        }
        return null;
    }
}