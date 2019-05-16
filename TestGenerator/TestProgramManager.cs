using System;
using System.Collections.Generic;
using System.IO;

namespace TestGenerator
{
    static public class TestProgramManager
    {
        static Type GetType(string typeName)
        {
            Type[] basicTypes = { typeof(int), typeof(short), typeof(float), typeof(double), typeof(char) };
            Dictionary<Type, string> nameMapping = new Dictionary<Type, string>{
                { typeof(int), "int" },
                { typeof(short), "short"}
            };
            foreach (Type basicType in basicTypes)
            {
                string name = "";
                if (nameMapping.TryGetValue(basicType, out string val)) name = val;
                else name = basicType.Name.ToLower();
                if (name == typeName.ToLower()) return basicType;
            }
            throw new UserViewableException($"{typeName} is either not a known basic type or poorly formatted");
        }
        static StatementType TokenType(string token)
        {
            switch (token)
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
        static public TestProgram GetProgram(string fileName)
        {
            StreamReader file = null;
            try
            {
                file = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            }
            catch (Exception e)
            {
                throw new UserViewableException(e.Message);
            }
            TestProgram testProgram = new TestProgram();
            Statement lastStatement = null;
            Stack<Statement> Conditions = new Stack<Statement>();
            int lineNum = 0;
            while (file.Peek() != -1)
            {
                try
                {
                    lineNum++;
                    string line = file.ReadLine().Trim();
                    if (line.Split(' ').Length < 2) continue;
                    string identifier = "";
                    if (line.Contains("END")) identifier = string.Join(' ', line.Split(' ')[0], line.Split(' ')[1]);
                    else identifier = line.Split(' ')[0];
                    Statement currentStatment = new Statement(line, TokenType(identifier), testProgram);
                    if (lastStatement == null) testProgram.rootStatement = currentStatment;
                    else lastStatement.nextStatements.Add(currentStatment);
                    lastStatement = currentStatment;
                    if (currentStatment.statmentType == StatementType.Condition || currentStatment.statmentType == StatementType.Loop)
                    {
                        Conditions.Push(currentStatment);
                    }
                    else if (currentStatment.statmentType == StatementType.Closure)
                    {
                        Statement statment = Conditions.Pop();
                        if (statment.statmentType == StatementType.Condition && currentStatment.Value != "END IF")
                        {
                            throw new UserViewableException($"Expected \"END IF\", Found \"{currentStatment.Value}\"");
                        }
                        statment.nextStatements.Add(currentStatment);
                    }
                    else if (currentStatment.statmentType == StatementType.Value)
                    {
                        if (!testProgram.variables.TryGetValue(line.Split(' ')[0], out Type val))
                        {
                            currentStatment.statmentType = StatementType.Declaration;
                            string type = line.Split(' ')[0];
                            string name = line.Split(' ')[1];
                            testProgram.variables.Add(name, GetType(type));
                        }
                    }
                }
                catch (Exception e)
                {
                    throw new UserViewableException($"ParserError:\nLine #{lineNum}: {e.Message}");
                }
            }
            return testProgram;
        }
        static public void DisplayVariables(TestProgram testProgram)
        {
            Console.WriteLine("Variables:");
            foreach (KeyValuePair<string, Type> variable in testProgram.variables)
            {
                Console.WriteLine($"\t{variable.Key} : {variable.Value.FullName}");
            }
        }
        static public void DisplayGraph(TestProgram testProgram)
        {
            Console.WriteLine("Graph:");
            int conditions = 0;
            Statement currentStatement = testProgram.rootStatement;
            while (true)
            {
                Console.Write("\t");
                for (int i = 0; i < conditions; i++)
                {
                    Console.Write(" |");
                }
                Console.WriteLine($" {currentStatement.Value}");
                if (currentStatement.nextStatements.Count == 0) break;
                Console.Write("\t");
                for (int i = 0; i < conditions; i++)
                {
                    Console.Write(" |");
                }
                if (currentStatement.statmentType == StatementType.Condition)
                {
                    Console.WriteLine(" |\\");
                    conditions++;
                }
                else if (currentStatement.statmentType == StatementType.Closure)
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
        static public Statement[][] GetAllPaths(TestProgram testProgram)
        {
            List<Statement[]> paths = new List<Statement[]>();
            List<Statement> tmp = new List<Statement>();
            void getPath(Statement statement)
            {
                if (!statement.Value.Contains("END")) tmp.Add(statement);
                if (statement.nextStatements.Count > 0)
                {
                    foreach (Statement next in statement.nextStatements)
                    {
                        getPath(next);
                    }
                }
                else paths.Add(tmp.ToArray());
                if (!statement.Value.Contains("END")) tmp.RemoveAt(tmp.Count - 1);
            }
            getPath(testProgram.rootStatement);
            return paths.ToArray();
        }
        static public void DisplayPaths(Statement[][] paths)
        {
            Console.WriteLine("Paths:");
            foreach (Statement[] path in paths)
            {
                bool first = true;
                foreach (Statement node in path)
                {
                    if (!first)
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
    }
}
