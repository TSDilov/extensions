﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Http.Telemetry;

namespace Microsoft.Extensions.Resilience;

internal static class TelemetryHelper
{
    internal static string GetDimensionOrUnknown(this string? dimension)
    {
        return string.IsNullOrEmpty(dimension) ? TelemetryConstants.Unknown : dimension!;
    }
}