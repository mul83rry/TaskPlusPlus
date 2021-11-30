using System.IO;

internal static class Logger
{
    private static string filePath;

    public static void Init()
    {
        var path = $"{Directory.GetCurrentDirectory()}" +
            $"{Path.DirectorySeparatorChar}Data";

        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);

        filePath = $"{Directory.GetCurrentDirectory()}" +
            $"{Path.DirectorySeparatorChar}" +
            $"Log.txt";
    }

    public static void Log(string log)
    {
        File.AppendAllText(filePath, $"{log}\n");
        //Console.WriteLine("New log, temp");
    }
}