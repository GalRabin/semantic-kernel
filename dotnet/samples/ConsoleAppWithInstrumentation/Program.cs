// Copyright (c) Microsoft. All rights reserved.

using ConsoleAppWithInstrumentation;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Planning.Sequential;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var logger = configureLogger();
        using var metricListener = new LoggingMeterListener(logger, scenario, tenantID);
        metricListener.Start();

        var kernel = createKernel(logger);
        var planner = createPlanner(kernel, logger);

        try
        {
            var plan = await planner.CreatePlanAsync("Write a poem about John Doe, then translate it into Italian.");
            var result = await kernel.RunAsync(plan);
        }
        finally
        {
            await Task.Delay(5000);
        }
    }

    private static LoggerDecorator configureLogger()
    {
        using var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddConsole();
        });
        var logger = loggerFactory.CreateLogger<Program>();
        return new LoggerDecorator(logger: logger, correlationID: correlationID);
    }

    private static IKernel createKernel(ILogger logger)
    {
        string folder = RepoFiles.SampleSkillsPath();
        var kernel = new KernelBuilder()
            .WithLogger(logger)
            .WithAzureTextCompletionService(
                Env.Var("AzureOpenAI__ChatDeploymentName"),
                Env.Var("AzureOpenAI__Endpoint"),
                Env.Var("AzureOpenAI__ApiKey"))
            .Build();
        kernel.ImportSemanticSkillFromDirectory(folder, "SummarizeSkill", "WriterSkill");
        return kernel;
    }

    private static ISequentialPlanner createPlanner(IKernel kernel, ILogger logger)
    {
        var plannerConfig = new SequentialPlannerConfig { };

        return new SequentialPlanner(kernel, plannerConfig).WithInstrumentation(logger);
    }

    private static string correlationID = Guid.NewGuid().ToString();
    private static string tenantID = Guid.NewGuid().ToString();
    private static string scenario = "SOA";
}
