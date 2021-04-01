using UnityEngine;

/// <summary>
/// https://www.gamasutra.com/view/feature/131790/simple_intersection_tests_for_games.php?print=1
/// https://wickedengine.net/2020/04/26/capsule-collision-detection/
/// </summary>

public static class ColliosionDetection
{
    /// <summary>
    /// Intersection test of oriented bounding box
    /// </summary>
    /// <param name="collider">Boxcollider</param>
    /// <param name="characterOrigin">the transform position of chracter collider</param>
    /// <returns></returns>
    public static Vector3 OrientedBoundingBox_OBB(BoxCollider collider, Vector3 characterOrigin) 
    {
        // Cache the collider transform
        Transform colTransform = collider.transform;

        // transform the point into the space of the collider
        var local = colTransform.InverseTransformPoint(characterOrigin);

        // Now, shift it to be in the center of the box
        local -= collider.center;

        // Inverse scale it by the colliders scale
        var localNorm = new Vector3(
            Mathf.Clamp(local.x, -collider.size.x * 0.5f, collider.size.x * 0.5f),
            Mathf.Clamp(local.y, -collider.size.y * 0.5f, collider.size.y * 0.5f),
            Mathf.Clamp(local.z, -collider.size.z * 0.5f, collider.size.z * 0.5f)
        );

        // Now we undo our transformations
        localNorm += collider.center;

        // Return resulting point
        return colTransform.TransformPoint(localNorm);
    }

    /// <summary>
    /// performs an closestPointAlgorithmn with different obstacle rotation
    /// </summary>
    /// <param name="collider">Boxcollider</param>
    /// <param name="characterOrigin">the transform position of chracter collider</param>
    /// <returns></returns>
    public static Vector3 ClosestPointOn(BoxCollider collider, Vector3 characterOrigin)
    {
        if (collider.transform.rotation == Quaternion.identity)
        {
            return collider.ClosestPointOnBounds(characterOrigin);
        }

        return OrientedBoundingBox_OBB(collider, characterOrigin);
    }

    /// <summary>
    /// performs an closestPointAlgorithmn with different obstacle rotation
    /// </summary>
    /// <param name="collider">spherecollider</param>
    /// <param name="characterOrigin">the transform position of chracter collider</param>
    /// <returns></returns>
    public static Vector3 ClosestPointOn(SphereCollider collider, Vector3 characterOrigin)
    {
        Vector3 p;

        p = characterOrigin - collider.transform.position;
        p.Normalize();

        p *= collider.radius * collider.transform.localScale.x;
        p += collider.transform.position;

        return p;
    }
}
