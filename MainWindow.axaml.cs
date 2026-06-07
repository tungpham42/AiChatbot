using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Input;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using Markdown.Avalonia; // Added namespace for Markdown

namespace AiChatbot;

public partial class MainWindow : Window
{
    private static readonly HttpClient client = new HttpClient();
    private const string ApiUrl = "https://api.soft.io.vn/test";

    // Use a StringBuilder to maintain the entire chat as a single Markdown document
    private StringBuilder _chatLog = new StringBuilder();

    public MainWindow()
    {
        InitializeComponent();
        
        // A much more professional welcome greeting
        _chatLog.AppendLine("# ✨ Soft.io AI Workspace\n*How can I assist you today?*\n\n---");
        UpdateChatDisplay();
    }

    private async void SendButton_Click(object? sender, RoutedEventArgs e)
    {
        await SendMessageToApi();
    }

    private async void InputBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            await SendMessageToApi();
        }
    }

    private async Task SendMessageToApi()
    {
        string prompt = InputBox.Text ?? string.Empty;
        if (string.IsNullOrWhiteSpace(prompt)) return;

        // 1. Update UI with clear role distinction
        _chatLog.AppendLine($"### 👤 You\n{prompt}\n");
        UpdateChatDisplay();
        
        InputBox.Text = string.Empty;
        
        // Intuitive UI lock & feedback while processing
        SendButton.IsEnabled = false;
        InputBox.IsEnabled = false;
        
        var btnText = this.FindControl<TextBlock>("SendButtonText");
        if (btnText != null) btnText.Text = "Thinking... ⏳";

        try
        {
            var requestData = new { prompt = prompt };
            string jsonPayload = JsonSerializer.Serialize(requestData);
            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(ApiUrl, content);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            using JsonDocument doc = JsonDocument.Parse(responseBody);

            if (doc.RootElement.TryGetProperty("result", out JsonElement resultElement))
            {
                // 2. Beautifully spaced AI response
                _chatLog.AppendLine($"### 🤖 AI Assistant\n{resultElement.GetString()}\n\n---\n");
            }
            else
            {
                _chatLog.AppendLine("> ⚠️ *Warning: The API responded but is missing the 'result' field.*\n\n---\n");
            }
        }
        catch (HttpRequestException ex)
        {
            _chatLog.AppendLine($"> ❌ *Network Error: {ex.Message}*\n\n---\n");
        }
        catch (Exception ex)
        {
            _chatLog.AppendLine($"> ❌ *Error: {ex.Message}*\n\n---\n");
        }
        finally
        {
            UpdateChatDisplay();
            
            // Re-enable input and restore normal state
            SendButton.IsEnabled = true;
            InputBox.IsEnabled = true;
            if (btnText != null) btnText.Text = "Send ✨";
            
            InputBox.Focus();
        }
    }

    // Helper method to push the updated string to the Markdown control
    private void UpdateChatDisplay()
    {
        var mdViewer = this.FindControl<MarkdownScrollViewer>("ChatHistory");
        if (mdViewer != null)
        {
            mdViewer.Markdown = _chatLog.ToString();
        }
    }
}