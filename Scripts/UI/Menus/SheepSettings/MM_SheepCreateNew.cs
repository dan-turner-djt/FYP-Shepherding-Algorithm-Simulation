using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MM_SheepCreateNew : GenericMenu
{
	public GameObject button_Back;
	public GameObject button_Start;
	public TMP_InputField inputField_name;
	public TMP_Dropdown dropdown_SheepAlgorithm;
	public TMP_Dropdown dropdown_SpawnType;
	public TMP_InputField inputField_flockSize;
	public TMP_InputField inputField_SDDist;


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
		interactables.Add(dropdown_SheepAlgorithm.gameObject);
		interactables.Add(dropdown_SpawnType.gameObject);
		interactables.Add(inputField_flockSize.gameObject);
		interactables.Add(inputField_SDDist.gameObject);


	}


	public override void TurnedOn(GameObject previousMenu)
	{
		if (previousMenu == null)
		{
			//going up in stack

			uiStackManager.sc.systemController.simulationSettings.SetDefaultSheepSettings();

			inputField_name.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.sheepSettingsName;
			dropdown_SheepAlgorithm.value = (int)uiStackManager.sc.systemController.simulationSettings.sheepAlgorithm;
			dropdown_SpawnType.value = (int)uiStackManager.sc.systemController.simulationSettings.spawnType;
			inputField_flockSize.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.flockSize.ToString();
			inputField_SDDist.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.sheepdogRepulsionDistance.ToString();
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




	public void StartButtonPressed()
	{
		string name = inputField_name.GetComponent<TMP_InputField>().text;

		if (name.Equals("") || name.Equals(uiStackManager.sc.systemController.simulationSettings.defaultSheepName))
		{
			string message = "Please enter a name";

			uiStackManager.SendMessageMenu(message);

			return;
		}

		if (!CheckFlockSizeInput())
		{
			string message = "Please enter a flock size between 1 and 100 inclusive";

			uiStackManager.SendMessageMenu(message);

			return;
		}

		if (!CheckSDDistInput())
		{
			string message = "Please enter a sheepdog detection distance between 0 and 100";

			uiStackManager.SendMessageMenu(message);

			return;
		}




		if (uiStackManager.sc.systemController.CheckIfFileAlreadyExists(name, 1))
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

		//write and load the settings

		SaveSettingsSelected();

		uiStackManager.RemoveComponentFromStack(gameObject);
	}


	bool CheckFlockSizeInput()
	{
		int input = Int32.Parse(inputField_flockSize.GetComponent<TMP_InputField>().text);

		if (input < 1 || input > uiStackManager.sc.systemController.simulationSettings.intMaxFlockSize)
		{
			return false;
		}

		return true;
	}

	bool CheckSDDistInput()
	{
		float input = float.Parse(inputField_SDDist.GetComponent<TMP_InputField>().text);

		if (input <= 0 || input > uiStackManager.sc.systemController.simulationSettings.maxSDDist)
		{
			return false;
		}

		return true;
	}


	void SaveSettingsSelected()
	{

		uiStackManager.sc.systemController.simulationSettings.SetNewSheepSettings(inputField_name.GetComponent<TMP_InputField>().text, (int)dropdown_SheepAlgorithm.value, Int32.Parse(inputField_flockSize.GetComponent<TMP_InputField>().text), (int)dropdown_SpawnType.value, float.Parse(inputField_SDDist.GetComponent<TMP_InputField>().text));
		uiStackManager.sc.systemController.WriteSimulationSettings(1);
	}

	public void BackButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack(gameObject);
	}
}
