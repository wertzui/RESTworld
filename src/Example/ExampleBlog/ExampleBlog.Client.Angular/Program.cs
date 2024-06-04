using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

var rwBuilder = builder.AddRestWorldWithSpaFrontend();

var app = rwBuilder.Build();

app.UseRestWorldWithSpaFrontend();

await app.RunAsync();