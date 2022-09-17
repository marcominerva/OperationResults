# OperationResults

[![Lint Code Base](https://github.com/marcominerva/OperationResults/actions/workflows/linter.yml/badge.svg)](https://github.com/marcominerva/OperationResults/actions/workflows/linter.yml)
[![CodeQL](https://github.com/marcominerva/OperationResults/actions/workflows/codeql.yml/badge.svg)](https://github.com/marcominerva/OperationResults/actions/workflows/codeql.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://github.com/marcominerva/OperationResultTools/blob/master/LICENSE)

A set of lightweight libraries to totally decouple operation results and actual application responses.

## Core library

[![NuGet](https://img.shields.io/nuget/v/OperationResultTools.svg?style=flat-square)](https://www.nuget.org/packages/OperationResultTools)
[![Nuget](https://img.shields.io/nuget/dt/OperationResulTools)](https://www.nuget.org/packages/OperationResultTools)

**Installation**

The library is available on [NuGet](https://www.nuget.org/packages/OperationResultTools). Just search *OperationResultTools* in the **Package Manager GUI** or run the following command in the **Package Manager Console**:

    Install-Package OperationResultTools

## ASP.NET Core integration library

[![NuGet](https://img.shields.io/nuget/v/OperationResultTools.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/OperationResultTools.AspNetCore)
[![Nuget](https://img.shields.io/nuget/dt/OperationResulTools.AspNetCore)](https://www.nuget.org/packages/OperationResultTools.AspNetCore)

This library provides HttpContext extensions to automatically map Operation Results (that may come, for example, from a business layer) to HTTP responses, along with the appropriate status codes.

A full example is available in the [Samples](https://github.com/marcominerva/OperationResults/tree/master/samples) folder. Search for the registration in the [Program.cs](https://github.com/marcominerva/OperationResults/blob/master/samples/OperationResults.Sample/Program.cs#L22) file and the usage in [Controllers](https://github.com/marcominerva/OperationResults/tree/master/samples/OperationResults.Sample/Controllers).

**Installation**

The library is available on [NuGet](https://www.nuget.org/packages/OperationResultTools.AspNetCore). Just search *OperationResultTools.AspNetCore* in the **Package Manager GUI** or run the following command in the **Package Manager Console**:

    Install-Package OperationResultTools.AspNetCore

**Contribute**

The project is constantly evolving. Contributions are welcome. Feel free to file issues and pull requests on the repo and we'll address them as we can.