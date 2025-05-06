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
            AddUIMessage(ChatRole.System, "🕐 初始化中，請稍候...");

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

            _messages.Add(new(ChatRole.System, "請使用繁體中文回答所有問題。"));

            AddUIMessage(ChatRole.System, "歡迎使用本工具！這是一個結合 NetStone 的自然語言 MCP 伺服器，可用來查詢《Final Fantasy XIV》的角色與世界資料。");
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(InputBox.Text))
            {
                SubmitMessage();
            }
            else
            {
                MessageBox.Show("請輸入問題!", "Info");
            }
        }

        private async void SubmitMessage()
        {
            string userMessage = InputBox.Text.Trim();

            _messages.Add(new(ChatRole.User, userMessage));
            AddUIMessage(ChatRole.User, userMessage);
            InputBox.Clear();
            SubmitButton.IsEnabled = false;

            AddUIMessage(ChatRole.Assistant, "思考中...");

            bool isFirst = true;
            string aiMessage = string.Empty;

            await foreach (var update in _chatClient.GetStreamingResponseAsync(_messages, new() { Tools = [.. _tools] }))
            {
                aiMessage += update.Text;

                if (isFirst)
                {
                    ChatPanel.Children.RemoveAt(ChatPanel.Children.Count - 1); // 移除 "思考中..."
                    AddUIMessage(ChatRole.Assistant, aiMessage);
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
            SubmitButton.IsEnabled = true;
        }


        private async void InputBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !string.IsNullOrWhiteSpace(InputBox.Text))
            {
                SubmitMessage();
            }
        }

        private void UpdateLastUIMessage(string newText)
        {
            if (ChatPanel.Children.Count > 0 &&
        ChatPanel.Children[^1] is Border lastBubble &&
        lastBubble.Child is TextBlock textBlock)
            {
                textBlock.Text = newText;
            }
        }

        private void AddUIMessage(ChatRole sender, string message, bool isLast = false)
        {
            var bubble = new Border
            {
                Background = sender == ChatRole.User ? Brushes.LightBlue : Brushes.LightGray,
                CornerRadius = new CornerRadius(10),
                Padding = new Thickness(10),
                Margin = new Thickness(5),
                MaxWidth = 300,
                HorizontalAlignment = sender == ChatRole.User ? HorizontalAlignment.Right : HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text = message,
                    TextWrapping = TextWrapping.Wrap,
                    FontSize = 14
                }
            };

            ChatPanel.Children.Add(bubble);
        }
    }
}