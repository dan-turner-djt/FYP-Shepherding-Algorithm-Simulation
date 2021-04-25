using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Collision
{

    public static float GroundCollision (Vector3 centre, float radius)
    {
        Vector3 groundPoint = GetHighestGroundPoint(centre, radius);
        return GetHeightDelta(centre, radius, groundPoint);
    }


    static Vector3 GetHighestGroundPoint (Vector3 centre, float radius)
    {
        Vector3 point = new Vector3();

        return point;
    }

    static float GetHeightDelta (Vector3 center, float radius, Vector3 groundPoint)
    {
        return 0;
    }


	
}

