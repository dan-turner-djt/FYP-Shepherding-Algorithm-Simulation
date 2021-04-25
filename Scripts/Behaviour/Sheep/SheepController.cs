using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheepController : ActorController
{
    public static float radius = 0.45f;

    RandomMovementInfo randomInfo = new RandomMovementInfo();

    float lastTime;
    public static float maxSpeed = 5;
    float randomRestSpeed = 0.5f;
    float randomAcceleration = 10;
    float randomDeceleration = 5;

    public bool completed = false;
    public float distanceToGoal = 0;
    public float angleToGoal = 0;
    public float distanceFromSheepdog = 0;
    public float combinedMetric = 0;
    public SheepdogController fleeingSheepdog = null;


    private void Awake()
    {
        DoAwake();
        sphereCollider = GetComponent<SphereCollider>();
    }

    private void Start()
    {
        base.DoStart();

        switch (sc.algorithm)
        {
            case (SimulationSettings.SheepAlgorithm.Random):
                velocity = RandomSetFirstTimeInfo();
                break;
            case (SimulationSettings.SheepAlgorithm.Boids):
                velocity = Boids.SetFirstTimeInfo();
                break;
            default:
                break;
        }

        //velocity = Boids.SetFirstTimeInfo();
    }

   


    public override void DoPreLayerUpdate(float deltaTime, float simulationSpeed)
    {
        base.DoPreLayerUpdate(deltaTime, simulationSpeed);

        //FollowBehaviour(deltaTime);

        
        
        Vector3 pos = transform.position;
        pos.y = 0;
        Vector3 goalPos = sc.targetGate.transform.position;
        goalPos.y = 0;


       
        //set metrics

        distanceToGoal = (goalPos - pos).magnitude;
        distanceFromSheepdog = (ExtVector3.FlattenPosition(transform.position) - ExtVector3.FlattenPosition(SimulationSceneController.sheepDogs[0].transform.position)).magnitude;


        Vector3 targetBoxPos = sc.targetGate.transform.position;
        targetBoxPos.y = 0;
        Vector3 sheepPos = transform.position;
        sheepPos.y = 0;

        Vector3 targetPoint = Geometry.ClosestPointOnLineSegmentToPoint(sheepPos, targetBoxPos - 1 * Vector3.forward, targetBoxPos + 1 * Vector3.forward);
        Vector3 sheepToTarget = targetPoint - sheepPos;

        angleToGoal = Vector3.Angle (facingDir, sheepToTarget.normalized);

    }

    public override void DoPostLayerUpdate(float deltaTime, float simulationSpeed)
    {
        base.DoPostLayerUpdate(deltaTime, simulationSpeed);

        FollowBehaviour(deltaTime);

    }


    public override void DoPostUpdate (float deltaTime, float simulationSpeed)
    {
        base.DoPostUpdate(deltaTime, simulationSpeed);


        //check completion

        if (transform.position.x < sc.targetGate.transform.position.x)
        {
             completed = true;
        }
        else
        {
             completed = false;
        }
        
    }


    public override void FollowBehaviour(float deltaTime)
    {
        if (completed)
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, 0.2f * deltaTime);
            return;
        }

        switch (sc.algorithm)
        {
            case (SimulationSettings.SheepAlgorithm.Random):
                RandomBehaviour(deltaTime);
                break;
            case (SimulationSettings.SheepAlgorithm.Boids):
                BoidsBehaviour(deltaTime);
                break;
            case (SimulationSettings.SheepAlgorithm.PSO):
                //velocity = AvoidOtherSheep(velocity, deltaTime);
                break;
            default:
                break;
        } 

        
        velocity = AvoidWalls(velocity, deltaTime);

        facingDir = (velocity.magnitude > 0) ? velocity.normalized : facingDir;

        //avoid sheepdogs
        Vector3 vDelta = AvoidSheepdogs(velocity, sc.systemController.simulationSettings.sheepdogRepulsionDistance);
        velocity += vDelta * deltaTime;

        facingDir = (velocity.magnitude > 0) ? velocity.normalized : facingDir;

        if (velocity.magnitude > maxSpeed)
        {
            velocity = velocity.normalized * maxSpeed;
        }
    }

    public void RandomBehaviour (float deltaTime)
    {
        velocity = RandomMovement(deltaTime);
    }

    public void BoidsBehaviour (float deltaTime)
    {
        velocity = Boids.BoidsBehaviour(deltaTime, velocity, transform.position, ignoreColliders, sheepLayer);
    }

    void LineMovement (float deltaTime)
    {
        
        if (lastTime == 0)
        {
            lastTime = sc.simulationTime;
            facingDir = new Vector3 (-1, 0, 0).normalized;
        }

        if (sc.simulationTime >= lastTime + 2 && false)
        {
            facingDir = facingDir * -1;
            lastTime = sc.simulationTime;
        }

        float currentSpeed = velocity.magnitude;
        if (currentSpeed < 6)
        {
            currentSpeed = Mathf.Min(currentSpeed + 5 * deltaTime,  6);
        }
        velocity = currentSpeed * facingDir;


        //AvoidWalls(deltaTime);

        AvoidOtherActors(deltaTime);

        
    }

    Vector3 RandomSetFirstTimeInfo ()
    {
        Vector3 v = 0.01f * randomDir();
        facingDir = v.normalized;
        return v;
    }

    Vector3 RandomMovement (float deltaTime)
    {
        //first time random setup

        if (randomInfo.nextSpeedChange == 0)
        {
            newRandomSpeed();
        }

        if (randomInfo.nextDirChange == 0)
        {
            newRandomDir();
        }

        

        /*if (sc.simulationTime > randomInfo.nextSpeedChange)
        {
            //newRandomSpeed();
        }

        if (sc.simulationTime > randomInfo.nextDirChange)
        {
            //newRandomDir();
        }



        if (velocity.magnitude != randomInfo.targetSpeed)
        {
            float currentSpeed = velocity.magnitude;


            if (randomInfo.targetSpeed > currentSpeed)
            {
                currentSpeed = Mathf.Min (currentSpeed + 5 * deltaTime, randomInfo.targetSpeed);
            }

            else if (randomInfo.targetSpeed < currentSpeed)
            {
                currentSpeed = Mathf.Max(currentSpeed - 5 * deltaTime, randomInfo.targetSpeed);
            }

            velocity = facingDir * currentSpeed;
        }

        if (facingDir != randomInfo.targetDir)
        {
            facingDir = (Vector3.Lerp(facingDir, randomInfo.targetDir, 4 * deltaTime)).normalized;
            velocity = velocity.magnitude * facingDir;
        }

        Vector3 oldDir = facingDir;*/



        float speed = velocity.magnitude;

        if (fleeingSheepdog == null)
        {
            
            if (sc.simulationTime > randomInfo.nextSpeedChange)
            {
                newRandomSpeed();
            }

            if (sc.simulationTime > randomInfo.nextDirChange)
            {
                newRandomDir();
            }

            float targetSpeed = randomInfo.targetSpeed;
            Vector3 targetDir = randomInfo.targetDir;


            if (speed < randomRestSpeed)
            {
               
                speed = Mathf.Min(targetSpeed, speed + randomAcceleration * deltaTime);
                
            }
            else if (speed > randomRestSpeed)
            {
                speed = Mathf.Max(targetSpeed, speed - randomDeceleration * deltaTime);
            }

            velocity = facingDir * speed;

            if (facingDir != randomInfo.targetDir)
            {
                facingDir = (Vector3.Lerp(facingDir, randomInfo.targetDir, 4 * deltaTime)).normalized;
                velocity = velocity.magnitude * facingDir;
            }
        }


        velocity = facingDir * speed;


        //avoid other sheep
        //velocity = AvoidOtherSheep(velocity, deltaTime);
        List<SheepController> foundSheep = SimulationSceneController.findSheepInLocalArea(transform.position, 1, ignoreColliders, sheepLayer);
        velocity += Boids.Separation(velocity, foundSheep, transform.position, deltaTime) * deltaTime;


        return velocity;
    }


    public Vector3 AvoidOtherSheep(Vector3 velocity, float deltaTime)
    {
        
        LayerMask layer;
        float checkSize;

        layer = sheepLayer;
        checkSize = 3;
        

        IList<Collider> foundActors = new List<Collider>();
        ExtPhysics.OverlapSphere(transform.position, checkSize, ignoreColliders, foundActors, layer);

        Vector3 steering = new Vector3();

        int total = 0;
        Vector3 averagePoint = new Vector3();

        for (int i = 0; i < foundActors.Count; i++)
        {
            ActorController actor = foundActors[i].GetComponent<ActorController>();

            if (actor.transform.position != transform.position)
            {
                /*float distance = (transform.position - actor.transform.position).magnitude;
                Vector3 diff = transform.position - actor.transform.position;
                diff = diff / (distance * distance);
                averageVector += diff;
                total += 1;*/

                total += 1;
                averagePoint += actor.transform.position;

            }
        }

        /*if (total > 0)
        {
            averageVector /= total;

            if (averageVector.magnitude > 0)
            {
                averageVector = averageVector.normalized * maxSpeed;
            }

            steering = averageVector - velocity;


            if (steering.magnitude > 20)
            {
                steering = 20 * steering.normalized;
            }
        }*/

        if (total > 0)
        {
            averagePoint = averagePoint / total;
            Vector3 averageNormal = transform.position - averagePoint;
            averageNormal.y = 0;
            averageNormal = averageNormal.normalized;

            Vector3 vectortowards = transform.position - averagePoint;
            vectortowards.y = 0;
            float distance = (vectortowards).magnitude;

            float angleBetween = Vector3.SignedAngle(velocity.normalized, -averageNormal, Vector3.up);
            float turnDir;
            if (Mathf.Approximately(angleBetween, 0))
            {
                turnDir = Mathf.Sign(0.5f - Mathf.Max(0.01f, Random.value));
            }
            else
            {
                turnDir = Mathf.Sign(angleBetween);
            }


            if (Mathf.Approximately(velocity.magnitude, 0))
            {
                //if there is no velocity then we can't turn away
                velocity = 0.01f * facingDir;
            }

            float rot = Vector3.SignedAngle(velocity.normalized, Vector3.forward, Vector3.up);
            Vector2 perpDir2D = Vector2.Perpendicular(new Vector2(averageNormal.x, averageNormal.z)) * -turnDir;
            Vector3 perpDir = new Vector3(perpDir2D.x, 0, perpDir2D.y).normalized;
            float targetAngle = Vector3.SignedAngle(perpDir, Vector3.forward, Vector3.up);

            float velocityMod = 1 - (Mathf.Abs(angleBetween) / 90);
            float distanceMod = 1 - Mathf.Min(distance, checkSize) / checkSize;
            float turnSpeed = Mathf.Clamp(25 * distanceMod * distanceMod * velocityMod * velocityMod, 2, 20);
            targetAngle = rot + 50 * turnDir;

            rot = ExtVector3.CustomLerpAngle(rot, targetAngle, turnSpeed, deltaTime, 0.5f);

            velocity = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * rot), 0, Mathf.Cos(Mathf.Deg2Rad * rot)).normalized * velocity.magnitude;

            //Debug.Log("velocityDir: " + velocity.normalized + ", targetAngle: " + targetAngle + "," + ", turnDir: " + turnDir);
        }

        return velocity;
    }

    public Vector3 AvoidSheepdogs (Vector3 velocity, float sheepdogRepulsionDistance)
    {
        //repulsion steering

        LayerMask layer;
        float checkSize;

        
        fleeingSheepdog = null;
        layer = sheepDogLayer;
        checkSize = sheepdogRepulsionDistance;
        

        IList<Collider> foundActors = new List<Collider>();
        ExtPhysics.OverlapSphere(transform.position, checkSize, ignoreColliders, foundActors, layer);

        Vector3 totalSteering = new Vector3();
        Vector3 repulsionSteering = new Vector3();

        int total = 0;
        Vector3 averageVector = new Vector3();

        for (int i = 0; i < foundActors.Count; i++)
        {
            ActorController actor = foundActors[i].GetComponent<ActorController>();

            if (actor.transform.position != transform.position)
            {
                float distance = (transform.position - actor.transform.position).magnitude;
                Vector3 diff = transform.position - actor.transform.position;
                diff = diff / (distance*distance);
                averageVector += diff;
                total += 1;

            }
        }

        if (total > 0)
        {
            averageVector /= total;

            if (averageVector.magnitude > 0)
            {
                averageVector = averageVector.normalized * 15;
            }

            repulsionSteering = averageVector - velocity;


            if (repulsionSteering.magnitude > 20)
            {
                repulsionSteering = 20 * repulsionSteering.normalized;
            }
        }

        if (repulsionSteering != Vector3.zero)
        {
            fleeingSheepdog = foundActors[0].transform.GetComponent<SheepdogController>();
        }



       
        //COM steering

        layer = sheepLayer;
        checkSize = 4;

        Vector3 COMSteering = new Vector3();

        List<SheepController> foundSheep = SimulationSceneController.findSheepInLocalArea(transform.position, 4, ignoreColliders, sheepLayer);
        Vector3 centreOfMass = SimulationSceneController.FindCentreOfMassOfGlobalSheep(transform.position, false, foundSheep);

        if (total > 0)
        {
            Vector3 dirToCOM = (centreOfMass - transform.position).normalized;
            COMSteering = dirToCOM * 15;

            COMSteering = COMSteering - velocity;

            if (COMSteering.magnitude > 20)
            {
                COMSteering = COMSteering.normalized * 20;
            }
        }

        totalSteering = repulsionSteering + 0.4f * COMSteering;

        if (totalSteering.magnitude > 20)
        {
            totalSteering = totalSteering.normalized * 20;
        }


        return totalSteering;
    }

    public void SetCombinedMetric (float f)
    {
        combinedMetric = f;
    }

    public float randomSpeed ()
    {
        return Mathf.Max(1, 6 * Random.value);
    }

    public Vector3 randomDir()
    {
        return (new Vector3(1 * (1 - Random.value * 2), 0, 1 * (1 - Random.value * 2))).normalized;
    }

    void newRandomSpeed ()
    {
        randomInfo.targetSpeed = Mathf.Max(1, randomRestSpeed * Random.value);
        randomInfo.nextSpeedChange = sc.simulationTime + 4 * Random.value;
    }

    void newRandomDir ()
    {
        randomInfo.targetDir = (new Vector3(1 * (1 - Random.value * 2), 0, 1 * (1 - Random.value * 2))).normalized;
        randomInfo.nextDirChange = sc.simulationTime + 4 * Random.value;
    }

    struct RandomMovementInfo
    {
        public float nextDirChange;
        public float nextSpeedChange;
        public float targetSpeed;
        public Vector3 targetDir;

    }

}
