﻿// Copyright (c) Microsoft. All rights reserved.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Planning.Handlebars;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Planners.UnitTests.Handlebars;

public sealed class HandlebarsPlannerTests
{
    private const string PlanString =
    @"```handlebars
{{!-- Step 1: Call Summarize function --}}  
{{set ""summary"" (SummarizePlugin-Summarize)}}  

{{!-- Step 2: Call Translate function with the language set to French --}}  
{{set ""translatedSummary"" (WriterPlugin-Translate language=""French"" input=(get ""summary""))}}  

{{!-- Step 3: Call GetEmailAddress function with input set to John Doe --}}  
{{set ""emailAddress"" (email-GetEmailAddress input=""John Doe"")}}  

{{!-- Step 4: Call SendEmail function with input set to the translated summary and email_address set to the retrieved email address --}}  
{{email-SendEmail input=(get ""translatedSummary"") email_address=(get ""emailAddress"")}}
```";

    [Theory]
    [InlineData("Summarize this text, translate it to French and send it to John Doe.")]
    public async Task ItCanCreatePlanAsync(string goal)
    {
        // Arrange
        var plugins = this.CreatePluginCollection();
        var kernel = this.CreateKernelWithMockCompletionResult(PlanString, plugins);
        var planner = new HandlebarsPlanner();

        // Act
        HandlebarsPlan plan = await planner.CreatePlanAsync(kernel, goal);

        // Assert
        Assert.True(!string.IsNullOrEmpty(plan.Prompt));
        Assert.True(!string.IsNullOrEmpty(plan.ToString()));
    }

    [Fact]
    public async Task EmptyGoalThrowsAsync()
    {
        // Arrange
        var kernel = this.CreateKernelWithMockCompletionResult(PlanString);

        var planner = new HandlebarsPlanner();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(async () => await planner.CreatePlanAsync(kernel, string.Empty));
    }

    [Fact]
    public async Task InvalidHandlebarsTemplateThrowsAsync()
    {
        // Arrange
        var kernel = this.CreateKernelWithMockCompletionResult("<plan>notvalid<</plan>");

        var planner = new HandlebarsPlanner();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<KernelException>(async () => await planner.CreatePlanAsync(kernel, "goal"));
        Assert.True(exception?.Message?.Contains("Could not find the plan in the results", StringComparison.InvariantCulture));
    }

    private Kernel CreateKernelWithMockCompletionResult(string testPlanString, KernelPluginCollection? plugins = null)
    {
        plugins ??= new KernelPluginCollection();

        var chatMessage = new ChatMessageContent(AuthorRole.Assistant, testPlanString);

        var chatCompletion = new Mock<IChatCompletionService>();
        chatCompletion
            .Setup(cc => cc.GetChatMessageContentsAsync(It.IsAny<ChatHistory>(), It.IsAny<PromptExecutionSettings>(), It.IsAny<Kernel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ChatMessageContent> { chatMessage });

        var serviceSelector = new Mock<IAIServiceSelector>();
        IChatCompletionService resultService = chatCompletion.Object;
        PromptExecutionSettings resultSettings = new();
        serviceSelector
            .Setup(ss => ss.TrySelectAIService<IChatCompletionService>(It.IsAny<Kernel>(), It.IsAny<KernelFunction>(), It.IsAny<KernelArguments>(), out resultService!, out resultSettings!))
            .Returns(true);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAIServiceSelector>(serviceSelector.Object);
        serviceCollection.AddSingleton<IChatCompletionService>(chatCompletion.Object);

        return new Kernel(serviceCollection.BuildServiceProvider(), plugins);
    }

    private KernelPluginCollection CreatePluginCollection()
    {
        return new()
        {
            KernelPluginFactory.CreateFromFunctions("email", "Email functions", new[]
            {
                KernelFunctionFactory.CreateFromMethod(() => "MOCK FUNCTION CALLED", "SendEmail", "Send an e-mail"),
                KernelFunctionFactory.CreateFromMethod(() => "MOCK FUNCTION CALLED", "GetEmailAddress", "Get an e-mail address")
            }),
            KernelPluginFactory.CreateFromFunctions("WriterPlugin", "Writer functions", new[]
            {
                KernelFunctionFactory.CreateFromMethod(() => "MOCK FUNCTION CALLED", "Translate", "Translate something"),
            }),
            KernelPluginFactory.CreateFromFunctions("SummarizePlugin", "Summarize functions", new[]
            {
                KernelFunctionFactory.CreateFromMethod(() => "MOCK FUNCTION CALLED", "Summarize", "Summarize something"),
            })
        };
    }
}
