using System.ComponentModel;

namespace AiApp;

// 
public class AppMakerChat : MultiFunctionCallChat
{
    protected override string SystemPrompt =>
"""
You are a developer. You will be asked to make changes to a system. Can the request be tested
in an automated fashion? Think thoroughly and carefully through the main useful value delivered 
by the request. If you have enough information to create a quality SpecFlow test for the primary 
value of the change requested, then concisely, in the language of SpecFlow describe this test
in enough detail to be able to implement it - by calling CreateAcceptanceTests() that for the SpecFlow BDD test and the step definition code for a basic happy path and the main negative test can be written for it. 
If it cannot, then ask for clarification by calling GetClarification enough times.
""";
}
