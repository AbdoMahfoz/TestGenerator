using System;
using System.Collections.Generic;
using System.IO;

namespace TestGenerator
{
    /*
     * The assumed file architecture is
     * psuedo code file where every statement is on a single line and every line contains a single statement
     * the only available keywords are (READ, PRINT, IF, END IF, DO, WHILE, END WHILE, FOR, END FOR)
     * The datatypes of these variables match those in C#
     * the assumption may expand to cover more and more cases as development progresses
     */
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Enter file name: ");
                string fileName = Console.ReadLine().Trim();
                TestProgram testProgram = TestProgramManager.GetProgram(fileName);
                TestProgramManager.DisplayProgram(testProgram);
                Console.WriteLine("Paths:");
                Statement[][] paths = TestProgramManager.GetAllPaths(testProgram);
                foreach(Statement[] path in paths)
                {
                    bool first = true;
                    foreach(Statement node in path)
                    {
                        if(!first)
                        {
                            Console.Write("->");
                        }
                        else
                        {
                            first = false;
                        }
                        Console.Write($"({node.Value})");
                    }
                    Console.WriteLine();
                }
            }
            catch(UserViewableException e)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal Error: {e.Message}");
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
