using Godot;
using System;

public partial class TmpMainMenu : CanvasLayer
{
    public Action<string, int> OnCustomPlay;
    public Action OnPlay;

    public override void _Ready()
    {
        ((Button)GetChild(0).GetChild(0).GetChild(0)).ButtonUp += () =>
        {
            if (OnPlay != null)
                OnPlay.Invoke();
        };
        ((Button)GetChild(0).GetChild(0).GetChild(1)).ButtonUp += () =>
        {
            GetTree().Quit();
        };
    }
}
