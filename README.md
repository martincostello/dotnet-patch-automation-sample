# .NET Patch Automation Sample

[![Build status](https://github.com/martincostello/dotnet-patch-automation-sample/workflows/build/badge.svg?branch=main&event=push)](https://github.com/martincostello/dotnet-patch-automation-sample/actions?query=workflow%3Abuild+branch%3Amain+event%3Apush)

## Introduction

With the productivity and performance benefits developers gain from using modern .NET
over .NET Framework, also comes the less-exciting flip-side - patching the version of
.NET in production environments every month to keep your applications secure. :calendar: :key:

Keeping up-to-date with security and reliability fixes is an important ongoing activity
within software development, but it’s not very exciting, and it can be easy to fall
behind - what if we could automate the process of patching our applications? :robot:

This sample repository demonstrates how we can use the flexibility of [GitHub Actions][github-actions]
together with tools such as [dotnet-outdated][dotnet-outdated-github] to automatically
patch .NET applications on a monthly basis with minimal manual effort. :rocket:

## How it Works

_TODO_

## Debugging

To debug the application locally outside of the integration tests, you will need
to [create a GitHub OAuth app][create-github-oauth-app] to obtain secrets for the
`GitHub:ClientId` and `GitHub:ClientSecret` options so that the [OAuth user authentication][user-oauth]
works and you can log into the Todo App UI.

> 💡 When creating the GitHub OAuth app, use `https://localhost:5001/sign-in-github`
as the _Authorization callback URL_.
>
> ⚠️ Do not commit GitHub OAuth secrets to source control. Configure them
with [User Secrets][user-secrets] instead.

## Building and Testing

Compiling the application yourself requires Git and the
[.NET SDK](https://www.microsoft.com/net/download/core "Download the .NET SDK")
to be installed (version `7.0.200` or later).

To build and test the application locally from a terminal/command-line, run the
following set of commands:

```powershell
git clone https://github.com/martincostello/dotnet-patch-automation-sample.git
cd dotnet-patch-automation-sample
./build.ps1
```

## Feedback

Any feedback or issues can be added to the issues for this project in
[GitHub](https://github.com/martincostello/dotnet-patch-automation-sample/issues "Issues for this project on GitHub.com").

## Repository

The repository is hosted in
[GitHub](https://github.com/martincostello/dotnet-patch-automation-sample "This project on GitHub.com"):
https://github.com/martincostello/dotnet-patch-automation-sample.git

## License

This project is licensed under the
[Apache 2.0](http://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license")
license.

[create-github-oauth-app]: https://docs.github.com/apps/oauth-apps/building-oauth-apps/creating-an-oauth-app
[dotnet-minimal-api-integration-testing]: https://github.com/martincostello/dotnet-minimal-api-integration-testing
[dotnet-outdated-github]: https://github.com/dotnet-outdated/dotnet-outdated
[dotnet-outdated-hanselman]: https://www.hanselman.com/blog/dotnet-outdated-helps-you-keep-your-projects-up-to-date
[fetch-metadata]: https://github.com/marketplace/actions/fetch-metadata-from-dependabot-prs
[github-actions]: https://github.com/features/actions
[update-net-sdk]: https://github.com/marketplace/actions/update-net-sdk
[user-oauth]: https://docs.microsoft.com/aspnet/core/security/authentication/social/
[user-secrets]: https://docs.microsoft.com/aspnet/core/security/app-secrets
