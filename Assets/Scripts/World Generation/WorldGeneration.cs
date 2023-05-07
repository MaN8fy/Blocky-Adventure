using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WorldGeneration : MonoBehaviour
{
    /*Block to be spawned*/
    public ScriptableBlock groundBlock;
    public ScriptableBlock topLevelBlock;
    public ScriptableBlock bottomLevelBlock;

    [InfoBox(@"@""Number of blocks in chunk: "" + BlocksInChunk(this.chunkLengthWidth, this.chunkDepth)")]
    [Tooltip("This value always adjusted to an even number")]
    [MinValue(4)]
    public int chunkLengthWidth;

    [Tooltip("This value always adjusted to an even number")]
    [MinValue(3)]
    public int chunkDepth;

    [InfoBox(@"@""Number of chunks rendered: "" + TotalChunksToRender(this.chunksToRenderAround)")]
    [MinValue(1)]
    public int chunksToRenderAround;

    [Tooltip("If value is \"0\", seed is random")]
    [MinValue(0)]
    public int seed;

    /*used for random generation*/
    [MinValue(1)]
    public float scale;

    private float calculatedRandom;

    /*All chunks are stored in this list and each chunk stores information about its voxels*/
    private List<Chunk> chunks = new List<Chunk>();
    private List<Chunk> renderedChunks = new List<Chunk>();

    /* these values are based on "Depth" and "LengthWidth" */
    private int chunkOneSideLength, chunkHeight;

    /*
    Active chunk is chunk where player currently is, last chunk is chunk which player left.
    They are used to determine which chunks have to be destroyed
    */
    private Vector3 activeChunk = new Vector3(0f, 0f, 0f);


    private void Awake() {
        if (seed == 0) {
            seed = UnityEngine.Random.Range(0, 999999);
        }

        //these two values are better to store as even number for further calculations
        chunkLengthWidth = checkAndEven(chunkLengthWidth);
        chunkDepth = checkAndEven(chunkDepth);

        //this is length from middle to border of chunk
        chunkOneSideLength = chunkLengthWidth/2;
        chunkHeight = chunkDepth/2;

        calculatedRandom = (float)chunkLengthWidth * scale + seed;

        //renders first bunch of chunks
        for (int x = -chunksToRenderAround; x <= chunksToRenderAround; x++) {
            for (int z = -chunksToRenderAround; z <= chunksToRenderAround; z++) {
                CreateChunk(x, z);
                StartCoroutine(RenderChunk(new Vector3(x, 0, z)));
            }
        }

    }

    //this function creates a chunk and then stores it in a list
    private void CreateChunk(int xOffset, int zOffset) {
        Vector3 chunkOffset = new Vector3(xOffset, 0, zOffset);
        Chunk chunk = new Chunk(chunkOffset);
        Voxel voxel;
                
        //setting position for voxels
        for (int x=-chunkOneSideLength; x < chunkOneSideLength; x++) {
            for (int z=-chunkOneSideLength; z < chunkOneSideLength; z++) {
                int maxHeight = calculateVoxelMaxHeight(x, z, chunkOffset);
                for (int y=-chunkHeight; y < maxHeight; y++) {
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

    //chunk rendering from list chunks based on offset
    IEnumerator RenderChunk(Vector3 chunkOffset) {
        Chunk chunk = GetChunkByOffset(chunkOffset);
        Vector3 offsetPosition = chunkOffset*chunkLengthWidth;     
        GameObject chunkObject;
        GameObject spawnedVoxel;
        BoxCollider chunkCollider;
        MeshRenderer chunkRenderer;
        string chunkName = String.Format("Chunk [{0}, {1}, {2}]", chunkOffset.x.ToString(), chunkOffset.y.ToString(), chunkOffset.z.ToString());

        chunkObject = new GameObject(chunkName);
        chunkObject.transform.parent = gameObject.transform;
        chunkRenderer = chunkObject.AddComponent<MeshRenderer>();
        chunkCollider = chunkObject.AddComponent<BoxCollider>();
        chunkCollider.isTrigger = true;
        chunkCollider.size = new Vector3(chunkLengthWidth-0.5f, chunkDepth+10f, chunkLengthWidth-0.5f);
        
        for (int index = 0; index<chunk.voxels.Count; index++)
        {
            int x = chunk.voxels[index].x;
            int y = chunk.voxels[index].y;
            int z = chunk.voxels[index].z;
            string voxelName = String.Format("{0}, {1}, {2}, {3}", chunk.voxels[index].GetBlock().ToString(), x.ToString(), y.ToString(), z.ToString());
            spawnedVoxel = new GameObject(voxelName, typeof(MeshFilter), typeof(MeshRenderer));
            spawnedVoxel.transform.parent = chunkObject.transform;
            spawnedVoxel.transform.position = new Vector3(x+0.5f, y+0.5f, z+0.5f);
            //make it guad !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        }
        chunkObject.transform.position = offsetPosition;
        
        renderedChunks.Add(chunk);

        yield return new WaitForSeconds(0.2f);
    }
    
    //is called when player enters new chunk
    public void updateWorldChunkEnter(Vector3 chunkPosition) {
        activeChunk = chunkPosition/chunkLengthWidth;
        Vector3 chunkOffset = new Vector3(0f, 0f, 0f);

        int minX = (int)activeChunk.x-chunksToRenderAround;
        int maxX = (int)activeChunk.x+chunksToRenderAround;

        int minZ = (int)activeChunk.z-chunksToRenderAround;
        int maxZ = (int)activeChunk.z+chunksToRenderAround;

        for (int x = minX; x <= maxX; x++) {
            for (int z = minZ; z <= maxZ; z++) {
                chunkOffset.x = x;
                chunkOffset.z = z;
                if (GetChunkByOffset(chunkOffset) == null) {
                    CreateChunk(x, z);
                }
                if (GetRenderedChunkByOffset(chunkOffset) == null) {
                    StartCoroutine(RenderChunk(chunkOffset));
                }
            }
        }
    }

    //is called when player exit new chunk
    public void updateWorldChunkExit() {
        Vector3 chunkOffset = new Vector3(0f, 0f, 0f);
        List<Chunk> toRemove = new List<Chunk>();
        int minX = (int)activeChunk.x-chunksToRenderAround;
        int maxX = (int)activeChunk.x+chunksToRenderAround;

        int minZ = (int)activeChunk.z-chunksToRenderAround;
        int maxZ = (int)activeChunk.z+chunksToRenderAround;

        for(int i = 0; i < renderedChunks.Count; i++) {
            if(renderedChunks[i].offset.x < minX || renderedChunks[i].offset.x > maxX) {
                DestroyChunk(renderedChunks[i].offset);
                toRemove.Add(renderedChunks[i]);                
            } else if(renderedChunks[i].offset.z < minZ || renderedChunks[i].offset.z > maxZ) {
                DestroyChunk(renderedChunks[i].offset);
                toRemove.Add(renderedChunks[i]);
            } else {
            
            }
        }
        
        foreach(Chunk chunk in toRemove) {
            renderedChunks.Remove(chunk);
        }
        toRemove.Clear();
    }

    public void DestroyChunk(Vector3 chunkToDeleteOffset) {
        GameObject chunkToDelete;
        string chunkName = String.Format("Chunk [{0}, {1}, {2}]", chunkToDeleteOffset.x.ToString(), chunkToDeleteOffset.y.ToString(), chunkToDeleteOffset.z.ToString());
        chunkToDelete = GameObject.Find(chunkName);
        Destroy(chunkToDelete);
    }

    public int calculateVoxelMaxHeight(int x, int z, Vector3 offset) {
        float xCoord = (float)x / calculatedRandom + offset.x;
        float zCoord = (float)z / calculatedRandom + offset.z;

        float maxHeightFloat = Mathf.PerlinNoise(xCoord, zCoord);
        float convertToChunkHeight = (maxHeightFloat*(chunkHeight+chunkHeight))-chunkHeight;
        int maxHeight = Mathf.RoundToInt(convertToChunkHeight);
        return maxHeight;
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
    
    public Voxel GetTopNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.y == chunkHeight) {
            return null;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y + 1, voxel.z);
            return neighbourVoxel;
        }
    }
    
    public Voxel GetFrontNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.x == chunkOneSideLength) {
            Vector3 offset = chunk.GetOffset();
            offset.x += 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(-chunkLengthWidth, voxel.y, voxel.z);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x + 1, voxel.y, voxel.z);
            return neighbourVoxel;
        }
    }
    
    public Voxel GetRightNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.z == chunkOneSideLength) {
            Vector3 offset = chunk.GetOffset();
            offset.z += 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y, -chunkLengthWidth);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y, voxel.z + 1);
            return neighbourVoxel;
        }
    }
    
    public Voxel GetBottomNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.y == -chunkHeight) {
            return null;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y - 1, voxel.z);
            return neighbourVoxel;
        }
    }
    
    public Voxel GetBackNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.x == -chunkOneSideLength) {
            Vector3 offset = chunk.GetOffset();
            offset.x -= 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(chunkOneSideLength, voxel.y, voxel.z);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x - 1, voxel.y, voxel.z);
            return neighbourVoxel;
        }
    }

    public Voxel GetLeftNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.z == -chunkLengthWidth) {
            Vector3 offset = chunk.GetOffset();
            offset.z -= 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y, chunkOneSideLength);
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

    public Chunk GetRenderedChunkByOffset(Vector3 offset) {
        foreach(Chunk chunk in renderedChunks) {
            if (chunk.GetOffset() == offset) {
                return chunk;
            }
        }
        return null;
    }
    
    //for inspector use
    public float BlocksInChunk(int width, int height) {
        float total = Mathf.Pow(width, 2)*height;
        return total;
    }

    //for inspector use
    public int TotalChunksToRender(int chunksAround) {
        int total;
        total = ((chunksAround*2)+1)*((chunksAround*2)+1);
        return total;
    }

    //check if value is even, if not -> +1
    public int checkAndEven(int num) {
        if (num%2 == 0) {
        
        } else {
            num++;
        }
        return num;
    }

    //for dev use
    public void ClearLog()
    {
    var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
    var type = assembly.GetType("UnityEditor.LogEntries");
    var method = type.GetMethod("Clear");
    method.Invoke(new object(), null);
    }
}
