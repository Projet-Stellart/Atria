using Godot;
using System;
using System.IO;
using System.Text.Json;

public static class SaveManager
{
    public const string savefilepath = "user://settings.json";
    public static ParamData LoadSettings()
    {
        string filepath = ProjectSettings.GlobalizePath(savefilepath);
        if (!File.Exists(filepath)) return new ParamData();
        string content = File.ReadAllText(filepath);
        return JsonSerializer.Deserialize<ParamData>(content);
    }
    public static void SaveSettings(ParamData data)
    {
        string filepath = ProjectSettings.GlobalizePath(savefilepath);
        string content = JsonSerializer.Serialize(data);
        File.WriteAllText(filepath, content);
    }
}

public class ParamData
{
    public bool mute {get; set;} = false;
    public float soundLevel {get; set;} = 1;
}
