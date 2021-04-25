using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MM_CreateNewMenu : GenericMenu
{
	public GameObject button_Back;
	public GameObject button_Start;
	public TMP_InputField inputField_name;
	public TextMeshProUGUI loadedLabel1;
	public TextMeshProUGUI loadedLabel2;


	private void Awake()
	{
		base.DoAwake();
	}

	void Start()
	{
		base.DoStart();

		interactables.Add(button_Back);
		interactables.Add(button_Start);
		interactables.Add(inputField_name.gameObject);
		

	}


	public override void TurnedOn(GameObject previousMenu)
	{
		if (previousMenu == null)
        {
			//going up in stack

			uiStackManager.sc.systemController.simulationSettings.SetDefaultSettings();

			inputField_name.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.settingsName;
		}

		
		//do the loaded labels

		if (!uiStackManager.sc.systemController.simulationSettings.sheepSettingsName.Equals(uiStackManager.sc.systemController.simulationSettings.defaultSheepName))
        {
			loadedLabel1.SetText("DONE");
        }
		else
        {
			loadedLabel1.SetText("");
		}

		if (!uiStackManager.sc.systemController.simulationSettings.sheepdogSettingsName.Equals(uiStackManager.sc.systemController.simulationSettings.defaultSheepdogName))
		{
			loadedLabel2.SetText("DONE");
		}
		else
		{
			loadedLabel2.SetText("");
		}

	}


	public override void ReceiveConfirmation(bool response)
	{

		if (toBeConfirmed == "startButtonAction")
		{
			StartButtonAction(response);
		}


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



	public void SheepCreateButtonPressed (GameObject nextMenu)
    {
		uiStackManager.AddComponentToStack(nextMenu);
	}

	public void SheepdogCreateButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}

	public void SheepLoadButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}

	public void SheepdogLoadButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}



	public void StartButtonPressed()
	{
		string name = inputField_name.GetComponent<TMP_InputField>().text;

		if (name.Equals("") || name.Equals(uiStackManager.sc.systemController.simulationSettings.defaultName))
        {
			string message = "Please enter a name";

			uiStackManager.SendMessageMenu(message);

			return;
		}

		if (uiStackManager.sc.systemController.simulationSettings.sheepSettingsName.Equals(uiStackManager.sc.systemController.simulationSettings.defaultSheepName))
		{
			string message = "No sheep settings have been loaded";

			uiStackManager.SendMessageMenu(message);

			return;
		}
		if (uiStackManager.sc.systemController.simulationSettings.sheepdogSettingsName.Equals(uiStackManager.sc.systemController.simulationSettings.defaultSheepdogName))
		{
			string message = "No sheepdog settings have been loaded";

			uiStackManager.SendMessageMenu(message);

			return;
		}


		if (uiStackManager.sc.systemController.CheckIfFileAlreadyExists (name, 0))
        {
			toBeConfirmed = "startButtonAction";
			string message = "Are you sure you want to overwrite " + name + "?";
			uiStackManager.AskForResponse(message);
		}
		else
        {
			StartButtonAction(true);
		}

		
	}


	public void StartButtonAction(bool response)
	{
		

		if (!response)
        {
			return;
        }

		SaveSettingsSelected();
		
		uiStackManager.sc.systemController.LoadSimulation();
	}


	void SaveSettingsSelected ()
    {

		uiStackManager.sc.systemController.simulationSettings.SetNewSimulationSettings(inputField_name.GetComponent<TMP_InputField>().text);
		uiStackManager.sc.systemController.WriteSimulationSettings(0);
	}

	public void BackButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack(gameObject);
	}
}
