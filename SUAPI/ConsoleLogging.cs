using System;
using System.Text;

class Logg {


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