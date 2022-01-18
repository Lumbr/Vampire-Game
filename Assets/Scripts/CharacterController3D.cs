using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VladsUsefulScripts;

public class CharacterController3D : MonoBehaviour
{
    public Transform cam;
    public float speed, turnSmoothTime = 0.1f, jumpHeight;
    float turnSmoothVelocity;
    InputMaster input;
    Vector2 direction;
    Rigidbody rb;
    Animator anim;
    #region Setup
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        input = new InputMaster();
        input.Enable();
    }
    private void OnDisable()
    {
        input.Disable();
    }
    #endregion
    void Update()
    {
        
        Aim();
    }
    
    void Aim()
    {
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, cam.eulerAngles.y, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    void FixedUpdate()
    {
        Move();
    }

    private void Move()
    {
        Vector2 lastDir = direction;
        direction = input.Player.Move.ReadValue<Vector2>();
        rb.velocity = (transform.forward * direction.y + transform.right * direction.x) * speed;
        Vector2 smoothDirection = Vector2.Lerp(lastDir.normalized, direction.normalized, 1);
        anim.SetFloat("X", smoothDirection.normalized.x);
        anim.SetFloat("Y", smoothDirection.normalized.y);
    }
}
