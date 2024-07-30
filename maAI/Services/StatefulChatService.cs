using LLama;
using maAI.Models;

namespace maAI.Services;

public class StatefulChatService : IDisposable
{
    private readonly ChatSession _session;
    private readonly LLamaContext _context;
    private readonly ILogger<StatefulChatService> _logger;
    private bool _continue = false;
    
    private const string SystemPrompt = "You are a helpful, smart, kind, and efficient AI assistant. You always fulfill the user's requests to the best of your ability.";

    public StatefulChatService(IConfiguration configuration, ILogger<StatefulChatService> logger)
    {
        var @params = new LLama.Common.ModelParams(configuration["ModelPath"]!)
        {
            ContextSize = 4096,
            MainGpu = 0
        };
        
        using var weights = LLamaWeights.LoadFromFile(@params);

        _logger = logger;
        _context = new LLamaContext(weights, @params);

        _session = new ChatSession(new InteractiveExecutor(_context));
        _session.History.AddMessage(LLama.Common.AuthorRole.System, SystemPrompt);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }

    public async Task<string> Send(SendMessageInput input)
    {

        if (!_continue)
        {
            _logger.LogInformation("Prompt: {text}", SystemPrompt);
            _continue = true;
        }
        _logger.LogInformation("Input: {text}", input.Text);
        var outputs = _session.ChatAsync(
            new LLama.Common.ChatHistory.Message(LLama.Common.AuthorRole.User, input.Text),
            new LLama.Common.InferenceParams()
            {
                RepeatPenalty = 1.0f,
                AntiPrompts = new string[] { "User: " },
            });

        var result = "";
        await foreach (var output in outputs)
        {
            _logger.LogInformation("Message: {output}", output);
            result += output;
        }

        return result;
    }

    public async IAsyncEnumerable<string> SendStream(SendMessageInput input)
    {
        if (!_continue)
        {
            _logger.LogInformation(SystemPrompt);
            _continue = true;
        }

        _logger.LogInformation(input.Text);

        var outputs = _session.ChatAsync(
            new LLama.Common.ChatHistory.Message(LLama.Common.AuthorRole.User, input.Text!)
            , new LLama.Common.InferenceParams()
            {
                RepeatPenalty = 1.0f,
                AntiPrompts = new string[] { "User:" },
            });

        await foreach (var output in outputs)
        {
            _logger.LogInformation(output);
            yield return output;
        }
    }
}
