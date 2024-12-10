using Godot;
using System;

public partial class EndingMenu : Control
{
	[Export] public string Objective;
	
	private RichTextLabel Puntuancion;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Puntuancion = GetNode<RichTextLabel>("Puntuacion");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		Puntuancion.Text = "Has recolectado un total de " + GlobalScript.coins + " monedas\n" +
		                   "Intenta no morir para no pederlas";
	}
	
	public void OnStartButtonPressed()
	{
		GetTree().ChangeSceneToFile(Objective);
	}
	
	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
}

