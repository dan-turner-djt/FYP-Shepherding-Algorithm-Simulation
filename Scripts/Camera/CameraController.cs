using System.Collections;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    SimulationSceneController sc;

    public GameObject overheadCamPos;

    public float sensitivity = 10f;
    public Transform sheepDog;
    public Transform followPosition;
    public Transform cameraTransform;
    public Transform cameraHolder;

    float pitch;
    float yDefault = 15;
    float yMin = 0;
    float yMax = 80f;
    float distance;
    float dDefault = 7;
    float overheadDistance = 25;
    float dMin = 4;
    float dMax = 40;

    Vector3 startPos;

    private void Start()
    {
        sc = GameObject.FindGameObjectWithTag("SceneController").GetComponent<SimulationSceneController>();

        distance = 16;
        pitch = yDefault;
        transform.localEulerAngles = new Vector3(pitch, transform.localEulerAngles.y, transform.localEulerAngles.z);

        startPos = transform.position;

        SetCameraMode();

        
    }


    public void DoFinalUpdate ()
    {
        int cameraMode = sc.systemController.simulationSettings.cameraMode;

        if (cameraMode != 2 && Input.GetKey(KeyCode.LeftShift))
        {
            float yaw = Input.GetAxisRaw("Mouse X") * sensitivity;
            Vector3 savedPos = transform.position;
            cameraHolder.Rotate(0, yaw, 0);
            transform.position = savedPos;
        }

        if (cameraMode == 1)
        {
            transform.position = followPosition.position;
        }
        else if (cameraMode == 0)
        {
            if (!Input.GetKey(KeyCode.Space))
            {
                MoveCameraControls();
            }
            
        }
       
        
       
        if (cameraMode != 2 && Input.GetKey(KeyCode.LeftShift))
        {
            pitch += -Input.GetAxisRaw("Mouse Y") * sensitivity;
            pitch = Mathf.Clamp(pitch, yMin, yMax);

            transform.localEulerAngles = new Vector3(pitch, transform.localEulerAngles.y, transform.localEulerAngles.z);
        }

        distance += Input.mouseScrollDelta.y * -100 * Time.deltaTime;
        distance = Mathf.Clamp(distance, dMin, dMax);

        cameraTransform.position = transform.position + distance * -transform.forward;
        
        
        

        
    }


    void MoveCameraControls ()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 moveInp = new Vector3(h, 0, v);
        Vector3 transformedInput = Vector3.zero;

        if (moveInp != Vector3.zero)
        {
    
            transformedInput = Quaternion.FromToRotation(cameraTransform.up, Vector3.up) * (cameraTransform.rotation * moveInp);
            transformedInput.y = 0.0f;
            transformedInput.Normalize();
        }

        transform.position += transformedInput * 10 * Time.deltaTime;

        if (Input.GetKey (KeyCode.Space))
        {
            transform.position += Vector3.up * 10 * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftControl))
        {
            transform.position += -Vector3.up * 10 * Time.deltaTime;
        }
    }

    public void ChangeCameraMode ()
    {
        sc.systemController.simulationSettings.cameraMode++;

        if (sc.systemController.simulationSettings.cameraMode > 2)
        {
            sc.systemController.simulationSettings.cameraMode = 0;
        }

        SetCameraMode();
    }

    public void SetCameraMode ()
    {
        int cameraMode = sc.systemController.simulationSettings.cameraMode;

        if (cameraMode == 0)
        {
            ChangeToFreeMode();
        }
        else if (cameraMode == 1)
        {
            ChangeToFollowMode();
        }
        else if (cameraMode == 2)
        {
            ChangeToOverheadMode();
        }
    }

    void ChangeToFreeMode ()
    {
        transform.position = startPos;
        distance = 15;
        cameraTransform.position = transform.position + distance * -cameraHolder.forward;
        pitch = yDefault;
        transform.localEulerAngles = new Vector3(pitch, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    void ChangeToFollowMode ()
    {
        transform.position = followPosition.position;
        distance = dDefault;
        cameraTransform.position = transform.position + distance * -cameraHolder.forward;
        pitch = yDefault;
        transform.localEulerAngles = new Vector3(pitch, transform.localEulerAngles.y, transform.localEulerAngles.z);
    }

    void ChangeToOverheadMode()
    {
        transform.position = overheadCamPos.transform.position;
        pitch = 80;
        transform.localEulerAngles = new Vector3(pitch, transform.localEulerAngles.y, transform.localEulerAngles.z);
        cameraHolder.eulerAngles = Vector3.zero;
        distance = overheadDistance;
        cameraTransform.position = transform.position + distance * -cameraHolder.forward;
        transform.position = overheadCamPos.transform.position;
    }
}
