using Godot;

public partial class NextLevel : Area2D
{
	[Export] public string Objective;

	public void OnPlayerEntered(Node body)
	{
		if (body is Player)
		{
			GetTree().ChangeSceneToFile(Objective);
		}

	}
	
}

