using Godot;

public partial class StartMenu : Control
{
	[Export] public string Objective;

	private void OnStartButtonPressed()
	{
		//Resetea las monedas al empezar el juego
		GlobalScript.coins = 0;
		GetTree().ChangeSceneToFile(Objective);
	}
	
	private void OnQuitButtonPressed()
	{
		GetTree().Quit();
	}
	
	
	
}
