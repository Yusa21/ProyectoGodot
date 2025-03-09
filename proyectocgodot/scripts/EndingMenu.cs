using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

public partial class EndingMenu : Control
{
    [Export] public string Objective;
    
    private RichTextLabel Puntuancion;
    private RichTextLabel TopScores;
    private TextEdit Nombre;
    private HttpRequest GetTopScoresRequest;
    private HttpRequest PublishScoreRequest;

    public override void _Ready()
    {
 
        Puntuancion = GetNode<RichTextLabel>("Puntuacion");
        Nombre = GetNode<TextEdit>("Nombre");
        TopScores = GetNode<RichTextLabel>("TopScores");
        
        // Creo los nodos de HttpRequest desde el codigo
        GetTopScoresRequest = new HttpRequest();
        PublishScoreRequest = new HttpRequest();
        
        // Los anado a la escena
        AddChild(GetTopScoresRequest);
        AddChild(PublishScoreRequest);
        
        // Les conecto las senales
        GetTopScoresRequest.RequestCompleted += OnTopScoresRequestCompleted;
        PublishScoreRequest.RequestCompleted += OnPublishScoreRequestCompleted;
        
        // Esta son las monedas locales
        Puntuancion.Text = "Has recolectado un total de " + GlobalScript.coins + " monedas\n" +
                          "Intenta no morir para no pederlas";
        
        // Pide las mejores puntuaciones 
        FetchTopScores();
        // Connect button signals
        GetNode("PublishScoreButton").Connect("pressed", new Callable(this, nameof(OnPublishButtonPressed)));
    }

    private void FetchTopScores()
    {
        string[] headers = new string[] { "Content-Type: application/json" };
        Error error = GetTopScoresRequest.Request("https://localhost:44362/api/Scores/top", headers);
        
        if (error != Error.Ok)
        {
            GD.Print("An error occurred while fetching top scores: " + error.ToString());
            TopScores.Text = "Error al cargar las puntuaciones más altas.";
        }
    }

    private void OnTopScoresRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode != 200)
        {
            GD.Print("Failed to get top scores. Response code: " + responseCode);
            TopScores.Text = "Error al cargar las puntuaciones más altas.";
            return;
        }

        string response = Encoding.UTF8.GetString(body);
        GD.Print("Response: " + response);
    
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        
            var scores = JsonSerializer.Deserialize<List<ScoreData>>(response, options);
            
            foreach (var score in scores)
            {
                GD.Print($"Parsed Score: Id={score.Id}, Player={score.Player}, Points={score.Points}");
            }
            
            string scoresText = "[center][b]MEJORES PUNTUACIONES[/b][/center]\n\n";
            int rank = 1;
        
            foreach (var score in scores)
            {
                scoresText += $"{rank}. {score.Player}: {score.Points} monedas\n";
                rank++;
            }
        
            GD.Print("Final formatted text: " + scoresText);
            TopScores.BbcodeEnabled = true;
            TopScores.Text = scoresText;
        }
        catch (Exception ex)
        {
            GD.Print("Error parsing top scores: " + ex.Message);
            GD.Print("Stack trace: " + ex.StackTrace);
            TopScores.Text = "Error al procesar las puntuaciones.";
        }
    }

    public void OnPublishButtonPressed()
    {
        string playerName = Nombre.Text.Trim();
        
        if (string.IsNullOrEmpty(playerName))
        {
            // Show error if no name is entered
            OS.Alert("Por favor, ingresa tu nombre antes de publicar tu puntuación.", "Nombre Requerido");
            return;
        }
        
        var scoreData = new ScoreData
        {
            Player = playerName,
            Points = GlobalScript.coins
        };
        
        string jsonData = JsonSerializer.Serialize(scoreData);
        
        string[] headers = new string[] { "Content-Type: application/json" };
        
        Error error = PublishScoreRequest.Request(
            "https://localhost:44362/api/Scores", 
            headers, 
            HttpClient.Method.Post, 
            jsonData
        );
        
        if (error != Error.Ok)
        {
            GD.Print("An error occurred while publishing score: " + error.ToString());
            OS.Alert("Error al publicar la puntuación.", "Error");
        }
    }

    private void OnPublishScoreRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode == 201 || responseCode == 200) // Created or OK
        {
            OS.Alert("¡Puntuación publicada con éxito!", "Éxito");
            FetchTopScores();
        }
        else
        {
            string response = Encoding.UTF8.GetString(body);
            GD.Print("Failed to publish score. Response code: " + responseCode + ", Response: " + response);
            OS.Alert("Error al publicar la puntuación.", "Error");
        }
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
public class ScoreData
{
    public long Id { get; set; }
    public string Player { get; set; }
    public long Points { get; set; }
}