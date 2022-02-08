using System;

public static class Terminal
{
    private static string[] spinnerOrder = { "/", "-", "\\", "|" };
    private static int index = 0;

    public static void drawSpinner()
    {
        //clear previous output
        for (int i = 0; i < 9; i++) { Console.Write("\b");  }
        for (int i = 0; i < 9; i++) { Console.Write(" "); }
        for (int i = 0; i < 9; i++) { Console.Write("\b"); }
        //print the spinner and increment index in list
        Console.Write("Working " + spinnerOrder[index]);
        if (index == spinnerOrder.Length-1)
            index = 0;
        else
            index++;
    }
    public static void drawPB(int max, int current)
    {
        //print out current out of total 
        String str = "";
        str += (current) + "/" + max;
        for (int i = 0; i < str.Length; i++) { Console.Write("\b"); }
        for (int i = 0; i < str.Length; i++) { Console.Write(" "); }
        for (int i = 0; i < str.Length; i++) { Console.Write("\b"); }
        Console.Write(str);

    }

    public static void exitOnKeyPress()
    {
        printInfo("\n\nPress any key to exit...");
        Console.ReadKey();
    }

    public static void printInfo(string info)
    {
        string output = "[INFO]    ";
        output += DateTime.Now + " : ";
        output += info;
        Console.WriteLine(output);
    }

    public static void printError(string info)
    {
        string output = "[ERROR]   ";
        output += DateTime.Now + " : ";
        output += info;
        Console.WriteLine(output);
    }
    public static void printWarning(string info)
    {
        string output = "[WARNING] ";
        output += DateTime.Now + " : ";
        output += info;
        Console.WriteLine(output);
    }

    public static void printErrorDetails(string info)
    {
        string output = "  [ERROR DETAIL] ";
        output += ": " + info;
        Console.WriteLine(output);
    }
}