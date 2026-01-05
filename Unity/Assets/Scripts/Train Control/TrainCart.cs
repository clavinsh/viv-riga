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
    private Quaternion initialRotationOffset;


    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        currentSpline = rail.Splines[0];

        var native = new NativeSpline(currentSpline);
        Vector3 localPosition = rail.transform.InverseTransformPoint(transform.position);
        SplineUtility.GetNearestPoint(native, localPosition, out float3 nearest, out float t);

        Vector3 forward = Vector3.Normalize(rail.transform.TransformDirection(native.EvaluateTangent(t)));
        Vector3 up = rail.transform.TransformDirection(native.EvaluateUpVector(t));
        Quaternion splineRotation = Quaternion.LookRotation(forward, up);

        // Store the difference between our current rotation and the spline's rotation
        initialRotationOffset = Quaternion.Inverse(splineRotation) * transform.rotation;
    }

    private void FixedUpdate()
    {
        var native = new NativeSpline(currentSpline);

        Vector3 localPosition = rail.transform.InverseTransformPoint(transform.position);
        float distance = SplineUtility.GetNearestPoint(native, localPosition, out float3 nearest, out float t);

        transform.position = rail.transform.TransformPoint(nearest);

        Vector3 forward = Vector3.Normalize(rail.transform.TransformDirection(native.EvaluateTangent(t)));
        Vector3 up = rail.transform.TransformDirection(native.EvaluateUpVector(t));

        Quaternion splineRotation = Quaternion.LookRotation(forward, up);
        transform.rotation = splineRotation * initialRotationOffset;

        Vector3 trackDirection = forward;

        if (Vector3.Dot(rb.linearVelocity, trackDirection) < 0)
        {
            trackDirection *= -1;
        }

        rb.linearVelocity = rb.linearVelocity.magnitude * trackDirection;
    }
}
