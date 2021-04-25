using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MM_SimulationChoiceMenu : GenericMenu
{
	public GameObject button_CreateNew;
	public GameObject button_LoadOld;
	public GameObject button_Back;


	private void Awake()
	{
		base.DoAwake();
	}

	private void Start()
	{
		base.DoStart();

		interactables.Add(button_CreateNew);
		interactables.Add(button_LoadOld);
		interactables.Add(button_Back);


	}


	public override void TurnedOn(GameObject previousMenu)
	{
		uiStackManager.sc.systemController.simulationSettings.SetDefaultSettings();
	}


	public override void ReceiveConfirmation(bool response)
	{

		toBeConfirmed = "";
	}


	public override void SleepMenu(GameObject exception = null)
	{
		DisableInteractables(exception);
	}


	public override void WakeMenu()
	{
		EnableInteractables();
	}



	public void CreateNewButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}


	public void LoadOldButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}

	public void BackButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack(gameObject);
	}


}
