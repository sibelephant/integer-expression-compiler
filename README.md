# Integer Expression Compiler

This is a  small compiler written in C# that supports integer expressions, variables, addition, and multiplication. It compiles source code into simple bytecode and executes that bytecode on a stack-based virtual machine.

## Requirements

- .NET 8 SDK

## Run

From the project directory:

```bash
dotnet run
```

The included sample prints:

```text
29
```

## Supported Language

Variable declarations use `var`, followed by a variable name, an equals sign, an expression, and a semicolon:

```text
var x = 2 + 3 * 4;
var answer = x * 2 + 1;
```

Supported values and operators:

- Integer literals, such as `0`, `12`, and `123`
- Variables, such as `x` and `answer`
- Addition: `+`
- Multiplication: `*`
- Statement terminator: `;`

Multiplication has higher precedence than addition. The expression `2 + 3 * 4` therefore evaluates to `14`.

## Compiler Pipeline

1. **Scanner** converts source text into tokens.
2. **Parser** builds an expression tree from variable declarations and expressions.
3. **Compiler** converts the expression tree into bytecode instructions.
4. **Virtual machine** executes the bytecode and stores variable values.

The bytecode operations are:

- `PushConstant`
- `LoadVariable`
- `StoreVariable`
- `Add`
- `Multiply`
- `Halt`

## Project Structure

```text
hello-csharp.csproj  .NET 8 console project
Program.cs           Scanner, parser, compiler, VM, and sample source
README.md            Project overview and usage


This is intentionally a minimal compiler. It does not support:

- Error handling or error reporting
- Functions
- Conditions
- Parentheses
- Subtraction or division
- Strings or other data types
- Loops
- Comments
- General-purpose assignment statements

The sample source is currently embedded directly in `Program.cs` rather than read from a file or command-line argument.
