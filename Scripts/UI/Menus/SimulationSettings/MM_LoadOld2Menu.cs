using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MM_LoadOld2Menu : GenericMenu
{
	public GameObject button_Back;
	public GameObject button_Start;
	public GameObject button_Edt;
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
		interactables.Add(button_Edt);
		interactables.Add(dropdown_Learning.gameObject);


	}


	public override void TurnedOn(GameObject previousMenu)
	{

		UpdateFileListAndChanges();
		
	}

	void UpdateFileListAndChanges ()
    {
		files = uiStackManager.sc.systemController.GetValidFilesInDirectory(0);

		if (files.Count > 0)
		{
			UpdateOtherFields();
		}
		else
		{
			//their are no saved files so just laod the default settings
			uiStackManager.sc.systemController.simulationSettings.SetDefaultSettings();

			dropdown_Learning.value = 1;
			dropdown_Learning.value = 0;

			inputField_CMD.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.collectAmount.ToString();
			inputField_PHP.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP.ToString();
			inputField_PHP2.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP2.ToString();
		}
	}



	void UpdateOtherFields ()
    {
		string currentName = uiStackManager.sc.systemController.simulationSettings.settingsName;

		uiStackManager.sc.systemController.SetSimulationSettingsFromFileName(currentName, 0);

		dropdown_Learning.value = (uiStackManager.sc.systemController.simulationSettings.isLearning) ? 1 : 0;

		inputField_CMD.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.collectAmount.ToString();
		inputField_PHP.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP.ToString();
		inputField_PHP2.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.PHP2.ToString();

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



	public void NameDropdownValueChanged ()
    {
		if (files.Count > 0)
        {
			UpdateOtherFields();
		}
		
    }


	public void StartButtonPressed()
	{
		StartButtonAction(true);
	}


	public void StartButtonAction(bool response)
	{

		uiStackManager.sc.systemController.LoadSimulation();

	}


	public void EditButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}



	public void BackButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack(gameObject);
	}
}
