using System;
using System.Collections.Generic;
using System.Linq;

namespace TestGenerator
{
    static public class TestCaseGenerator
    {
        static void Merge(List<TestCase> a, List<TestCase> b)
        {
            List<TestCase> bin = new List<TestCase>();
            TestCase[] baseCases = a.ToArray();
            foreach(TestCase t2 in b)
            {
                if (baseCases.Length > 0)
                {
                    foreach (TestCase t in baseCases)
                    {
                        TestCase tmp = new TestCase()
                        {
                            Values = new Dictionary<string, object>(t.Values)
                        };
                        bool eraseTest = false;
                        bool dontErase = false;
                        foreach (var key in t2.Values.Keys)
                        {
                            if (!t.Values.Keys.Contains(key))
                            {
                                eraseTest = true;
                                tmp.Values.Add(key, t2.Values[key]);
                            }
                            else
                            {
                                dontErase = true;
                                tmp.Values[key] = t2.Values[key];
                            }
                        }
                        a.Add(tmp);
                        if (eraseTest && !dontErase) bin.Add(t);
                    }
                }
                else
                {
                    a.Add(t2);
                }
            }
            foreach(TestCase t in bin)
            {
                if(a.Contains(t)) a.Remove(t);
            }
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
            try
            {
                if (varType == typeof(int)) return int.Parse(varValue);
                if (varType == typeof(short)) return short.Parse(varValue);
                if (varType == typeof(double)) return double.Parse(varValue);
                if (varType == typeof(float)) return float.Parse(varValue);
                if (varType == typeof(char)) return varValue[0];
            }
            catch (FormatException)
            {
                throw new UserViewableException($"CaseGenerator: Variable Error:\n" +
                                                $"{varValue} is either not of type {varType.Name}, which is the type of the varaible {varName}, " +
                                                $"or is poorly formatted");
            }
            throw new Exception("An unknown variable type attempted to parse it's value string. Please update TestCaseGenerator.GetValue method");
        }
        static List<TestCase> GetCasesFromExpression(object[] expression, Dictionary<string, Pair<object, object>> variables)
        {
            List<TestCase> OneVariableCases(string varName, object constant)
            {
                List<TestCase> res = new List<TestCase>();
                Random r = new Random(DateTime.Now.Second);
                if (constant.GetType() != typeof(short))
                {
                    res.AddRange(new TestCase[]
                    {
                        new TestCase
                        {
                            Values = new Dictionary<string, object>()
                            {
                                { varName, r.Next((int)constant - 1) }
                            }
                        },
                        new TestCase
                        {
                            Values = new Dictionary<string, object>
                            {
                                { varName, r.Next((int)constant + 1, int.MaxValue) }
                            }
                        },
                        new TestCase
                        {
                            Values = new Dictionary<string, object>
                            {
                                { varName, constant }
                            }
                        }
                    });
                }
                else
                {
                    res.AddRange(new TestCase[]
                    {
                        new TestCase
                        {
                            Values = new Dictionary<string, object>()
                            {
                                { varName, r.Next((short)constant - 1) }
                            }
                        },
                        new TestCase
                        {
                            Values = new Dictionary<string, object>
                            {
                                { varName, r.Next((short)constant + 1, short.MaxValue) }
                            }
                        },
                        new TestCase
                        {
                            Values = new Dictionary<string, object>
                            {
                                { varName, constant }
                            }
                        }
                    });
                }
                if(constant.GetType() == typeof(float) || constant.GetType() == typeof(double))
                {
                    foreach(var values in from e in res select e.Values)
                    {
                        foreach(var key in values.Keys)
                        {
                            if(constant.GetType() == typeof(double))
                            {
                                values[key] = (int)values[key] + r.NextDouble();
                            }
                            else
                            {
                                values[key] = (int)values[key] + (float)r.NextDouble();
                            }
                        }
                    }
                }
                return res;
            }
            if (expression[0].GetType() == typeof(string) && expression[1].GetType() == typeof(string))
            {
                throw new NotImplementedException("Two variables in the same condition is not yet implmented");
            }
            else if (expression[0].GetType() == typeof(string))
            {
                return OneVariableCases((string)expression[0], expression[1]);
            }
            else if (expression[1].GetType() == typeof(string))
            {
                return OneVariableCases((string)expression[1], expression[0]);
            }
            else
            {
                return new List<TestCase>();
            }
        }
        static List<TestCase> EvaluateTestCases(Statement statement, Dictionary<string, Pair<object, object>> variables, Dictionary<string, object> constants)
        {
            string condition = statement.Value.Substring(statement.Value.IndexOf(' ')).Replace(" ", "");
            string[] operands = condition.Split(new string[] { "<", ">", "==", "<=", ">=" }, StringSplitOptions.None);
            string op = null;
            if (condition.Contains("<")) op = "<";
            else if (condition.Contains(">")) op = ">";
            else if (condition.Contains("==")) op = "==";
            else if (condition.Contains(">=")) op = ">=";
            else if (condition.Contains("<=")) op = "<=";
            else throw new UserViewableException($"CaseGenerator: Invalid condition \"{condition}\": operator not recognized");
            if (op == null) op = "";
            object[] expression = new object[2];
            void deriveExpressionPart(ref object expressionPart, string operand)
            {
                if (variables.ContainsKey(operand)) expressionPart = operand;
                else if (constants.TryGetValue(operand, out object val)) expressionPart = val;
                else
                {
                    if (int.TryParse(operand, out int intVal)) expressionPart = intVal;
                    else if (float.TryParse(operand, out float floatVal)) expressionPart = floatVal;
                    else if (double.TryParse(operand, out double doubleVal)) expressionPart = doubleVal;
                    else if (short.TryParse(operand, out short shortVal)) expressionPart = shortVal;
                    else if (char.TryParse(operand, out char charVal)) expressionPart = charVal;
                    else throw new UserViewableException($"CaseGenerator: Invalid operand \"{operand}\":\n" +
                                                         $"operand is neither a known variable or a constant value");
                }
            }
            deriveExpressionPart(ref expression[0], operands[0]);
            deriveExpressionPart(ref expression[1], operands[1]);
            if(expression[0] is string s && expression[1].GetType() != typeof(string))
            {
                if(statement.testProgram.variables[s] != expression[1].GetType())
                {
                    throw new UserViewableException($"CaseGenerator: TypeMismatch:\n" +
                        $"Variable {s} of type {statement.testProgram.variables[s].Name} is being compared with {operands[1]} of type {expression[1].GetType().Name}");
                }
            }
            else if(expression[1] is string s2 && expression[0].GetType() != typeof(string))
            {
                if (statement.testProgram.variables[s2] != expression[0].GetType())
                {
                    throw new UserViewableException($"CaseGenerator: TypeMismatch:\n" +
                        $"Variable {s2} of type {statement.testProgram.variables[s2].Name} is being compared with {operands[0]} of type {expression[0].GetType().Name}");
                }
            }
            return GetCasesFromExpression(expression, variables);
        }
        static List<TestCase> Solve(Statement statement, Dictionary<string, Pair<object, object>> variables, Dictionary<string, object> constants)
        {
            List<TestCase> myCases = null;
            string[] tokens = DigestLine(statement.Value);
            switch (statement.statmentType)
            {
                case StatementType.Read:
                    variables.Add(tokens[0], new Pair<object, object>(null, null));
                    if (constants.ContainsKey(tokens[0])) constants.Remove(tokens[0]);
                    break;
                case StatementType.Value:
                    if (variables.ContainsKey(tokens[0])) throw new NotImplementedException("Variable assingment after READ is not handled");
                    object value = GetValue(tokens[0], tokens[1], statement.testProgram);
                    if (!constants.TryAdd(tokens[0], value)) constants[tokens[0]] = value;
                    break;
                case StatementType.Condition:
                    myCases = EvaluateTestCases(statement, variables, constants);
                    break;
            }
            if (myCases == null)
            {
                myCases = new List<TestCase>();
            }
            foreach (Statement next in statement.nextStatements)
            {
                Merge(myCases, Solve(next, variables, constants));
            }
            return myCases;
        }
        static public TestCase[] GetTestCases(TestProgram testProgram)
        {
            Dictionary<string, object> constants = new Dictionary<string, object>();
            foreach (var variable in testProgram.variables)
            {
                constants.Add(variable.Key, Activator.CreateInstance(variable.Value));
            }
            return Solve(testProgram.rootStatement, new Dictionary<string, Pair<object, object>>(), constants).ToArray();
        }
        static public void DisplayTestCases(params TestCase[] testCases)
        {
            Console.WriteLine("Test cases:");
            int i = 0;
            foreach(var test in testCases)
            {
                i++;
                Console.WriteLine($"\tTest case #{i}:");
                foreach(var pair in test.Values)
                {
                    Console.WriteLine($"\t\t{pair.Key} = {pair.Value.ToString()}");
                }
            }
        }
    }
}
