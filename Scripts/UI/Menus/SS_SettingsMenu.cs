using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SS_SettingsMenu : GenericMenu
{
	public GameObject button_Close;
	public TMP_InputField settingsName;
	public TMP_InputField sheepAlgorithm;
	public TMP_InputField flockSize;
	

	private void Awake()
	{
		base.DoAwake();
	}

	void Start()
	{
		base.DoStart();

		interactables.Add(button_Close);
	

	}


	public override void TurnedOn(GameObject previousMenu)
	{
		UpdateFields();

	}


	void UpdateFields ()
    {
		settingsName.text = uiStackManager.sc.systemController.simulationSettings.settingsName;
		sheepAlgorithm.text = uiStackManager.sc.systemController.simulationSettings.sheepAlgorithm.ToString();
		flockSize.text = uiStackManager.sc.systemController.simulationSettings.flockSize.ToString();
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




	public void CloseButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack(gameObject);
	}
}
