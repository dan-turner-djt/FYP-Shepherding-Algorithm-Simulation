using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEditor;
using System;

public class SystemController : MonoBehaviour
{
	private static SystemController instance;

	
	
	// Fixed aspect ratio parameters
	static public bool FixedAspectRatio = true;
	static public float TargetAspectRatio = 16f / 9f;


	// List of horizontal resolutions to include
	int[] resolutions = new int[] { 800, 1024, 1280, 1400, 1600, 1920 };

	public Resolution DisplayResolution;
	public List<Vector2> WindowedResolutions;

	int currWindowedRes;


	public List<string> qualityList = new List<string>();


	public SimulationSettings simulationSettings;


	void Awake()
	{
		DontDestroyOnLoad(this.gameObject);

		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}


    void Start()
    {
		StartCoroutine(InitResolutions());

		simulationSettings = new SimulationSettings();

		CheckCreateFolders();
	}

	void Update()
    {
        
    }


	private IEnumerator InitResolutions ()
    {
		yield return null;

		DisplayResolution = Screen.currentResolution;


		float screenAspect = (float)DisplayResolution.width / DisplayResolution.height;

		WindowedResolutions = new List<Vector2>();

		foreach (int w in resolutions)
		{
			if (w < DisplayResolution.width)
			{
				// Adding resolution only if it's 20% smaller than the screen
				if (w < DisplayResolution.width * 0.8f)
				{
					Vector2 windowedResolution = new Vector2(w, Mathf.Round(w / TargetAspectRatio));
					if (windowedResolution.y < DisplayResolution.height * 0.8f)
						WindowedResolutions.Add(windowedResolution);
				}
			}
		}


		//FullscreenResolutions = FullscreenResolutions.OrderBy(resolution => resolution.x).ToList();

		bool found = false;


		for (int i = 0; i < WindowedResolutions.Count; i++)
		{
			if (WindowedResolutions[i].x == Screen.width && WindowedResolutions[i].y == Screen.height)
			{
				found = true;
				currWindowedRes = i;
				break;
			}
		}

		if (!found)
			SetResolution(WindowedResolutions.Count - 1);
		
	}


	public List<Vector2> GetResolutionList()
	{
		return WindowedResolutions;
	}


	public int GetCurrentResolution()
	{
		return currWindowedRes;
	}

	public void SetResolution(int index)
	{
		currWindowedRes = index;
		Vector2 r = WindowedResolutions[currWindowedRes];


		Debug.Log("Setting resolution to " + (int)r.x + "x" + (int)r.y);
		Screen.SetResolution((int)r.x, (int)r.y, false);
	}



	void FillQualityList()
	{
		for (int i = 0; i < QualitySettings.names.Length; i++)
		{
			qualityList.Add(QualitySettings.names[i]);
		}
	}

	public List<string> GetQualityList()
	{
		if (qualityList.Count == 0)
		{
			FillQualityList();
		}

		return qualityList;
	}


	public int GetCurrentQuality ()
    {
		return QualitySettings.GetQualityLevel();
    }

	public void SetQuality(int index)
	{
		QualitySettings.SetQualityLevel(index, true);
	}



	//file stuff
	public void WriteSimulationSettings (int folder)
    {
		string fileName;
		if (folder == 0)
        {
			fileName = simulationSettings.settingsName + ".txt";
		}
		else if (folder == 1)
        {
			fileName = simulationSettings.sheepSettingsName + ".txt";
		}
		else
        {
			fileName = simulationSettings.sheepdogSettingsName + ".txt";
		}


		
		string path = Path.Combine (Application.persistentDataPath, GetFolder(folder), fileName);

		if (!File.Exists(path))
        {
			File.WriteAllText(path, "");
        }

		string content = "";
		if (folder == 0)
		{
			content = simulationSettings.settingsName + "," + simulationSettings.sheepSettingsName + "," + simulationSettings.sheepdogSettingsName + ';';
		}
		else if (folder == 1)
		{
			content = simulationSettings.sheepSettingsName + "," + simulationSettings.sheepAlgorithm.ToString() + "," + simulationSettings.flockSize.ToString() + "," + simulationSettings.spawnType.ToString() + "," + simulationSettings.sheepdogRepulsionDistance.ToString() + ";";
		}
		else if (folder == 2)
		{
			content = simulationSettings.sheepdogSettingsName + "," + ((simulationSettings.isLearning)? "YES" : "NO") + "," + simulationSettings.collectAmount.ToString() + "," + simulationSettings.PHP.ToString() + "," + simulationSettings.PHP2.ToString() + ";";
		}


		File.WriteAllText(path, content);
		
    }

	public void DeleteFile (string fileName, int folder)
    {
		string fullName = fileName + ".txt";
		string path = Path.Combine (Application.persistentDataPath, GetFolder(folder), fullName);

		if (File.Exists(path))
		{
			File.Delete(path);
			
		}
	}

	public bool CheckIfFileAlreadyExists (string name, int folder)
    {
		string fileName = name + ".txt";
		string path = Path.Combine (Application.persistentDataPath, GetFolder(folder), fileName);

		if (File.Exists(path))
		{
			return true;
		}

		else return false;
	}


	public string CheckIfSettingsUsedInSimulation (string name, int folder)
    {
		string result = "";

		List<string> files = GetValidFilesInDirectory(0);

		for (int i = 0; i < files.Count; i++)
		{
			string fullName = files[i] + ".txt";

			string path = Path.Combine(Application.persistentDataPath, GetFolder(0));

			var sr = new StreamReader(path + "\\" + fullName);
			var contents = sr.ReadToEnd();
			sr.Close();

			var allInfo = contents.Split(';');
			var info = allInfo[0].Split(',');

			if (info.Length > 1)
			{
				if (info[folder].Equals(name))
                {
					result = files[i];
					break;
                }

			}
		}


		
		return result;
    }


	public List<string> GetValidFilesInDirectory (int folder)
    {
		List<string> files = new List<string>();

		string path = Path.Combine (Application.persistentDataPath, GetFolder(folder));
		DirectoryInfo dir = new DirectoryInfo(path);
		FileInfo[] info = dir.GetFiles("*.txt");


		foreach (FileInfo f in info)
        {
			string fileName = f.Name;
			string realName = "";

			for (int i = 0; i < fileName.Length; i++)
            {
				if (fileName[i].Equals('.'))
                {
					break;
                }

				realName = realName + fileName[i];
            }
			

			files.Add(realName);
        }


		return files;
	}


	public void SetSimulationSettingsFromFileName (string fileName, int folder)
    {
		string fullName = fileName + ".txt";

		string path = Path.Combine (Application.persistentDataPath, GetFolder(folder));

		var sr = new StreamReader(path + "\\" + fullName);
		var contents = sr.ReadToEnd();
		sr.Close();

		var settingsPart = contents.Split(';');
		var lines = settingsPart[0].Split(',');

		string settingsName = lines[0];

		if (folder == 0)
        {
			string sheepSettingsName = lines[1];
			string sheepdogSettingsName = lines[2];

			simulationSettings.SetNewSimulationSettings(settingsName);
			SetSimulationSettingsFromFileName(sheepSettingsName, 1);
			SetSimulationSettingsFromFileName(sheepdogSettingsName, 2);
        }
		else if (folder == 1)
        {
			int sheepAlgorithm = simulationSettings.GetSheepAlgorithmIntFromString(lines[1]);
			int flockSize = Int32.Parse(lines[2]);
			int spawnType = simulationSettings.GetSpawnTypeIntFromString(lines[3]);
			float sheepdogDist = float.Parse(lines[4]);

			simulationSettings.SetNewSheepSettings(settingsName, sheepAlgorithm, flockSize, spawnType, sheepdogDist);
		}
		else if (folder == 2)
        {
			bool isLearning = String.Equals(lines[1], "YES") ? true : false;
			float CMD = float.Parse(lines[2]);
			float PHP = float.Parse(lines[3]);
			float PHP2 = float.Parse(lines[4]);

			simulationSettings.SetNewSheepdogSettings(settingsName, isLearning, CMD, PHP, PHP2);
        }


		

		
	}


	public void WriteSimulationResultsToFile (string fileName, float timeTaken, float distanceTravelled, int folder)
    {
		fileName = simulationSettings.settingsName + ".txt";
		string path = Path.Combine (Application.persistentDataPath, GetFolder(folder), fileName);

		string content = timeTaken.ToString() + ',' + distanceTravelled.ToString() + ':';

		StreamWriter sw = File.AppendText(path);
		sw.Write(content);
		sw.Close();
	}

	public void WriteSimulationResultsToFileWithParams (string fileName, float timeTaken, float distanceTravelled, int folder, float CMD, float PHP, float PHP2)
	{
		fileName = simulationSettings.settingsName + ".txt";
		string path = Path.Combine(Application.persistentDataPath, GetFolder(folder), fileName);

		string content = timeTaken.ToString() + ',' + distanceTravelled.ToString() + ',' + CMD.ToString() + ',' + PHP.ToString() + ',' + PHP2.ToString() + ':';

		StreamWriter sw = File.AppendText(path);
		sw.Write(content);
		sw.Close();
	}


	public List<string> GetResultsFromName (string fileName, int folder)
    {
		List<string> results = new List<string>();

		if (fileName.Equals ("NewSimulation"))
        {
			DeleteFile(fileName, 0);
			return results;
        }

		

		string fullName = fileName + ".txt";

		string path = Path.Combine (Application.persistentDataPath, GetFolder(folder));

		var sr = new StreamReader(path + "\\" + fullName);
		var contents = sr.ReadToEnd();
		sr.Close();

		var allInfo = contents.Split(';');

		var resultsPart = allInfo[1];

		if (resultsPart.Length > 1)
        {
			var resultsArray = resultsPart.Split(':');

			foreach (var resultPart in resultsArray)
            {
				if (resultPart.Length > 0)
                {
					results.Add(resultPart);
				}
				
            }

        }

		return results;
	}

	public List<string> GetSettingsFromName (string fileName, int folder)
    {
		List<string> settings = new List<string>();

		string fullName = fileName + ".txt";

		string path = Path.Combine (Application.persistentDataPath, GetFolder(folder));

		var sr = new StreamReader(path + "\\" + fullName);
		var contents = sr.ReadToEnd();
		sr.Close();

		var allInfo = contents.Split(';');
		var settingsPart = allInfo[0];

		var settingsSplit = settingsPart.Split(',');

		if (folder == 0)
        {
			settings.AddRange(GetSettingsFromName(settingsSplit[1], 1));
			settings.AddRange(GetSettingsFromName(settingsSplit[2], 2));
		}
		else if (folder == 1)
        {
			settings.Add(settingsSplit[1]);
			settings.Add(settingsSplit[2]);
			settings.Add(settingsSplit[4]); //SRD
		}
		else if (folder == 2)
        {
			settings.Add(settingsSplit[1]);
			settings.Add(settingsSplit[2]);
			settings.Add(settingsSplit[3]); //PHP
			settings.Add(settingsSplit[4]); //PHP2
		}
		

		return settings;
	}


	string GetFolder (int i)
    {
		string path;

		if (i == 0)
        {
			path = "simulation";

		}
		else if (i == 1)
        {
			path = "sheep";
        }
		else
        {
			path = "sheepdog";
        }

		return path;
    }

	void CheckCreateFolders ()
    {
		for (int i = 0; i < 3; i++)
        {
			string path = Path.Combine(Application.persistentDataPath, GetFolder(i));
			
			if (!Directory.Exists(path))
            {
				Directory.CreateDirectory(path);
			}
        }
    }


	public void LoadSimulation()
	{
		ChangeScene("SimulationTest");
	}


	public void LeaveSimulation()
	{
		ChangeScene("MainMenu");
	}


	public void ChangeScene(string scene)
	{
		LoadNewScene(scene);
		
	}


	void LoadNewScene(string scene)
	{
		SceneManager.LoadScene(scene);
	}


	public void QuitProgram ()
    {
		Application.Quit();
    }

}