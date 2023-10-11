using System.ComponentModel;

namespace AiApp;

[AttributeUsage(AttributeTargets.Method)]
public class CallableFunctionAttribute : Attribute
{
    public string Description { get; }

    public CallableFunctionAttribute(string description)
    {
        Description = description;
    }
}
