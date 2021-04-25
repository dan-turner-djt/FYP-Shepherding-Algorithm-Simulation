using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PSO : MonoBehaviour
{
    static float W = 0.99999999f;
    static float c1 = 0.7f;
    static float c2 = 0.5f;
    static float target = 3;

    static float target_error = 4;

    static List <Vector3> pbest_position = new List<Vector3>();
    static List<float> pbest_fitness_value = new List<float>();
    static Vector3 gbest_position;
    static float gbest_fitness_value;

    static Vector3 targetPosition;

    public static void Initalize (List<SheepController> actors)
    {
        pbest_fitness_value = Enumerable.Repeat(Mathf.Infinity, actors.Count).ToList();
        gbest_fitness_value = Mathf.Infinity;
        gbest_position = new Vector3(Mathf.Infinity, 0, Mathf.Infinity);


        for (int i = 0; i < actors.Count; i++)
        {
            pbest_position.Add (actors[i].transform.position);
            actors[i].velocity = Boids.randomDir() * Boids.randomSpeed();
        }

    }


    public static void DoPSO(float deltaTime, List<SheepController> actors, List<Component> ignoreColliders, LayerMask actorsLayer)
    {
        float averageFitness = 0;

        Vector3 centreOfMass = SimulationSceneController.FindCentreOfMassOfGlobalSheep(Vector3.zero, true, SimulationSceneController.sheep);
        targetPosition = centreOfMass;

        //targetPosition = new Vector3(6, 0, -14);

        int count = 0;
        gbest_fitness_value = fitness_function(gbest_position, targetPosition);

        for (int i = 0; i < actors.Count; i++)
        {
            if (actors[i].completed)
            {
                continue;
            }


            count++;

            Vector3 myPos = ExtVector3.FlattenPosition(actors[i].transform.position);
            float fitness_candidate = fitness_function(myPos, targetPosition);

            pbest_fitness_value[i] = fitness_function(pbest_position[i], targetPosition);
            if (fitness_candidate < pbest_fitness_value[i])
            {
                pbest_fitness_value[i] = fitness_candidate;
                pbest_position[i] = myPos;
            }


            if (fitness_candidate < gbest_fitness_value)
            {
                gbest_fitness_value = fitness_candidate;
                gbest_position = myPos;
            }

            averageFitness += fitness_candidate;

        }

        if (count > 0)
        {
            averageFitness = averageFitness / actors.Count;
        }
        

        //Debug.Log(averageFitness);

        if (Mathf.Abs(gbest_fitness_value - target) < target_error)
        {
            //return;
            /*for (int i = 0; i < actors.Count; i++)
            {
                actors[i].velocity = Vector3.MoveTowards (actors[i].velocity, Vector3.zero, 5 * deltaTime);
            }
            return;*/
        }

        if (Mathf.Abs(gbest_fitness_value - target) < target_error)
        {
            /*//return;
            for (int i = 0; i < actors.Count; i++)
            {
                actors[i].velocity = Vector3.MoveTowards (actors[i].velocity, Vector3.zero, 5 * deltaTime);
            }
            return;*/
        }


        for (int i = 0; i < actors.Count; i++)
        {
            if (actors[i].completed)
            {
                continue;
            }

            Vector3 myPos = ExtVector3.FlattenPosition(actors[i].transform.position);
            float maxDelta = 3;
            Vector3 dp = pbest_position[i] - myPos;
            Vector3 cappedDp = dp.normalized * Mathf.Min (dp.magnitude, maxDelta);
            Vector3 dg = gbest_position - myPos;
            Vector3 cappedDg = dg.normalized * Mathf.Min (dg.magnitude, maxDelta);
            Vector3 new_velocity = (W * actors[i].velocity) + ((c1 * Random.value) * cappedDp + (c2 * Random.value) * cappedDg) * deltaTime;
            new_velocity.y = 0;

            List<SheepController> nearSheep = SimulationSceneController.findSheepInLocalArea(actors[i].transform.position, 1, ignoreColliders, actorsLayer);
            new_velocity += Boids.Separation(new_velocity, nearSheep, actors[i].transform.position, deltaTime) * deltaTime;
            new_velocity.y = 0;

            actors[i].velocity = new_velocity;

            
        }


    }


    static float fitness_function (Vector3 position, Vector3 targetPosition)
    {
        float dx = targetPosition.x;
        float dz = targetPosition.z;

        return (position.x - dx) * (position.x - dx) + (position.z - dz) * (position.z - dz);
    }

    static bool IsSheepCloseEnough (Vector3 centreOfMass, SheepController currentSheep)
    {
        List<SheepController> s = new List<SheepController>();
        s.Add(currentSheep);
        if (SimulationSceneController.EvaluateSheepToCOM(centreOfMass, s) < 3)
        {
            return true;
        }

        return false;
    }

    
}
