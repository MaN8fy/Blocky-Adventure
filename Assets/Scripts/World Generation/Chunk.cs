using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Each chunk have information about each voxel it has
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
        for (int index = 0; index<voxels.Count; index++)
        {
            if(voxels[index].x == x && voxels[index].y == y && voxels[index].z == z) {
                return voxels[index];
            }
        }
        return null;
    }
}