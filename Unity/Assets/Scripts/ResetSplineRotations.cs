using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ResetSplineRotations : MonoBehaviour
{
    [ContextMenu("Reset All Knot Rotations")]
    public void ResetRotations()
    {
        var container = GetComponent<SplineContainer>();
        
        if (container == null)
        {
            Debug.LogError("No SplineContainer found!");
            return;
        }

#if UNITY_EDITOR
        Undo.RecordObject(container, "Reset Knot Rotations");
#endif

        for (int s = 0; s < container.Splines.Count; s++)
        {
            var spline = container.Splines[s];
            for (int i = 0; i < spline.Count; i++)
            {
                // Set tangent mode to Bezier first
                spline.SetTangentMode(i, TangentMode.Mirrored);
                
                // Now reset rotation
                var knot = spline[i];
                knot.Rotation = quaternion.identity;
                spline.SetKnot(i, knot);
            }
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(container);
#endif
        
        Debug.Log("Rotations reset!");
    }
}