// Copyright (c) Martin Costello, 2023. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using Microsoft.AspNetCore.HttpOverrides;
using TodoApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTodoApi()
                .AddGitHubAuthentication()
                .AddRazorPages();

if (string.Equals(builder.Configuration["CODESPACES"], "true", StringComparison.OrdinalIgnoreCase))
{
    builder.Services.Configure<ForwardedHeadersOptions>(
        options => options.ForwardedHeaders |= ForwardedHeaders.XForwardedHost);
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
}

app.UseStatusCodePagesWithReExecute("/error", "?id={0}");

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseAuthentication()
   .UseAuthorization();

app.MapAuthenticationRoutes()
   .MapTodoApiRoutes();

app.MapRazorPages();

app.Run();

public partial class Program
{
    // Expose the Program class for use with WebApplicationFactory<T>
}
