using DSL.Lexer;
using DSL.Parser;
using DSL.Interpreter;

string code = File.ReadAllText("script.dsl");

var lexer = new Lexer(code);
var tokens = lexer.Tokenize();

var parser = new Parser(tokens);
var ast = parser.Parse();

var interpreter = new Interpreter();
interpreter.Execute(ast);
