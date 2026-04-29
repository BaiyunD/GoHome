using System;
using System.IO;
using UnityEngine;

[Serializable]
public class LaunchSettingsData
{
    public int Version = 1;
    public string SelectedPlayerDataPath;
}

public static class LaunchSettingsService
{
    private const string DEFAULT_FILE_NAME = "launch_settings.json";

    public static string GetDefaultSettingsPath()
    {
        return Path.Combine(Application.persistentDataPath, DEFAULT_FILE_NAME);
    }

    public static LaunchSettingsData Load()
    {
        string path = GetDefaultSettingsPath();
        if (!File.Exists(path))
        {
            return new LaunchSettingsData();
        }

        try
        {
            string json = File.ReadAllText(path);
            LaunchSettingsData data = JsonUtility.FromJson<LaunchSettingsData>(json);
            return data ?? new LaunchSettingsData();
        }
        catch (Exception)
        {
            return new LaunchSettingsData();
        }
    }

    public static void Save(LaunchSettingsData data)
    {
        if (data == null)
        {
            data = new LaunchSettingsData();
        }

        string path = GetDefaultSettingsPath();
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static void SetSelectedPlayerDataPath(string resourcesPath)
    {
        LaunchSettingsData data = Load();
        data.SelectedPlayerDataPath = resourcesPath;
        Save(data);
    }

    public static void ClearSelectedPlayerDataPath()
    {
        SetSelectedPlayerDataPath(string.Empty);
    }

    public static bool TryLoadSelectedPlayerTemplate(out PlayerData template)
    {
        template = null;
        LaunchSettingsData data = Load();
        if (data == null || string.IsNullOrWhiteSpace(data.SelectedPlayerDataPath))
        {
            return false;
        }

        template = Resources.Load<PlayerData>(data.SelectedPlayerDataPath);
        return template != null;
    }
}

