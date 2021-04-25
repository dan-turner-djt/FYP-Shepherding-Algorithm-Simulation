using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MM_SettingsMenu : GenericMenu
{

    public GameObject button_Back;
    public GameObject button_Apply;
	public TMP_Dropdown dropdown_Resolution;
	public TMP_Dropdown dropdown_Quality;


    private void Awake()
    {
        base.DoAwake();
    }

    void Start()
    {
        base.DoStart();

        interactables.Add(button_Back);
        interactables.Add(button_Apply);
		interactables.Add(dropdown_Resolution.gameObject);
		interactables.Add(dropdown_Quality.gameObject);

    }


	public override void TurnedOn(GameObject previousMenu)
	{
		DoResolutionDropdown();
		DoQualityDropdown();
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


	void DoResolutionDropdown()
	{
		dropdown_Resolution.options.Clear();

		List<Vector2> resolutionOptions = uiStackManager.sc.systemController.GetResolutionList();

		for (int i = 0; i < resolutionOptions.Count; i++)
		{
			string s = "" + resolutionOptions[i].x + "x" + resolutionOptions[i].y + "";
			dropdown_Resolution.options.Add(new TMP_Dropdown.OptionData() { text = s});
		}


		int currentIndex = uiStackManager.sc.systemController.GetCurrentResolution();
		dropdown_Resolution.value = currentIndex;

	}

	void DoQualityDropdown ()
    {
		dropdown_Quality.options.Clear();

		List<string> qualityOptions = uiStackManager.sc.systemController.GetQualityList();

		for (int i = 0; i < qualityOptions.Count; i++)
        {
			dropdown_Quality.options.Add(new TMP_Dropdown.OptionData() { text = qualityOptions[i] });
		}


		int currentIndex = uiStackManager.sc.systemController.GetCurrentQuality();
		dropdown_Quality.value = currentIndex;

	}


	public void ApplyButtonPressed()
	{
		ApplyButtonAction(true);
	}


	public void ApplyButtonAction (bool response)
    {
		int resolutionSelection = dropdown_Resolution.value;
		uiStackManager.sc.systemController.SetResolution(resolutionSelection);

		int qualitySelection = dropdown_Quality.value;
		uiStackManager.sc.systemController.SetQuality(qualitySelection);

		
	}


	public void BackButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack (gameObject);
	}


}
