using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


public class PlayerSpawner : MonoBehaviour
{
    public GameObject playerObject;
    public WorldGeneration world;

    private float spawnLocationY;

    void Start()
    {
        spawnLocationY = world.calculateVoxelMaxHeight(0, 0, new Vector3(0f, 0f, 0f));
        spawnLocationY = spawnLocationY+0.5f;
        playerObject.transform.position = new Vector3(0f, spawnLocationY, 0);
    }
}
