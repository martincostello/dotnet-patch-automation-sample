﻿// Copyright (c) Martin Costello, 2023. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

namespace TodoApp;

public class BrowserFixtureOptions
{
    public string BrowserType { get; set; } = Microsoft.Playwright.BrowserType.Chromium;

    public string? BrowserChannel { get; set; }

    public bool CaptureTrace { get; set; } = BrowserFixture.IsRunningInGitHubActions;

    public bool CaptureVideo { get; set; } = BrowserFixture.IsRunningInGitHubActions;

    public string? TestName { get; set; }

    public string? Build { get; set; }

    public string? OperatingSystem { get; set; }

    public string? OperatingSystemVersion { get; set; }

    public string? PlaywrightVersion { get; set; }

    public string? ProjectName { get; set; }
}
