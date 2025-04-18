﻿// Copyright (c) Martin Costello, 2023. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using AspNet.Security.OAuth.GitHub;
using JustEat.HttpClientInterception;
using MartinCostello.Logging.XUnit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace TodoApp;

public class TodoAppFixture : WebApplicationFactory<Program>, ITestOutputHelperAccessor
{
    public TodoAppFixture()
    {
        ClientOptions.AllowAutoRedirect = false;
        ClientOptions.BaseAddress = new Uri("https://localhost");
        Interceptor = new HttpClientInterceptorOptions().ThrowsOnMissingRegistration();
    }

    public HttpClientInterceptorOptions Interceptor { get; }

    public ITestOutputHelper? OutputHelper { get; set; }

    public void ClearOutputHelper()
        => OutputHelper = null;

    public void SetOutputHelper(ITestOutputHelper value)
        => OutputHelper = value;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration(configBuilder =>
        {
            var dataDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }

            var config = new[]
            {
                KeyValuePair.Create<string, string?>("DataDirectory", dataDirectory),
                KeyValuePair.Create<string, string?>("GitHub:ClientId", "github-id"),
                KeyValuePair.Create<string, string?>("GitHub:ClientSecret", "github-secret"),
                KeyValuePair.Create<string, string?>("GitHub:EnterpriseDomain", string.Empty)
            };

            configBuilder.AddInMemoryCollection(config);
        });

        builder.ConfigureLogging(loggingBuilder => loggingBuilder.ClearProviders().AddXUnit(this));
        builder.UseSolutionRelativeContentRoot(Path.Combine("src", "TodoApp"), "*.slnx");

        builder.ConfigureServices(services =>
        {
            services.AddHttpClient();

            services.AddSingleton<IHttpMessageHandlerBuilderFilter, HttpRequestInterceptionFilter>(
                _ => new HttpRequestInterceptionFilter(Interceptor));

            services.AddSingleton<IPostConfigureOptions<GitHubAuthenticationOptions>, RemoteAuthorizationEventsFilter>();
            services.AddScoped<LoopbackOAuthEvents>();
        });

        Interceptor.RegisterBundle("oauth-http-bundle.json");
    }
}
