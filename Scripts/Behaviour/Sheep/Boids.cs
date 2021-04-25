using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids
{
    
    static float maxForce = 6;
    static float maxSpeed = 6;

    public static Vector3 SetFirstTimeInfo()
    {
        return randomSpeed() * randomDir();
    }


    public static Vector3 BoidsBehaviour(float deltaTime, Vector3 velocity, Vector3 position, List<Component> ignoreColliders, LayerMask actorsLayer)
    {
        Vector3 acceleration = new Vector3();


        List<SheepController> foundSheep = SimulationSceneController.findSheepInLocalArea(position, 8, ignoreColliders, actorsLayer);
        //IList<Collider> foundSheep = new List<Collider>();
        //ExtPhysics.OverlapSphere(position, 8, ignoreColliders, foundSheep, actorsLayer);


        acceleration += Align(velocity, foundSheep, position, deltaTime) * 0.6f;
        acceleration += Cohesion(velocity, foundSheep, position, deltaTime);
        acceleration += Separation(velocity, foundSheep, position, deltaTime);


        velocity += acceleration * deltaTime;

        if (velocity.magnitude > maxSpeed)
        {
            velocity = maxSpeed * velocity.normalized;
        }


        return velocity;
    }


    public static Vector3 Align (Vector3 velocity, List<SheepController> foundActors, Vector3 position, float deltaTime)
    {
        Vector3 steering = new Vector3();

        int total = 0;
        Vector3 averageVector = new Vector3();

        position = ExtVector3.FlattenPosition(position);


        for (int i = 0; i < foundActors.Count; i++)
        {
            SheepController actor = foundActors[i];
            Vector3 actorPos = ExtVector3.FlattenPosition(actor.transform.position);

            if (actor.completed)
            {
                continue;
            }

            if (actorPos != position)
            {
                averageVector += actor.velocity;
                total += 1;

            }
        }

        if (total > 0)
        {
            averageVector /= total;
            averageVector = averageVector.normalized * maxSpeed;

            steering = averageVector - velocity;
        }



        return steering;
    }


    public static Vector3 Cohesion(Vector3 velocity, List<SheepController> foundActors, Vector3 position, float deltaTime)
    {
        Vector3 steering = new Vector3();

        Vector3 centreOfMass = SimulationSceneController.FindCentreOfMassOfGlobalSheep(position, false, foundActors);


        if (centreOfMass != Vector3.zero)
        {
            Vector3 vecToCom = ExtVector3.FlattenPosition(centreOfMass - position);


            if (vecToCom.magnitude > 0)
            {
                vecToCom = vecToCom.normalized * maxSpeed;
            }

            steering = vecToCom - velocity;

            if (steering.magnitude > maxForce)
            {
                steering = steering.normalized * maxForce;
            }
        }



        return steering;
    }


    public static Vector3 Separation (Vector3 velocity, List<SheepController> foundActors, Vector3 position, float deltatime)
    {
        Vector3 steering = new Vector3();

        int total = 0;
        Vector3 averageVector = new Vector3();

        position = ExtVector3.FlattenPosition(position);


        for(int i = 0; i < foundActors.Count; i++)
        {
            SheepController actor = foundActors[i];
            Vector3 actorPos = ExtVector3.FlattenPosition(actor.transform.position);

            if (actor.completed)
            {
                continue;
            }

            if (actorPos != position)
            {
                float distance = (position - actorPos).magnitude;
                Vector3 diff = position - actorPos;
                diff = diff / distance;
                averageVector += diff;
                total += 1;

            }
        }

        if (total > 0)
        {
            averageVector /= total;


            if (averageVector.magnitude > 0)
            {
                averageVector = averageVector.normalized * maxSpeed;
            }

            steering = averageVector - velocity;

            if (steering.magnitude > maxForce)
            {
                steering = maxForce * steering.normalized;
            }
        }

        return steering;
    }


    public static float randomSpeed()
    {
        return Mathf.Max(1, 6 * Random.value);
    }

    public static Vector3 randomDir()
    {
        return (new Vector3(1 * (1 - Random.value * 2), 0, 1 * (1 - Random.value * 2))).normalized;
    }
}
