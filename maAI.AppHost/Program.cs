var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.maAI>("maai");

builder.Build().Run();
