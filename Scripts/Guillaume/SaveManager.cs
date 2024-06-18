using Godot;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

public static class SaveManager
{
    public const string savefilepath = "user://atria_settings.json";
    public static ParamData saveparam;
    public static void LoadSettings()
    {
        string filepath = ProjectSettings.GlobalizePath(savefilepath);
        Debug.Print(File.Exists(filepath).ToString());
        if (!File.Exists(filepath))
        {
            saveparam = new ParamData();
            return;
        }
        string content = File.ReadAllText(filepath);
        saveparam = JsonSerializer.Deserialize<ParamData>(content);
    }
    public static void SaveSettings()
    {
        string filepath = ProjectSettings.GlobalizePath(savefilepath);
        string content = JsonSerializer.Serialize(saveparam, new JsonSerializerOptions() { WriteIndented = true });
        File.WriteAllText(filepath, content);
    }
}

public class ParamData
{
    public bool mute {get; set;} = false;
    public float soundLevel {get; set;} = 100;
    public int windowMode { get; set;} = 0;
    public string windowRes { get; set;} = "1280x720";
}
