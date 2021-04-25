using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MM_LoadOldMenu : GenericMenu
{
	public GameObject button_Back;
	public GameObject button_Start;
	public GameObject button_Delete;
	public TMP_Dropdown dropdown_FileNames;
	public TMP_Dropdown dropdown_SheepAlgorithm;
	public TMP_Dropdown dropdown_SpawnType;
	public TMP_InputField inputField_FlockSize;
	public TMP_InputField inputField_SDDist;

	public GameObject part2Menu;

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

		UpdateFileListAndChanges();
		
	}

	void UpdateFileListAndChanges ()
    {
		files = uiStackManager.sc.systemController.GetValidFilesInDirectory(0);

		if (files.Count > 0)
		{
			DoFilesDropdown();
			UpdateOtherFields();
		}
		else
		{
			//their are no saved files so just laod the default settings
			uiStackManager.sc.systemController.simulationSettings.SetDefaultSettings();

			dropdown_FileNames.options.Clear();
			dropdown_FileNames.options.Add(new TMP_Dropdown.OptionData() { text = "" });
			int TempInt = dropdown_FileNames.value;
			dropdown_FileNames.value = dropdown_FileNames.value + 1;
			dropdown_FileNames.value = TempInt;

			dropdown_SheepAlgorithm.options.Clear();
			dropdown_SheepAlgorithm.options.Add(new TMP_Dropdown.OptionData() { text = uiStackManager.sc.systemController.simulationSettings.sheepAlgorithm.ToString() });
			TempInt = dropdown_SheepAlgorithm.value;
			dropdown_SheepAlgorithm.value = dropdown_SheepAlgorithm.value + 1;
			dropdown_SheepAlgorithm.value = TempInt;

			dropdown_SpawnType.options.Clear();
			dropdown_SpawnType.options.Add(new TMP_Dropdown.OptionData() { text = uiStackManager.sc.systemController.simulationSettings.spawnType.ToString() });
			TempInt = dropdown_SpawnType.value;
			dropdown_SpawnType.value = dropdown_SpawnType.value + 1;
			dropdown_SpawnType.value = TempInt;

			inputField_FlockSize.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.flockSize.ToString();
			inputField_SDDist.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.sheepdogRepulsionDistance.ToString();
		}
	}

	void DoFilesDropdown ()
    {
		List<string> names = new List<string> ();

		dropdown_FileNames.options.Clear();


		for (int i = 0; i < files.Count; i++)
		{
			dropdown_FileNames.options.Add(new TMP_Dropdown.OptionData() { text = files[i] });
		}

		dropdown_FileNames.value = 1; //force it to update due to a change
		dropdown_FileNames.value = 0;
	}



	void UpdateOtherFields ()
    {
		string currentName = dropdown_FileNames.options[dropdown_FileNames.value].text;

		uiStackManager.sc.systemController.SetSimulationSettingsFromFileName(currentName, 0);
		
		
		dropdown_SheepAlgorithm.options.Clear();
		dropdown_SheepAlgorithm.options.Add(new TMP_Dropdown.OptionData() { text = uiStackManager.sc.systemController.simulationSettings.sheepAlgorithm.ToString() });
		int TempInt = dropdown_SheepAlgorithm.value;
		dropdown_SheepAlgorithm.value = dropdown_SheepAlgorithm.value + 1;
		dropdown_SheepAlgorithm.value = TempInt;

		dropdown_SpawnType.options.Clear();
		dropdown_SpawnType.options.Add(new TMP_Dropdown.OptionData() { text = uiStackManager.sc.systemController.simulationSettings.spawnType.ToString() });
		TempInt = dropdown_SpawnType.value;
		dropdown_SpawnType.value = dropdown_SpawnType.value + 1;
		dropdown_SpawnType.value = TempInt;

		inputField_FlockSize.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.flockSize.ToString();
		inputField_SDDist.GetComponent<TMP_InputField>().text = uiStackManager.sc.systemController.simulationSettings.sheepdogRepulsionDistance.ToString();
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



	public void NameDropdownValueChanged ()
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

		uiStackManager.AddComponentToStack(part2Menu);

	}


	public void EditButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}


	public void DeleteButtonPressed()
	{
		if (files.Count > 0)
        {
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

			uiStackManager.sc.systemController.DeleteFile(fileName, 0);

			UpdateFileListAndChanges();
		}
		
	}


	public void BackButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack(gameObject);
	}
}
