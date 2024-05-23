using Godot;
using System;

public class SettingsManager
{
    public SettingsManager singleton;

    public SettingsManager()
    {
        if (singleton != null) throw new ArgumentException();
        singleton = this;
    }
    public SettingsData settings;
}

public struct SettingsData
{
    public string ip;

}