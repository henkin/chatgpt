using AiApp;
using AiApp.Tests;
using FluentAssertions;

namespace Gptd.ChatLib.Tests;

public class FunctionCallTests
{
    [Fact]
    public async void BasicSingleFunctionTest()
    {
	    var appModel = new BasicMultiFunctionChat();
	    var content = await appModel.Ask("This has got to be a cool string!");
	    content.Should().Contain("did it!");
    }
}