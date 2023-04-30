using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WorldGeneration : MonoBehaviour
{
    /*
    Zkusit getNeighbour přes max a min Y, všechny XZ
    perlin noise  scale + offset + random
    */
    [SerializeField]
    private ScriptableBlock groundBlock;
    [SerializeField]
    private ScriptableBlock topLevelBlock;
    [SerializeField]
    private ScriptableBlock bottomLevelBlock;

    [InfoBox(@"@""Number of blocks in chunk: "" + BlocksInChunk(this.chunkSize, this.chunkHeight)")]
    public int chunkSize;
    public int chunkHeight;

    [InfoBox(@"@""Number of chunks rendered: "" + TotalChunksToRender(this.chunksToRenderAround)")]
    public int chunksToRenderAround;

    [InfoBox("If empty or 0 seed is random")]
    public int seed;

    private List<Chunk> chunks = new List<Chunk>();
    private Chunk chunk;
    private Vector3 chunkOffset;
    private GameObject chunkObject;
    private GameObject voxelObject;
    private int chunkIndex;
        
    private void Start() {
        for (int chunkXOffset = -chunksToRenderAround; chunkXOffset <= chunksToRenderAround; chunkXOffset++) {
            for (int chunkZOffset = -chunksToRenderAround; chunkZOffset <= chunksToRenderAround; chunkZOffset++) {
                createChunk(chunkXOffset, chunkZOffset);
                renderChunk(chunks[chunkIndex]);
                chunkIndex++;
            }
        }
    }

    //creates a chunk, containing voxels
    private void createChunk(int xOffset, int zOffset) {
        chunkOffset = new Vector3(xOffset, 0, zOffset);
        Chunk chunk = new Chunk(chunkOffset);
        Voxel voxel;
        //setting position for voxels
        for (int x=-chunkSize; x < chunkSize; x++) {
            for (int y=-chunkHeight; y < chunkHeight; y++) {
                for (int z=-chunkSize; z < chunkSize; z++) {
                    //make it perlin
                    voxel = new Voxel(groundBlock);
                    voxel.x = x;
                    voxel.y = y;
                    voxel.z = z;
                    chunk.voxels.Add(voxel);
                }
            }
        }
        chunks.Add(chunk);
    }

    //renders chunk 
    private void renderChunk(Chunk chunk) {
        Vector3 chunkOffset = chunk.GetOffset(); 
        Vector3 offsetPosition = chunkOffset*chunkSize;
        BoxCollider chunkCollider;
        string chunkName = String.Format("Chunk [{0}, {1}, {2}]", chunkOffset.x.ToString(), chunkOffset.y.ToString(), chunkOffset.z.ToString());
        
        chunkObject = new GameObject(chunkName);
        chunkObject.transform.parent = gameObject.transform;
        chunkCollider = chunkObject.AddComponent<BoxCollider>();
        chunkCollider.isTrigger = true;
        chunkCollider.size = new Vector3(chunkSize, chunkHeight, chunkSize);

        for (int x=-chunkSize; x < chunkSize; x++) {
            for (int y=-chunkHeight; y < chunkHeight; y++) {
                for (int z=-chunkSize; z < chunkSize; z++) {
                    Voxel voxel = chunk.GetVoxel(x, y, z);
                    if (voxel != null) {
                        string voxelName = String.Format("{0}, {1}, {2}, {3}",voxel.GetBlock().ToString(), x.ToString(), y.ToString(), z.ToString());
                        voxelObject = new GameObject(voxelName);
                        voxelObject.transform.parent = chunkObject.transform;
                        voxelObject.transform.position = new Vector3(x, y, z);
                        //make it guad
                    }
                }
            }
        }
        chunkObject.transform.position = offsetPosition;
    }

    public List<Voxel> GetAllVoxelNeighbours(Voxel voxel, Chunk chunk){
        List<Voxel> voxels = new List<Voxel>();   
        voxels.Add(GetTopNeighbour(voxel, chunk));
        voxels.Add(GetBackNeighbour(voxel, chunk));
        voxels.Add(GetRightNeighbour(voxel, chunk));
        voxels.Add(GetBottomNeighbour(voxel, chunk));
        voxels.Add(GetBackNeighbour(voxel, chunk));
        voxels.Add(GetLeftNeighbour(voxel, chunk));
        return voxels;
    }

    //Top NB
    public Voxel GetTopNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.y == chunkSize) {
            Vector3 offset = chunk.GetOffset();
            offset.y += 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(voxel.x, 0, voxel.z);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y + 1, voxel.z);
            return neighbourVoxel;
        }
    }
    //Front NB
    public Voxel GetFrontNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.x == chunkSize) {
            Vector3 offset = chunk.GetOffset();
            offset.x += 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(0, voxel.y, voxel.z);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x + 1, voxel.y, voxel.z);
            return neighbourVoxel;
        }
    }
    //Right NB
    public Voxel GetRightNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.z == chunkSize) {
            Vector3 offset = chunk.GetOffset();
            offset.z += 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y, 0);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y, voxel.z+ 1);
            return neighbourVoxel;
        }
    }
    //Bottom NB
    public Voxel GetBottomNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.y == 0) {
            Vector3 offset = chunk.GetOffset();
            offset.y -= 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(voxel.x, chunkSize, voxel.z);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y - 1, voxel.z);
            return neighbourVoxel;
        }
    }
    //Back NB
    public Voxel GetBackNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.x == 0) {
            Vector3 offset = chunk.GetOffset();
            offset.x -= 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(chunkSize, voxel.y, voxel.z);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x - 1, voxel.y, voxel.z);
            return neighbourVoxel;
        }
    }
    //Left NB
    public Voxel GetLeftNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.z == 0) {
            Vector3 offset = chunk.GetOffset();
            offset.z -= 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y, chunkSize);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y, voxel.z - 1);
            return neighbourVoxel;
        }
    }
    
    public Chunk GetChunkByOffset(Vector3 offset) {
        foreach(Chunk chunk in chunks) {
            if (chunk.GetOffset() == offset) {
                return chunk;
            }
        }
        return null;
    }
    
    public static float BlocksInChunk(int width, int height) {
        float total = (width*width)*height;
        return total;
    }

    public static int TotalChunksToRender(int chunksAround) {
        int total;
        total = ((chunksAround*2)+1)*((chunksAround*2)+1);
        return total;
    }
}
