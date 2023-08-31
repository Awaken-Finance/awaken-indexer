using AElf.Types;
using AElfIndexer.Grains.State.Client;

namespace Swap.Indexer.Tests.Helper;

public static class LogEventHelper
{
    public static LogEventInfo ConvertAElfLogEventToLogEventInfo(LogEvent logEvent)
    {
        var logEventInfo = new LogEventInfo
        {
            // Address = logEvent.Address,
            // Topics = logEvent.Topics,
            // Data = logEvent.Data,
            ExtraProperties = new Dictionary<string, string>
            {
                // {"Indexed", JsonConvert.SerializeObject(logEvent.Indexed.Select(x => x.ToBase64()).ToList())},
                {"Indexed", logEvent.Indexed.ToString()},
                {"NonIndexed", logEvent.NonIndexed.ToBase64()}
            }
        };
        return logEventInfo;
    }
}