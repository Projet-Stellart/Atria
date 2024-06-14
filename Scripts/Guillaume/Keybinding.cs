using Godot;
using System;
using System.Collections.Generic;

public class Keybinding
{
    private Button currentButton;

    // Remplacez ces chemins par vos chemins exacts
    [Export] private NodePath Jump;
    [Export] private NodePath Crouch;
    [Export] private NodePath JumpLabel;
    [Export] private NodePath CrouchLabel;
    [Export] private NodePath infoPanelPath;

    private Button jump;
    private Button crouch;
    private Label jumpLabel;
    private Label crouchLabel;
    private PanelContainer infoPanel;

    public void _Ready()
    {
        /*jump = GetNode<Button>(Jump);
        crouch = GetNode<Button>(Crouch);
        jumpLabel = GetNode<Label>(JumpLabel);
        crouchLabel = GetNode<Label>(CrouchLabel);
        infoPanel = GetNode<PanelContainer>(infoPanelPath);

        jump.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array { jump });
        crouch.Connect("pressed", this, nameof(OnButtonPressed), new Godot.Collections.Array { crouch });

        UpdateLabels();

        infoPanel.Hide();*/
    }

    private void OnButtonPressed(Button button)
    {
        currentButton = button;
        infoPanel.Show();
    }

    public void _Input(InputEvent @event)
    {
        if (currentButton == null) return;

        if (@event is InputEventKey || @event is InputEventMouseButton)
        {
            // Cette partie est pour la suppression des affectations en double
            // Ajouter toutes les touches assignées à un dictionnaire
            Dictionary<string, string> allIES = new Dictionary<string, string>();
            foreach (var ia in InputMap.GetActions())
            {
                foreach (var iae in InputMap.ActionGetEvents(ia))
                {
                    allIES[iae.AsText()] = ia;
                }
            }

            // Vérifier si la nouvelle touche est déjà dans le dictionnaire
            // Si oui, supprimer l'ancienne
            if (allIES.ContainsKey(@event.AsText()))
            {
                InputMap.ActionEraseEvents(allIES[@event.AsText()]);
            }

            // Cette partie est là où le remappage réel se produit
            // Effacer l'événement dans la carte d'entrée
            InputMap.ActionEraseEvents(currentButton.Name);
            // Et assigner le nouvel événement à cela
            InputMap.ActionAddEvent(currentButton.Name, @event);

            // Après qu'une touche est assignée, réinitialiser currentButton
            currentButton = null;
            infoPanel.Hide();

            UpdateLabels();
        }
    }

    private void UpdateLabels()
    {
        var eb1 = InputMap.ActionGetEvents("Button_01");
        jumpLabel.Text = eb1.Count > 0 ? eb1[0].AsText() : "";

        var eb2 = InputMap.ActionGetEvents("Button_02");
        crouchLabel.Text = eb2.Count > 0 ? eb2[0].AsText() : "";
    }
}
