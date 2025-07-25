namespace Aurora;

internal static class Writer
{
    private static readonly List<string> Queue = [];

    public static void AddToQueue(string message)
    {
        Queue.Add(message);
    }

    public static void PushToStream()
    {
        foreach (string message in Queue)
        {
            Console.Write(message);
            GlobalVariables.LOGGER.Debug($"(Written To Stream) {message}");
        }
        
        Queue.Clear();
    }

    public static void ClearQueue()
    {
        Queue.Clear();
    }
}