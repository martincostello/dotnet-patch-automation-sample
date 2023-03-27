// Copyright (c) Martin Costello, 2023. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Security.Claims;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TodoApp;

public static class AuthenticationEndpoints
{
    private const string DeniedPath = "/denied";
    private const string RootPath = "/";
    private const string SignInPath = "/sign-in";
    private const string SignOutPath = "/sign-out";

    private const string GitHubAvatarClaim = "urn:github:avatar";
    private const string GitHubProfileClaim = "urn:github:profile";

    public static IServiceCollection AddGitHubAuthentication(this IServiceCollection services)
    {
        return services
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(options =>
            {
                options.LoginPath = SignInPath;
                options.LogoutPath = SignOutPath;
            })
            .AddGitHub()
            .Services
            .AddOptions<GitHubAuthenticationOptions>(GitHubAuthenticationDefaults.AuthenticationScheme)
            .Configure<IConfiguration>((options, configuration) =>
            {
                options.AccessDeniedPath = DeniedPath;
                options.CallbackPath = SignInPath + "-github";
                options.ClientId = configuration["GitHub:ClientId"] ?? string.Empty;
                options.ClientSecret = configuration["GitHub:ClientSecret"] ?? string.Empty;
                options.EnterpriseDomain = configuration["GitHub:EnterpriseDomain"];

                options.Scope.Add("user:email");
                options.ClaimActions.MapJsonKey(GitHubProfileClaim, "html_url");

                if (string.IsNullOrEmpty(options.EnterpriseDomain))
                {
                    options.ClaimActions.MapJsonKey(GitHubAvatarClaim, "avatar_url");
                }
            })
            .ValidateOnStart()
            .Services;
    }

    public static string? GetAvatarUrl(this ClaimsPrincipal user)
        => user.FindFirst(GitHubAvatarClaim)?.Value;

    public static string GetProfileUrl(this ClaimsPrincipal user)
        => user.FindFirst(GitHubProfileClaim)!.Value;

    public static string GetUserId(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)!.Value;

    public static string GetUserName(this ClaimsPrincipal user)
        => user.FindFirst(GitHubAuthenticationConstants.Claims.Name)!.Value;

    public static IEndpointRouteBuilder MapAuthenticationRoutes(this IEndpointRouteBuilder builder)
    {
        builder.MapGet(DeniedPath, () => Results.Redirect(RootPath + "?denied=true"))
               .ExcludeFromDescription();

        builder.MapGet(SignOutPath, () => Results.Redirect(RootPath))
               .ExcludeFromDescription();

        builder.MapPost(SignInPath, () => Results.Challenge(new() { RedirectUri = RootPath }, new[] { GitHubAuthenticationDefaults.AuthenticationScheme }))
               .ExcludeFromDescription();

        builder.MapPost(SignOutPath, () => Results.SignOut(new() { RedirectUri = RootPath }, new[] { CookieAuthenticationDefaults.AuthenticationScheme }))
               .ExcludeFromDescription()
               .RequireAuthorization();

        return builder;
    }
}
