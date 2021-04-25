using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GenericMenu : GenericUIComponent
{

	public List<GameObject> interactables;

	public enum MenuType
	{
		Fixed, Floating
	}
	public MenuType menuType;
	public bool temporaryMenu = false;


	public override void DoAwake()
	{
		base.DoAwake();
	}

	public override void DoStart()
	{
		base.DoStart();
	}



	public override void DoUpdate()
	{

	}


	public virtual void TurnedOn(GameObject previousMenu)
	{

	}


	public virtual void TurnedOff()
	{

	}

	public override void Cancelled()
    {

    }


	public virtual void SleepMenu(GameObject exception = null)
	{
		DisableInteractables(exception);
	}


	public virtual void WakeMenu()
	{
		EnableInteractables();
	}





	public void EnableInteractables()
	{
		for (int i = 0; i < interactables.Count; i++)
		{
			if (interactables[i].GetComponent<Button>() is Button)
			{
				interactables[i].GetComponent<Button>().interactable = true;
			}
			else if (interactables[i].GetComponent<TMP_Dropdown>() is TMP_Dropdown)
			{
				interactables[i].GetComponent<TMP_Dropdown>().interactable = true;
			}
			else if (interactables[i].GetComponent<TMP_InputField>() is TMP_InputField)
			{
				interactables[i].GetComponent<TMP_InputField>().interactable = true;
			}
		}
	}


	public void DisableInteractables(GameObject exception = null)
	{
		for (int i = 0; i < interactables.Count; i++)
		{
			if (GameObject.ReferenceEquals(exception, interactables[i])) continue;

			if (interactables[i].GetComponent<Button>() is Button)
            {
				interactables[i].GetComponent<Button>().interactable = false;
			}
			else if (interactables[i].GetComponent<TMP_Dropdown>() is TMP_Dropdown)
            {
				interactables[i].GetComponent<TMP_Dropdown>().interactable = false;
			}
			else if (interactables[i].GetComponent<TMP_InputField>() is TMP_InputField)
			{
				interactables[i].GetComponent<TMP_InputField>().interactable = false;
			}



		}
	}



}