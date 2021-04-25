using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MM_SheepdogCreateNew : GenericMenu
{
	public GameObject button_Back;
	public GameObject button_Start;
	public TMP_InputField inputField_name;
	public TMP_Dropdown dropdown_Learning;
	public TMP_InputField inputField_CMD;
	public TMP_InputField inputField_PHP;
	public TMP_InputField inputField_PHP2;


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
		interactables.Add(dropdown_Learning.gameObject);
		interactables.Add(inputField_CMD.gameObject);
		interactables.Add(inputField_PHP.gameObject);
		interactables.Add(inputField_PHP2.gameObject);


	}


	public override void TurnedOn(GameObject previousMenu)
	{
		if (previousMenu == null)
		{
			//going up in stack

			uiStackManager.sc.systemController.simulationSettings.SetDefaultSheepdogSettings();

			inputField_name.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.sheepdogSettingsName;
			dropdown_Learning.value = (uiStackManager.sc.systemController.simulationSettings.isLearning)? 1 : 0;
			inputField_CMD.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.collectAmount.ToString();
			inputField_PHP.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP.ToString();
			inputField_PHP2.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP2.ToString();
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

		if (name.Equals("") || name.Equals(uiStackManager.sc.systemController.simulationSettings.defaultSheepdogName))
		{
			string message = "Please enter a name";

			uiStackManager.SendMessageMenu(message);

			return;
		}



		if (uiStackManager.sc.systemController.CheckIfFileAlreadyExists(name, 2))
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




	void SaveSettingsSelected()
	{
		uiStackManager.sc.systemController.simulationSettings.SetNewSheepdogSettings(inputField_name.GetComponent<TMP_InputField>().text, (dropdown_Learning.value == 1)? true : false, float.Parse(inputField_CMD.GetComponent<TMP_InputField>().text), float.Parse(inputField_PHP.GetComponent<TMP_InputField>().text), float.Parse(inputField_PHP2.GetComponent<TMP_InputField>().text));
		uiStackManager.sc.systemController.WriteSimulationSettings(2);

	}

	public void BackButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack(gameObject);
	}


	bool CheckCMDInput()
	{
		float input = float.Parse(inputField_CMD.GetComponent<TMP_InputField>().text);

		if (input <= 0 || input > uiStackManager.sc.systemController.simulationSettings.maxCMD)
		{
			return false;
		}

		return true;
	}
}
