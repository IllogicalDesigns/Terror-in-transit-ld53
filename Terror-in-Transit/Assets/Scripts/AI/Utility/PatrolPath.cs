using System.Collections.Generic;
using UnityEngine;

public class PatrolPath : MonoBehaviour {
    public List<Transform> points = new List<Transform>(); // List of points to create the loop

    private void OnDrawGizmos() {
        if (points == null || points.Count < 2) return;

        // Draw the first line
        Gizmos.color = Color.white;
        Gizmos.DrawLine(points[0].position, points[1].position);

        // Draw the rest of the lines
        for (int i = 1; i < points.Count - 1; i++) {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(points[i].position, points[i + 1].position);
        }

        // Draw the last line to close the loop
        Gizmos.color = Color.white;
        Gizmos.DrawLine(points[points.Count - 1].position, points[0].position);
    }
}
