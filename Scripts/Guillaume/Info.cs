using Godot;
using System;

public partial class Info : Control
{

    public void SetInfo(string d)
        {
            var InfoRect = GetNode<RichTextLabel>("TextEdit");

            InfoRect.Text = "[center]" + d;
        }

}
