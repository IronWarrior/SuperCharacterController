using UnityEngine;
using System.Collections;

/// <summary>
/// Primarily used for debugging purposes, this runs the same nearest point on mesh algorith
/// as the BSPTree, however there is no partitioning of the mesh; every triangle is evaluated
/// to find the nearest
/// </summary>
[RequireComponent(typeof(MeshCollider))]
public class BruteForceMesh : MonoBehaviour {

    private int triangleCount;
    private Vector3[] vertices;
    private int[] tris;
    private Vector3[] triangleNormals;

    private Mesh mesh;

	// Use this for initialization
	void Awake () {
        mesh = GetComponent<MeshCollider>().sharedMesh;

        tris = mesh.triangles;
        vertices = mesh.vertices;

        triangleCount = mesh.triangles.Length / 3;

        triangleNormals = new Vector3[triangleCount];

        for (int i = 0; i < tris.Length; i += 3)
        {
            Vector3 normal = Vector3.Cross((vertices[tris[i + 1]] - vertices[tris[i]]).normalized, (vertices[tris[i + 2]] - vertices[tris[i]]).normalized).normalized;

            triangleNormals[i / 3] = normal;
        }
	}

    public Vector3 ClosestPointOn(Vector3 to)
    {
        to = transform.InverseTransformPoint(to);

        Vector3 closest = ClosestPointOnTriangle(tris, to);

        return transform.TransformPoint(closest);
    }

    Vector3 ClosestPointOnTriangle(int[] triangles, Vector3 to)
    {
        float shortestDistance = float.MaxValue;

        Vector3 shortestPoint = Vector3.zero;

        // Iterate through all triangles
        for (int i = 0; i < triangles.Length; i += 3)
        {
            int triangle = i;

            Vector3 p1 = vertices[tris[triangle]];
            Vector3 p2 = vertices[tris[triangle + 1]];
            Vector3 p3 = vertices[tris[triangle + 2]];

            Vector3 nearest;

            Math3d.ClosestPointOnTriangleToPoint(ref p1, ref p2, ref p3, ref to, out nearest);

            float distance = (to - nearest).sqrMagnitude;

            if (distance <= shortestDistance)
            {
                shortestDistance = distance;
                shortestPoint = nearest;
            }
        }

        return shortestPoint;
    }
}
