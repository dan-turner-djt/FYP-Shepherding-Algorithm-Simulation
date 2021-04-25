using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MM_SheepdogLoadOld : GenericMenu
{
	public GameObject button_Back;
	public GameObject button_Start;
	public GameObject button_Delete;
	public TMP_Dropdown dropdown_FileNames;
	public TMP_Dropdown dropdown_Learning;
	public TMP_InputField inputField_CMD;
	public TMP_InputField inputField_PHP;
	public TMP_InputField inputField_PHP2;


	List<string> files;

	private void Awake()
	{
		base.DoAwake();
	}

	void Start()
	{
		base.DoStart();

		interactables.Add(button_Back);
		interactables.Add(button_Start);
		interactables.Add(button_Delete);
		interactables.Add(dropdown_FileNames.gameObject);


	}


	public override void TurnedOn(GameObject previousMenu)
	{

		if (previousMenu == null)
		{
			//going up in stack

			UpdateFileListAndChanges();
		}

	}

	void UpdateFileListAndChanges()
	{
		files = uiStackManager.sc.systemController.GetValidFilesInDirectory(2);

		if (files.Count > 0)
		{
			DoFilesDropdown();
			UpdateOtherFields();
		}
		else
		{
			//their are no saved files so just laod the default settings
			uiStackManager.sc.systemController.simulationSettings.SetDefaultSheepdogSettings();

			dropdown_FileNames.options.Clear();
			dropdown_FileNames.options.Add(new TMP_Dropdown.OptionData() { text = "" });
			int TempInt = dropdown_FileNames.value;
			dropdown_FileNames.value = dropdown_FileNames.value + 1;
			dropdown_FileNames.value = TempInt;

			TempInt = (uiStackManager.sc.systemController.simulationSettings.isLearning)? 1 : 0;
			dropdown_Learning.value = dropdown_Learning.value + 1;
			dropdown_Learning.value = TempInt;

			inputField_CMD.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.collectAmount.ToString();
			inputField_PHP.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP.ToString();
			inputField_PHP2.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP2.ToString();

		}
	}

	void DoFilesDropdown()
	{
		List<string> names = new List<string>();

		dropdown_FileNames.options.Clear();


		for (int i = 0; i < files.Count; i++)
		{
			dropdown_FileNames.options.Add(new TMP_Dropdown.OptionData() { text = files[i] });
		}

		dropdown_FileNames.value = 1; //force it to update due to a change
		dropdown_FileNames.value = 0;
	}



	void UpdateOtherFields()
	{
		string currentName = dropdown_FileNames.options[dropdown_FileNames.value].text;

		uiStackManager.sc.systemController.SetSimulationSettingsFromFileName(currentName, 2);

		int TempInt = (uiStackManager.sc.systemController.simulationSettings.isLearning)? 1 : 0;
		dropdown_Learning.value = dropdown_Learning.value + 1;
		dropdown_Learning.value = TempInt;

		inputField_CMD.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.collectAmount.ToString();
		inputField_PHP.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP.ToString();
		inputField_PHP2.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP2.ToString();

	}


	public override void ReceiveConfirmation(bool response)
	{
		if (toBeConfirmed == "deleteButtonAction")
		{
			DeleteButtonAction(response);
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



	public void NameDropdownValueChanged()
	{
		if (files.Count > 0)
		{
			UpdateOtherFields();
		}

	}


	public void StartButtonPressed()
	{
		string name = dropdown_FileNames.options[dropdown_FileNames.value].text;

		if (name.Equals(""))
		{
			string message = "Nothing to load";

			uiStackManager.SendMessageMenu(message);

			return;
		}

		StartButtonAction(true);
	}


	public void StartButtonAction(bool response)
	{
		//load settings

		uiStackManager.RemoveComponentFromStack(gameObject);

	}



	public void DeleteButtonPressed()
	{
		if (files.Count > 0)
		{
			string fileName = dropdown_FileNames.options[dropdown_FileNames.value].text;
			string used = uiStackManager.sc.systemController.CheckIfSettingsUsedInSimulation(fileName, 2);

			if (!used.Equals(""))
			{
				string msg = "Cannot delete because these settings are used in " + fileName;

				uiStackManager.SendMessageMenu(msg);

				return;
			}


			toBeConfirmed = "deleteButtonAction";
			string message = "Are you sure you want to delete?";
			uiStackManager.AskForResponse(message);
		}
	}

	public void DeleteButtonAction(bool confirmed)
	{
		if (confirmed)
		{
			string fileName = dropdown_FileNames.options[dropdown_FileNames.value].text;

			uiStackManager.sc.systemController.DeleteFile(fileName, 2);

			UpdateFileListAndChanges();
		}

	}


	public void BackButtonPressed()
	{
		uiStackManager.sc.systemController.simulationSettings.SetDefaultSheepdogSettings();
		uiStackManager.RemoveComponentFromStack(gameObject);
	}

	public override void Cancelled()
	{
		uiStackManager.sc.systemController.simulationSettings.SetDefaultSheepdogSettings();
	}
}
