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