using UnityEngine;
using System.Collections;

public static class SuperCollider {

    public static Vector3 ClosestPointOnSurface(Collider collider, Vector3 to, float radius)
    {
        if (collider is BoxCollider)
        {
            return SuperCollider.ClosestPointOnSurface((BoxCollider)collider, to);
        }
        else if (collider is SphereCollider)
        {
            return SuperCollider.ClosestPointOnSurface((SphereCollider)collider, to);
        }
        else if (collider is CapsuleCollider)
        {
            return SuperCollider.ClosestPointOnSurface((CapsuleCollider)collider, to);
        }
        else if (collider is MeshCollider)
        {
            RPGMesh rpgMesh = collider.GetComponent<RPGMesh>();

            if (rpgMesh != null)
            {
                return rpgMesh.ClosestPointOn(to, radius, false, false);
            }

            BSPTree bsp = collider.GetComponent<BSPTree>();

            if (bsp != null)
            {
                return bsp.ClosestPointOn(to, radius);
            }

            BruteForceMesh bfm = collider.GetComponent<BruteForceMesh>();

            if (bfm != null)
            {
                return bfm.ClosestPointOn(to);
            }
        }
        else if (collider is TerrainCollider)
        {
            return SuperCollider.ClosestPointOnSurface((TerrainCollider)collider, to);
        }

        return Vector3.zero;
    }

    public static Vector3 ClosestPointOnSurface(SphereCollider collider, Vector3 to)
    {
        Vector3 p;

        //Need to take center property into account
        var centerPosition = collider.transform.position + collider.center;
        p = to - centerPosition;
        p.Normalize();

        p *= collider.radius * collider.transform.localScale.x;
        p += centerPosition;

        return p;
    }

    public static Vector3 ClosestPointOnSurface(BoxCollider collider, Vector3 to)
    {
        // Cache the collider transform
        var ct = collider.transform;

        // Firstly, transform the point into the space of the collider
        var local = ct.InverseTransformPoint(to);

        // Now, shift it to be in the center of the box
        local -= collider.center;

        //Pre multiply to save operations.
        var halfSize = collider.size * 0.5f;

        // Clamp the points to the collider's extents
        var localNorm = new Vector3(
                Mathf.Clamp(local.x, -halfSize.x, halfSize.x),
                Mathf.Clamp(local.y, -halfSize.y, halfSize.y),
                Mathf.Clamp(local.z, -halfSize.z, halfSize.z)
            );

        //Calculate distances from each edge
        var dx = Mathf.Min(Mathf.Abs(halfSize.x - localNorm.x), Mathf.Abs(-halfSize.x - localNorm.x));
        var dy = Mathf.Min(Mathf.Abs(halfSize.y - localNorm.y), Mathf.Abs(-halfSize.y - localNorm.y));
        var dz = Mathf.Min(Mathf.Abs(halfSize.z - localNorm.z), Mathf.Abs(-halfSize.z - localNorm.z));

        // Select a face to project on
        if (dx < dy && dx < dz)
        {
            localNorm.x = Mathf.Sign(localNorm.x) * halfSize.x;
        }
        else if (dy < dx && dy < dz)
        {
            localNorm.y = Mathf.Sign(localNorm.y) * halfSize.y;
        }
        else if (dz < dx && dz < dy)
        {
            localNorm.z = Mathf.Sign(localNorm.z) * halfSize.z;
        }

        // Now we undo our transformations
        localNorm += collider.center;

        // Return resulting point
        return ct.TransformPoint(localNorm);
    }

    // Courtesy of Moodie
    public static Vector3 ClosestPointOnSurface(CapsuleCollider collider, Vector3 to)
    {
        Transform ct = collider.transform; // Transform of the collider

        float lineLength = collider.height - collider.radius * 2; // The length of the line connecting the center of both sphere
        Vector3 dir = Vector3.up;

        Vector3 upperSphere = dir * lineLength * 0.5f + collider.center; // The position of the radius of the upper sphere in local coordinates
        Vector3 lowerSphere = -dir * lineLength * 0.5f + collider.center; // The position of the radius of the lower sphere in local coordinates

        Vector3 local = ct.InverseTransformPoint(to); // The position of the controller in local coordinates

        Vector3 p = Vector3.zero; // Contact point
        Vector3 pt = Vector3.zero; // The point we need to use to get a direction vector with the controller to calculate contact point

        if (local.y < lineLength * 0.5f && local.y > -lineLength * 0.5f) // Controller is contacting with cylinder, not spheres
            pt = dir * local.y + collider.center;
        else if (local.y > lineLength * 0.5f) // Controller is contacting with the upper sphere 
            pt = upperSphere;
        else if (local.y < -lineLength * 0.5f) // Controller is contacting with lower sphere
            pt = lowerSphere;

        //Calculate contact point in local coordinates and return it in world coordinates
        p = local - pt;
        p.Normalize();
        p = p * collider.radius + pt;
        return ct.TransformPoint(p);

    }

    public static Vector3 ClosestPointOnSurface(TerrainCollider collider, Vector3 to, bool fast = true)
    {
        Vector3 result = to;

        //Get the terrain data
        var terrainData = collider.terrainData;
        if (terrainData != null)
        {
            // Cache the collider transform
            var ct = collider.transform;

            // Firstly, transform the point into the space of the collider
            var local = ct.InverseTransformPoint(to);

            //Where are we positioned on the terrain
            var percentZ = Mathf.Clamp01(local.z / terrainData.size.z);
            var percentX = Mathf.Clamp01(local.x / terrainData.size.x);

            Vector3 localNorm;
            if (fast)
            {
                //Cheap way the assumes the closest point is straight up (Works for most cases):

                //Flip X and Z for this method
                var height = terrainData.GetInterpolatedHeight(percentX, percentZ);

                localNorm = new Vector3(local.x, height, local.z);
            }
            else
            {
                //Find closest point on heightmap triangle (Better for when terrain is very steep and the collider has penetrated a large amount):

                //What is our heightmap position
                var heightmapZ = percentZ * terrainData.heightmapResolution;
                var heightmapX = percentX * terrainData.heightmapResolution;

                //When we do our triangle checking how far is one heightmap sample
                var offsetZ = (1.0f / terrainData.size.z) * terrainData.heightmapResolution;
                var offsetX = (1.0f / terrainData.size.x) * terrainData.heightmapResolution;

                //Get the heights for our corners
                var heights = terrainData.GetHeights(Mathf.FloorToInt(heightmapZ), Mathf.FloorToInt(heightmapX), 2, 2);

                //Find corner for the appropriate triangle.
                Vector3 firstCorner, secondCorner, thirdCorner;
                if (heightmapZ % 1.0f < 0.5f)
                {
                    firstCorner = new Vector3(local.x, heights[0, 0], local.z);
                    secondCorner = new Vector3(local.x + offsetX, heights[0, 1], local.z);

                    if (heightmapX % 1.0f < 0.5f)
                    {
                        thirdCorner = new Vector3(local.x, heights[1, 0], local.z + offsetZ);
                    }
                    else
                    {
                        thirdCorner = new Vector3(local.x + offsetX, heights[1, 1], local.z + offsetZ);
                    }
                }
                else
                {
                    firstCorner = new Vector3(local.x, heights[1, 0], local.z + offsetZ);
                    secondCorner = new Vector3(local.x + offsetX, heights[1, 1], local.z + offsetZ);

                    if (heightmapX % 1.0f < 0.5f)
                    {
                        thirdCorner = new Vector3(local.x, heights[0, 0], local.z);
                    }
                    else
                    {
                        thirdCorner = new Vector3(local.x + offsetX, heights[0, 1], local.z);
                    }
                }

                //Find normal for our triangle
                var normal = Vector3.Cross(firstCorner - thirdCorner, secondCorner - thirdCorner).normalized;
                normal.y = Mathf.Abs(normal.y);
                Debug.DrawLine(to, to + normal, Color.red);

                //Find distance from triangle
                var distance = Vector3.Dot(normal, local);

                Debug.Log("Normal: " + normal + " Distance: " + distance);

                //Multiply by our normal to get on the surface and then transform back.
                localNorm = local - (normal * distance);
            }

            //Transform back.
            result = ct.TransformPoint(localNorm);
        }

        //Return resulting point
        return result;
    }
}
