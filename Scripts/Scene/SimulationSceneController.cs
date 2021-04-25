using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationSceneController : GeneralSceneController
{

    CameraController cameraController;
    public HUDController hudController;
    
    public GameObject spawnBox;
    public GameObject targetBox;
    public GameObject targetGate;
    public Camera cam;
    public GameObject HUDObject;
    public GameObject sheepPrefab;

    public int simulationRunning = 1;
    public float simulationSpeed = 1;
    public bool simulationFinished = false;

    public static List<ActorController> actors = new List<ActorController>();
    public static List<SheepController> sheep = new List<SheepController>();
    public static List<SheepdogController> sheepDogs = new List<SheepdogController>();

    public const int maxVelocitySteps = 20; //A safety in case we are moving very fast we dont want to divide our velocity into to many steps since that can cause lag and freeze the simulation, so we prefer to have the collision be unsafe.

    public float simulationTime;

    public SimulationSettings.SheepAlgorithm algorithm;

    float runningTime = 0;
    float totalDistanceTravelled = 0;

    public bool isLearning;
    public static bool isWatching = true;

    public static float timeSpentCollecting = 0;

    public LayerMask actorsLayer;

    void Awake()
    {
        base.DoAwake();

        actors.Clear();
        sheep.Clear();
        sheepDogs.Clear();
        timeSpentCollecting = 0;
    }

    private void Start()
    {
        base.DoStart();

        UIStackManager.AddComponentToStack(HUDObject);

        cameraController = cam.GetComponentInParent<CameraController>();

        List<SheepController> newActors = SpawnSheep();
        sheep.AddRange(newActors);
        actors.AddRange(sheep);

        algorithm = systemController.simulationSettings.sheepAlgorithm;

        if (algorithm == SimulationSettings.SheepAlgorithm.PSO)
        {
            PSO.Initalize(sheep);
        }

        isLearning = systemController.simulationSettings.isLearning;

        hudController.DoStart();

        simulationSpeed = systemController.simulationSettings.simulationSpeed;
        hudController.UpdateSpeedButton();


        //this has to be done after PSO initialises to make sure PSO begins with the default y pos
        /*foreach (ActorController actor in sheep)
        {
            //actor.transform.position = actor.GroundCollision(actor.transform.position);
        }
        Physics.SyncTransforms();*/

        if (isLearning)
        {
            hudController.ActivateWatchButton();

            if (isWatching)
            {
                ChangeToWatch();
            }
            else
            {
                ChangeToSpeedUp();
            }

            if (systemController.simulationSettings.currentIteration == 0)
            {
                SetDefaultParameters();
            }

        }

    }


    List<SheepController> SpawnSheep()
    {
        List<SheepController> spawned = new List<SheepController>();
        List<Vector3> spawnCoords = new List<Vector3>();  

        switch (systemController.simulationSettings.spawnType)
        {
            case (SimulationSettings.SpawnType.Random):
                spawnCoords = Spawn.GetRandomSheepCoords(systemController.simulationSettings.flockSize, spawnBox);
                break;
            case (SimulationSettings.SpawnType.Circular):
                spawnCoords = Spawn.GetCircularSheepCoords(systemController.simulationSettings.flockSize, spawnBox);
                break;
            default:
                break;
        }

        foreach (Vector3 coords in spawnCoords)
        {
            GameObject sheep = Instantiate(sheepPrefab, coords, Quaternion.identity);
            spawned.Add(sheep.GetComponent<SheepController>());
        }

        

        Physics.SyncTransforms();

        return spawned;
    }



    private void Update()
    {
        base.DoUpdate();


        //allow herding algorithm changes from keyboard for testing

        if (Input.GetKeyDown (KeyCode.M))
        {
            ConstructedBehaviour.ToggleMetric();
        }

        if (Input.GetKeyDown (KeyCode.O))
        {
            ConstructedBehaviour.ToggleAscending();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ConstructedBehaviour.ToggleAllowTargetChange();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            ConstructedBehaviour.TogglePickSheepTargets();
        }



        if (!simulationFinished)
        {
            float deltaTime = Time.deltaTime;

            if (simulationRunning > 0)
            {
                for (int i = 0; i < simulationSpeed; i++)
                {
                    MainSimulationLoop(deltaTime);
                }


                runningTime += Time.deltaTime * simulationRunning * simulationSpeed;

                //update distance travelled
                foreach (SheepdogController sheepDog in sheepDogs)
                {
                    totalDistanceTravelled += sheepDog.distanceTravelled;
                }


                int objectiveProgress = CheckObjectiveProgress();


                float time = runningTime;
                float distance = totalDistanceTravelled;
                int progress = objectiveProgress;
                int totalSheep = systemController.simulationSettings.flockSize;

                hudController.UpdateInfo(time, distance, progress, totalSheep);


                if (progress == totalSheep)
                {
                    FinishSimulation();
                }

                bool stopAfterTooLong = true;
                if (stopAfterTooLong && runningTime > 300)
                {
                    //got stuck, didn't work, so finish
                    runningTime *= 100;
                    totalDistanceTravelled *= 100;
                    FinishSimulation();
                }
            }


        }

       
    }

    private void LateUpdate()
    {
        cameraController.DoFinalUpdate();
    }

    public int CheckObjectiveProgress ()
    {
        BoxCollider col = targetBox.GetComponent<BoxCollider>();
        Collider[] results = Physics.OverlapBox(targetBox.transform.position, col.bounds.size/2, targetBox.transform.rotation, actorsLayer);

        return results.Length;
    }


    public override bool CanRemoveBottomMenu(GameObject menu)
    {
        return false;
    }


    public void TogglePause ()
    {
        if (simulationRunning == 0)
        {
            simulationRunning = 1;
        }
        else
        {
            simulationRunning = 0;
        }
    }

    public void ChangeSpeed ()
    {
        if (simulationSpeed == 0.5f)
        {
            simulationSpeed = 1f;
        }
        else
        {
            simulationSpeed *= 2f;
        }

        if (simulationSpeed > 16)
        {
            simulationSpeed = 0.5f;
        }

        systemController.simulationSettings.simulationSpeed = simulationSpeed;
    }

    public void ChangeCameraMode ()
    {
        cameraController.ChangeCameraMode();
    }


    public void RestartSimulation ()
    {
        systemController.simulationSettings.currentIteration = 0;
        systemController.ChangeScene("SimulationTest");
    }


    public void NextLearningIteration ()
    {
        systemController.simulationSettings.currentIteration++;
        systemController.ChangeScene("SimulationTest");
    }
   
    public void ExitSimulation ()
    {
        systemController.simulationSettings.currentIteration = 0;
        systemController.LeaveSimulation();
    }



    void MainSimulationLoop (float deltaTime)
    {
        if (simulationSpeed == 0.5f)
        {
            deltaTime = deltaTime / 2;
        }

        simulationTime += simulationSpeed * deltaTime / simulationSpeed;

        //first update
        foreach (ActorController actor in actors)
        {
            actor.DoPreLayerUpdate(deltaTime, simulationSpeed);
        }

        //move onto same layer
        List<float> savedVerticals = new List<float>();
        foreach (ActorController actor in actors)
        {
            Vector3 pos = actor.transform.position;
            savedVerticals.Add(pos.y);
            actor.transform.position = new Vector3(pos.x, spawnBox.transform.position.y, pos.z);
        }
        Physics.SyncTransforms();


        //do behaviour

        if (algorithm == SimulationSettings.SheepAlgorithm.PSO)
        {
            if (sheep.Count > 0)
            {
                PSO.DoPSO(deltaTime, sheep, sheep[0].ignoreColliders, sheep[0].sheepLayer);
            }
            
        }



        //second update
        foreach (ActorController actor in sheep)
        {
            actor.DoPostLayerUpdate(deltaTime, simulationSpeed);
        }
        foreach (ActorController actor in sheepDogs)
        {
            actor.DoPostLayerUpdate(deltaTime, simulationSpeed);
        }

        //restore to original height
        for (int i = 0; i < savedVerticals.Count; i++)
        {
            Vector3 pos = actors[i].transform.position;
            actors[i].transform.position = new Vector3(pos.x, savedVerticals[i], pos.z);
        }
        Physics.SyncTransforms();


        ////////////////////// Collision Stuff ///////////////////

        foreach (ActorController actor in actors)
        {
            actor.PrepareForCollision(deltaTime);
        }

        float biggestDistance = 0;
        if (actors.Count == 1)
        {
            biggestDistance = actors[0].collisionVelocity.magnitude;
        }
        else
        {
            foreach (var actor in actors)
            {
                //cant condense this into previous loop as collision velocity must be set for everyone first
                Vector3 moveVector = actor.collisionVelocity;

                //loop through and add distances together to see which pair is the greatest
                foreach (var newActor in actors)
                {
                    //if the same
                    if (newActor == actor)
                    {
                        continue;
                    }
                    else
                    {
                        Vector3 otherMoveVector = newActor.collisionVelocity;
                        float combinedDistance = (moveVector - otherMoveVector).magnitude;

                        //find what the fastest is
                        if (moveVector.magnitude > biggestDistance)
                        {
                            biggestDistance = moveVector.magnitude;
                        }
                        if (combinedDistance > biggestDistance)
                        {
                            biggestDistance = combinedDistance;
                        }
                    }
                }
            }
        }


        //calculate time step

        //We cut our velocity up into steps so that we never move more than a certain amount of our radius per step.
        //This prevents tunneling and acts as a "Continuous Collision Detection", but is not as good as using a CapsuleCast.
        float maxRadiusMove = 0.5f / 3;
        int steps = 1;

        if (biggestDistance > maxRadiusMove)
        {
            steps = Mathf.CeilToInt(biggestDistance / maxRadiusMove);
            if (steps > maxVelocitySteps)
            {
                steps = maxVelocitySteps;

            }
        }

        //Debug.Log(biggestDistance);
        //Debug.Log (steps);

        //calculate each's step velocity before we begin
        foreach (var actor in actors)
        {
            actor.SetStepVelocity(steps);
        }

        List<ActorController> actorsToUpdate = new List<ActorController>();
        actorsToUpdate.AddRange(actors);
        Physics.SyncTransforms();

        //do collision updates
        for (int i = 0; i < steps; i++)
        {
            bool allCollisionsSuccessful = true;
            int counter = 0;

            do
            {
                allCollisionsSuccessful = true;

                //then do collision and update for depenetratables
                foreach (var actor in actorsToUpdate)
                {
                    bool collisionSuccessful = actor.DoCollisionUpdate(deltaTime, actor.collisionInfo.stepVelocity);

                    if (!collisionSuccessful)
                    {
                        //Debug.Log("someones collision failed!!");
                        allCollisionsSuccessful = false;

                    }

                    Physics.SyncTransforms();
                }

                counter++;
            }
            while (!allCollisionsSuccessful && counter < 1);
            
            if (!allCollisionsSuccessful)
            {
                Debug.Log("safe loop failed");
            }
        }


        foreach (ActorController actor in actors)
        {
            actor.FinalizeAfterCollision(deltaTime);
        }

        foreach (ActorController actor in actors)
        {
            actor.DoPostUpdate(deltaTime, simulationSpeed);
        }
    }


    public void FinishSimulation ()
    {
        simulationFinished = true;

        if (isLearning)
        {
            SimulationSettings.AItotalTime += runningTime;
            SimulationSettings.AItotalDistance += totalDistanceTravelled;

            Debug.Log("total: " + SimulationSettings.AItotalTime + ", " + SimulationSettings.AItotalDistance);

            if ((systemController.simulationSettings.currentIteration+1) % systemController.simulationSettings.numForAverage == 0)
            {
                float averageRunningTime = SimulationSettings.AItotalTime / systemController.simulationSettings.numForAverage;
                float averageTotalDistanceTravelled = SimulationSettings.AItotalDistance / systemController.simulationSettings.numForAverage;

                SaveSimulationResultsWithParams(averageRunningTime, averageTotalDistanceTravelled);

                SimulationSettings.AItotalTime = 0;
                SimulationSettings.AItotalDistance = 0;

                ChooseNewParameters();
            }

            if (systemController.simulationSettings.currentIteration == systemController.simulationSettings.maxIterations-1)
            {
                Debug.Log("COMPLETE!");
            }
            else
            {
                NextLearningIteration();
            }

            
        }
        else
        {
            hudController.ActivateSaveResultsButton();
        }

        
    }

    void SetDefaultParameters ()
    {
        systemController.simulationSettings.collectAmount = systemController.simulationSettings.defaultCollectAmount;
        systemController.simulationSettings.PHP = systemController.simulationSettings.defaultPHP;
    }

    void ChooseNewParameters()
    {
        systemController.simulationSettings.collectAmount = systemController.simulationSettings.defaultCollectAmount + (Random.value - 0.5f) * 4f;
        systemController.simulationSettings.PHP = systemController.simulationSettings.defaultPHP + (Random.value - 0.5f) * 0.5f;
        systemController.simulationSettings.PHP2 = systemController.simulationSettings.defaultPHP2 + (Random.value - 0.5f) * 1;

    }

    public static List<SheepController> findSheepInLocalArea(Vector3 position, float checkSize, List<Component> ignoreColliders, LayerMask sheepLayer)
    {
        List<SheepController> foundSheep = new List<SheepController>();

        IList<Collider> foundActors = new List<Collider>();
        ExtPhysics.OverlapSphere(position, checkSize, ignoreColliders, foundActors, sheepLayer);

        foreach (Collider col in foundActors)
        {
            SheepController currentSheep = col.transform.GetComponent<SheepController>();

            if (currentSheep.completed)
            {
                continue;
            }

            foundSheep.Add(currentSheep);
        }

        return foundSheep;
    }


    public static Vector3 FindCentreOfMassOfGlobalSheep(Vector3 myPos, bool includeSelf, List<SheepController> foundSheep)
    {
        Vector3 centreOfMass = new Vector3();

        int total = 0;

        for (int i = 0; i < foundSheep.Count; i++)
        {
            SheepController actor = foundSheep[i];

            if (actor.completed)
            {
                continue;
            }

            Vector3 actorPos = ExtVector3.FlattenPosition(actor.transform.position);
            myPos = ExtVector3.FlattenPosition(myPos);

            if (!includeSelf && myPos == actorPos)
            {
                continue;
            }

            centreOfMass += actorPos;
            total += 1;

        }

        if (total > 0)
        {
            centreOfMass = centreOfMass / total;
        }

        return centreOfMass;
    }

    public static float EvaluateSheepToCOM (Vector3 centreOfMass, List<SheepController> foundSheep)
    {
        float averageDist = 0;

        if (foundSheep.Count == 0)
        {
            return 0;
        }

        foreach (SheepController currentSheep in foundSheep)
        {
            if (currentSheep.completed)
            {
                continue;
            }

            Vector3 sheepPos = ExtVector3.FlattenPosition (currentSheep.transform.position);
            Vector3 COMToSheep = sheepPos - centreOfMass;
            float mag = COMToSheep.magnitude;
            averageDist += mag;
        }

        averageDist = averageDist / foundSheep.Count;

        return averageDist;
    }

    public static bool CheckIsSheepFurtherThanMaxCOMDist(Vector3 centreOfMass, List<SheepController> foundSheep, float collectAmount)
    {
        
        foreach (SheepController currentSheep in foundSheep)
        {
            if (currentSheep.completed)
            {
                continue;
            }

            Vector3 sheepPos = ExtVector3.FlattenPosition(currentSheep.transform.position);
            Vector3 COMToSheep = sheepPos - centreOfMass;
            float mag = COMToSheep.magnitude;
            
            if (mag > collectAmount)
            {
                return true;
            }
        }

        return false;
    }

    public static float FindFurthestSheepDistFromCOM(Vector3 centreOfMass, List<SheepController> foundSheep)
    {
        float furthestDist = 0;

        foreach (SheepController currentSheep in foundSheep)
        {
            if (currentSheep.completed)
            {
                continue;
            }

            Vector3 sheepPos = ExtVector3.FlattenPosition(currentSheep.transform.position);
            Vector3 COMToSheep = sheepPos - centreOfMass;
            float mag = COMToSheep.magnitude;

            if (mag > furthestDist)
            {
                furthestDist = mag;
            }
        }

        return furthestDist;
    }



    public void SaveSimulationResults ()
    {
        systemController.WriteSimulationResultsToFile(systemController.simulationSettings.settingsName, runningTime, totalDistanceTravelled, 0);
    }

    public void SaveSimulationResultsWithParams(float rt, float tdt)
    {
        float CMD = systemController.simulationSettings.collectAmount;
        float PHP = systemController.simulationSettings.PHP;
        float PHP2 = systemController.simulationSettings.PHP2;
        systemController.WriteSimulationResultsToFileWithParams (systemController.simulationSettings.settingsName, rt, tdt, 0, CMD, PHP, PHP2);

        Debug.Log("SAVED: " + rt + ", " + tdt);
    }

    public void AddToTimeSpentCollecting(float deltaTime)
    {
        timeSpentCollecting += simulationSpeed * deltaTime / simulationSpeed;
    }

    public void AddSheepdogToList (SheepdogController actor)
    {
        actors.Add(actor);
        sheepDogs.Add(actor);

    }

    public void ToggleIsWatching ()
    {
        isWatching = !isWatching;
    }

    public void ChangeToWatch ()
    {
        MeshRenderer[] mrs = FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];

        foreach (MeshRenderer mr in mrs)
        {
            mr.enabled = true;
        }

        simulationSpeed = 1;
        hudController.UpdateSpeedButton();
    }


    public void ChangeToSpeedUp ()
    {
        MeshRenderer[] mrs = FindObjectsOfType(typeof(MeshRenderer)) as MeshRenderer[];

        foreach (MeshRenderer mr in mrs)
        {
            mr.enabled = false;
        }

        simulationSpeed = 64;
        hudController.UpdateSpeedButton();
    }
}
