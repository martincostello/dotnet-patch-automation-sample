# .NET Patch Automation Sample

[![Build status][build-badge]][build-status]
[![OpenSSF Scorecard][ossf-badge]][ossf-scorecard]

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

## The Problem

Previously, [Microsoft did not make automatic updates to .NET available through Windows Update][dotnet-updates-annoucement].
Although this has been available [since April 2022][dotnet-updates-available], it is still
not enabled by default and only applies to Windows .NET applications which rely on .NET
being installed on the machine.

For self-contained deployments and applications deployed to any other operating system, such
as Linux, there is still no mechanism for applying .NET patches automatically. This requires
developers to update their applications month-to-month to ensure they stay secure
([and supported][dotnet-support-policy]).

New updates are announced in the [dotnet/announcements][patch-tuesday-annoucements] repository,
as well as any individual [CVEs][patch-tuesday-cves] that apply. Developers need to check
these announcements on a regular cadence to ensure they are aware of any updates that are
available that need to be applied to the applications they maintain. In a distrubuted software
architecture, this can potentially be a significant number of applications to update.

This maintenance burden eats into the time developers have to work on new features and
other changes that bring meaningful value to their applications and their businesses.

By tapping into the machine-readable [.NET release notes][dotnet-release-notes] and harnessing
the power of [GitHub Actions][github-actions], we can automate the process of updating
applications whose code is stored in GitHub to remove much of the manual burden of keeping
applications patched and up-to-date.

## How it Works

This repository contains a [GitHub Actions workflow][update-workflow] that uses a GitHub Actions
reusable workflow from the [martincostello/update-dotnet-sdk][update-dotnet-sdk-workflow] repository.
This workflow uses the [Update .NET SDK][update-dotnet-sdk] GitHub Action to check for updates
to the .NET SDK compared to the version specified in the [`global.json` file][global-json] that
is checked into this repository.

If an update is available, the workflow will also check for any Microsoft-published NuGet packages
that are eligible to be updated as part of the same release of the .NET SDK and update those too.

If an update to the .NET SDK was found, the workflow will create a pull request with the changes
targeting the default branch of the repository. This repository's continuous integration will
then run against the pull request to ensure that the changes do not break the application.

The changes and pull request are performed on behalf of a [GitHub App][github-apps]. A GitHub app
is used to allow the workflow to create pull requests on behalf of the application, rather than
act as a specific user. The GitHub app's private key is used to [generate a GitHub access token][workflow-application-token-action]
to use to authenticate with the GitHub repository. This is used instead of `GITHUB_TOKEN` to overcome
[restrictions to triggering other workflows][github-token-restrictions] which would otherwise
prevent the GitHub Actions [continuous integration][continuous-integration] from running as part of pull requests.

When the pull request is opened, a [second workflow][approve-and-merge-workflow] will run to verify
whether the pull request contains the changes that are expected from such an update, such as the
account it was raised from and what was changed as part of the pull request. If the changes are
those which are expected, then the pull request will be approved and auto-merge will be enabled.

Assuming that the CI build passes, the pull request will then be merged automatically, completing
the patch updates.

More information about the reusable workflow can be found [here][update-workflow-docs].

> The ASP.NET Core application used to demonstrate the automation process is based on the
> Todo App sample from my [_Integration Testing ASP.NET Core Minimal APIs_][dotnet-minimal-api-integration-testing] repository.

### Updating the .NET SDK

The [martincostello/update-dotnet-sdk][update-dotnet-sdk] GitHub Action is used to check for and
apply updates to the .NET SDK. This action uses the [.NET release notes][dotnet-release-notes] JSON
files in the [dotnet/core][dotnet-core] repository to determine the latest version of the .NET SDK
compared to the version in the `global.json` file of a repository. For example, the releases for
.NET 6 can be found in the [`release-notes/6.0/releases.json` file][dotnet-releases-json-60].

If an update is available, the action will update the the `global.json` file to use the latest
version of the .NET SDK that is available for that release channel (6.0, 7.0, 8.0 etc.) and then open
a pull request with the changes.

### Updating NuGet packages

If an update to the .NET SDK is made, the workflow will also check for any Microsoft-published NuGet
packages that were made available as part of the same release of the .NET SDK. For example, new
versions of the `Microsoft.AspNetCore.*` packages are published for each new .NET patch release.

By default, only packages with the following ID prefixes are updated as part of these checks:

- `Microsoft.AspNetCore.`
- `Microsoft.EntityFrameworkCore.`
- `Microsoft.Extensions.`
- `System.Text.Json`

The list of packages that are updated can be changed by specifiying them as a comma-separated list
via the `include-nuget-packages` input parameter to the GitHub Actions workflow.

These package updates are checked for and applied by the [dotnet-outdated][dotnet-outdated-github]
.NET global tool. More information about dotnet-outdated can be found [here][dotnet-outdated-hanselman].

The package updates are constrained to the same release channel as the NuGet package references in
the application already used. For example, if .NET 6 packages are used, then only patch updates for
the `6.0.x` NuGet packages will be applied; any package updates for .NET 7 (or later) are ignored.

### Approving and Merging Pull Requests

Once the pull request for any updates is opened, [another GitHub workflow][approve-and-merge-workflow]
will run to verify that that changes included in that pull request match the expected changes for a
.NET SDK update.

This workflow only runs when the pull request is opened by the same account that is configured for
the GitHub app that is used to apply the .NET SDK updates. This is to prevent the workflow from
running against pull requests created by any other GitHub user.

Both of the tools used to apply the Git commits write their commit messages in the same format as
[Dependabot][dependabot] does, by including some machine-readable YAML in the commit message. This
block of YAML includes the names of each package that is updated, as well as the [SemVer][semver-2]
major/minor/patch update type for each package. This is based on the approach used by the
[Fetch Metadata from Dependabot PRs][fetch-metadata] GitHub action that can be used to auto-approve
Dependabot updates.

For example, here is the message for a commit that updates the .NET SDK to version `7.0.203`:

```text
Update .NET SDK
Update .NET SDK to version 7.0.203.

---
updated-dependencies:
- dependency-name: Microsoft.NET.Sdk
  dependency-type: direct:production
  update-type: version-update:semver-patch
...
```

The workflow parses this information to check for which packages were updated. If the packages that
were updated all match the list of allowed prefixes, all of the commits were authored by the
GitHub user that opened the pull request, and all of the updates are only for a patch release, then
the workflow determines that the pull request is _"trusted"_ and is safe to approve and merge.

If the changes are as expected, then the workflow will approve the pull request and enable auto-merge.
Assuming that the continuous integration succeeds, which is used as a measure of quality and stability
of the changes in the pull request, then the pull request will be merged to the default branch automatically
with no human intervention required.

If any of the required status checks fail, then the pull request will be left in a pending state for
a human to review and determine the appropriate course of action to take.

If any unexpected changes are present in the pull request, then the pull request will not be approved
and auto-merge will not be enabled. If these changes were introduced after the pull request was already
approved, then the review will be dismissed and auto-merge will be disabled.

### Further Considerations

The workflow in this sample is designed to be as safe as possible while being easy to set up, but
there are some aspects that are not covered by the workflow in the aim of simplicity that you may
want to consider before adopting this approach for your applications.

#### Branch Protections

This sample repository is set up with the following branch protections for the default branch:

- A pull request us required before merging
- Status checks must pass before merging
- No accounts are allowed to bypass the branch protections

These requirements protect the default branch from having the automated patches from being merged
in without them being validated as not breaking the application and having a "second pair of eyes"
review the changes on the pull request by a second GitHub account/app.

In order for the pull requests to be auto-merged, the number of required reviewers cannot be more
than one and [code owner][code-owners] review cannot be required. This is because GitHub apps cannot
be made code owners of a repository, and requiring more than one reviewer would still require a human
to approve the pull request. This could be overcome with multiple appoval bots, but that would likely
be excessive to configure.

The accounts used to open the pull requests and approve the pull requests must be different accounts
as GitHub does not allow an account to approve its own pull request.

#### Deployment tests

If you practice continuous deployment, then you may want to consider adding tests as part of your
deployment process to ensure that the application is still working as expected after the pull request
has been merged to your default branch. This helps validate that the changes in the pull request do
not break the application for any functionality that is not exercised by your continuous integration's
tests and that the changes are safe to deploy into your production environment.

### Examples

#### Pull Requests

Below are links to example pull requests demonstrating different behaviours of the workflows.

- [Approving a pull request raised by a GitHub app][approved-and-merged-bot]
- [Approving a pull request raised by a GitHub user][approved-and-merged-pat]
- [Dismissing approval when other changes are detected][approval-dismissed]

#### Workflows

Further examples of workflows for updating the .NET SDK with different types of GitHub credentials
can be found [in the README of the update-dotnet-sdk action][update-workflow-docs].

## Feedback

Any feedback or issues can be added to the issues for this project in [GitHub][issues].

## Repository

The repository is hosted in [GitHub][repository]: <https://github.com/martincostello/dotnet-patch-automation-sample.git>

## License

This project is licensed under the [Apache 2.0][license] license.

[approval-dismissed]: https://github.com/martincostello/dotnet-patch-automation-sample/pull/36 "Dismissed pull request"
[approve-and-merge-workflow]: https://github.com/martincostello/dotnet-patch-automation-sample/blob/main/.github/workflows/approve-and-merge.yml "approve-and-merge workflow"
[approved-and-merged-bot]: https://github.com/martincostello/dotnet-patch-automation-sample/pull/80 "Approved pull request created by a GitHub app"
[approved-and-merged-pat]: https://github.com/martincostello/dotnet-patch-automation-sample/pull/37 "Approved pull request created by a GitHub user"
[build-badge]: https://github.com/martincostello/dotnet-patch-automation-sample/actions/workflows/build.yml/badge.svg?branch=main&event=push
[build-status]: https://github.com/martincostello/dotnet-patch-automation-sample/actions/workflows/build.yml?query=branch%3Amain+event%3Apush
[code-owners]: https://docs.github.com/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners "About code owners"
[continuous-integration]: https://github.com/martincostello/dotnet-patch-automation-sample/blob/main/.github/workflows/build.yml "The continuous integration workflow to build and test the application"
[dependabot]: https://docs.github.com/code-security/dependabot/dependabot-version-updates/about-dependabot-version-updates "About Dependabot version updates"
[dotnet-core]: https://github.com/dotnet/core "The .NET Core repository"
[dotnet-minimal-api-integration-testing]: https://github.com/martincostello/dotnet-minimal-api-integration-testing "The martincostello/dotnet-minimal-api-integration-testing repository"
[dotnet-outdated-github]: https://github.com/dotnet-outdated/dotnet-outdated "The .NET Outdated repository"
[dotnet-outdated-hanselman]: https://www.hanselman.com/blog/dotnet-outdated-helps-you-keep-your-projects-up-to-date "dotnet outdated helps you keep your projects up to date"
[dotnet-release-notes]: https://github.com/dotnet/core/tree/main/release-notes ".NET release notes"
[dotnet-releases-json-60]: https://github.com/dotnet/core/blob/main/release-notes/6.0/releases.json ".NET 6 release notes JSON"
[dotnet-support-policy]: https://dotnet.microsoft.com/platform/support/policy/dotnet-core ".NET and .NET Core Support Policy"
[dotnet-updates-annoucement]: https://devblogs.microsoft.com/dotnet/net-core-updates-coming-to-microsoft-update/ ".NET Core 2.1, 3.1, and .NET 5.0 updates are coming to Microsoft Update"
[dotnet-updates-available]: https://devblogs.microsoft.com/dotnet/server-operating-systems-auto-updates/ ".NET Automatic Updates for Server Operating Systems"
[fetch-metadata]: https://github.com/marketplace/actions/fetch-metadata-from-dependabot-prs "The Fetch Metadata from Dependabot PRs GitHub Action"
[github-actions]: https://github.com/features/actions "GitHub Actions documentation"
[github-apps]: https://docs.github.com/apps/creating-github-apps/creating-github-apps/about-apps "About GitHub apps"
[github-token-restrictions]: https://docs.github.com/actions/using-workflows/triggering-a-workflow#triggering-a-workflow-from-a-workflow "Triggering a workflow from a workflow"
[global-json]: https://github.com/martincostello/dotnet-patch-automation-sample/blob/main/global.json "This repository's global.json file"
[issues]: https://github.com/martincostello/dotnet-patch-automation-sample/issues "Issues for this project on GitHub.com"
[license]: https://www.apache.org/licenses/LICENSE-2.0.txt "The Apache 2.0 license"
[ossf-badge]: https://api.securityscorecards.dev/projects/github.com/martincostello/dotnet-patch-automation-sample/badge "Open Source Security Foundation (OSSF) Scorecard Badge"
[ossf-scorecard]: https://securityscorecards.dev/viewer/?uri=github.com/martincostello/dotnet-patch-automation-sample "Open Source Security Foundation (OSSF) Scorecard"
[patch-tuesday-annoucements]: https://github.com/dotnet/announcements/labels/Patch-Tuesday "Announcements labelled Patch-Tuesday"
[patch-tuesday-cves]: https://github.com/dotnet/announcements/labels/Security "Announcements labelled Security"
[repository]: https://github.com/martincostello/dotnet-patch-automation-sample "This project on GitHub.com"
[semver-2]: https://semver.org/ "Semantic Versioning 2.0.0"
[update-dotnet-sdk]: https://github.com/marketplace/actions/update-net-sdk "The Update .NET SDK GitHub Action"
[update-dotnet-sdk-workflow]: https://github.com/martincostello/update-dotnet-sdk/blob/main/.github/workflows/update-dotnet-sdk.yml "The update-dotnet-sdk reusable workflow"
[update-workflow]: https://github.com/martincostello/dotnet-patch-automation-sample/blob/main/.github/workflows/update-dotnet-sdk.yml "The update-dotnet-sdk workflow for this repository"
[update-workflow-docs]: https://github.com/martincostello/update-dotnet-sdk#advanced-workflow "The documentation for the update-dotnet-sdk reusable workflow"
[workflow-application-token-action]: https://github.com/marketplace/actions/workflow-application-token-action "The workflow-application-token-action GitHub action"
