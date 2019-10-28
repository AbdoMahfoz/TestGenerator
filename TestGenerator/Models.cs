using System;
using System.Collections.Generic;

namespace TestGenerator
{
    public enum StatementType { Read, Write, Condition, Loop, Closure, Value, Declaration }
    public class TestProgram
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
    public class Statement
    {
        public TestProgram testProgram;
        public List<Statement> nextStatements;
        public StatementType statmentType;
        public string Value;
        public Statement()
        {
            nextStatements = new List<Statement>();
        }
        public Statement(string Value, StatementType statmentType, TestProgram testProgram, params Statement[] nextStatements)
        {
            this.testProgram = testProgram;
            this.Value = Value;
            this.statmentType = statmentType;
            this.nextStatements = new List<Statement>(nextStatements);
        }
    }
    public class TestCase : IEquatable<TestCase>
    {
        public Dictionary<string, object> Values;
        public TestCase()
        {
            Values = new Dictionary<string, object>();
        }
        public TestCase(params KeyValuePair<string, object>[] Values)
        {
            this.Values = new Dictionary<string, object>(Values);
        }
        public bool Equals(TestCase other)
        {
            if(Values.Keys.Count != other.Values.Keys.Count)
            {
                return false;
            }
            foreach(var key in Values.Keys)
            {
                if(!other.Values.ContainsKey(key) || Values[key] != other.Values[key])
                {
                    return false;
                }
            }
            return true;
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator==(TestCase t1, TestCase t2)
        {
            return t1.Equals(t2);
        }
        public static bool operator!=(TestCase t1, TestCase t2)
        {
            return !t1.Equals(t2);
        }
    }
    public class Pair<TFirst, TSecond>
    {
        public TFirst First;
        public TSecond Second;
        public Pair(TFirst First, TSecond Second)
        {
            this.First = First;
            this.Second = Second;
        }
        public Pair() {}
    }
    public class UserViewableException : Exception
    {
        public UserViewableException() : base() { }
        public UserViewableException(string message) : base(message) { }
    }
}
