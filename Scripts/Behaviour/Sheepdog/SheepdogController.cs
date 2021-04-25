using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepdogController : ActorController
{
    public static float radius;

    public float distanceTravelled = 0;

    private void Awake()
    {
        DoAwake();

        sphereCollider = GetComponent<SphereCollider>();
    }


    private void Start()
    {
        base.DoStart();

        sc.AddSheepdogToList(this);
    }

    public override void DoPreLayerUpdate(float deltaTime, float simulationSpeed)
    {
        base.DoPreLayerUpdate(deltaTime, simulationSpeed);

    }

    public override void DoPostLayerUpdate(float deltaTime, float simulationSpeed)
    {
        base.DoPostLayerUpdate(deltaTime, simulationSpeed);

        if (Input.GetKey(KeyCode.Space))
        {
            PlayerTest(deltaTime);

        }
        else
        {

            FollowBehaviour(deltaTime);

        }
    }


    public override void DoPostUpdate(float deltaTime, float simulationSpeed)
    {
        base.DoPostUpdate(deltaTime, simulationSpeed);



        distanceTravelled = actualDistanceMoved;
    }


    protected void PlayerTest(float deltaTime)
    {
        //this is to move the object around for testing things, outside the simulation's control

        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        input = input.normalized;

        Vector3 transformedInput = Quaternion.FromToRotation(sc.cam.transform.up, Vector3.up) * (sc.cam.transform.rotation * input);
        transformedInput.y = 0.0f;
        input = transformedInput.normalized;

        if (input.magnitude > 0)
        {
            float speed = velocity.magnitude;
            velocity += input * 140 * deltaTime;
            speed += 40 * deltaTime;
            velocity = speed * velocity.normalized;

            if (velocity.magnitude > 12)
            {
                velocity = 12 * velocity.normalized;
            }
        }
        else
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, 40 * deltaTime);

            if (velocity.magnitude > 0.05f)
            {
                velocity = Vector3.zero;
            }
        }

    }

    public override void FollowBehaviour(float deltaTime)
    {
        Vector3 target = ConstructedBehaviour.DoBehaviour(this.gameObject, velocity, SimulationSceneController.sheep, sc.targetGate, sc.systemController.simulationSettings.sheepdogRepulsionDistance, sheepLayer, ignoreColliders, sc.systemController.simulationSettings.collectAmount, sc.systemController.simulationSettings.PHP, sc.systemController.simulationSettings.PHP2, deltaTime);

        //float speed = Mathf.Max (velocity.magnitude, 0.001f);
        float speed = velocity.magnitude;
        

        float targetSpeed = target.magnitude;

        if (speed < targetSpeed)
        {
            speed = Mathf.Min(targetSpeed, speed + 10f * deltaTime);
        }
        else if (speed > targetSpeed)
        {
            speed = Mathf.Max(targetSpeed, speed - 10f * deltaTime);
        }
        
        velocity = velocity.normalized * speed;
        facingDir = (speed > 0)? velocity.normalized : facingDir;

        //Vector3 newVelocity = ExtVector3.CustomControlledLerp(velocity.normalized, target.normalized, 16, 5, 16, deltaTime).normalized * speed;
        facingDir = ExtVector3.TurnDirection(facingDir, target.normalized, 8, deltaTime, Vector3.up).normalized;
        Vector3 newVelocity = facingDir * speed;
        float vMod = (Vector3.Lerp(velocity.normalized, target.normalized, 12 * deltaTime)).magnitude;
        //velocity = newVelocity.normalized * Mathf.Max (newVelocity.magnitude * vMod, 2);

        velocity = newVelocity.normalized * newVelocity.magnitude * vMod;
       


        //velocity = AvoidWalls(velocity, deltaTime);
    }
}
