using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MM_HomeMenu : GenericMenu
{

	public GameObject button_Start;
	public GameObject button_Results;
	public GameObject button_Settings;
	public GameObject button_Quit;


	private void Awake()
    {
        base.DoAwake();
    }

    private void Start()
    {
        base.DoStart();

		interactables.Add(button_Start);
		interactables.Add(button_Results);
		interactables.Add(button_Settings);
		interactables.Add(button_Quit);
	}


	public override void TurnedOn(GameObject previousMenu)
	{

	}


	public override void ReceiveConfirmation(bool response)
	{

		if (toBeConfirmed == "quitButtonAction")
		{
			QuitButtonAction(response);
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



	public void StartButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}

	public void ResultsButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}


	public void SettingsButtonPressed(GameObject nextMenu)
	{
		uiStackManager.AddComponentToStack(nextMenu);
	}

	public void QuitButtonPressed()
	{
		toBeConfirmed = "quitButtonAction";
		string message = "Are you sure you want to quit?";
		uiStackManager.AskForResponse(message);
	}

	public void QuitButtonAction(bool response)
	{
		if (response)
		{
			uiStackManager.sc.systemController.QuitProgram();
		}

	}
}
