namespace maAI.Models;

public class HistoryInput
{
    public List<HistoryItem> Messages { get; set; } = [];
    public class HistoryItem
    {
        public string Role { get; set; } = "User";
        public string Content { get; set; } = "";
    }
}