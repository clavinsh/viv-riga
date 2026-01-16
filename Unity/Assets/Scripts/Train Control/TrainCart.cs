using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

[RequireComponent(typeof(Rigidbody))]
public class RailCart : MonoBehaviour
{
    [SerializeField] private SplineContainer rail = null;
    private Spline currentSpline;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentSpline = rail.Splines[0];
    }

    private void FixedUpdate()
    {
        var native = new NativeSpline(currentSpline);
        Vector3 localPosition = rail.transform.InverseTransformPoint(transform.position);
        float distance = SplineUtility.GetNearestPoint(native, localPosition, out float3 nearest, out float t);
        
        transform.position = rail.transform.TransformPoint(nearest);
        
        Vector3 forward = Vector3.Normalize(rail.transform.TransformDirection(native.EvaluateTangent(t)));
        
        // Flatten to horizontal - only rotate around Y axis
        forward.y = 0;
        if (forward.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
        }
        
        // Get full 3D tangent for velocity direction
        Vector3 trackDirection = Vector3.Normalize(rail.transform.TransformDirection(native.EvaluateTangent(t)));
        if (Vector3.Dot(rb.linearVelocity, trackDirection) < 0)
        {
            trackDirection *= -1;
        }
        rb.linearVelocity = rb.linearVelocity.magnitude * trackDirection;
    }
}