# OperationResults

[![Lint Code Base](https://github.com/marcominerva/OperationResults/actions/workflows/linter.yml/badge.svg)](https://github.com/marcominerva/OperationResults/actions/workflows/linter.yml)
[![CodeQL](https://github.com/marcominerva/OperationResults/actions/workflows/codeql.yml/badge.svg)](https://github.com/marcominerva/OperationResults/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/marcominerva/OperationResultTools/blob/master/LICENSE)

A set of lightweight libraries to totally decouple operation results and actual application responses.

## Core library

[![NuGet](https://img.shields.io/nuget/v/OperationResultTools.svg?style=flat-square)](https://www.nuget.org/packages/OperationResultTools)
[![Nuget](https://img.shields.io/nuget/dt/OperationResultTools)](https://www.nuget.org/packages/OperationResultTools)

**Installation**

The library is available on [NuGet](https://www.nuget.org/packages/OperationResultTools). Just search for *OperationResultTools* in the **Package Manager GUI** or run the following command in the **.NET CLI**:

    dotnet add package OperationResultTools

## ASP.NET Core integration library (Controller-based project)

[![NuGet](https://img.shields.io/nuget/v/OperationResultTools.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/OperationResultTools.AspNetCore)
[![Nuget](https://img.shields.io/nuget/dt/OperationResultTools.AspNetCore)](https://www.nuget.org/packages/OperationResultTools.AspNetCore)

_Note: This is the library to use if you're working with Controller._

This library provides HttpContext extension methods to automatically map Operation Results (that may come, for example, from a business layer) to HTTP responses, along with the appropriate status codes.

A full example is available in the [Example folder](https://github.com/marcominerva/OperationResults/tree/master/examples). Search for the registration in the [Program.cs](https://github.com/marcominerva/OperationResults/blob/master/examples/Controllers/OperationResults.Example/Program.cs#L24-L40) file and the usage in [Controllers folder](https://github.com/marcominerva/OperationResults/tree/master/examples/Controllers/OperationResults.Example/Controllers).

**Installation**

The library is available on [NuGet](https://www.nuget.org/packages/OperationResultTools.AspNetCore). Just search for *OperationResultTools.AspNetCore* in the **Package Manager GUI** or run the following command in the **.NET CLI**:

    dotnet add package OperationResultTools.AspNetCore

## ASP.NET Core integration library (Minimal API project)

[![NuGet](https://img.shields.io/nuget/v/OperationResultTools.AspNetCore.Http.svg?style=flat-square)](https://www.nuget.org/packages/OperationResultTools.AspNetCore.Http)
[![Nuget](https://img.shields.io/nuget/dt/OperationResultTools.AspNetCore.Http)](https://www.nuget.org/packages/OperationResultTools.AspNetCore.Http)

_Note: This is the library to use if you're working with Minimal APIs._

This library provides HttpContext extension methods to automatically map Operation Results (that may come, for example, from a business layer) to HTTP responses, along with the appropriate status codes.

A full example is available in the [Examples folder](https://github.com/marcominerva/OperationResults/tree/master/examples). Search for the [registration](https://github.com/marcominerva/OperationResults/blob/master/examples/MinimalApis/OperationResults.Example/Program.cs#L23-L35) and the [usage](https://github.com/marcominerva/OperationResults/blob/master/examples/MinimalApis/OperationResults.Example/Program.cs#L51-L106) in [Program.cs](https://github.com/marcominerva/OperationResults/blob/master/examples/MinimalApis/OperationResults.Example/Program.cs) file.

**Installation**

The library is available on [NuGet](https://www.nuget.org/packages/OperationResultTools.AspNetCore.Http). Just search for *OperationResultTools.AspNetCore.Http* in the **Package Manager GUI** or run the following command in the **.NET CLI**:

    dotnet add package OperationResultTools.AspNetCore.Http


**Contribute**

The project is constantly evolving. Contributions are welcome. Feel free to file issues and pull requests on the repo and we'll address them as we can.
