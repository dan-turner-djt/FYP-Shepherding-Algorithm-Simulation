using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtLayerMask
{
	public static int ignoreRaycastMask = ~LayerMask.GetMask("Ignore Raycast");
	public static LayerMask physicsSoloCastLayer = LayerMask.NameToLayer("PhysicsSoloCast");
	public static int physicsSoloCastMask = LayerMask.GetMask("PhysicsSoloCast");

	static bool layersAreSet = LareLayersAreSet();
	static bool LareLayersAreSet()
	{
		if (physicsSoloCastMask == 0)
		{
			Debug.LogError("Layer PhysicsSoloCast is not defined. ExtLayerMask.physicsSoloCastLayer and ExtLayerMask.physicsSoloCastMask will return the wrong value");
			return false;
		}

		if (layersAreSet) { } 

		return true;
	}
}
