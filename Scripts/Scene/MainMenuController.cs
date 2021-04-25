using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuController : GeneralSceneController
{

    public GameObject homeMenuObject;


    void Awake()
    {
        base.DoAwake();
            
    }


    void Start()
    {
        base.DoStart();

        UIStackManager.AddComponentToStack(homeMenuObject);
    }

    private void Update()
    {
        base.DoUpdate();
    }


    public override bool CanRemoveBottomMenu(GameObject menu)
    {
        return false;
    }


   


}
