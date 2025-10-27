// Copyright (c) Martin Costello, 2023. All rights reserved.
// Licensed under the Apache 2.0 license. See the LICENSE file in the project root for full license information.

using System.Net;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Hosting;

namespace TodoApp;

public sealed class HttpServerFixture : TodoAppFixture
{
    public HttpServerFixture()
    {
        UseKestrel(
            (server) => server.Listen(
                IPAddress.Loopback, 0, (listener) => listener.UseHttps(
                    (https) => https.ServerCertificate = X509CertificateLoader.LoadPkcs12FromFile("localhost-dev.pfx", "Pa55w0rd!"))));
    }

    public string ServerAddress
    {
        get
        {
            StartServer();
            return ClientOptions.BaseAddress.ToString();
        }
    }
}
