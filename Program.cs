const string source = """
var x = 2 + 3 * 4;
var answer = x * 2 + 1;
""";

var syntaxTree = new Parser(new Scanner(source).Scan()).ParseProgram();
var compiler = new Compiler();
var program = compiler.Compile(syntaxTree);
var virtualMachine = new VirtualMachine(program);

virtualMachine.Run();
Console.WriteLine(virtualMachine.GetVariable("answer"));

public record Token(TokenType Type, string Lexeme);

public enum TokenType
{
    Plus,
    Star,
    Equal,
    Semicolon,
    Integer,
    Identifier,
    Var,
    EndOfFile
}

public sealed class Scanner(string source)
{
    public List<Token> Scan()
    {
        var tokens = new List<Token>();
        var position = 0;

        while (position < source.Length)
        {
            var character = source[position];

            if (char.IsWhiteSpace(character))
            {
                position++;
            }
            else if (char.IsDigit(character))
            {
                var start = position;
                while (position < source.Length && char.IsDigit(source[position]))
                {
                    position++;
                }

                tokens.Add(new Token(TokenType.Integer, source[start..position]));
            }
            else if (char.IsLetter(character))
            {
                var start = position;
                while (position < source.Length && char.IsLetterOrDigit(source[position]))
                {
                    position++;
                }

                var lexeme = source[start..position];
                tokens.Add(new Token(lexeme == "var" ? TokenType.Var : TokenType.Identifier, lexeme));
            }
            else
            {
                tokens.Add(character switch
                {
                    '+' => new Token(TokenType.Plus, "+"),
                    '*' => new Token(TokenType.Star, "*"),
                    '=' => new Token(TokenType.Equal, "="),
                    ';' => new Token(TokenType.Semicolon, ";"),
                    _ => new Token(TokenType.EndOfFile, "")
                });
                position++;
            }
        }

        tokens.Add(new Token(TokenType.EndOfFile, ""));
        return tokens;
    }
}

public abstract record Expression;
public record IntegerExpression(int Value) : Expression;
public record VariableExpression(string Name) : Expression;
public record BinaryExpression(Expression Left, TokenType Operator, Expression Right) : Expression;
public record VariableDeclaration(string Name, Expression Value);
public record SyntaxProgram(IReadOnlyList<VariableDeclaration> Declarations);

public sealed class Parser(IReadOnlyList<Token> tokens)
{
    private int position;

    public SyntaxProgram ParseProgram()
    {
        var declarations = new List<VariableDeclaration>();
        while (Current.Type != TokenType.EndOfFile)
        {
            Read(TokenType.Var);
            var name = Read(TokenType.Identifier).Lexeme;
            Read(TokenType.Equal);
            var value = ParseAddition();
            Read(TokenType.Semicolon);
            declarations.Add(new VariableDeclaration(name, value));
        }

        return new SyntaxProgram(declarations);
    }

    private Expression ParseAddition()
    {
        var expression = ParseMultiplication();
        while (Current.Type == TokenType.Plus)
        {
            var operation = Read(TokenType.Plus).Type;
            expression = new BinaryExpression(expression, operation, ParseMultiplication());
        }

        return expression;
    }

    private Expression ParseMultiplication()
    {
        var expression = ParsePrimary();
        while (Current.Type == TokenType.Star)
        {
            var operation = Read(TokenType.Star).Type;
            expression = new BinaryExpression(expression, operation, ParsePrimary());
        }

        return expression;
    }

    private Expression ParsePrimary()
    {
        if (Current.Type == TokenType.Integer)
        {
            return new IntegerExpression(int.Parse(Read(TokenType.Integer).Lexeme));
        }

        return new VariableExpression(Read(TokenType.Identifier).Lexeme);
    }

    private Token Current => tokens[position];

    private Token Read(TokenType expected)
    {
        var token = Current;
        position++;
        return token;
    }
}

public enum OpCode
{
    PushConstant,
    LoadVariable,
    StoreVariable,
    Add,
    Multiply,
    Halt
}

public record Instruction(OpCode Operation, int Operand = 0);
public record CompiledProgram(IReadOnlyList<Instruction> Instructions, IReadOnlyDictionary<string, int> Variables);

public sealed class Compiler
{
    private readonly List<Instruction> instructions = [];
    private readonly Dictionary<string, int> variables = [];

    public CompiledProgram Compile(SyntaxProgram program)
    {
        foreach (var declaration in program.Declarations)
        {
            CompileExpression(declaration.Value);
            Emit(OpCode.StoreVariable, GetVariableSlot(declaration.Name));
        }

        Emit(OpCode.Halt);
        return new CompiledProgram(instructions, variables);
    }

    private void CompileExpression(Expression expression)
    {
        switch (expression)
        {
            case IntegerExpression integer:
                Emit(OpCode.PushConstant, integer.Value);
                break;
            case VariableExpression variable:
                Emit(OpCode.LoadVariable, GetVariableSlot(variable.Name));
                break;
            case BinaryExpression binary:
                CompileExpression(binary.Left);
                CompileExpression(binary.Right);
                Emit(binary.Operator == TokenType.Plus ? OpCode.Add : OpCode.Multiply);
                break;
        }
    }

    private int GetVariableSlot(string name)
    {
        if (!variables.TryGetValue(name, out var slot))
        {
            slot = variables.Count;
            variables[name] = slot;
        }

        return slot;
    }

    private void Emit(OpCode operation, int operand = 0) => instructions.Add(new Instruction(operation, operand));
}

public sealed class VirtualMachine(CompiledProgram program)
{
    private readonly int[] variables = new int[program.Variables.Count];
    private readonly Stack<int> stack = [];

    public void Run()
    {
        var instructionPointer = 0;
        while (true)
        {
            var instruction = program.Instructions[instructionPointer++];
            switch (instruction.Operation)
            {
                case OpCode.PushConstant:
                    stack.Push(instruction.Operand);
                    break;
                case OpCode.LoadVariable:
                    stack.Push(variables[instruction.Operand]);
                    break;
                case OpCode.StoreVariable:
                    variables[instruction.Operand] = stack.Pop();
                    break;
                case OpCode.Add:
                    stack.Push(stack.Pop() + stack.Pop());
                    break;
                case OpCode.Multiply:
                    stack.Push(stack.Pop() * stack.Pop());
                    break;
                case OpCode.Halt:
                    return;
            }
        }
    }

    public int GetVariable(string name) => variables[program.Variables[name]];
}