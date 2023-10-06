using System.ComponentModel;
using System.Reflection;

namespace AiApp.Tests;

public class BasicMultiFunctionChat : MultiFunctionCallChat
{
    protected override string SystemPrompt => """
You are a decision engine. You are given a string by the user, and you have to decide whether the string is cool or not cool. 
To determine this, you must call a function is_cool() with the string. 
If is_cool() returns 'yup' (or another positive response), then the string is cool, and you must call the function do_cool_thing() with the string and return "was cool: X" where X is the return value of do_cool_thing().
Otherwise, you should call do_uncool() with the string, and return to the user the string 'wasnt cool: X', where X is the return value of do_uncool(). 
""";
/*
 * After you supply the list of changes, I will execute the commands as a user on a modern ubuntu box, while logged in and already in the project directory, as a regular user. I will then run the command 'make', and give you back an error message or end the connection - which will mean success. If you get an error message, send me further changes by calling the function.
 *
 * Try to predict that if the directory contents looks like it could be created with some more advanced version of a code-generating command like 'dotnet new', make the command for that. 
 */
    [CallableFunction("is_cool", "Returns a string indicating whether the input string is cool or not cool. The input string is cool only if return value is positive (eg 'yes' or 'yup', etc)")]
    public async Task<string> IsCool(IsCoolParams isCoolParams)
    {
        return "sure is cool!";
    }
    
    [CallableFunction("do_cool_thing", "Does the cool thing with the input string. Call only if the string is cool")]
    public async Task<string> DoCoolThing(DoCoolParams doCoolParams)
    {
        return "did it!";
    }
}


public class IsCoolParams
{
    [Description("The string to check if it's cool or not cool")]
    public string input;
}

public class DoCoolParams
{
    [Description("The string to do cool things to")]
    public string cool_string;
}
