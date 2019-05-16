using System;

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
                TestProgramManager.DisplayVariables(testProgram);
                TestProgramManager.DisplayGraph(testProgram);
                TestProgramManager.DisplayPaths(TestProgramManager.GetAllPaths(testProgram));
                TestCase[] testCases = TestCaseGenerator.GetTestCases(testProgram);
                TestCaseGenerator.DisplayTestCases(testProgram, testCases);
            }
            catch(UserViewableException v)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal Error: {v.Message}");
                Console.ForegroundColor = oldColor;
            }
            catch(NotImplementedException n)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"The given code requires a non implemented functionality: {n.Message}");
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
