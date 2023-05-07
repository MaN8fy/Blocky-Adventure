using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed;
    public float sprintSpeed;
    public float jumpHeight;

    private float currentSpeed;
    private float gravityValue = -9.81f;
    private Vector2 move;
    private bool isGrounded;
    
    private CustomInput controls;
    private CharacterController controller;
    private Vector3 playerVelocity;

  
    private void Awake() {
        controls = new CustomInput();
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        jumpHeight = jumpHeight / 1.5f;
    }

    void Update() {
        isGrounded = controller.isGrounded;
        Walk();
        Sprint();
        Jump();
        Gravity();
    }

    //Player actions
    private void Walk() {
        move = controls.Player.Movement.ReadValue<Vector2>();

        Vector3 movement = (move.y * transform.forward) + (move.x * transform.right);
        controller.Move(movement * currentSpeed * Time.deltaTime);
    }

    private void Sprint() {
        if (controls.Player.Sprint.inProgress) {
            currentSpeed = sprintSpeed;
        } else {
            currentSpeed = walkSpeed;
        }
    }

    private void Jump() {
        if (controls.Player.Jump.triggered && isGrounded) {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * -3.0f * gravityValue);
        }
    }

    //Makes player fall
    private void Gravity() {
        if (isGrounded && playerVelocity.y < 0) {
            playerVelocity.y = 0f;
        }
        playerVelocity.y += gravityValue * Time.deltaTime;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    private void OnEnable() {
        controls.Enable();
    }

    private void OnDisable() {
        controls.Disable();
    }

}