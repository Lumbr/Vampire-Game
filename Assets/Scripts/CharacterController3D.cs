using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CharacterController3D : MonoBehaviour
{
    public float maxSpeed, maxSprintSpeed;
    //public CinemachineTargetGroup tGroup;
    public Transform cam, enemy;
    public InputActionReference inputFile;
    public float turnSmoothTime;
    public bool lockedOn;
    bool lockLastFrame;
    [HideInInspector] public float speed;
    float turnSmoothVelocity;
    Vector3 moveDir;
    Rigidbody rb;
    Animator anim;
    InputMaster input;

    #region Setup
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
    }
    void OnEnable()
    {
        input = new InputMaster();
        input.Enable();
    }
    void OnDisable()
    {
        input.Disable();
    }
    #endregion
    void Update()
    {
        Vector3 direction = new Vector3(input.Player.Move.ReadValue<Vector2>().x, 0, input.Player.Move.ReadValue<Vector2>().y); //get directional input from player and assign it to the right axes
        if (!lockedOn)
        {
            if (Physics.Raycast(cam.position, cam.forward, out RaycastHit hit))
            {
                if (hit.collider.TryGetComponent(out Enemy enemy))
                {
                    this.enemy = enemy.transform;
                }
                else enemy = null;

            }
            else enemy = null;
        }

        if (enemy && !lockLastFrame && input.Player.LockOn.controls.Any(c => c.IsPressed())) lockedOn = !lockedOn;
        if (lockedOn) 
        {
            CinemachineFreeLook freelook = VirtualCameraSingleton.instance.GetComponent<CinemachineFreeLook>();
            Vector3 toEnemy = (transform.position - enemy.position).normalized;
            float targetAngle = Mathf.Lerp(freelook.m_XAxis.Value, (Mathf.Atan2(-toEnemy.x, -toEnemy.z) * Mathf.Rad2Deg), turnSmoothTime);
            freelook.m_XAxis.Value = targetAngle;
            freelook.m_YAxis.Value = .6f;
            VirtualCameraSingleton.instance.GetComponent<CinemachineInputProvider>().XYAxis = null;
        }
        else
        {
            VirtualCameraSingleton.instance.GetComponent<CinemachineInputProvider>().XYAxis = inputFile;
        }
        //else tGroup.RemoveMember(enemy);
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.eulerAngles = new Vector3(0, angle, 0);
            moveDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            // I'm not even gonna try with this one. figure out angles for character to move using weird maths I found on brackeys
        }
        lockLastFrame = input.Player.LockOn.controls.Any(c => c.IsPressed());
    }
    void FixedUpdate()
    {
        Vector2 localVelocity = new Vector2(Vector3.Dot(rb.velocity, transform.right), Vector3.Dot(rb.velocity, transform.forward)); //figure out velocity relative to the player
        Vector2 direction = input.Player.Move.ReadValue<Vector2>(); //get directional input from player

        if (input.Player.Sprint.controls.Any(c => c.IsPressed())) SpeedCalc(direction, maxSprintSpeed); //Sprint
        else SpeedCalc(direction, maxSpeed); //Walk

        anim.SetFloat("X", Mathf.Lerp(anim.GetFloat("X"), localVelocity.x, turnSmoothTime)); //animation shit
        anim.SetFloat("Y", Mathf.Lerp(anim.GetFloat("Y"), localVelocity.y, turnSmoothTime));
    }

    void SpeedCalc(Vector2 direction, float maxSpeed)
    {
        if (direction.magnitude >= 0.1f) speed += maxSpeed / 10; //if input exists, increase speed
        speed = Drag(speed, maxSpeed / 20); //decrease speed by drag
        speed = SmoothClamp(speed, -maxSpeed, maxSpeed, maxSpeed / 10); //clamp speed to max speed so it doesn't go over
        rb.velocity = new Vector3(moveDir.x * speed, rb.velocity.y, moveDir.z * speed); //set velocity to speed
    }
    float Drag(float val, float drag)
    {
        if (val >= 0) val -= drag * .8f;
        else val += drag * .8f;
        if (val > -drag && val < drag) val = 0;
        return val;
    }
    float SmoothClamp(float val, float min, float max, float drag)
    {
        if (val > max) val -= drag;
        if (val < min) val += drag;
        return val;
    }
}
