using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol.Transport;
using ModelContextProtocol.Protocol.Types;
using OpenAI;
using OpenAI.Chat;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetStoneClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _apiKey = "";
        private string _model = "gpt-4o-mini";
        private IChatClient _chatClient;
        private List<Microsoft.Extensions.AI.ChatMessage> _messages = new List<Microsoft.Extensions.AI.ChatMessage>();
        private IList<McpClientTool> _tools;
        private List<ChatResponseUpdate> _updates = [];

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await InitializeMCP();
        }

        private async Task InitializeMCP()
        {
            var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
            {
                Name = "helpTroubleshooter",
                Command = "dotnet",
                Arguments = ["run", "--project", "../../../../../src/NetStoneMCP.csproj", "--no-build"],
            });

            var client = await McpClientFactory.CreateAsync(clientTransport);

            _tools = await client.ListToolsAsync();

            _apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;

            if (string.IsNullOrEmpty(_apiKey))
            {
                MessageBox.Show("API Key is Empty!", "Info");
                return;
            }

            _chatClient = new OpenAIClient(_apiKey).GetChatClient(_model).AsIChatClient()
                                .AsBuilder().UseFunctionInvocation().Build();
        }

        private async void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(InputBox.Text))
            {
                string userMessage = InputBox.Text.Trim();

                _messages.Add(new(ChatRole.User, userMessage));
                AddUIMessage("User", userMessage);
                InputBox.Clear();

                AddUIMessage("AI", "思考中...");

                bool isFirst = true;
                string aiMessage = string.Empty;

                await foreach (var update in _chatClient.GetStreamingResponseAsync(_messages, new() { Tools = [.. _tools] }))
                {
                    aiMessage += update.Text;

                    if (isFirst)
                    {
                        ChatPanel.Children.RemoveAt(ChatPanel.Children.Count - 1); // 移除 "思考中..."
                        AddUIMessage("AI", aiMessage);
                        isFirst = false;
                    }
                    else
                    {
                        UpdateLastUIMessage(aiMessage);
                    }

                    _updates.Add(update);
                }

                _messages.AddMessages(_updates);
                ScrollViewer.ScrollToEnd();
            }
        }

        private void UpdateLastUIMessage(string newText)
        {
            if (ChatPanel.Children.Count > 0 &&
                ChatPanel.Children[^1] is TextBlock lastBlock)
            {
                lastBlock.Text = $"AI: {newText}";
            }
        }

        private void AddUIMessage(string sender, string message, bool isLast = false)
        {
            var textBlock = new TextBlock
            {
                Text = $"{sender}: {message}",
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5),
                FontSize = 14
            };

            ChatPanel.Children.Add(textBlock);
        }
    }
}