using System;
using System.Collections.Generic;

namespace TestGenerator
{
    public enum StatementType { Read, Write, Condition, Loop, Closure, Value }
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
    public class TestCase
    {

    }
    public class UserViewableException : Exception
    {
        public UserViewableException() : base() { }
        public UserViewableException(string message) : base(message) { }
    }
}
