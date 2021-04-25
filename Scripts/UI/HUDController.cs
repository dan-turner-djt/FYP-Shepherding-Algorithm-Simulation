using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDController : GenericMenu
{
    SimulationSceneController sc;

    public GameObject button_Quit;
	public GameObject button_ShowSettings;
	public GameObject button_Play;
	public GameObject button_Speed;
	public GameObject button_Restart;
	public GameObject button_Camera;
	public GameObject button_SaveResults;
	public GameObject button_Watch;

	public TextMeshProUGUI timerText;
	public TextMeshProUGUI distanceText;
	public TextMeshProUGUI progressText;
	public GameObject learningProgress;
	public TextMeshProUGUI learningProgressText;


	private void Awake()
    {
		base.DoAwake();

        sc = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SimulationSceneController>();
    }

    public void DoStart()
    {
		base.DoStart();

		interactables.Add(button_Quit);
		interactables.Add(button_ShowSettings);
		interactables.Add(button_Play);
		interactables.Add(button_Speed);
		interactables.Add(button_Restart);
		interactables.Add(button_Camera);
		interactables.Add(button_SaveResults);
		interactables.Add(button_Watch);


		if (sc.isLearning)
        {
			learningProgress.SetActive(true);
        }
		else
        {
			learningProgress.SetActive(false);
		}
	}


	public void UpdateInfo (float timerTime, float distance, int progress, int totalSheep)
    {
		float t = timerTime;

		float minutes = t / 60;
		string minutesTens = ((int)minutes / 10).ToString();
		string minutesUnits = ((int)minutes % 10).ToString();
		float seconds = t % 60;
		string secondsTens = ((int)seconds / 10).ToString();
		string secondsUnits = ((int)seconds % 10).ToString();
		float decimalSeconds = (t % 1) * 100;
		string decimalSecondsTens = ((int)decimalSeconds / 10).ToString();
		string decimalSecondsUnits = ((int)decimalSeconds % 10).ToString();

		timerText.SetText(minutesTens + "" + minutesUnits + ":" + secondsTens + "" + secondsUnits + "." + decimalSecondsTens + "" + decimalSecondsUnits);

		distanceText.SetText((Mathf.Round (distance * 100)/100).ToString());


		string progressString = "";
		if (progress == totalSheep)
		{
			progressString = "COMPLETE";
		}
		else
        {
			progressString = progress.ToString() + "/" + totalSheep.ToString();
		}


		progressText.SetText(progressString);

		if (sc.isLearning)
        {
			string learningProgressString = "";
			if (sc.systemController.simulationSettings.currentIteration == sc.systemController.simulationSettings.maxIterations)
			{
				learningProgressString = "COMPLETE";
			}
			else
			{
				learningProgressString = (Mathf.FloorToInt(sc.systemController.simulationSettings.currentIteration/10)+1).ToString() + "/" + sc.systemController.simulationSettings.learningTryIterations.ToString() + ", ";
				learningProgressString += ((sc.systemController.simulationSettings.currentIteration % sc.systemController.simulationSettings.numForAverage)+1).ToString() + "/" + sc.systemController.simulationSettings.numForAverage.ToString() + ", ";
				learningProgressString += ("Testing CMD = " + sc.systemController.simulationSettings.collectAmount + ", CSA = " + sc.systemController.simulationSettings.PHP + ", CDC = " + sc.systemController.simulationSettings.PHP2);
			}


			learningProgressText.SetText(learningProgressString);
		}


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


	public void ShowSettingsButtonPressed (GameObject nextMenu)
    {
		uiStackManager.AddComponentToStack(nextMenu);
    }

	public void PlayButtonPressed()
	{
		sc.TogglePause();

		int p = sc.simulationRunning;

		if (p == 0)
        {
			button_Play.GetComponentInChildren<TextMeshProUGUI>().text = "PLAY";
        }
        else
        {
			button_Play.GetComponentInChildren<TextMeshProUGUI>().text = "PAUSE";
		}
	}

	public void SpeedButtonPressed()
	{
		sc.ChangeSpeed();

		UpdateSpeedButton();
	}

	public void UpdateSpeedButton()
    {
		float s = sc.simulationSpeed;
		button_Speed.GetComponentInChildren<TextMeshProUGUI>().text = "SPEED " + s.ToString();
	}

	public void CameraButtonPressed()
	{
		sc.ChangeCameraMode();
	}

	public void RestartButtonPressed()
	{
		sc.RestartSimulation();
	}


	public void QuitButtonPressed()
	{
		toBeConfirmed = "quitButtonAction";
		string message = "Are you sure you want to quit?";
		uiStackManager.AskForResponse(message);
	}


	public void ActivateWatchButton ()
    {
		button_Watch.SetActive(true);
    }

	public void ActivateSaveResultsButton()
	{
		button_SaveResults.SetActive(true);
	}

	public void SaveResultsButtonPressed()
	{
		sc.SaveSimulationResults();
	}

	public void WatchButtonPressed()
	{
		sc.ToggleIsWatching();

		if (SimulationSceneController.isWatching)
        {
			button_Watch.GetComponentInChildren<TextMeshProUGUI>().text = "SPEED UP";

			sc.ChangeToWatch();

		}
		else
        {
			button_Watch.GetComponentInChildren<TextMeshProUGUI>().text = "WATCH";

			sc.ChangeToSpeedUp();
		}
	}


	public void QuitButtonAction(bool response)
	{
		if (response)
		{
			sc.ExitSimulation();
		}

	}
}
