using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ConstructedBehaviour 
{
    static SheepController currentSheep;

    static public bool pickSheepTargets = false;
    static public bool allowTargetChange = false;
    static public bool orderAscending = false;
    static public bool orderByDistance = true; //false = order by direction
    static public int COMamount = -1;

    public static Vector3 DoBehaviour (GameObject sheepdog, Vector3 velocity, List<SheepController> sheep, GameObject targetBox, float sheepdogRepulsionDistance, LayerMask layer, List<Component> ignoreColliders, float collectAmount, float CSA, float PHP2, float deltaTime)
    {
        LayerMask collisionLayers = ActorController.collisionLayers;

        Vector3 sheepdogPos = ExtVector3.FlattenPosition(sheepdog.transform.position);

        Vector3 result = new Vector3();

        if (sheep.Count == 0)
        {
            return result;
        }



        if (pickSheepTargets)
        {
            //choose sheep targets

            bool shouldChangeTarget = false;

            if (allowTargetChange)
            {
                shouldChangeTarget = true;
            }
            else
            {
                shouldChangeTarget = currentSheep == null || currentSheep.completed;
            }

            if (shouldChangeTarget)
            {
                ChooseNextSheepSimple(sheep);
            }


            Vector3 sheepPos = currentSheep.transform.position;
            sheepPos.y = 0;


            Vector3 sheepToTarget = GetDirFromFollowTargetToTargetPos(targetBox, sheepPos);
            result = PushSheepTowardsTarget(sheepPos, sheepdogPos, sheepToTarget, sheepdogRepulsionDistance, velocity, layer, ignoreColliders, collisionLayers);

        }
        else
        {


            Vector3 centreOfMass = Vector3.zero;
            List<SheepController> sheepToCheck = new List<SheepController>();
            
            if (COMamount < 0)
            {
                //use all sheep
                sheepToCheck.AddRange(sheep);
                centreOfMass = SimulationSceneController.FindCentreOfMassOfGlobalSheep(Vector3.zero, true, sheepToCheck);
            }
            else
            {
                //use local area sheep
                List<SheepController> foundSheep = SimulationSceneController.findSheepInLocalArea(sheepdog.transform.position, COMamount, ignoreColliders, layer);
                sheepToCheck.AddRange(foundSheep);
                centreOfMass = SimulationSceneController.FindCentreOfMassOfGlobalSheep(sheepdog.transform.position, true, sheepToCheck);
            }

            bool currentlyCollecting = SimulationSceneController.CheckIsSheepFurtherThanMaxCOMDist(centreOfMass, sheepToCheck, collectAmount);


            if (sheepdog.transform.position.x < targetBox.transform.position.x)
            {
                //somehow got stuck in target box so move to get out

                result = targetBox.transform.position + 1 * Vector3.right;
                result.y = 0;
            }

            else if (currentlyCollecting)
            {
                //choose sheep targets

                bool shouldChangeTarget = false;

                if (allowTargetChange)
                {
                    shouldChangeTarget = true;
                }
                else
                {
                    if (currentSheep == null || currentSheep.completed)
                    {
                        shouldChangeTarget = true;
                    }
                    else
                    {
                        Vector3 sheepToCOM = centreOfMass - ExtVector3.FlattenPosition(currentSheep.transform.position);
                        //Debug.Log(sheepToCOM.magnitude);

                        float furthestSheepDistFromCom = SimulationSceneController.FindFurthestSheepDistFromCOM(centreOfMass, sheepToCheck);
                        float furthestExtraDist = furthestSheepDistFromCom - collectAmount;

                        float comparedAmount = (collectAmount) + CSA * (furthestExtraDist / 2);

                        if (sheepToCOM.magnitude < comparedAmount)
                        {
                            shouldChangeTarget = true;
                        }
                    }
                }

                if (shouldChangeTarget)
                {
                    ChooseNextSheepAdvanced(sheep, PHP2);
                    //ChooseNextSheepSimple(sheep);
                }

                Vector3 sheepPos = currentSheep.transform.position;
                sheepPos.y = 0;

                Vector3 sheepToTarget = centreOfMass - sheepPos;
                result = PushSheepTowardsTarget(sheepPos, sheepdogPos, sheepToTarget, sheepdogRepulsionDistance, velocity, layer, ignoreColliders, collisionLayers);

                if (sheep.Count > 0)
                {
                    sheep[0].sc.AddToTimeSpentCollecting(deltaTime);
                }
            }
            else
            {
                //push the flock toward the goal

                Vector3 targetPos = centreOfMass - (sheepdogRepulsionDistance + SheepdogController.radius - 2.5f) * GetDirFromFollowTargetToTargetPos(targetBox, centreOfMass).normalized;
                result = targetPos;

                //Debug.Log("pushing");
            }

           

        } 


        Vector3 sheepdogToTarget = result - sheepdogPos;
        float targetSpeed = 10;
        targetSpeed = Mathf.Clamp(sheepdogToTarget.magnitude * 6, 0, 10);
        targetSpeed = Mathf.Clamp(targetSpeed, 0, 10);
        result = (sheepdogToTarget).normalized * targetSpeed;



        return result;
    }


    static void ChooseNextSheepSimple (List<SheepController> sheep)
    {
        //order them in desired fashion

        if (orderByDistance)
        {
            //order them in closest to goal order
            sheep = sheep.OrderBy(o => o.distanceToGoal).ToList();
        }
        else
        {
            //order them in closest to goal order
            sheep = sheep.OrderBy(o => o.angleToGoal).ToList();
        }

        if (!orderAscending)
        {
            sheep.Reverse();
        }


        for (int i = 0; i < sheep.Count; i++)
        {
            if (sheep[i].completed)
            {
                continue;
            }

            currentSheep = sheep[i];
            break;
        }
    }

    static void ChooseNextSheepAdvanced(List<SheepController> sheep, float CDC)
    {
        //order them in desired fashion

        for (int i = 0; i < sheep.Count; i++)
        {
            if (sheep[i].completed)
            {
                continue;
            }

            float combinedMetric = (Mathf.Pow(sheep[i].distanceToGoal, CDC)) - sheep[i].distanceFromSheepdog;
            sheep[i].SetCombinedMetric(combinedMetric);
        }


        sheep = sheep.OrderBy(o => o.combinedMetric).ToList();

        if (!orderAscending)
        {
            sheep.Reverse();
        }


        for (int i = 0; i < sheep.Count; i++)
        {
            if (sheep[i].completed)
            {
                continue;
            }

            currentSheep = sheep[i];
            break;
        }
    }



    static Vector3 GetDirFromFollowTargetToTargetPos (GameObject targetBox, Vector3 followTargetPos)
    {
        Vector3 targetBoxPos = targetBox.transform.position;
        targetBoxPos.y = 0;

        Vector3 targetPoint = Geometry.ClosestPointOnLineSegmentToPoint(followTargetPos, targetBoxPos - 1 * Vector3.forward, targetBoxPos + 1 * Vector3.forward);
        Vector3 dirToTarget = targetPoint - followTargetPos;

        return dirToTarget;
    }

    static Vector3 PushSheepTowardsTarget (Vector3 sheepPos, Vector3 sheepdogPos, Vector3 dirToTarget, float sheepdogRepulsionDistance, Vector3 sheepdogVelocity, LayerMask layer, List<Component> ignoreColliders, LayerMask collisionLayers)
    {
        Vector3 sheepToTarget = dirToTarget;
        Vector3 sheepdogToSheep = sheepPos - sheepdogPos;

        Vector3 sheepDogTargetPos;

        if (ExtVector3.MagnitudeInDirection(-sheepdogToSheep.normalized, sheepToTarget.normalized) > 0.1f)
        {
            //if we are still between sheep and target


            Vector2 perpDir2D = Vector2.Perpendicular(new Vector2(sheepToTarget.x, sheepToTarget.z));
            Vector3 perpDir = new Vector3(perpDir2D.x, 0, perpDir2D.y).normalized;
            Vector3 sheepToTargetPerpDir = perpDir;

            //Debug.DrawRay(sheepPos, sheepToTargetPerpDir * 5, Color.red);

            Vector3 point1 = sheepPos + sheepToTargetPerpDir * (sheepdogRepulsionDistance + SheepdogController.radius + 0.001f);
            Vector3 point2 = sheepPos - sheepToTargetPerpDir * (sheepdogRepulsionDistance + SheepdogController.radius + 0.001f);

            sheepDogTargetPos = ((sheepdogPos - point1).magnitude < (sheepdogPos - point2).magnitude) ? point1 : point2;

            RaycastHit hit;
            if (Physics.Raycast(sheepPos, (sheepDogTargetPos - sheepPos).normalized, out hit, (sheepDogTargetPos - sheepPos).magnitude + SheepdogController.radius, collisionLayers))
            {
                sheepDogTargetPos = sheepPos - (sheepDogTargetPos - sheepPos).normalized * (hit.distance - SheepdogController.radius);
            }
        }
        else
        {
            //if we passed the sheep and are behind it

            float sheepBuffer = sheepdogRepulsionDistance + SheepdogController.radius - 2.5f;
            sheepDogTargetPos = sheepPos - sheepToTarget.normalized * (sheepBuffer);
            Vector3 sheepDogTargetPosB = sheepDogTargetPos;


            RaycastHit hit;
            if (Physics.Raycast(sheepPos, -sheepToTarget, out hit, (sheepDogTargetPos - sheepPos).magnitude + SheepdogController.radius, collisionLayers))
            {
                sheepDogTargetPos = sheepPos - sheepToTarget.normalized * (hit.distance - SheepdogController.radius);
            }


            if (ExtVector3.IsInDirection(sheepdogVelocity.normalized, sheepToTarget) && ExtVector3.IsInDirection(sheepToTarget, sheepdogToSheep)) //if already behind sheep and facing right way
            {
                if ((sheepPos - sheepDogTargetPos).magnitude > ExtVector3.MagnitudeInDirection(sheepdogToSheep, sheepToTarget))
                {
                    sheepDogTargetPosB = sheepPos - sheepToTarget.normalized * (ExtVector3.MagnitudeInDirection(sheepdogToSheep, sheepToTarget) - SheepController.radius);
                }
            }

            //pick the one which is closer to the sheep
            if ((sheepDogTargetPosB - sheepPos).magnitude < (sheepDogTargetPos - sheepPos).magnitude)
            {
                sheepDogTargetPos = sheepDogTargetPosB;
            }

            //but it can't be too close or it may cause problems in walls and corners
            if ((sheepDogTargetPos - sheepPos).magnitude < SheepController.radius)
            {
                sheepDogTargetPos = sheepPos - sheepToTarget * SheepController.radius;
            }
        }


        //Debug.DrawRay(targetPoint, -sheepToTarget * 20, Color.green, 1);
        //Debug.DrawRay(sheepdogPos, sheepdogToTarget * 10, Color.red, 1);

        //Debug.Log(currentSheep.fleeingSheepdog + ", " + currentSheep.velocity.magnitude);

        return sheepDogTargetPos;
    }

    public static void ToggleAscending ()
    {
        orderAscending = !orderAscending;
    }

    public static void ToggleMetric()
    {
        orderByDistance = !orderByDistance;
    }

    public static void ToggleAllowTargetChange()
    {
        allowTargetChange = !allowTargetChange;
    }

    public static void TogglePickSheepTargets()
    {
        pickSheepTargets = !pickSheepTargets;
    }

}
