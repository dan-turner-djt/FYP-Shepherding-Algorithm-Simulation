using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Spawn
{
    public static List<Vector3> GetCircularSheepCoords (int flockSize, GameObject spawnBox)
    {
        List<Vector3> spawnCoords = new List<Vector3>();

        Vector3 centre = spawnBox.transform.position;
        spawnCoords.Add(centre);

        int remaining = flockSize - 1;
        int sheepPerRing = 8;

        int counter = 0;
        while (remaining > 0)
        {
            sheepPerRing += counter * 2;

            if (sheepPerRing > remaining)
            {
                sheepPerRing = remaining;
            }

            
            float angle = 360 / sheepPerRing;
            float radius = (counter+1) * 2;

            

            for (int j = 0; j < sheepPerRing; j++)
            {
                float newAngle = angle * j;
                Vector3 pos = centre + Vector3.right * radius * Mathf.Cos(newAngle * Mathf.Deg2Rad) + Vector3.forward * radius * Mathf.Sin(newAngle * Mathf.Deg2Rad);
                spawnCoords.Add(pos);
            }

            remaining -= sheepPerRing;
            counter++;
        }

        


        return spawnCoords;
    }


    public static List<Vector3> GetRandomSheepCoords(int flockSize, GameObject spawnBox)
    {
        List<Vector3> spawnCoords = new List<Vector3>();
        Vector3 centre = spawnBox.transform.position;
        Collider col = spawnBox.GetComponent<BoxCollider>();
        
        for (int i = 0; i < flockSize; i++)
        {
            bool acceptable = true;
            Vector3 coords = new Vector3();

            do
            {
                acceptable = true;

                coords.x = centre.x - col.bounds.size.x / 2 + Random.value * col.bounds.size.x;
                coords.z = centre.z - col.bounds.size.z / 2 + Random.value * col.bounds.size.z;
                coords.y = centre.y;

                for (int j = 0; j < spawnCoords.Count; j++)
                {
                    Vector3 otherCoords = spawnCoords[j];

                    if ((otherCoords - coords).magnitude < 2)
                    {
                        acceptable = false;
                        break;
                    }
                }
            }
            while (!acceptable);

            spawnCoords.Add(coords);
        }


        return spawnCoords;
    }
}
