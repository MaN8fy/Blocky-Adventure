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
    private int maxHeight;

    /*
    Active chunk is chunk where player currently is, last chunk is chunk which player left.
    They are used to determine which chunks have to be destroyed
    */
    private Vector3 activeChunk = new Vector3(0f, 0f, 0f);

    GameObject chunkObject;
    GameObject spawnedVoxel;
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;
    List <Vector2> uvs;

    int lastVertex; 

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

        calculatedRandom = (float)chunkLengthWidth * scale;

        //renders first bunch of chunks
        for (int x = -chunksToRenderAround-1; x <= chunksToRenderAround+1; x++) {
            for (int z = -chunksToRenderAround-1; z <= chunksToRenderAround+1; z++) {
                CreateChunk(x, z);
            }
        }

        for (int x = -chunksToRenderAround; x <= chunksToRenderAround; x++) {
            for (int z = -chunksToRenderAround; z <= chunksToRenderAround; z++) {
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
                maxHeight = calculateVoxelMaxHeight(x, z, chunkOffset);
                for (int y=-chunkHeight; y < maxHeight; y++) {
                    voxel = assumeVoxelType(y);
                    voxel.x = x;
                    voxel.y = y;
                    voxel.z = z;
                    voxel.chunkOffset = chunkOffset;
                    chunk.voxels.Add(voxel);
                }
            }
        }
        chunks.Add(chunk);
    }

    public Voxel assumeVoxelType(int y) {
        Voxel voxel;
        if (y >= (float)chunkHeight*0.5f) {
            voxel = new Voxel(topLevelBlock);
        } else if (y < (float)chunkHeight*-0.4f) {
            voxel = new Voxel(bottomLevelBlock);
        } else if (y < maxHeight-3) {
            voxel = new Voxel(bottomLevelBlock);
        } else {
            voxel = new Voxel(groundBlock);
        }
        return voxel;
    } 

    //chunk rendering from list chunks based on offset
    IEnumerator RenderChunk(Vector3 chunkOffset) {
        Chunk chunk = GetChunkByOffset(chunkOffset);
        Vector3 offsetPosition = chunkOffset*chunkLengthWidth;     
        BoxCollider chunkCollider;
        MeshRenderer chunkRenderer;
        string chunkName = String.Format("Chunk [{0}, {1}, {2}]", chunkOffset.x.ToString(), chunkOffset.y.ToString(), chunkOffset.z.ToString());

        chunkObject = new GameObject(chunkName);
        chunkObject.transform.parent = gameObject.transform;
        chunkRenderer = chunkObject.AddComponent<MeshRenderer>();
        chunkCollider = chunkObject.AddComponent<BoxCollider>();
        chunkCollider.isTrigger = true;
        chunkCollider.size = new Vector3(chunkLengthWidth, chunkDepth+10f, chunkLengthWidth);
        
        chunk.chunkObject = chunkObject;

        for (int index = 0; index<chunk.voxels.Count; index++)
        {   
            GenerateBlock(chunk, index);
        }
        chunk.chunkObject.transform.position = offsetPosition;
        chunk.chunkObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        renderedChunks.Add(chunk);

        yield return new WaitForSeconds(0.2f);
    }
    
    //this func. is called when player enters new chunk
    public void updateWorldChunkEnter(Vector3 chunkPosition) {
        activeChunk = chunkPosition/chunkLengthWidth;
        Vector3 chunkOffset = new Vector3(0f, 0f, 0f);

        int minX = (int)activeChunk.x-chunksToRenderAround;
        int maxX = (int)activeChunk.x+chunksToRenderAround;

        int minZ = (int)activeChunk.z-chunksToRenderAround;
        int maxZ = (int)activeChunk.z+chunksToRenderAround;

        for (int x = minX-1; x <= maxX+1; x++) {
            for (int z = minZ-1; z <= maxZ+1; z++) {
                chunkOffset.x = x;
                chunkOffset.z = z;
                if (GetChunkByOffset(chunkOffset) == null) {
                    CreateChunk(x, z);
                }
            }
        }

        for (int x = minX; x <= maxX; x++) {
            for (int z = minZ; z <= maxZ; z++) {
                chunkOffset.x = x;
                chunkOffset.z = z;
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

    //Perlin noise calculation
    public int calculateVoxelMaxHeight(int x, int z, Vector3 offset) {
        float xCoord = (float)x / calculatedRandom + seed + offset.x;
        float zCoord = (float)z / calculatedRandom + seed + offset.z;

        float maxHeightFloat = Mathf.PerlinNoise(xCoord, zCoord);
        float convertToChunkHeight = (maxHeightFloat*(chunkHeight+chunkHeight))-chunkHeight;
        int maxHeight = Mathf.RoundToInt(convertToChunkHeight);
        return maxHeight;
    }

    /*couple of functions to get neughbour voxels*/
    public List<Voxel> GetAllVoxelNeighbours(Voxel voxel, Chunk chunk){
        List<Voxel> voxels = new List<Voxel>();   
        voxels.Add(GetTopNeighbour(voxel, chunk));
        voxels.Add(GetFrontNeighbour(voxel, chunk));
        voxels.Add(GetRightNeighbour(voxel, chunk));
        voxels.Add(GetBottomNeighbour(voxel, chunk));
        voxels.Add(GetBackNeighbour(voxel, chunk));
        voxels.Add(GetLeftNeighbour(voxel, chunk));
        return voxels;
    }
    
    public Voxel GetTopNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.y == chunkHeight-1) {
            return null;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y + 1, voxel.z);
            return neighbourVoxel;
        }
    }
    
    public Voxel GetFrontNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.x == chunkOneSideLength-1) {
            Vector3 offset = chunk.GetOffset();
            offset.x += 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(-chunkOneSideLength, voxel.y, voxel.z);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x + 1, voxel.y, voxel.z);
            return neighbourVoxel;
        }
    }
    
    public Voxel GetRightNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.z == chunkOneSideLength-1) {
            Vector3 offset = chunk.GetOffset();
            offset.z += 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y, -chunkOneSideLength);
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
            neighbourVoxel = chunk.GetVoxel(chunkOneSideLength-1, voxel.y, voxel.z);
            return neighbourVoxel;
        } else {
            neighbourVoxel = chunk.GetVoxel(voxel.x - 1, voxel.y, voxel.z);
            return neighbourVoxel;
        }
    }

    public Voxel GetLeftNeighbour(Voxel voxel, Chunk chunk){
        Voxel neighbourVoxel;
        if(voxel.z == -chunkOneSideLength) {
            Vector3 offset = chunk.GetOffset();
            offset.z -= 1;
            chunk = GetChunkByOffset(offset);
            if(chunk == null) {
                return null;
            }
            neighbourVoxel = chunk.GetVoxel(voxel.x, voxel.y, chunkOneSideLength-1);
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
    
    public void GenerateBlock(Chunk chunk, int index) {
        
        int x = chunk.voxels[index].x;
        int y = chunk.voxels[index].y;
        int z = chunk.voxels[index].z;
        string voxelName = String.Format("{0}, {1}, {2}, {3}", chunk.voxels[index].GetScriptableBlock().ToString(), x.ToString(), y.ToString(), z.ToString());
        spawnedVoxel = new GameObject(voxelName);
        spawnedVoxel.layer = LayerMask.NameToLayer("Block");

        spawnedVoxel.transform.parent = chunk.chunkObject.transform;
        spawnedVoxel.transform.localPosition = new Vector3(x+0, y, z);

        GenerateFaces(chunk, index);

        chunk.voxels[index].voxelObject = spawnedVoxel;
    }
    
    public void GenerateFaces(Chunk chunk, int index) {
        List<Voxel> neighbours = GetAllVoxelNeighbours(chunk.voxels[index], chunk);

        if (neighbours[0] == null || neighbours[1] == null || neighbours[2] == null || neighbours[3] == null || neighbours[4] == null || neighbours[5] == null) {            
            MeshFilter voxelMeshFilter;
            MeshRenderer voxelMeshRenderer;
            BoxCollider voxelCol;
            mesh = new Mesh();
            voxelMeshFilter = spawnedVoxel.AddComponent<MeshFilter>();
            voxelMeshRenderer = spawnedVoxel.AddComponent<MeshRenderer>();
            voxelCol = spawnedVoxel.AddComponent<BoxCollider>();
            voxelCol.size = new Vector3(1f, 1f, 1f);
            voxelCol.center = new Vector3(0.5f, 0.5f, 0.5f); 
            vertices = new List<Vector3>();
            triangles = new List<int>();
            uvs = new List<Vector2>();

            if(neighbours[0] == null) {
            GenerateTopFace();
            }
            if(neighbours[1] == null) {
                GenerateFrontFace();
            }
            if(neighbours[2] == null) {
                GenerateRightFace();
            }
            if(neighbours[3] == null) {
                GenerateBottomFace();
            }
            if(neighbours[4] == null) {
                GenerateBackFace();
            }
            if(neighbours[5] == null) {
                GenerateLeftFace();
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.SetUVs(0, uvs.ToArray());
            mesh.RecalculateNormals();
            voxelMeshFilter.mesh = mesh;
            voxelMeshRenderer.material = chunk.voxels[index].block.material;
        }
    }

    public void GenerateTopFace() {
        lastVertex = vertices.Count;

        //declare vertices
        vertices.Add(Vector3.up + Vector3.right); 
        vertices.Add(Vector3.one);
        vertices.Add(Vector3.forward + Vector3.up);
        vertices.Add(Vector3.up);

        //first triangle
        triangles.Add(lastVertex+2);
        triangles.Add(lastVertex+1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex+3);
        triangles.Add(lastVertex+2);
    }

    public void GenerateFrontFace() {
        lastVertex = vertices.Count;

        //declare vertices
        vertices.Add(Vector3.right + Vector3.forward); 
        vertices.Add(Vector3.one);
        vertices.Add(Vector3.right + Vector3.up);
        vertices.Add(Vector3.right);

        //first triangle
        triangles.Add(lastVertex+2);
        triangles.Add(lastVertex+1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex+3);
        triangles.Add(lastVertex+2);
    }

    public void GenerateRightFace() {
        lastVertex = vertices.Count;

        //declare vertices
        vertices.Add(Vector3.forward); 
        vertices.Add(Vector3.forward + Vector3.up);
        vertices.Add(Vector3.forward + Vector3.up + Vector3.right);
        vertices.Add(Vector3.forward + Vector3.right);

        //first triangle
        triangles.Add(lastVertex+2);
        triangles.Add(lastVertex+1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex+3);
        triangles.Add(lastVertex+2);
    }

    public void GenerateBottomFace() {
        lastVertex = vertices.Count;

        //declare vertices
        vertices.Add(new Vector3()); 
        vertices.Add(Vector3.forward);
        vertices.Add(Vector3.forward + Vector3.right);
        vertices.Add(Vector3.right);

        //first triangle
        triangles.Add(lastVertex+2);
        triangles.Add(lastVertex+1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex+3);
        triangles.Add(lastVertex+2);
    }

    public void GenerateBackFace() {
        lastVertex = vertices.Count;

        //declare vertices
        vertices.Add(new Vector3()); 
        vertices.Add(Vector3.up);
        vertices.Add(Vector3.forward + Vector3.up);
        vertices.Add(Vector3.forward);

        //first triangle
        triangles.Add(lastVertex+2);
        triangles.Add(lastVertex+1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex+3);
        triangles.Add(lastVertex+2);
    }

    public void GenerateLeftFace() {
        lastVertex = vertices.Count;

        //declare vertices
        vertices.Add(Vector3.right); 
        vertices.Add(Vector3.up + Vector3.right);
        vertices.Add(Vector3.up);
        vertices.Add(new Vector3());

        //first triangle
        triangles.Add(lastVertex+2);
        triangles.Add(lastVertex+1);
        triangles.Add(lastVertex);

        //second triangle
        triangles.Add(lastVertex);
        triangles.Add(lastVertex+3);
        triangles.Add(lastVertex+2);
    }

    public ScriptableBlock GetScriptableBlock(Vector3 voxelOffset, Vector3 chunkPosition) {
        ScriptableBlock block;
        block = GetChunkByOffset(chunkPosition/chunkLengthWidth).GetVoxel((int)voxelOffset.x, (int)voxelOffset.y, (int)voxelOffset.z).GetScriptableBlock();

        return block;
    }

    public void DestroyVoxel(Vector3 voxelOffset, Vector3 chunkPosition, GameObject voxelBlock) {
        if (voxelOffset.y > -chunkHeight) {
            Chunk chunk = GetChunkByOffset(chunkPosition/chunkLengthWidth);
            Voxel voxel = chunk.GetVoxel((int)voxelOffset.x, (int)voxelOffset.y, (int)voxelOffset.z);
            Destroy(voxelBlock);
            
            chunk.voxels.Remove(voxel);

            UpdateAllNeighbours(voxel, chunk);
        }
    }

    public void SpawnNewCube(RaycastHit hit) {

        Transform defaultTransform = hit.collider.transform;
        Vector3 spawnDirection = defaultTransform.position-hit.point;
        Vector3 defaultChunkOffset = defaultTransform.parent.gameObject.transform.position/chunkLengthWidth;
        Vector3 spawnPosition = defaultTransform.localPosition;
        Vector3 chunkOffset = defaultChunkOffset;
        

        if (spawnDirection.y == -1 ) { //top good
            if (defaultTransform.localPosition.y + 1 >= chunkHeight) {
                return;
            } else {
                spawnPosition.y = spawnPosition.y+1f;
            }
        } else if (spawnDirection.x == -1 ) { //front goood
            if (defaultTransform.localPosition.x + 1 >= chunkOneSideLength) { //out of chunk space
                chunkOffset.x = chunkOffset.x+1f;
                spawnPosition.x = -chunkOneSideLength;
            } else {
                spawnPosition.x = spawnPosition.x+1f;
            }
        } else if (spawnDirection.z == -1 ) { //right good
            if (defaultTransform.localPosition.z + 1 >= chunkOneSideLength) { //out of chunk space
                chunkOffset.z = chunkOffset.z+1f;
                spawnPosition.z = -chunkOneSideLength;
            } else {
                spawnPosition.z = spawnPosition.z+1f;
            }
        } else if (spawnDirection.y == 0 ) { //bottom good
            if (defaultTransform.localPosition.y - 1 <= -chunkHeight) {
                return;
            } else {
                spawnPosition.y = spawnPosition.y-1f;
            }
        } else if (spawnDirection.x == 0 ) { //back
            if (defaultTransform.localPosition.x - 1 <= -chunkOneSideLength) { //out of chunk space
                chunkOffset.x = chunkOffset.x-1f;
                spawnPosition.x = chunkOneSideLength;
            } else {
                spawnPosition.x = spawnPosition.x-1f;
            }
        } else if (spawnDirection.z == 0 ) { //left
            if (defaultTransform.localPosition.z - 1 <= -chunkOneSideLength) { //out of chunk space
                chunkOffset.z = chunkOffset.z-1f;
                spawnPosition.z = chunkOneSideLength;
            } else {
                spawnPosition.z = spawnPosition.z-1f;
            }
        } else {
            return;
        }
        
        Chunk chunk = GetChunkByOffset(chunkOffset);
        Voxel voxel;

        voxel = new Voxel(groundBlock);
        voxel.x = (int)spawnPosition.x;
        voxel.y = (int)spawnPosition.y;
        voxel.z = (int)spawnPosition.z;
        voxel.chunkOffset = chunkOffset;
        chunk.voxels.Add(voxel);

        GenerateBlock(chunk, chunk.voxels.Count-1);
        UpdateAllNeighbours(voxel, chunk);
    }

    public void UpdateAllNeighbours(Voxel voxel, Chunk chunk) {
        List<Voxel> voxels = GetAllVoxelNeighbours(voxel, chunk);
        for (int i = 0; i < voxels.Count; i++) {
            if (voxels[i] != null) {
                Chunk curr_chunk = GetChunkByOffset(voxels[i].chunkOffset);
                int index = curr_chunk.voxels.IndexOf(voxels[i]);
                Destroy(voxels[i].voxelObject);
                GenerateBlock(curr_chunk, index);
            }
        }
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
}
