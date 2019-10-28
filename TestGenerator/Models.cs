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
    public class TestCase
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
    }
    public class UserViewableException : Exception
    {
        public UserViewableException() : base() { }
        public UserViewableException(string message) : base(message) { }
    }
    public class Pair<TFirst, TSecond>
    {
        public TFirst First { get; set; }
        public TSecond Second { get; set; }
        public Pair(TFirst First, TSecond Second)
        {
            this.First = First;
            this.Second = Second;
        }
        public Pair() {}
    }
}
