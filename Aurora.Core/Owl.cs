namespace Aurora;

public static class Owl
{
    public static void Show()
    {
        using StreamReader streamReader = new StreamReader("owl.txt");
        string owl = streamReader.ReadToEnd();
        
        Console.Clear();
        Console.WriteLine(owl);
        Console.WriteLine("Syntax error: Owls are confused, and so am I");
        Environment.Exit(0);
    }
}