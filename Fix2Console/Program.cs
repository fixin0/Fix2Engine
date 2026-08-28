using System;

namespace Fix2Console;

public class Program
{
    public static void Main(string[] args)
    {
        using var app = new Terminal(args);
    }
}
