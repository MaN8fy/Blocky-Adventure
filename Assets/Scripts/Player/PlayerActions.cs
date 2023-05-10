using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActions : MonoBehaviour
{
    CustomInput controls;
    
    float startTime;
    public float AttackDelay;
    private int AttackHoldCounter;

    public float rayLength;
    public LayerMask layersToHit;
    Ray ray;
    RaycastHit hit;
    RaycastHit oldHit;

    Transform cameraTransform;

    ScriptableBlock blockToSpawn;

    public WorldGeneration world;

    private Inventory inventory;
    
    void Awake()
    {
        controls = new CustomInput();
    }

    private void Start() {
        inventory = GetComponent<Inventory>();
        blockToSpawn = inventory.Blocks[inventory.currentItem];
    }

    void Update()
    {
        CheckAttack();
        CheckScroll();
    }

    private void CheckAttack(){
        if (controls.Player.Attack.inProgress) {
            if (startTime + AttackDelay <= Time.time ) {
                startTime = Time.time;
                AttackHold();
            }
        }
    }

    private void SecondAction() {
        if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, rayLength)) { //hitted something
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Block")) { //if block
                world.SpawnNewCube(hit, blockToSpawn);
            }
        }
    }

    private void AttackHold() {

        cameraTransform = Camera.main.transform;
        if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, rayLength)) { //hitted something
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Block")) { //if block
                if(oldHit.collider == hit.collider) { //good block
                    Vector3 voxelOffset = hit.collider.gameObject.transform.localPosition;
                    Vector3 chunkPosition = hit.collider.gameObject.transform.parent.gameObject.transform.position;
                    
                    ScriptableBlock block = world.GetScriptableBlock(voxelOffset, chunkPosition);
                    
                    int destroyDifficulty = block.destroyDifficulty;
                    if (destroyDifficulty <= AttackHoldCounter) { //Destroy!
                        world.DestroyVoxel(voxelOffset, chunkPosition, hit.collider.gameObject);
                        AttackHoldCounter = 0;
                    } else { //hold up
                        AttackHoldCounter++;
                    }
                } else { //wrong block
                    AttackHoldCounter = 0;
                }
                oldHit = hit;
            }
        }
    }

    private void CheckScroll() {
        float scroll;
        scroll = controls.Player.Inventory.ReadValue<float>();
        if (scroll > 0) {
            inventory.switchItem(1);
            blockToSpawn = inventory.Blocks[inventory.currentItem];
        } else if (scroll < 0) {
            inventory.switchItem(-1);
            blockToSpawn = inventory.Blocks[inventory.currentItem];
        }

    }

    private void AttackButtonPressed(InputAction.CallbackContext obj)
    {
        startTime = 0f;
        AttackHoldCounter = 0;

        cameraTransform = Camera.main.transform;
        if(Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, rayLength)) {
            if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Block")) {
                oldHit = hit;
            }
        }
    }

    private void AttackButtonCanceled(InputAction.CallbackContext obj)
    {
        AttackHoldCounter = 0;
    }

    private void SecondaryActionButtonPressed(InputAction.CallbackContext obj)
    {
        SecondAction();
    }

    private void SecondaryActionButtonCanceled(InputAction.CallbackContext obj)
    {
        
    }

    private void OnEnable() {
        controls.Enable();
        controls.Player.Attack.started += AttackButtonPressed;
        controls.Player.Attack.canceled += AttackButtonCanceled;
        controls.Player.SecondaryAction.started += SecondaryActionButtonPressed;
        controls.Player.SecondaryAction.canceled += SecondaryActionButtonCanceled;
    }

    private void OnDisable() {
        controls.Disable();
        controls.Player.Attack.started -= AttackButtonPressed;
        controls.Player.Attack.canceled -= AttackButtonCanceled;
        controls.Player.SecondaryAction.started -= SecondaryActionButtonPressed;
        controls.Player.SecondaryAction.canceled -= SecondaryActionButtonCanceled;
    }
}