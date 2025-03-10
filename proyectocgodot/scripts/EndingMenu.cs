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
    private int retryCount = 0;
    private const int MAX_RETRIES = 5;
    private const float RETRY_DELAY = 3.0f;

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
        TopScores.Text = retryCount > 0 
            ? $"Cargando puntuaciones (intento {retryCount})..."
            : "Cargando puntuaciones...";
        
        string[] headers = new string[] { "Content-Type: application/json" };

        GetTopScoresRequest.Timeout = 15;
    
        Error error = GetTopScoresRequest.Request("https://apipsp-30tv.onrender.com/api/Scores/top", headers);
    
        if (error != Error.Ok)
        {
            GD.Print("An error occurred while fetching top scores: " + error.ToString());
            HandleRetryOrFail("Error al cargar las puntuaciones más altas.");
        }
    }


    private void OnTopScoresRequestCompleted(long result, long responseCode, string[] headers, byte[] body)
    {
        if (responseCode != 200)
        {
            GD.Print("Failed to get top scores. Response code: " + responseCode);
        
            string errorMessage = responseCode == 502 
                ? "El servidor se está iniciando. Por favor, espera un momento..."
                : "Error al cargar las puntuaciones más altas.";
            
            HandleRetryOrFail(errorMessage);
            return;
        }
        
        retryCount = 0;

        string response = Encoding.UTF8.GetString(body);
        GD.Print("Response: " + response);
    
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        
            var scores = JsonSerializer.Deserialize<List<ScoreData>>(response, options);
            
            string scoresText = "[center][b]MEJORES PUNTUACIONES[/b][/center]\n\n";
            int rank = 1;
        
            foreach (var score in scores)
            {
                scoresText += $"{rank}. {score.Player}: {score.Points} monedas\n";
                rank++;
            }
        
            TopScores.BbcodeEnabled = true;
            TopScores.Text = scoresText;
        }
        catch (Exception ex)
        {
            GD.Print("Error parsing top scores: " + ex.Message);
            TopScores.Text = "Error al procesar las puntuaciones.";
        }
    }
    
    private void HandleRetryOrFail(string errorMessage)
    {
        if (retryCount < MAX_RETRIES)
        {
            retryCount++;
            GD.Print($"Retrying... Attempt {retryCount} of {MAX_RETRIES}");
        
            var timer = GetTree().CreateTimer(RETRY_DELAY);
            timer.Timeout += () => FetchTopScores();
            
            TopScores.Text = $"{errorMessage}\nReintentando ({retryCount}/{MAX_RETRIES})...";
        }
        else
        {
            retryCount = 0;
            TopScores.Text = "No se pudieron cargar las puntuaciones después de varios intentos.\nPor favor, inténtalo más tarde.";
        }
    }

    public void OnPublishButtonPressed()
    {
        string playerName = Nombre.Text.Trim();
        
        if (string.IsNullOrEmpty(playerName))
        {
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
            "https://apipsp-30tv.onrender.com/api/Scores", 
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