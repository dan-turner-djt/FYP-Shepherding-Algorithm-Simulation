using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class SphereCollisionDetect
{

	public static List<SphereCollisionInfo> DetectCollisions(Vector3 detectionOrigin, Vector3 realOrigin, Vector3 directionUp, float radius, int mask, IList<Component> ignoreColliders, List<SphereCollisionInfo> resultBuffer, Vector3 transformUp, bool forWalls = false, float checkOffset = 0, bool multipleContactsPerCollider = true)
	{
		return DetectSphereCollisions(detectionOrigin, realOrigin, radius, mask, ignoreColliders, resultBuffer, transformUp, checkOffset, multipleContactsPerCollider, forWalls);
	}


	static List<Collider> colliderBufferSphere = new List<Collider>();
	static List<ContactInfo> contactsBufferSphere = new List<ContactInfo>();
	public static List<SphereCollisionInfo> DetectSphereCollisions(Vector3 detectionOrigin, Vector3 realOrigin, float radius, int mask, IList<Component> ignoreColliders, List<SphereCollisionInfo> resultBuffer, Vector3 transformUp, float checkOffset = 0, bool multipleContactsPerCollider = true, bool forWalls = false)
	{
		resultBuffer.Clear();
		colliderBufferSphere.Clear();

		ExtPhysics.OverlapSphere(detectionOrigin, radius + checkOffset, ignoreColliders, colliderBufferSphere, mask);
		//Debug.Log (colliderBufferSphere.Count);
		if (colliderBufferSphere.Count == 0) return resultBuffer;

		for (int i = 0; i < colliderBufferSphere.Count; i++)
		{
			contactsBufferSphere = ExtCollider.ClosestPointsOnSurface(colliderBufferSphere[i], detectionOrigin, radius + checkOffset, contactsBufferSphere, multipleContactsPerCollider, forWalls);

			for (int j = 0; j < contactsBufferSphere.Count; j++)
			{
				//We calculate sphereDetectionOriginInCapsule for our depenetration method since we need to know where the spheres detection origin would be within the capsule.
				Vector3 sphereDetectionOriginInSphere = detectionOrigin;
				
				//We store just the radius, not radius + checkOffset, so that our depenetration method has the correct radius to depenetrate with.
				resultBuffer.Add(new SphereCollisionInfo(true, colliderBufferSphere[i], sphereDetectionOriginInSphere, realOrigin, radius, contactsBufferSphere[j].point, contactsBufferSphere[j].normal, transformUp));
			}
		}

		return resultBuffer;
	}



	public static ActorController.DepenetrationInfo Depenetrate(ActorController ppc, List<SphereCollisionInfo> collisionPoints, int maxIterations = 4)
	{
		ActorController.DepenetrationInfo depenInfo = new ActorController.DepenetrationInfo();
		depenInfo.Initialize();

		if (collisionPoints.Count > 0 && maxIterations > 0)
		{
			Vector3 depenetrationVelocity = Vector3.zero;

			Vector3 totalNormal = Vector3.zero;
			List<CollisionPointInfo> collisionPointsInfo = new List<CollisionPointInfo>();
			for (int i = 0; i < collisionPoints.Count; i++)
			{
				CollisionPointInfo cpi = new CollisionPointInfo();
				cpi.cp = collisionPoints[i];

				totalNormal += cpi.cp.interpolatedNormal;

				if (collisionPoints[i].collider.gameObject.layer == 11) //10 = Wall
                {
					cpi.isWall = true;
                }


				collisionPointsInfo.Add(cpi);
			}
			depenInfo.pointsInfo = collisionPointsInfo;
			depenInfo.averageNormal = totalNormal.normalized;

			int counter = 0;

			//Since with each iteration we are using old collision data, higher maxIterations does not mean more accuracy. You will need to tune it to your liking.
			for (int i = 0; i < maxIterations; i++)
			{
				counter++;

				bool depenetrated = false;
				for (int j = 0; j < depenInfo.pointsInfo.Count; j++)
				{
					CollisionPointInfo cpi = depenInfo.pointsInfo[j];
					SphereCollisionInfo cp = cpi.cp;
					Vector3 detectOriginOffset = depenInfo.totalDepenetration + depenetrationVelocity + cp.detectionOrigin;

					cpi.SetInfo(i, ppc, detectOriginOffset); //sets stuff in the struct
					Vector3 depenetrationNormal = cpi.GetDepenetrationNormal();
					depenInfo.pointsInfo[j] = cpi;

					Vector3 depenetration = Geometry.DepenetrateSphereFromPlaneInDirection(detectOriginOffset, cp.sphereRadius, depenetrationNormal, cp.closestPointOnSurface, cp.interpolatedNormal).distance * depenetrationNormal;
					if (ExtVector3.MagnitudeInDirection(depenetration, depenetrationNormal, false) <= 0) continue;


					depenetrationVelocity += depenetration + 0.0001f * depenetrationNormal;
					//depenetrationVelocity += depenetration;
					depenetrated = true;
				}


				if (!depenetrated) break;

				depenInfo.totalDepenetration += depenetrationVelocity;
				depenetrationVelocity = Vector3.zero;
			}
			string tagName = ppc.transform.tag;
			//Debug.Log(tagName + ": depenetration loops: " + counter);
		}

		return depenInfo;
	}



	//I think this works fine with our capsule detection, but doesnt really work with spheres shaping a capsule. The reason for this is
	//when having spheres shape a capsule, its possible for a sphere to detect a hit and set the interpolated normal in a way that blocks all other hits behind it, however,
	//when this sphere depenetrates, since it isnt taking into account that we wanted to treat it like a capsule, it wont depenetrate enough for the hits behind it to be resolved.
	//However, since our capsule DetectCollisions handles placing the spheres properly to form a capsule, it should work with that.
	//This method is pretty similar to the "CleanUp" method in our meshbsptree.
	static List<MPlane> ignoreBehindPlanes = new List<MPlane>();
	public static List<SphereCollisionInfo> CleanByIgnoreBehindPlane(List<SphereCollisionInfo> collisionPoints)
	{
		if (collisionPoints.Count > 1)
		{
			ignoreBehindPlanes.Clear();

			//Taking advantage of C# built in QuickSort algorithm
			collisionPoints.Sort(SphereCollisionInfo.SphereCollisionComparerDescend.defaultComparer);

			for (int i = collisionPoints.Count - 1; i >= 0; i--)
			{
				if (!MPlane.IsBehindPlanes(collisionPoints[i].closestPointOnSurface, ignoreBehindPlanes, -.0001f))
				{
					ignoreBehindPlanes.Add(new MPlane(collisionPoints[i].interpolatedNormal, collisionPoints[i].closestPointOnSurface, false));
				}
				else
				{
					collisionPoints.RemoveAt(i);
				}
			}
		}

		return collisionPoints;
	}




	public struct CollisionPointInfo
	{
		public SphereCollisionInfo cp;
		public Vector3 depenetrationNormal;
		public bool slopeTooSteep;
		public bool invalidStep;
		public Vector3 depenetrationInNormalDir;
		public bool isWall;

		public void SetInfo(int i, ActorController ppc, Vector3 originOffset)
		{
			this.depenetrationNormal = cp.interpolatedNormal;

		}


		public Vector3 GetDepenetrationNormal()
		{
			return depenetrationNormal;
		}


	}
}
