using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttacks : MonoBehaviour
{
    public Collider hitter;
    public float maxCount = 0.1f;
    public float count = 0;
    bool lastPress;
    Animator anim;
    InputMaster input;
    void Start()
    {
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
    void Update()
    {

        if (!lastPress && input.Player.Attack.controls.Any(c => c.IsPressed()))
        {
            anim.SetInteger("Attack", anim.GetInteger("Attack") + 1);
        }
        lastPress = input.Player.Attack.controls.Any(c => c.IsPressed());
    }
    void ComboDone(int val)
    {
        if(anim.GetInteger("Attack") <= val || val >= 3) anim.SetInteger("Attack", 0);
    }
    void Vel()
    {
        GetComponent<CharacterController3D>().speed += 1.4f;
        GetComponent<CharacterController3D>().SpeedCalc(transform.forward, 1);
    }
    void ColliderOn()
    {
        hitter.enabled = true;
    }
    void ColliderOff()
    {
        hitter.enabled = false;
    }
}