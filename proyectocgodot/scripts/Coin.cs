using Godot;
using System;

public partial class Coin : Area2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print("I'm a coin");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	
	}
	
	public void _OnBodyEntered(Node body)
	{
		GD.Print("+1 moneda"); 
		QueueFree();
		
	}
	

	
	
}
