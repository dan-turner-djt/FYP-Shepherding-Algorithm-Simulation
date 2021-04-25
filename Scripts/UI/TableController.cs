using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class TableController : MonoBehaviour
{
    public GameObject entryTemplateForSingle;
    public GameObject entryTemplateForAll;
    public GameObject contentContainer;
    public GameObject headForAll;

    List<GameObject> entries = new List<GameObject>();

    public void DoStart ()
    {


    }


    public void UpdateMode (int mode)
    {
        if (mode == 0)
        {
            headForAll.SetActive(true);
        }
        else
        {
            headForAll.SetActive(true);
        }
    }


    public void UpdateTable(List<List<string>> results, bool AI)
    {
        Debug.Log("update single!");

        DestroyOldEntries();

        float totalTime = 0;
        float totalDistance = 0;

        for (int i = 0; i < results.Count; i++)
        {
            List<string> resultsInfo = new List<string>();
            resultsInfo.AddRange(results[i]);

            string algorithm = resultsInfo[1];
            string flockSize = resultsInfo[2];
            string SRD = resultsInfo[3];
            string CMD = resultsInfo[4];
            string PHP = resultsInfo[5];
            string PHP2 = resultsInfo[6];
            string time = resultsInfo[7];
            string distance = resultsInfo[8];
            string name = resultsInfo[0];

            totalTime += float.Parse(time);
            totalDistance += float.Parse(distance);

            Debug.Log("name: " + name + ", time: " + time + ", distance: " + distance);

            CMD = ((Mathf.Round(float.Parse(CMD) * 100)) / 100.0).ToString();
            PHP = ((Mathf.Round(float.Parse(PHP) * 100)) / 100.0).ToString();
            PHP2 = ((Mathf.Round(float.Parse(PHP2) * 100)) / 100.0).ToString();


            CreateNewEntryExtended(name, algorithm, flockSize, SRD, CMD, PHP, PHP2, time, distance);
        }

        if (results.Count > 0)
        {
            if (!AI)
            {
                float averageTime = totalTime / results.Count;
                float averageDistance = totalDistance / results.Count;

                CreateNewEntryExtended("Average", "", "", "", "", "", "", averageTime.ToString(), averageDistance.ToString());
            }
        }

        
    }


    public void UpdateTableForAll (List<List<string>> allResults)
    {
        Debug.Log("update all!");


        DestroyOldEntries();


        for (int i = 0; i < allResults.Count; i++)
        {
            List<string> resultsInfo = allResults[i];
            string name = resultsInfo[0];
            string algorithm = resultsInfo[1];
            string flockSize = resultsInfo[2];
            string SRD = resultsInfo[3];
            string CMD = resultsInfo[4];
            string PHP = resultsInfo[5];
            string PHP2 = resultsInfo[6];
            string time = resultsInfo[7];
            string distance = resultsInfo[8];
            
            Debug.Log("name: " + name + ", time: " + time + ", distance: " + distance);

            CMD = ((Mathf.Round(float.Parse(CMD) * 100)) / 100.0).ToString();
            PHP = ((Mathf.Round(float.Parse(PHP) * 100)) / 100.0).ToString();
            PHP2 = ((Mathf.Round(float.Parse(PHP2) * 100)) / 100.0).ToString();


            CreateNewEntryExtended (name, algorithm, flockSize, SRD, CMD, PHP, PHP2, time, distance);
        }


    }


    void CreateNewEntry (string name, string time, string distance)
    {
        time = FormatTimeString.FormatTextForShowing(float.Parse(time));

        GameObject newEntry = Instantiate(entryTemplateForSingle);
        newEntry.transform.SetParent(contentContainer.transform, false);
        TextMeshProUGUI nameText = newEntry.transform.Find("NameLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI timeText = newEntry.transform.Find("TimeLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI distanceText = newEntry.transform.Find("DistanceLabel").GetComponent<TextMeshProUGUI>();

        nameText.SetText(name);
        timeText.SetText(time);
        distanceText.SetText(distance);

        entries.Add(newEntry);
    }


    void CreateNewEntryExtended (string name, string algorithm, string flockSize, string SRD, string CMD, string PHP, string PHP2, string time, string distance)
    {
        time = FormatTimeString.FormatTextForShowing(float.Parse(time));

        GameObject newEntry = Instantiate(entryTemplateForAll);
        newEntry.transform.SetParent(contentContainer.transform, false);
        TextMeshProUGUI nameText = newEntry.transform.Find("NameLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI algorithmText = newEntry.transform.Find("AlgorithmLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI flockSizeText = newEntry.transform.Find("FlockSizeLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI SRDText = newEntry.transform.Find("SRDLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI CMDText = newEntry.transform.Find("CMDLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI PHPText = newEntry.transform.Find("PHPLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI PHP2Text = newEntry.transform.Find("PHP2Label").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI timeText = newEntry.transform.Find("TimeLabel").GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI distanceText = newEntry.transform.Find("DistanceLabel").GetComponent<TextMeshProUGUI>();

        nameText.SetText(name);
        algorithmText.SetText(algorithm);
        flockSizeText.SetText(flockSize);
        SRDText.SetText(SRD);
        CMDText.SetText(CMD);
        PHPText.SetText(PHP);
        PHP2Text.SetText(PHP2);
        timeText.SetText(time);
        distanceText.SetText(distance);

        entries.Add(newEntry);
    }

    void DestroyOldEntries()
    {
        foreach (GameObject entry in entries)
        {
            Destroy(entry);
        }

        entries.Clear();
    }
}
