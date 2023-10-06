using System.Reflection;
using AiLib;
using AiLib.Chat;
using AiLib.Model;
using Newtonsoft.Json;
using Serilog;

namespace AiApp;

public abstract class MultiFunctionCallChat
{
    // Logger instance for logging information and errors.
    protected static readonly ILogger Logger = new LoggerConfiguration()
        .WriteTo.Console() // Optional: Log to the console
        .WriteTo.Seq("http://localhost:5341") // Optional: Log to Seq
        .CreateLogger();

    // OpenAI API instance for chat functionality.
    protected OpenAIAPI _api;

    // Array of chat messages.
    public ChatMessage[] Messages { get; set; }

    // System prompt for generating responses.
    protected virtual string SystemPrompt => "";

    /// <summary>
    /// Gets the list of function definitions.
    /// This property should be overridden in derived classes to provide specific function definitions.
    /// </summary>
    protected virtual List<ICallableFunction> FunctionDefinitions { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseChat"/> class.
    /// </summary>
    public MultiFunctionCallChat()
    {
        _api = new OpenAIAPI();

        // Initialize the Messages array with a system message.
        Messages = new ChatMessage[]
        {
            new(ChatMessageRole.System, SystemPrompt),
        };
        RegisterFunctions();
        Logger.Information("Constructor called, SystemPrompt: {SystemPrompt}", SystemPrompt);
    }

    /// <summary>
    /// Asks a user prompt and generates a response from the AI model.
    /// This method handles function calls in the AI responses until a complete response is obtained.
    /// </summary>
    /// <param name="userPrompt">The user prompt.</param>
    /// <returns>The generated response from the AI model.</returns>
    public virtual async Task<string> Ask(string userPrompt)
    {
        Logger.Warning("User: {UserPrompt}", userPrompt);

        var (resultString, firstChatChoice) = await AppendMessageAndGetFirstResult(
            new ChatMessage(ChatMessageRole.User, userPrompt)
        );

        while (firstChatChoice.FinishReason == "function_call")
        {
            Logger.Information("Function call detected: {FunctionCallChoice}", firstChatChoice);
            ChatMessage response = await HandleFunctionCall(firstChatChoice);

            (resultString, firstChatChoice) = await AppendMessageAndGetFirstResult(response);

            if (resultString != null)
            {
                Logger.Warning("AI: {ResultString}", resultString);
            }
        }

        // inline function that appends messages to the Messages array and calls getNextCompletionResults
        async Task<(string?, ChatChoice)> AppendMessageAndGetFirstResult(ChatMessage message)
        {
            Messages = Messages.Append(message).ToArray();
            var results = await GetNextCompletionResults();
            return (results.ToString(), results.Choices.First());
        }

        await Log.CloseAndFlushAsync();
        return resultString;
    }

    /// <summary>
    /// Gets the next completion results from the OpenAI API.
    /// This method retrieves the next set of completion results based on the current chat messages.
    /// </summary>
    /// <returns>The completion results from the OpenAI API.</returns>
    private async Task<ChatResult> GetNextCompletionResults()
    {
        try
        {
            var results = await _api.Chat.CreateChatCompletionAsync(new ChatRequest()
            {
                Model = Model.ChatGPTTurbo0613,
                Temperature = 0.4,
                MaxTokens = 3000,
                Messages = Messages,
                Functions = FunctionDefinitions.Any()
                    ? FunctionDefinitions.Select(f => f.GetFunction).ToList()
                    : null
            });
            return results;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    protected async Task<ChatMessage> HandleFunctionCall(ChatChoice chatChoice)
    {
        var functionCall = chatChoice.Message.FunctionCall;
        var functionName = functionCall.Name;

        try
        {
            Logger.Information("FunctionCallDispatch called, name: {Name}, args: {Args}", functionName,
                functionCall.Arguments);

            var matchingFunction = FindMatchingFunction(functionName);
            var functionParams = DeserializeFunctionArguments(functionCall.Arguments, matchingFunction);
            var functionResult = await CallFunctionWithParams(matchingFunction, functionParams);
            var returnMessage = CreateReturnMessage(functionName, functionResult);

            return (returnMessage);
        }
        catch (Exception e)
        {
            Logger.Error(e, "Error in FunctionCallDispatch");
            return (null);
        }
    }

    private ICallableFunction FindMatchingFunction(string functionName)
    {
        var matchingFunction = FunctionDefinitions.FirstOrDefault(f => f.Name == functionName);
        if (matchingFunction == null)
        {
            Logger.Error("No matching function handler found for function name: {Name}", functionName);
            throw new Exception($"No matching function handler found for function name: {functionName}");
        }

        return matchingFunction;
    }

    private object DeserializeFunctionArguments(string arguments, ICallableFunction matchingFunction)
    {
        Logger.Debug("FunctionCallDispatch {Name}, {Args}", matchingFunction.Name, arguments);
        return JsonConvert.DeserializeObject(arguments, matchingFunction.ParamType)!;
    }

    private async Task<object> CallFunctionWithParams(ICallableFunction matchingFunction, object functionParams)
    {
        Logger.Debug("Calling {Name} with {Params}", matchingFunction.Name, functionParams);
        object result = await matchingFunction.Call(functionParams!);
        Logger.Debug("Called {Name} with {Params} returned {Result}",
            matchingFunction.Name,
            functionParams,
            result);

        return result;
    }

    private ChatMessage CreateReturnMessage(string functionName, object functionResult)
    {
        var returnMessage = new ChatMessage(
            ChatMessageRole.Function,
            functionResult?.ToString() ?? "")
        {
            Name = functionName
        };

        return returnMessage;
    }

    /// <summary>
    /// Registers the functions by scanning the class for methods with the CallableFunction attributes, then adds them to the list of functions, using the equivalint of CreateFunction.
    /// </summary>
    private void RegisterFunctions()
    {
        var methods = GetType().GetMethods();
        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<CallableFunctionAttribute>();
            if (attribute != null)
            {
                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                {
                    throw new Exception(
                        $"CallableFunction {attribute.Name} has {parameters.Length} parameters, but should have exactly 1");
                }

                var parameterType = parameters[0].ParameterType;
                var functionType = typeof(CallableFunction<>).MakeGenericType(parameterType);
                var function = Activator.CreateInstance(functionType) as ICallableFunction;
                function.Name = attribute.Name;
                function.Description = attribute.Description;
                function.Parameters = SchemaLookup.GetSchemaForType(parameterType);

                // call CallableFunction through reflection to create the function with the FuncFallback
                // property set to a lambda that calls the method on this class's method with the CallableFunctionAttribute.
                var makeGenericType = typeof(Func<,>).MakeGenericType(parameterType, typeof(Task<string>));
                var funcCallback = method.CreateDelegate(makeGenericType, this);
                function.GetType().GetProperty("FuncCallback")!.SetValue(function, funcCallback);
                
                // get the instance of CallableFunction<> and set the FuncCallback property to a lambda that calls the method
                FunctionDefinitions.Add(function);
            }
        }
    }
    
    protected CallableFunction<TParams> CreateFunction<TParams>(
        string Name,
        string Description,
        Func<TParams, Task<string>> FuncCallback,
        bool IsTerminal = false)
    {
        return new CallableFunction<TParams>()
        {
            Name = Name,
            Description = Description,
            Parameters = SchemaLookup.GetSchemaForType(typeof(TParams)),
            FuncCallback = FuncCallback
        };
    }
}