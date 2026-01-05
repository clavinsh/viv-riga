using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TrainEngine : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float power = 10;
    private InputAction moveAction;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        moveAction = InputSystem.actions.FindAction("Move");
    }

    private void FixedUpdate()
    {
        Vector2 moveValue = moveAction.ReadValue<Vector2>();

        if (moveValue.y > 0.5f)
        {
            Throttle(power);
        }
        else if (moveValue.y < -0.5f)
        {
            Throttle(-power);
        }
    }


    private void Throttle(float power)
    {
        rb.AddForce(transform.forward * power);
    }
}
