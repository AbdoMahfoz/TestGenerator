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
    enum StatementType { Read, Write, Condition, Loop, Closure, Value }
    class TestProgram
    {
        public Statement rootStatement;
        public Dictionary<string, Type> variables;
        public TestProgram()
        {
            variables = new Dictionary<string, Type>();
        }
        public TestProgram(Statement rootStatement, params KeyValuePair<string, Type>[] variables)
        {
            this.rootStatement = rootStatement;
            this.variables = new Dictionary<string, Type>(variables);
        }
    }
    class Statement
    {
        public List<Statement> nextStatements;
        public StatementType statmentType;
        public string Value;
        public Statement()
        {
            nextStatements = new List<Statement>();
        }
        public Statement(string Value, StatementType statmentType, params Statement[] nextStatements)
        {
            this.Value = Value;
            this.statmentType = statmentType;
            this.nextStatements = new List<Statement>(nextStatements);
        }
    }
    class Program
    {
        static Type GetType(string typeName)
        {
            Type[] basicTypes = { typeof(int), typeof(short), typeof(float), typeof(double) };
            Dictionary<Type, string> nameMapping = new Dictionary<Type, string>{
                { typeof(int), "int" },
                { typeof(short), "short"}
            };
            foreach(Type basicType in basicTypes)
            {
                string name = "";
                if(nameMapping.TryGetValue(basicType, out string val))
                {
                    name = val;
                }
                else
                {
                    name = basicType.Name.ToLower();
                }
                if(name == typeName.Trim().ToLower())
                {
                    return basicType;
                }
            }
            throw new Exception($"{typeName} is either not a known basic type or poorly formatted");
        }
        static StatementType TokenType(string token)
        {
            switch(token.Trim())
            {
                case "READ":
                    return StatementType.Read;
                case "PRINT":
                    return StatementType.Write;
                case "IF":
                    return StatementType.Condition;
                case "WHILE":
                case "DO":
                case "FOR":
                    return StatementType.Loop;
                case "END IF":
                case "END WHILE":
                case "END FOR":
                    return StatementType.Closure;
                default:
                    return StatementType.Value;
            }
        }
        static TestProgram GetProgram(string fileName)
        {
            StreamReader file = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            TestProgram testProgram = new TestProgram();
            Statement lastStatement = null;
            Stack<Statement> Conditions = new Stack<Statement>();
            int lineNum = 0;
            while(file.Peek() != -1)
            {
                try
                {
                    lineNum++;
                    string line = file.ReadLine();
                    if(line.Split(' ').Length < 2)
                    {
                        continue;
                    }
                    string identifier = "";
                    if(line.Contains("END"))
                    {
                        identifier = string.Join(' ', line.Split(' ')[0], line.Split(' ')[1]);
                    }
                    else
                    {
                        identifier = line.Split(' ')[0];
                    }
                    Statement currentStatment = new Statement(line, TokenType(identifier));
                    if (lastStatement == null)
                    {
                        testProgram.rootStatement = currentStatment;
                    }
                    else
                    {
                        lastStatement.nextStatements.Add(currentStatment);
                    }
                    lastStatement = currentStatment;
                    if (currentStatment.statmentType == StatementType.Condition || currentStatment.statmentType == StatementType.Loop)
                    {
                        Conditions.Push(currentStatment);
                    }
                    else if (currentStatment.statmentType == StatementType.Closure)
                    {
                        Statement statment = Conditions.Pop();
                        statment.nextStatements.Add(currentStatment);
                    }
                    else if (currentStatment.statmentType == StatementType.Value)
                    {
                        if(!testProgram.variables.TryGetValue(line.Split(' ')[0].Trim(), out Type val))
                        {
                            string type = line.Split(' ')[0];
                            string name = line.Split(' ')[1];
                            testProgram.variables.Add(name, GetType(type));
                        }
                    }
                }
                catch(Exception e)
                {
                    throw new Exception($"Line #{lineNum}: {e.Message}");
                }
            }
            return testProgram;
        }
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Enter file name: ");
                string fileName = Console.ReadLine().Trim();
                TestProgram testProgram = GetProgram(fileName);
                Console.WriteLine("Analysis Completed:\n\tVariables:");
                foreach(KeyValuePair<string, Type> variable in testProgram.variables)
                {
                    Console.WriteLine($"\t\t{variable.Key} : {variable.Value.FullName}");
                }
                Console.WriteLine("\tGraph:");
                int conditions = 0;
                Statement currentStatement = testProgram.rootStatement;
                while(true)
                {
                    Console.Write("\t\t");
                    for (int i = 0; i < conditions; i++)
                    {
                        Console.Write(" |");
                    }
                    Console.WriteLine($" {currentStatement.Value.Trim()}");
                    if(currentStatement.nextStatements.Count == 0)
                    {
                        break;
                    }
                    Console.Write("\t\t");
                    for (int i = 0; i < conditions; i++)
                    {
                        Console.Write(" |");
                    }
                    if(currentStatement.statmentType == StatementType.Condition)
                    {
                        Console.WriteLine(" |\\");
                        conditions++;
                    }
                    else if(currentStatement.statmentType == StatementType.Closure)
                    {
                        Console.WriteLine("/");
                        conditions--;
                    }
                    else
                    {
                        Console.WriteLine(" |");
                    }
                    currentStatement = currentStatement.nextStatements[0];
                }
            }
            catch(Exception e)
            {
                ConsoleColor oldColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Fatal Error: {e.Message}");
                Console.ForegroundColor = oldColor;
            }
        }
    }
}
