using Godot;



public partial class KillZone : Area2D
{
	
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
		
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public void _OnBodyEntered(Node body)
    {
        GD.Print("You died");
        Engine.TimeScale = 0.5f;
        Timer timer = GetNode<Timer>("Timer");
        timer.Start();
    }

    public void _OnTimerOut()
    {
        Engine.TimeScale = 1f;
        GetTree().ReloadCurrentScene();
    } 
	
}