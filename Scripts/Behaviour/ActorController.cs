using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActorController : MonoBehaviour
{
    public SimulationSceneController sc;

	public SphereCollider sphereCollider;
	public GameObject pointer;
    public float sphereRadius { get { return sphereCollider.radius * transform.lossyScale.x; } }

	public CollisionInfo collisionInfo;
	CollisionHandleInfo collisionHandleInfo = new CollisionHandleInfo();
	public static LayerMask collisionLayers;
	public LayerMask geometryLayer;
	public LayerMask wallLayer;
	public LayerMask sheepLayer;
	public LayerMask sheepDogLayer;
	public List<Component> ignoreColliders = new List<Component>();
	const float maxRadiusMoveDivider = 3f;
	float maxRadiusMove { get { return sphereRadius / maxRadiusMoveDivider; } }
	const float tinyOffset = .0001f;
	const float smallOffset = .002f; //.002f
	const float safeCheckOffset = 0.04f;//.02f; 

	float walkableSlopeLimit = 60;

	public Vector3 velocity = Vector3.zero;
	public Vector3 collisionVelocity = Vector3.zero;
	public float actualDistanceMoved = 0;
	Vector3 lastSafePos;
	public Vector3 facingDir = Vector3.forward;

    protected float currentSimulationSpeed;


	protected void DoAwake()
    {
		sc = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SimulationSceneController>();
		collisionLayers = wallLayer + sheepLayer;

	}

	protected void DoStart()
    {
        
		lastSafePos = transform.position;
		ignoreColliders.Add(sphereCollider);
    }

    public virtual void DoPreLayerUpdate(float deltaTime, float simulationSpeed)
    {
        currentSimulationSpeed = simulationSpeed;

       
    }

	public virtual void DoPostLayerUpdate(float deltaTime, float simulationSpeed)
	{
		

	}

	public virtual void DoPostUpdate(float deltaTime, float simulationSpeed)
    {

    }

	public virtual void FollowBehaviour (float simulationSpeed)
    {

    }



	/////////////////////////////////////
	///////// COLLISION STUFF ///////////
	/////////////////////////////////////


	public void PrepareForCollision(float deltaTime)
	{
		collisionVelocity = velocity * deltaTime;
		collisionInfo = new CollisionInfo();
		collisionInfo.pointsInfo = new List<SphereCollisionDetect.CollisionPointInfo>();
		collisionInfo.origin = transform.position;
		collisionInfo.targetPosition = collisionInfo.origin + collisionVelocity;
		Vector3 previousHitNormal = Vector3.zero;
		actualDistanceMoved = 0;
	}

	public bool DoCollisionUpdate(float deltaTime, Vector3 stepVelocity)
	{
		DoCollisionIteration(deltaTime, stepVelocity);

		if (!collisionInfo.collisionSuccessful)
		{
			//Debug.Log ("Aborting collision");
			return false;
		}

		return true;
	}


	List<SphereCollisionInfo> collisionPointBuffer = new List<SphereCollisionInfo>();
	public void DoCollisionIteration(float deltaTime, Vector3 stepVelocity)
	{
		Vector3 previousOrigin = collisionInfo.origin;
		collisionInfo.origin += stepVelocity;
		collisionInfo.targetPosition = collisionInfo.origin;
		float negativeOffset = 0;

		

		for (collisionInfo.temporaryAttempts = 0; collisionInfo.temporaryAttempts < collisionHandleInfo.maxCollisionCheckIterations; collisionInfo.temporaryAttempts++)
		{
			Vector3 hitNormal = Vector3.zero;
			bool hasHit = false;

			//It is important for us to have a negativeOffset, otherwise our collision detection methods might keep telling us we are penetrated...
			if (collisionInfo.temporaryAttempts > 0 && negativeOffset > -safeCheckOffset) negativeOffset += -smallOffset;

			//Do grounding here
			collisionInfo.origin = GroundCollision(collisionInfo.origin);

			if (CheckOverlaps(collisionInfo.origin, Vector3.up, sphereRadius + negativeOffset, ignoreColliders, collisionLayers))
			{

				//Debug.Log ("hit something");
				List<SphereCollisionInfo> collisionPoints = SphereCollisionDetect.DetectCollisions(collisionInfo.origin, collisionInfo.origin, Vector3.up, sphereRadius, collisionLayers, ignoreColliders, collisionPointBuffer, Vector3.up, false, safeCheckOffset);

				//Debug.Log (collisionPoints.Count);

				if (collisionPoints.Count > 0)
				{

					if (Input.GetKeyDown(KeyCode.P))
					{
						//DrawContactsDebug (collisionPoints, .5f, Color.magenta, Color.green);
					}


					//We do the main depenetration method
					DepenetrationInfo depenInfo = SphereCollisionDetect.Depenetrate(this, collisionPoints, collisionHandleInfo.maxDepenetrationIterations);
					//Vector3 depenetration = Vector3.ClampMagnitude(depenInfo.totalDepenetration, maxRadiusMove); //We clamp to make sure we dont depenetrate too much into possibly unsafe areas **this myay be risky but unconfirmed**
					Vector3 depenetration = depenInfo.totalDepenetration;

					collisionInfo.origin += depenetration;
					collisionInfo.foundWalkableGroundNormal = (depenInfo.foundWalkableGroundNormal) ? true : collisionInfo.foundWalkableGroundNormal;
					//collisionInfo.pointsInfo.AddRange (depenInfo.pointsInfo);
					collisionInfo.pointsInfo = depenInfo.pointsInfo;  //directly setting rather than adding means we only keep the points of the last step velocity loop through (less to loop through later)

					hitNormal = (depenetration != Vector3.zero) ? depenetration.normalized : hitNormal;


					//go through and limit velocity velocity via collsion planes
					//porbably not necessary but may help speed up collision overall in awkward situations
					//note: only changes the step velocity which will not persist past the end of the collision loops
					for (int k = 0; k < depenInfo.pointsInfo.Count; k++)
					{
						SphereCollisionDetect.CollisionPointInfo cpi = depenInfo.pointsInfo[k];

						if (ExtVector3.IsInDirection(collisionInfo.stepVelocity, -cpi.depenetrationNormal, tinyOffset, true))
						{
							//important for upright-only collision
							if (CanWalkOnSlope(cpi.depenetrationNormal, Vector3.up))
							{
								//dont wanna think a concave slope or edge weve walked onto is a wall
								continue;
							}
						}
					}
					//im doing this to stop the direction from changing, which means overall the speed in the direction will just be reduced, but means that on the last runthrough the same wall will still be detected
					//collisionInfo.stepVelocity = stepVelocityDir * ExtVector3.MagnitudeInDirection (collisionInfo.stepVelocity, stepVelocityDir); 


					//Final check if we are safe, if not then we just move a little and hope for the best.
					if (FinalOverlapCheck(collisionInfo.origin, Vector3.up, sphereRadius + (negativeOffset - smallOffset), ignoreColliders, collisionLayers))
					{
						//Debug.Log("Still not safe yet");
						
						//collisionInfo.origin += (depenInfo.averageNormal * smallOffset);
						//collisionInfo.origin += hitNormal * smallOffset;

						//Debug.Log (ExtVector3.PrintFullVector3 (depenInfo.averageNormal));
					}


					hasHit = true;
				}
			}

			collisionInfo.targetPosition = collisionInfo.origin;

			if (hasHit)
			{
				//this hasHit is only set to true for the main collision, as if we set it true after a grounding collision, because of the ground offset, it will get stuck in a continous loop, so instead
				//we just set the target position directly in the grounding collison too, and it will break and take that target position if it proceeds to find no colliding walls afterwards
				collisionInfo.attempts++;
				//previousHitNormal = hitNormal;
			}
			else
			{
				break;
			}



		}

		if (transform.tag == "Sheepdog")
        {
			//Debug.Log("total loops: " + collisionInfo.temporaryAttempts);
		}
		

		if (collisionInfo.temporaryAttempts >= collisionHandleInfo.maxCollisionCheckIterations)
		{
			string tagName = transform.tag;
			Debug.Log(tagName + ": Collision has failed!");
			
			collisionInfo.hasCollided = true;
			collisionInfo.collisionSuccessful = false;

		}
		else
		{
			collisionInfo.hasCollided = (collisionInfo.attempts > 0);
			collisionInfo.collisionSuccessful = true;
			collisionInfo.safeMoveDirection = collisionInfo.targetPosition - transform.position;

		}

		MovePlayerAfterCollision(deltaTime);

		return;
	}

	public Vector3 GroundCollision(Vector3 origin)
	{
		Vector3 newOrigin = origin;

		RaycastHit hitInfo = ExtPhysics.SphereCast(origin + Vector3.up * 1, sphereRadius, -Vector3.up, ignoreColliders, 10, geometryLayer);  //geometry layer here to avoid other actors being used

		if (hitInfo.collider != null)
        {
			Vector3 toMove;
			newOrigin += sphereRadius * 10 * Vector3.up;
			newOrigin.y = hitInfo.point.y + sphereRadius;

			Vector3 depenetration = (Geometry.DepenetrateSphereFromPlaneInDirection(newOrigin, sphereRadius, Vector3.up, hitInfo.point, hitInfo.normal).distance + 0.00001f) * Vector3.up;
			newOrigin += depenetration;

			//Debug.Log("hit!");
        }
		else
        {
			//Debug.Log("not hit!");
        }

		return newOrigin;
	}

	public void MovePlayerAfterCollision(float deltaTime)
	{
		Vector3 oldPos = transform.position;

		if (collisionInfo.collisionSuccessful)
		{
			transform.Translate(collisionInfo.safeMoveDirection, Space.World);
			lastSafePos = transform.position;

			//Debug.Log ("x: " + transform.position.x + ", y: " + transform.position.y + ", z: " + transform.position.z);
		}
		else
		{
			transform.position = lastSafePos;
			collisionInfo.velocity = Vector3.zero;
		}


		collisionInfo.origin = transform.position;
		actualDistanceMoved += Vector3.ProjectOnPlane((transform.position - oldPos), Vector3.up).magnitude;
	}

	public void FinalizeAfterCollision(float deltaTime)
	{
		if (collisionInfo.collisionSuccessful)
		{
			//We handle redirecting our velocity. First we just default it to the targetVelocity, and not include the additional velocity added because it should be one-time only.
			collisionInfo.velocity = collisionVelocity;


			foreach (SphereCollisionDetect.CollisionPointInfo cpi in collisionInfo.pointsInfo)
            {
				Vector3 normal = cpi.depenetrationNormal;

				if (!CanWalkOnSlope (normal, Vector3.up))
                {
					normal = Vector3.ProjectOnPlane(normal, Vector3.up);

					if (this is SheepController)
                    {
						if (cpi.isWall && ExtVector3.MagnitudeInDirection (collisionInfo.velocity, -normal) > 0)
                        {
							bool hadSpeedBefore = velocity.magnitude > 0;

							collisionInfo.velocity = Vector3.ProjectOnPlane(collisionInfo.velocity, normal);
							facingDir = (!Mathf.Approximately(velocity.magnitude, 0)) ? velocity.normalized : facingDir;

							//we can't ever have speed be completely 0 or it could lead to a whole range of problems with turning behaviour
							if (hadSpeedBefore && Mathf.Approximately (collisionInfo.velocity.magnitude, 0))
                            {
								collisionInfo.velocity = facingDir.normalized * 0.001f;
                            }
						}
						
					}
					
					collisionInfo.velocity = Vector3.ProjectOnPlane(collisionInfo.velocity, Vector3.up);
                }
            }
		}

	
		velocity = collisionInfo.velocity / deltaTime;
		facingDir = (!Mathf.Approximately (velocity.magnitude, 0))? velocity.normalized : facingDir;

		float newPointerRot = Vector3.SignedAngle(Vector3.forward, facingDir, Vector3.up);
		transform.localEulerAngles = new Vector3(0, newPointerRot, 0);

		//Debug.Log(velocity);
	}


	public Vector3 AvoidWalls(Vector3 velocity, float deltaTime)
	{
		float maxDetectionDistance = sphereRadius + 3;

		List<SphereCollisionInfo> collisionPointBuffer = new List<SphereCollisionInfo>();
		List<SphereCollisionInfo> collisionPoints = SphereCollisionDetect.DetectCollisions(transform.position, transform.position, Vector3.up, maxDetectionDistance, wallLayer, ignoreColliders, collisionPointBuffer, Vector3.up, true, 0.0001f);

		int total = 0;
		Vector3 averagePoint = new Vector3();
		Vector3 averageNormal = new Vector3();

		for (int i = 0; i < collisionPoints.Count; i++)
		{
			SphereCollisionInfo info = collisionPoints[i];

			float angleBetween = Vector3.Angle(velocity.normalized, -info.normal);

			//Debug.Log("angleBetween: " + angleBetween + ", velocityDir: " + velocity.normalized + ", normal: " + -info.normal);

			if (angleBetween <= 90)
			{
				averagePoint += info.closestPointOnSurface;
				averageNormal += info.normal;
				total += 1;
			}
		}

		if (total > 0)
        {
			averagePoint = averagePoint / total;
			averageNormal = (averageNormal / total).normalized;

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
			float velocityMod = 1 - (Mathf.Abs(angleBetween) / 90);
			float distanceMod = 1 - Mathf.Min(distance, maxDetectionDistance) / maxDetectionDistance;
			float turnSpeed = Mathf.Clamp(30 * distanceMod * distanceMod * velocityMod * velocityMod, 2, 20);
			float targetAngle = rot + 50 * turnDir;

			float newRot = ExtVector3.CustomLerpAngle(rot, targetAngle, turnSpeed, deltaTime, 0.5f);
			float deltaRot = Mathf.DeltaAngle(rot, newRot);
			newRot = rot + Mathf.Min(deltaRot, 20);

			velocity = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * newRot), 0, Mathf.Cos(Mathf.Deg2Rad * newRot)).normalized * velocity.magnitude;

			//Debug.Log("velocityDir: " + velocity.normalized + ", targetAngle: " + targetAngle + "," + ", turnDir: " + turnDir);
		}

		return velocity;

	}


	public void AvoidOtherActors (float deltaTime)
    {
		IList<Collider> foundActors = new List<Collider>();
		ExtPhysics.OverlapSphere(transform.position, 5, ignoreColliders, foundActors, sheepLayer);

		if (foundActors.Count == 0)
        {
			return;
        }

		ActorController closestActor = foundActors[0].GetComponent<ActorController>();
		float shortestDistance = float.MaxValue;
		bool found = false;

		float totalDisplacement = 0;

		foreach (Collider actor in foundActors)
        {
			Vector3 ourPos = new Vector3(transform.position.x, 0, transform.position.z);
			Vector3 otherPos = new Vector3(actor.transform.position.x, 0, actor.transform.position.z);
			ActorController other = actor.GetComponent<ActorController>();
			Vector3 otherProjectedPos = otherPos + other.velocity;

			//IntersectPoints intersects = Geometry.ClosestPointsOnTwoLineSegments(ourPos, ourPos + velocity, otherPos, otherProjectedPos);
			Vector3 closestPoint = Geometry.ClosestPointOnLineToPoint(otherPos, ourPos, velocity.normalized);

			Vector3 perpDir = closestPoint - otherPos;
			float distBetweenPos = perpDir.magnitude;

			if (distBetweenPos <= 2f)
            {
				totalDisplacement += -Vector3.SignedAngle(facingDir, (otherPos - ourPos).normalized, Vector3.up);

				float dist = (otherPos - ourPos).magnitude;

				if (dist < shortestDistance)
                {
					found = true;
					shortestDistance = dist;
					closestActor = other;
                }
            }
        }

		if (found)
        {
			float distanceRatio = (shortestDistance == 0) ? 1 : 1 / (shortestDistance * shortestDistance);
			//float turnSpeed = 150 * distanceRatio;
			float turnSpeed = 150;

			Vector2 perpDir2D = Vector2.Perpendicular(new Vector2(facingDir.x, facingDir.z)) * ((totalDisplacement == 0)? 1 : Mathf.Sign(totalDisplacement));
			Vector3 perpDir = new Vector3(perpDir2D.x, 0, perpDir2D.y).normalized;
			float targetAngle = Vector3.SignedAngle(perpDir, Vector3.forward, Vector3.up);

			float rot = Vector3.SignedAngle(facingDir, Vector3.forward, Vector3.up);
			rot = Mathf.MoveTowardsAngle(rot, rot+90*((totalDisplacement == 0) ? 1 : Mathf.Sign(totalDisplacement)), turnSpeed * deltaTime);
			facingDir = new Vector3(-Mathf.Sin(Mathf.Deg2Rad * rot), 0, Mathf.Cos(Mathf.Deg2Rad * rot)).normalized;
			velocity = velocity.magnitude * facingDir.normalized;
		}
    }


	protected bool CheckOverlaps(Vector3 origin, Vector3 transformUp, float radius, List<Component> ignoreColliders, LayerMask mask)
	{
		return ExtPhysics.CheckSphere(origin, radius, ignoreColliders, mask);
	}

	protected bool FinalOverlapCheck(Vector3 origin, Vector3 transformUp, float radius, List<Component> ignoreColliders, LayerMask mask)
	{
		return ExtPhysics.CheckSphere(origin, radius, ignoreColliders, mask);
	}

	public bool CanWalkOnSlope(Vector3 normal, Vector3 comparedNormal)
	{
		if (normal == Vector3.zero) return false;
		return ExtVector3.Angle(normal, comparedNormal) < walkableSlopeLimit;
	}

	public void SetStepVelocity(int steps)
	{
		collisionInfo.stepVelocity = collisionVelocity / steps;
	}

	public float GetRadius ()
    {
		return sphereRadius;
    }



	public struct CollisionInfo
	{
		public Vector3 origin;
		public Vector3 targetPosition;
		public Vector3 safeMoveDirection;
		public Vector3 velocity;
		public Vector3 stepVelocity;
		public bool hasCollided;
		public bool hasFailed;
		public int temporaryAttempts;
		public int attempts;
		public bool collisionSuccessful;
		public bool foundWalkableGroundNormal;
		public bool foundMovingPlatform;
		public List<SphereCollisionDetect.CollisionPointInfo> pointsInfo;
	}

	public class CollisionHandleInfo
	{
		public int maxCollisionCheckIterations = 6; //On average it runs 2 to 3 times, but on surfaces with opposing normals it could run much more.
		public int maxDepenetrationIterations = 10;
		public int maxVelocitySteps = 20; //A safety in case we are moving very fast we dont want to divide our velocity into to many steps since that can cause lag and freeze the game, so we prefer to have the collision be unsafe.
		public bool abortIfFailedThisFrame = true; //Prevents us from constantly trying and failing this frame which causes lots of lag if using subUpdater, which would make subUpdater run more and lag more...
		public bool tryBlockAtSlopeLimit = true;
		public bool cleanByIgnoreBehindPlane;
		public bool depenetrateEvenIfUnsafe;
	}

	public struct DepenetrationInfo
	{
		public Vector3 totalDepenetration;
		public List<SphereCollisionDetect.CollisionPointInfo> pointsInfo;
		public Vector3 averageNormal;
		public bool foundWalkableGroundNormal;

		public void Initialize()
		{
			pointsInfo = new List<SphereCollisionDetect.CollisionPointInfo>();
		}
	}



	
}
