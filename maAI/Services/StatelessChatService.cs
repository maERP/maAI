﻿using LLama;
using LLama.Common;
using System.Text;
using static LLama.LLamaTransforms;
using LLama.Common;

namespace maAI.Services;

public class StatelessChatService
{
    private readonly LLamaContext _context;
    private readonly ChatSession _session;

    public StatelessChatService(IConfiguration configuration)
    {
        var @params = new ModelParams(configuration["ModelPath"]!)
        {
            ContextSize = 512,
        };

        // todo: share weights from a central service
        using var weights = LLamaWeights.LoadFromFile(@params);

        _context = new LLamaContext(weights, @params);

        // TODO: replace with a stateless executor
        _session = new ChatSession(new InteractiveExecutor(_context))
            .WithOutputTransform(
                new LLamaTransforms.KeywordTextOutputStreamTransform(new string[] { "User:", "Assistant:" },
                    redundancyLength: 8))
            .WithHistoryTransform(new HistoryTransform());
    }

    public async Task<string> SendAsync(ChatHistory history)
    {
        var result = _session.ChatAsync(history, new InferenceParams()
        {
            AntiPrompts = new string[] { "User:" },
        });

        var sb = new StringBuilder();
        await foreach (var r in result)
        {
            Console.Write(r);
            sb.Append(r);
        }

        return sb.ToString();
    }
}

public class HistoryTransform : DefaultHistoryTransform
{
    public override string HistoryToText(ChatHistory history)
    {
        return base.HistoryToText(history) + "\n Assistant:";
    }
}