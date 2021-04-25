using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class MM_ResultsMenu : GenericMenu
{
	public TableController tableController;

	public GameObject button_Back;
	public GameObject button_Switch;
	public TextMeshProUGUI button_Switch_Text;
	public GameObject name_Dropdown;
	public GameObject order_Dropdown;
	public GameObject group_Dropdown;
	public TMP_Dropdown dropdown_Order;
	public TMP_Dropdown dropdown_Group;
	public TMP_Dropdown dropdown_FileNames;
	

	List<string> files;
	List<string> orderOptions = new List<string>();
	List<string> groupOptions = new List<string>();

	int displayMode = 0; //0 = all, 1 = single

	private void Awake()
	{
		base.DoAwake();
	}

	void Start()
	{
		base.DoStart();

		interactables.Add(button_Switch);
		interactables.Add(button_Back);
		
		interactables.Add(dropdown_FileNames.gameObject);
		interactables.Add(dropdown_Order.gameObject);
		interactables.Add(dropdown_Group.gameObject);

		groupOptions.Add("None");
		groupOptions.Add("Algorithm");
		groupOptions.Add("Flock Size");

		orderOptions.Add("Time");
		orderOptions.Add("Distance");

		displayMode = 0;
		SetDisplayMode();
	}


	public override void TurnedOn(GameObject previousMenu)
	{
		files = uiStackManager.sc.systemController.GetValidFilesInDirectory(0);

		UpdateFileListAndChanges();

	}

	void UpdateFileListAndChanges()
	{
		DoFilesDropdown();
		DoGroupDropdown();
		DoOrderDropdown();

		UpdateTable();
		
	}

	void DoFilesDropdown()
	{
		List<string> names = new List<string>();

		dropdown_FileNames.options.Clear();



		for (int i = 0; i < files.Count; i++)
		{
			dropdown_FileNames.options.Add(new TMP_Dropdown.OptionData() { text = files[i] });
		}

		dropdown_FileNames.value = 0;
	}

	void DoOrderDropdown()
	{
		
		dropdown_Order.options.Clear();


		for (int i = 0; i < orderOptions.Count; i++)
		{
			dropdown_Order.options.Add(new TMP_Dropdown.OptionData() { text = orderOptions[i] });
		}

		dropdown_Order.value = 0;
	}


	void DoGroupDropdown()
	{
		
		dropdown_Group.options.Clear();


		for (int i = 0; i < groupOptions.Count; i++)
		{
			dropdown_Group.options.Add(new TMP_Dropdown.OptionData() { text = groupOptions[i] });
		}

		dropdown_Group.value = 0;
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



	public void NameDropdownValueChanged()
	{
		if (files.Count > 0)
		{
			UpdateTable();
		}

	}


	public void OrderDropdownValueChanged()
    {
		if (files.Count > 0)
		{
			UpdateTable();
		}
	}

	public void GroupDropdownValueChanged()
	{
		if (files.Count > 0)
		{
			UpdateTable();
		}
	}

	public void UpdateTable ()
    {

		if (displayMode == 0)
        {
			string orderBy;
			string groupBy;

			if (dropdown_Order.options.Count > 0)
            {
				orderBy = dropdown_Order.options[dropdown_Order.value].text;
				groupBy = dropdown_Group.options[dropdown_Group.value].text;
			}
			else
            {
				orderBy = "Time";
				groupBy = "None";
			}

			
			
			UpdateTableForAll(orderBy, groupBy);
        }
		else
        {
			UpdateTableForSingle();
        }

    }

	public void UpdateTableForSingle ()
    {

		string currentName = dropdown_FileNames.options[dropdown_FileNames.value].text;

		List<string> results = uiStackManager.sc.systemController.GetResultsFromName(currentName, 0);
		List<List<string>> allResults = new List<List<string>>();

		bool AI = false;

		for (int j = 0; j < results.Count; j++)
		{
			var resultsInfo = results[j].Split(',');
			string time = resultsInfo[0];
			string distance = resultsInfo[1];


			string name = "";

			
			//Debug.Log("time: " + time + "distance: " + distance);

			
			name = j.ToString();
			
			List<string> resultsRow = new List<string>();
			resultsRow.Add(name);
			var settingsPart = uiStackManager.sc.systemController.GetSettingsFromName(currentName, 0);
			resultsRow.Add(settingsPart[0]);
			resultsRow.Add(settingsPart[1]);
			resultsRow.Add(settingsPart[2]);

			if (AI)
			{
				resultsRow.Add(resultsInfo[2]); //CMD
				resultsRow.Add(resultsInfo[3]); //PHP
				resultsRow.Add(resultsInfo[4]); //PHP2

				Debug.Log(resultsInfo[2] + ", " + resultsInfo[3]);
			}
			else
            {
				resultsRow.Add(settingsPart[4]); //CMD
				resultsRow.Add(settingsPart[5]); //PHP
				resultsRow.Add(settingsPart[6]); //PHP2
			}

			
			resultsRow.Add(time);
			resultsRow.Add(distance);

			allResults.Add(resultsRow);

			AI = (settingsPart[3].Equals("YES")) ? true : false;

		}

		//get best one

		if (AI)
        {
			float bestScore = Mathf.Infinity;
			int bestResult = 0;

			for (int k = 0; k < results.Count; k++)
            {
				var resultsInf = results[k].Split(',');
				string time = resultsInf[0];
				string distance = resultsInf[1];

				float score = (float.Parse(time) + float.Parse(distance)) / 2;

				if (score < bestScore)
                {
					bestScore = score;
					bestResult = k;
                }
			}

			var resultsInfo = results[bestResult].Split(',');
			string bestTime = resultsInfo[0];
			string bestDistance = resultsInfo[1];

			List<string> resultsRow = new List<string>();
			resultsRow.Add("BEST");
			var settingsPart = uiStackManager.sc.systemController.GetSettingsFromName(currentName, 0);
			resultsRow.Add(settingsPart[0]);
			resultsRow.Add(settingsPart[1]);
			resultsRow.Add(settingsPart[2]);
			resultsRow.Add(resultsInfo[2]); //CMD
			resultsRow.Add(resultsInfo[3]); //PHP
			resultsRow.Add(resultsInfo[4]); //PHP2
			resultsRow.Add(bestTime);
			resultsRow.Add(bestDistance);

			Debug.Log("best:" + resultsInfo[2] + ", " + resultsInfo[3]);

			allResults.Add(resultsRow);

		}


		
		tableController.UpdateTable(allResults, AI);
	}

	public void UpdateTableForAll (string orderBy = "", string groupBy = "None")
    {
		
		List<List<string>> allResults = new List<List<string>>();

		for (int i = 0; i < files.Count; i++)
		{
			
			List<string> results = uiStackManager.sc.systemController.GetResultsFromName(files[i], 0);

			float totalTime = 0;
			float totalDistance = 0;
			float averageTime = 0;
			float averageDistance = 0;

			for (int j = 0; j < results.Count; j++)
			{
				var resultsInfo = results[j].Split(',');
				string time = resultsInfo[0];
				string distance = resultsInfo[1];
				string name = j.ToString();

				totalTime += float.Parse(time);
				totalDistance += float.Parse(distance);
			}

			if (results.Count > 0)
			{
				averageTime = totalTime / results.Count;
				averageDistance = totalDistance / results.Count;
			}

			List<string> resultsRow = new List<string>();
			resultsRow.Add(files[i]);

			var settingsPart = uiStackManager.sc.systemController.GetSettingsFromName(files[i], 0);
			resultsRow.Add(settingsPart[0]);
			resultsRow.Add(settingsPart[1]);
			resultsRow.Add(settingsPart[2]);
			resultsRow.Add(settingsPart[4]); //CMD
			resultsRow.Add(settingsPart[5]); //PHP
			resultsRow.Add(settingsPart[6]); //PHP2
			resultsRow.Add(averageTime.ToString());
			resultsRow.Add(averageDistance.ToString());

			allResults.Add(resultsRow);
		}

		List<List<string>> sortedResults = allResults;

		if (orderBy != "")
		{
			sortedResults = OrderResults(sortedResults, orderBy);
		}

		if (groupBy != "None")
        {
			sortedResults = GroupResults(sortedResults, groupBy);
        }

		
		
		tableController.UpdateTableForAll(sortedResults);

	}
	
	public List<List<string>> OrderResults (List<List<string>> results, string orderBy)
    {
		List<List<string>> sortedResults = results;

		switch (orderBy)
        {
			case "Time":
				sortedResults = results.OrderBy(o => float.Parse(o[4])).ToList();
				break;
			case "Distance":
				sortedResults = results.OrderBy(o => float.Parse(o[5])).ToList();
				break;
			default:
				break;

		}

		return sortedResults;
	}

	public List<List<string>> GroupResults(List<List<string>> results, string groupBy)
	{
		List<List<string>> sortedResults = results;

		switch (groupBy)
		{
			case "Flock Size":
				sortedResults = results.OrderBy(o => float.Parse(o[2])).ToList();
				break;
			case "Algorithm":
				sortedResults = results.OrderBy(o => (o[1])).ToList();
				break;
			case "None":
				break;
			default:
				break;

		}

		Debug.Log("grouped");

		return sortedResults;
	}



	public void SwitchMode ()
    {
		if (displayMode == 0)
        {
			name_Dropdown.SetActive(false);
			order_Dropdown.SetActive(true);
			group_Dropdown.SetActive(true);
			tableController.UpdateMode(0);
        }
		else
        {
			name_Dropdown.SetActive(true);
			order_Dropdown.SetActive(false);
			group_Dropdown.SetActive(false);
			tableController.UpdateMode(1);
		}
    }

	public void SwitchButtonPressed ()
    {
		if (displayMode == 0)
		{
			displayMode = 1;
		}
		else
        {
			displayMode = 0;
		}

		SetDisplayMode();
    }

	public void SetDisplayMode ()
    {
		if (displayMode == 0)
		{
			button_Switch_Text.SetText("SINGLE");
		}
		else
		{
			button_Switch_Text.SetText("ALL");
		}

		SwitchMode();
		UpdateFileListAndChanges();
	}


	public void BackButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack(gameObject);
	}
}

