using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GeneralSceneController : MonoBehaviour
{
	public GameObject systemControllerPrefab;

	[HideInInspector]
    public SystemController systemController;
	protected GeneralInput input;
	protected UIStackManager UIStackManager;



	protected void DoAwake ()
    {
		//create a system controller if we dont find one already made, and assign it

		GameObject[] systemControllerList = GameObject.FindGameObjectsWithTag("SystemController");

		if (systemControllerList.Length == 0)
		{
			GameObject newSystemControllerObject = Instantiate(systemControllerPrefab);
			systemController = newSystemControllerObject.GetComponent<SystemController>();
		}
		else
        {
			systemController = systemControllerList[0].GetComponent<SystemController>();
		}

		input = gameObject.GetComponent<GeneralInput>();
		UIStackManager = gameObject.GetComponent<UIStackManager>();

	}


	protected void DoStart ()
    {
		
    }


	public virtual void DoUpdate ()
    {
		input.DoUpdate();
		UIStackManager.DoUpdate();
	}



	public virtual bool CanRemoveBottomMenu(GameObject menu)
	{
		return true;
	}
}
