using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateWorldAroundChunk : MonoBehaviour
{
    public WorldGeneration worldGeneration;

    private void OnTriggerEnter(Collider chunk) {
        worldGeneration.updateWorldChunkEnter(chunk.transform.position);
    }

    private void OnTriggerExit() {
        worldGeneration.updateWorldChunkExit();
    }

}
