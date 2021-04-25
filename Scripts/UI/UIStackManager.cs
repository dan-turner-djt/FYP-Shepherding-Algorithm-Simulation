using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStackManager : MonoBehaviour
{

	public GeneralSceneController sc;
	GeneralInput input;

	public GameObject confirmMenuPrefab;
	public GameObject messageMenuPrefab;

	List<ComponentInfo> UIStack = new List<ComponentInfo>();


	void Start()
	{
		sc = GameObject.FindGameObjectWithTag("SceneController").GetComponent<GeneralSceneController>();
		input = gameObject.GetComponent<GeneralInput>();
	}


	public void DoUpdate()
	{
		if (UIStack.Count > 0)
		{
			if (input.GetButtonDown("Cancel"))
			{
				if (UIStack.Count == 1)
				{
					if (sc.CanRemoveBottomMenu(UIStack[0].componentObject))
					{
						CancelComponent();
					}
				}
				else
				{
					CancelComponent();
				}



			}

		}


		if (UIStack.Count > 0)
		{
			UIStack[UIStack.Count - 1].GetScript().DoUpdate();
		}

	}


	public void CancelComponent()
	{
		if (UIStack[UIStack.Count - 1].componentObject.tag == "ConfirmMenu")
		{
			//turns a cancel into a "no" response
			GetConfirmationResponse(false, UIStack[UIStack.Count - 1].componentObject);
		}
		else
		{
			UIStack[UIStack.Count - 1].GetScript().Cancelled();
			RemoveComponentFromStack(UIStack[UIStack.Count - 1].componentObject, true);
		}


	}


	public void AddComponentToStack(GameObject component)
	{
		ComponentInfo componentInfo = new ComponentInfo(component);
		GenericUIComponent.ComponentType componentType = componentInfo.GetScript().componentType;


		if (UIStack.Count > 0)
		{
			//deal with previous component

			ComponentInfo previousComponentInfo = UIStack[UIStack.Count - 1];
			GenericUIComponent.ComponentType previousComponentType = previousComponentInfo.GetScript().componentType;

			if (previousComponentType == GenericUIComponent.ComponentType.Menu)
			{
				if (componentType == GenericUIComponent.ComponentType.Menu)
				{
					//MenuInfo menuInfo = new MenuInfo (component);
					MenuInfo previousMenuInfo = new MenuInfo(previousComponentInfo.componentObject);
					DisableMenu(previousMenuInfo, componentInfo);
				}
			

			}
		}


		UIStack.Add(componentInfo);



		if (componentType == GenericUIComponent.ComponentType.Menu)
		{
			//deal with new component

			MenuInfo menuInfo = new MenuInfo(component);
			EnableMenu(menuInfo, new ComponentInfo(null));
		}
	}


	public void RemoveComponentFromStack(GameObject component, bool cancelled = false)
	{
		ComponentInfo currentComponent = UIStack[UIStack.Count - 1];
		GenericUIComponent.ComponentType currentComponentType = currentComponent.GetScript().componentType;

		if (!GameObject.ReferenceEquals(component, currentComponent.componentObject))
		{
			Debug.Log("trying to remove a component which isnt currently on the top of the stack!");
			return;
		}

		UIStack.RemoveAt(UIStack.Count - 1);


		if (currentComponentType == GenericUIComponent.ComponentType.Menu)
		{
			//deal with component removed

			MenuInfo menuInfo = new MenuInfo(currentComponent.componentObject);

			if (menuInfo.GetScript().temporaryMenu)
			{
				Destroy(menuInfo.menuObject);
			}
			else
			{
				DisableMenu(menuInfo, new ComponentInfo(null));
			}


		}




		if (UIStack.Count > 0)
		{
			//deal with new component reenabled

			ComponentInfo newComponent = UIStack[UIStack.Count - 1];
			GenericUIComponent.ComponentType newComponentType = newComponent.GetScript().componentType;

			if (newComponentType == GenericUIComponent.ComponentType.Menu)
			{
				if (currentComponentType == GenericUIComponent.ComponentType.Menu)
				{
					//MenuInfo currentMenuInfo = new MenuInfo (currentComponent.componentObject);
					MenuInfo newMenuInfo = new MenuInfo(newComponent.componentObject);
					EnableMenu(newMenuInfo, currentComponent);
				}

			}

		}


	}


	public GameObject GetLatestStackGameObject()
	{
		if (UIStack.Count > 0)
		{
			return UIStack[UIStack.Count - 1].componentObject;
		}

		return null;
	}


	/*public void AddMenuToStack (GameObject menu)
	{
		MenuInfo menuInfo = new MenuInfo (menu);


		if (UIStack.Count > 0) 
		{
			//disable first menu
			DisableMenu(UIStack[UIStack.Count-1], menuInfo);
		}


		UIStack.Add (menuInfo);
		EnableMenu (menuInfo, new MenuInfo (null));


	}*/

	/*public void RemoveMenuFromStack ()
	{
		MenuInfo currentMenu = UIStack [UIStack.Count - 1];
		currentMenu.GetScript().TurnedOff ();

		UIStack.RemoveAt (UIStack.Count - 1);
		DisableMenu (currentMenu, new MenuInfo (null));


		if (UIStack.Count > 0) 
		{
			//reenable lower level menu
			MenuInfo newMenu = UIStack[UIStack.Count-1];
			EnableMenu(newMenu, currentMenu);
		}
	}*/


	public void EnableMenu(MenuInfo menu, ComponentInfo previousComponent)
	{

		if (previousComponent.componentObject == null)
		{
			//going up in stack

			menu.menuObject.SetActive(true);
			menu.GetScript().TurnedOn(null);
		}
		else
		{
			//going down in stack

			GenericUIComponent.ComponentType previousComponentType = previousComponent.GetScript().componentType;

			if (previousComponentType == GenericUIComponent.ComponentType.Menu)
			{
				MenuInfo previousMenu = new MenuInfo(previousComponent.componentObject);

				//removing an old menu
				GenericMenu.MenuType previousMenuType = previousMenu.GetScript().menuType;

				if (previousMenuType == GenericMenu.MenuType.Fixed)
				{
					menu.menuObject.SetActive(true);
					menu.GetScript().TurnedOn(previousMenu.menuObject);
				}
				else
				{
					menu.GetScript().WakeMenu();
					menu.GetScript().TurnedOn(previousMenu.menuObject);
				}
			}

		}
	}


	public void DisableMenu(MenuInfo menu, ComponentInfo newComponent)
	{

		if (newComponent.componentObject != null)
		{
			//going up in stack

			GenericUIComponent.ComponentType newComponentType = newComponent.GetScript().componentType;

			if (newComponentType == GenericUIComponent.ComponentType.Menu)
			{
				MenuInfo newMenu = new MenuInfo(newComponent.componentObject);
				GenericMenu.MenuType newMenuType = newMenu.GetScript().menuType;

				if (newMenuType == GenericMenu.MenuType.Fixed)
				{
					menu.GetScript().TurnedOff();
					menu.menuObject.SetActive(false);
				}
				else
				{
					menu.GetScript().SleepMenu();
				}
			}

		}

		else
		{
			//going down in stack

			menu.GetScript().TurnedOff();
			menu.menuObject.SetActive(false);
		}

	}


	public void SendMessageMenu (string message)
    {
		GameObject messageMenuInstance = Instantiate(messageMenuPrefab, UIStack[UIStack.Count - 1].componentObject.transform.parent);
		AddComponentToStack(messageMenuInstance);
		UIStack[UIStack.Count - 1].GetScript().SetMessage(message);
	}


	public void AskForResponse(string message)
	{
		

		GameObject confirmMenuInstance = Instantiate(confirmMenuPrefab, UIStack[UIStack.Count - 1].componentObject.transform.parent);
		AddComponentToStack(confirmMenuInstance);
		UIStack[UIStack.Count - 1].GetScript().SetMessage(message);

	}


	public void GetConfirmationResponse(bool response, GameObject menuName)
	{
		if (UIStack[UIStack.Count - 1].componentObject.Equals(menuName))
		{
			RemoveComponentFromStack(UIStack[UIStack.Count - 1].componentObject);
			UIStack[UIStack.Count - 1].GetScript().ReceiveConfirmation(response);

		}
		else
		{
			Debug.LogError("Error! Menu not on top of stack!");
		}

	}


	public void PassResponseUp(bool response)
	{
		UIStack[UIStack.Count - 2].GetScript().ReceiveConfirmation(response);
	}


	public struct ComponentInfo
	{
		public GameObject componentObject;
		GenericUIComponent script;

		public ComponentInfo(GameObject component)
		{
			this.componentObject = component;
			script = null;
			//script = menuObject.GetComponent<GenericMenu> ();
		}

		public GenericUIComponent GetScript()
		{
			if (script == null)
			{
				script = componentObject.GetComponent<GenericUIComponent>();
			}

			return script;
		}
	}


	public struct MenuInfo
	{
		public GameObject menuObject;
		GenericMenu script;

		public MenuInfo(GameObject menu)
		{
			this.menuObject = menu;
			script = null;
			//script = menuObject.GetComponent<GenericMenu> ();
		}

		public GenericMenu GetScript()
		{
			if (script == null)
			{
				script = menuObject.GetComponent<GenericMenu>();
			}

			return script;
		}
	}

}
