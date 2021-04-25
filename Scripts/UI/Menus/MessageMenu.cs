using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MessageMenu : GenericMenu
{
	public TextMeshProUGUI textObj;


	void Awake()
	{
		base.DoAwake();
	}

	void Start()
	{
		base.DoStart();
	}


	public override void DoUpdate()
	{

	}


	public override void TurnedOn(GameObject previousMenu)
	{

	}


	public override void SetMessage(string message)
	{
		textObj.SetText(message);
	}


	public void OKButtonPressed()
	{
		uiStackManager.RemoveComponentFromStack(gameObject);
	}


}
