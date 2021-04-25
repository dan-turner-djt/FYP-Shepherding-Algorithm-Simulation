using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulationSettings
{
    public string settingsName;
    public string sheepSettingsName;
    public string sheepdogSettingsName;
    public int flockSize;
    public int intMaxFlockSize = 100;
    public int maxSDDist = 100;
    public int maxCMD = 100;
    public bool isLearning = false;
    public float collectAmount = 8;
    public float PHP = 2;
    public float PHP2 = 2;
    public float maxPHP = 100;
    public float maxPHP2 = 100;

    public float sheepdogRepulsionDistance;

    public int cameraMode = 0; //0 = free, 1 = follow, 2 = overhead
    public float simulationSpeed = 1;

    public enum SheepAlgorithm
    {
        Random, Boids, PSO
    }
    public SheepAlgorithm sheepAlgorithm;

    public enum SpawnType
    {
        Random, Circular
    }
    public SpawnType spawnType;


    public string defaultName = "NewSimulation";
    public string defaultSheepName = "NewSheepSettings";
    public string defaultSheepdogName = "NewSheepdogSettings";
    private int defaultFlockSize = 20;
    private int defaultSheepdogRepulsionDistance = 8;
    public SheepAlgorithm defaultSheepAlgorithm = SheepAlgorithm.Random;
    public SpawnType defaultSpawnType = SpawnType.Random;
    public float defaultCollectAmount = 4;
    public float defaultPHP = 1;
    public float defaultPHP2 = 1.5f;


    public int learningTryIterations = 50;
    public int maxIterations = 0;
    public int currentIteration = 0;
    public int numForAverage = 10;
    public static float AItotalTime = 0;
    public static float AItotalDistance = 0;


    public SimulationSettings ()
    {
        SetDefaultSettings();
        maxIterations = learningTryIterations * numForAverage;
    }


    public void SetDefaultSettings ()
    {
        settingsName = defaultName;

        SetDefaultSheepSettings();
        SetDefaultSheepdogSettings();

        currentIteration = 0;
    }

    public void SetDefaultSheepSettings ()
    {
        sheepSettingsName = defaultSheepName;
        flockSize = defaultFlockSize;
        sheepAlgorithm = defaultSheepAlgorithm;
        spawnType = defaultSpawnType;
        sheepdogRepulsionDistance = defaultSheepdogRepulsionDistance;
        
    }

    public void SetDefaultSheepdogSettings ()
    {
        sheepdogSettingsName = defaultSheepdogName;
        isLearning = false;
        collectAmount = defaultCollectAmount;
        PHP = defaultPHP;
        PHP2 = defaultPHP2;
    }

    public void SetNewSimulationSettings(string _name)
    {
        settingsName = _name;
    }

    public void SetNewSheepSettings (string _name, int _sheepAlgorithm, int _flockSize, int _spawnType, float _sheepdogDist)
    {
        sheepSettingsName = _name;
        flockSize = _flockSize;
        sheepAlgorithm = (SheepAlgorithm) _sheepAlgorithm;
        spawnType = (SpawnType) _spawnType;
        sheepdogRepulsionDistance = _sheepdogDist;
    }

    public void SetNewSheepdogSettings(string _name, bool _isLearning, float _CMD, float _PHP, float _PHP2)
    {
        sheepdogSettingsName = _name;
        isLearning = _isLearning;
        collectAmount = _CMD;
        PHP = _PHP;
        PHP2 = _PHP2;
    }

    public void SetNewSettings2 (bool _isLearning)
    {
        isLearning = _isLearning;
    }

    public int GetSheepAlgorithmIntFromString (string s)
    {
        int index = 0;

        if (s.Equals (SheepAlgorithm.Random.ToString()))
        {
            index =  0;
        }
        else if (s.Equals(SheepAlgorithm.Boids.ToString()))
        {
            index = 1;
        }
        else if (s.Equals(SheepAlgorithm.PSO.ToString()))
        {
            index = 2;
        }

        return index;
    }

    public int GetSpawnTypeIntFromString(string s)
    {
        int index = 0;

        if (s.Equals(SpawnType.Random.ToString()))
        {
            index = 0;
        }
        else if (s.Equals(SpawnType.Circular.ToString()))
        {
            index = 1;
        }

        return index;
    }

    public int GetSpawnType(string s)
    {
        int index = 0;

        if (s.Equals(SpawnType.Random.ToString()))
        {
            index = 0;
        }
        else if (s.Equals(SpawnType.Circular.ToString()))
        {
            index = 1;
        }

        return index;
    }
}
