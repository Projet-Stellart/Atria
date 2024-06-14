using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Keybinding : Control
{
    [Export] public string ActionName;
    [Export] public string ActionDisplayName;

    private bool wait;

    public override void _Ready()
    {
        GetNode<Label>("BindLabel").Text = ActionDisplayName;
    }

    public override void _Process(double delta)
    {
        if (wait)
        {
            foreach (Key key in Enum.GetValues(typeof(Key)))
            {
                if (Input.IsKeyPressed(key))
                {
                    RebindAction(key);
                    break;
                }
            }
        }
    }

    private void RebindAction(Key key)
    {
        InputMap.ActionEraseEvents(ActionName);
        InputEventKey newKeyEvent = new InputEventKey
        {
            PhysicalKeycode = key
        };
        InputMap.ActionAddEvent(ActionName, newKeyEvent);
        GetNode<Button>("BindButton").Text = key.ToString().Capitalize();
        wait = false;
    }

    private void _on_button_pressed()
    {
        GetNode<Button>("BindButton").Text = "Press a new Key";
        wait = true;
    }


}
