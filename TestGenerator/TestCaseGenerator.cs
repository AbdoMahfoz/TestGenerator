using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestGenerator
{
    static public class TestCaseGenerator
    {
        static void Merge(List<TestCase> a, List<TestCase> b)
        {
            throw new NotImplementedException("I forgot to implement the Merge function...");
        }
        static string[] DigestLine(string line)
        {
            string[] tokens = line.Split('=');
            string[] innerToken = tokens[0].Split(' ');
            tokens[0] = innerToken[(innerToken.Length > 1) ? 1 : 0];
            return tokens;
        }
        static object GetValue(string varName, string varValue, TestProgram testProgram)
        {
            Type varType = testProgram.variables[varName];
            if(varType == typeof(int)) return int.Parse(varValue);
            if(varType == typeof(short)) return short.Parse(varValue);
            if(varType == typeof(double)) return double.Parse(varValue);
            if(varType == typeof(float)) return float.Parse(varValue);
            if(varType == typeof(char)) return varValue[0];
            throw new Exception("An unknown variable type attempted to parse it's value string. Please update TestCaseGenerator.GetValue method");
        }
        static List<TestCase> EvaluateTestCases(Statement statement, SortedSet<string> variables, Dictionary<string, object> constants)
        {
            List<TestCase> res = new List<TestCase>();
            string condition = statement.Value.Substring(statement.Value.IndexOf(' ')).Replace(" ", "");
            string[] operands = condition.Split(new string[] { "<", ">", "==", "<=", ">=" }, StringSplitOptions.None);
            string op = null;
            if (condition.Contains("<")) op = "<";
            else if (condition.Contains(">")) op = ">";
            else if (condition.Contains("==")) op = "==";
            else if (condition.Contains(">=")) op = ">=";
            else if (condition.Contains("<=")) op = "<=";
            else throw new UserViewableException($"CaseGenerator: Invalid condition \"{condition}\": operator not recognized");
            List<object> expression = new List<object>();
            if(variables.Contains(operands[0])) expression.Add(operands[0]);
            else expression.Add(constants[operands[0]]);
            
            return res;
        }
        static List<TestCase> Solve(Statement statement, SortedSet<string> variables, Dictionary<string, object> constants)
        {
            List<TestCase> myCases = null;
            string[] tokens = DigestLine(statement.Value);
            switch(statement.statmentType)
            {
                case StatementType.Read:
                    variables.Add(tokens[0]);
                    if (constants.ContainsKey(tokens[0])) constants.Remove(tokens[0]);
                    break;
                case StatementType.Value:
                    if (variables.Contains(tokens[0])) variables.Remove(tokens[0]);
                    object value = GetValue(tokens[0], tokens[1], statement.testProgram);
                    if (!constants.TryAdd(tokens[0], value)) constants[tokens[0]] = value;
                    break;
                case StatementType.Condition:
                    myCases = EvaluateTestCases(statement, variables, constants);
                    break;
            }
            if(myCases == null)
            {
                myCases = new List<TestCase>();
            }
            foreach(Statement next in statement.nextStatements)
            {
                List<TestCase> cases = Solve(next, variables, constants);
                Merge(myCases, cases);
            }
            return myCases;
        }
        static public TestCase[] GetTestCases(TestProgram testProgram)
        {
            Dictionary<string, object> constants = new Dictionary<string, object>();
            foreach(var variable in testProgram.variables)
            {
                constants.Add(variable.Key, Activator.CreateInstance(variable.Value));
            }
            return Solve(testProgram.rootStatement, new SortedSet<string>(), constants).ToArray();
        }
    }
}
