using AiLib.ChatFunctions;

namespace AiApp;

public interface ICallableFunction
{
    public string Name { get; set; }
    public string Description { get; set; }
    public object Parameters { get; set; }
    Type ParamType { get; }
    Function GetFunction { get; }
    public Task<object> Call(object param);
}

public class CallableFunction<TParams> : Function, ICallableFunction
{
    public Type ParamType => typeof(TParams);
    public Function GetFunction => new(Name, Description, Parameters);
    public Func<TParams, Task<object>> FuncCallback { get; set; } = null!;
    
    public async Task<object> Call(object param)
    {
        return await FuncCallback((TParams) param);
    }
}