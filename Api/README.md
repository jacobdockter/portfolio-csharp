# Portfolio Lambdas

Holds library of lambdas for Portfolio suite.

## Getting Started

**You must have the C# extension installed in VS Code for this to work**
Ensure dotnet tool suite is installed:

1. dotnet new --install Amazon.Lambda.Templates
2. dotnet tool install -g Amazon.Lambda.Tools

List available commands: `dotnet new list`

Create a new lambda: `dotnet new lambda.EmptyFunction --name "NAME" --region "us-west-2" --profile "ProfileName"`
For help: `dotnet new lambda.EmptyFunction --help`
