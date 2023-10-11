using System.ComponentModel;

namespace AiApp;

public class CodeFileParserChat : MultiFunctionCallChat
{
    protected override string SystemPrompt =>
        """
        You are an expert software architect and programmer. You are given a string that is the name of a file (eg 'foo.cs:') followed by a newline, then the text of a source code file. You have to carefully read the source file, step by step, ensure that you understand the each function and what it does, and call ParseCodeFile() function.

        The ParseCodeFile is going to take the hierarchical AST-like information, describing the language specific code constructs. In C# it might be classes -> methods. In javascript: modules -> classes -> function or modules -> functions. This information should later should enable me to output a string like the following, describing the file:
        CallableFunctionAttribute.cs CallableFunctionAttribute:Attribute@4-14{Name@6,Description@7,ctor(name,description)@9-13}";
        """;

    // [CallableFunction("ParseCodeFile", "Creates an index of the source code file")]
    // public async Task<string> ParseCodeFile(CreateIndexParams createIndexParams)
    // {
    //     this.CreateIndexParams = createIndexParams;
    //     return "";
    // }
    //
    // public CreateIndexParams CreateIndexParams { get; set; } = null!;
}

/*
 * Example (delineated by dashed lines):
   ----------------
   CallableFunctionAttribute.cs:
   using System.ComponentModel;
   
   namespace AiApp;
   
   [AttributeUsage(AttributeTargets.Method)]
   public class CallableFunctionAttribute : Attribute
   {
   public string Name { get; }
   public string Description { get; }
   
   public CallableFunctionAttribute(string name, string description)
   {
   Name = name;
   Description = description;
   }
   }
   ----------------
 */
public class CreateIndexParams
{
    public string fileName;
    [Description("In C# it would be namespaces, in javascript it would be require or import statements, etc. Just the string names.")]
    public string[] reference;
    [Description("The heirarchical AST-like information, describing the language specific code constructs. In C# it might be classes -> methods. In javascript: modules -> classes -> function or modules -> functions. etc.")]
    public ModuleDefinition[] moduleDefinitions;
}

public class ModuleDefinition
{
    [Description("Name of the code construct (eg class name, function name, etc)")]
    public string name;
    [Description("If the code construct can inherit from classes or interfaces, their name. Otherwise null.")]
    public string[]? implements;
    [Description("Start line number of the code construct.")]
    public int start;
    [Description("End line number of the code construct. null if single line.")]
    public int? end;
    public ModuleDefinition[] submodules;
}