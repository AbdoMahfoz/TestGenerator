using System;
using System.Collections.Generic;
using System.Text;

namespace TestGenerator
{
    static public class CodeRunner
    {
        static public bool EvaluateCondition<T>(T first, T second, string op) where T : IComparable<T>
        {
            switch (op)
            {
                case "<":
                    if (first.CompareTo(second) < 0) return true;
                    else return false;
                case ">":
                    if (first.CompareTo(second) > 0) return true;
                    else return false;
                case "<=":
                    if (first.CompareTo(second) <= 0) return true;
                    else return false;
                case ">=":
                    if (first.CompareTo(second) >= 0) return true;
                    else return false;
                case "==":
                    if (first.CompareTo(second) == 0) return true;
                    else return false;
                default:
                    throw new UserViewableException($"Runner: Unknown operator \"{op}\"");
            }
        }
        static object TransformString(Type T, string s)
        {
            if (T == typeof(int)) return int.Parse(s);
            if (T == typeof(short)) return short.Parse(s);
            if (T == typeof(double)) return double.Parse(s);
            if (T == typeof(float)) return float.Parse(s);
            if (T == typeof(char)) return s[0];
            throw new Exception($"unknown type {T.Name} supploed to TransformString");
        }
        static object TransformString(string s)
        {
            if (int.TryParse(s, out int i)) return i;
            if (short.TryParse(s, out short sh)) return sh;
            if (float.TryParse(s, out float f)) return f;
            if (double.TryParse(s, out double d)) return d;
            if (char.TryParse(s, out char c)) return c;
            throw new Exception($"Unrecognized constant {s}");
        }
        static public string[] RunProgram(TestProgram testProgram, TestCase testCase)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            List<string> output = new List<string>();
            foreach (var key in testProgram.variables.Keys)
            {
                if (testCase.Values.TryGetValue(key, out object val))
                {
                    values.Add(key, val);
                }
                else
                {
                    values.Add(key, Activator.CreateInstance(testProgram.variables[key]));
                }
            }
            Statement current = testProgram.rootStatement;
            while(true)
            {
                int nextIndex = 0;
                switch(current.statmentType)
                {
                    case StatementType.Read:
                        string token = current.Value.Split(' ')[1].Trim();
                        if(testCase.Values.TryGetValue(token, out object val3))
                        {
                            values[token] = val3;
                        }
                        else
                        {
                            values[token] = Activator.CreateInstance(testProgram.variables[token]);
                        }
                        break;
                    case StatementType.Write:
                        token = current.Value.Split(' ')[1].Trim();
                        output.Add(values[token].ToString());
                        break;
                    case StatementType.Condition:
                        string[] tokens = current.Value.Trim().Split(' ');
                        object first = null, second = null;
                        if (values.TryGetValue(tokens[1], out object val)) first = val;
                        if (values.TryGetValue(tokens[3], out object val2)) second = val2;
                        if (first != null && second == null) second = TransformString(testProgram.variables[tokens[1]], tokens[3]);
                        if (first == null && second != null) first = TransformString(testProgram.variables[tokens[3]], tokens[1]);
                        var method = typeof(CodeRunner).GetMethod(nameof(EvaluateCondition));
                        method = method.MakeGenericMethod(first.GetType());
                        bool res = (bool)method.Invoke(null, new object[] { first, second, tokens[2] });
                        if (!res) nextIndex = 1;
                        break;
                }
                if(current.nextStatements.Count == 0)
                {
                    break;
                }
                current = current.nextStatements[nextIndex];
            }
            return output.ToArray();
        }
    }
}
