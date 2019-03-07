
# AWS Project


# Development Software

- Visual Studio 2017
- AWS Tookkit for Visual Studio
- Postman
- npm and npm module serverless [https://serverless.com/]


# Project Setup and deployment

There are two different ways to setup an ASP.NET Core Lambda project:

1. Create a standard ASP.NET Core site and add Lambda support (favorite method)
2. Using the template in the AWS Toolkit for VS to create an AWS Serverless Application

## Option 1 - Standard ASP.NET Core Web API with Lambda Support (favorite method)

I like this method best due to:

- It's a standard ASP.NET Core project
- Central place to manage dependency injection
- You control the routing at the application level.  Less boilerplate for CloudFormation
- The Microsoft.AspNetCore.App meta-package is already on the AWS server and pre-JITted for speed.  This includes Entity Framework Core.
- You can use standard authorization policies

I used the npm module `serverless` to deploy the code to AWS.  The deployment process is scriptable from any CI service.
It creates the AWS CloudFormation definition, API Gateway, and Lambda functions.

Code for project: [https://github.com/HeathHopkins/AwsStandardAspApplication]
AWS Published: [https://cmtz91mtb0.execute-api.us-east-1.amazonaws.com/dev/api/hospitals]

The latest version of ASP.NET Core suppported on AWS Lambda is 2.1.6 as of 2019-03-05 [https://github.com/aws/aws-lambda-dotnet#version-status]  2.2 is not supported yet.

### Project Setup

New Project -> ASP.NET Core Web Application
Select .NET Core, ASP.NET Core 2.1, and Empty Project
Edit the csproj file to read like this: (2.1.6 is latest version supported by Lambda)

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>netcoreapp2.1</TargetFramework>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.App" Version="2.1.6" />
    <PackageReference Include="Amazon.Lambda.Core" Version="1.1.0" />
    <PackageReference Include="Amazon.Lambda.Serialization.Json" Version="1.5.0" />
    <PackageReference Include="Amazon.Lambda.AspNetCoreServer" Version="3.0.2" />
  </ItemGroup>
</Project>
```

Add the Lambda entry point by creating a file at the root of the project named LambdaEntryPoint.cs with the contents like:

```csharp
using Microsoft.AspNetCore.Hosting;
namespace AwsStandardAspApplication
{
    public class LambdaEntryPoint : Amazon.Lambda.AspNetCoreServer.APIGatewayProxyFunction
    {
        protected override void Init(IWebHostBuilder builder)
        {
            builder.UseStartup<Startup>();
        }
    }
}
```

Add a file at the root named serverless.yml with contents like:

```yml
service: AwsStandardAspApplication

provider:
  name: aws
  runtime: dotnetcore2.1
  region: us-east-1

package:
  artifact: bin/release/netcoreapp2.1/deploy-package.zip

functions:
  api:
    handler: AwsStandardAspApplication::AwsStandardAspApplication.LambdaEntryPoint::FunctionHandlerAsync
    events:
     - http:
         path: /{proxy+}
         method: ANY
```

### Add Build Scripts and Deploy

Install the AWS Lambda tools for your system `dotnet tool install --global Amazon.Lambda.Tools --version 3.0.1`

Create a file at the root named build.ps1 with the contents like:

```powershell
dotnet restore
dotnet lambda package --configuration release --framework netcoreapp2.1 --output-package bin/release/netcoreapp2.1/deploy-package.zip
```

Deploy with `serverless deploy -v`


### Adding API Endpoints

These are standard ASP.NET Core actions on an ApiController.

Add a folder named "Controllers"
Add a new empty API Controller named "HospitalsController"
The controller will automatically be decorated with a route attribute that puts it at "/api/hospital"

Here's an example set of actions for a hospital:

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwsStandardAspApplication.Models;
using Microsoft.AspNetCore.Mvc;

namespace AwsStandardAspApplication.Controllers
{
    //[Authorize(Policy = Policies.Hospital)]
    [Route("api/[controller]")]
    [ApiController]
    public class HospitalsController : ControllerBase
    {
        private readonly DataContext _db;

        public HospitalsController(DataContext db) // injected dependencies
        {
            _db = db;
        }

        // route: GET /api/hospitals
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            await _db.MockQuery(); // simulate database query
            var model = new List<String>
            {
                "General Hospital",
                "Sacred Heart"
            };
            return Ok(model);
        }

        // route: GET /api/hospitals/1
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetHospital(int id)
        {
            await _db.MockQuery(); // simulate database query
            var hospital = "Sacred Heart";
            if (hospital == null)
                return NotFound();
            return Ok(hospital);
        }

        // route: POST /api/hospitals
        [HttpPost]
        public async Task<IActionResult> CreateHospital(HospitalDTO hospitalDTO)
        {
            await _db.MockQuery(); // simulate database query
            return CreatedAtAction(nameof(GetHospital), new { id = 2 });
        }

        // route: POST /api/hospitals/search
        [HttpPost]
        public async Task<IActionResult> SearchHospitals(SearchHospitalDTO hospitalDTO)
        {
            await _db.MockQuery(); // simulate database query
            var results = new List<string>();
            return Ok(results);
        }

        public class SearchHospitalDTO
        {
            public string Name { get; set; }
            public string ZipCode { get; set; }
        }

        // route: GET /api/hospitals/forzipcode/30009
        [HttpGet("forzipcode/{zipcode}")]
        public async Task<IActionResult> GetForZipCode(string zipcode)
        {
            await _db.MockQuery(); // simulate database query
            var model = new List<String>
            {
                "General Hospital",
                "Sacred Heart"
            };
            return Ok(model);
        }

        // route: DELETE /api/hospitals/1
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteHospital(int id)
        {
            await _db.MockQuery(); // simulate database query
            var hospital = "Sacred Heart";
            if (hospital == null)
                return NotFound();
            await _db.MockQuery(); // simulate database delete
            return Ok();
        }
    }
}
```

### Testing

You can test by starting a debug session and Visual Studio will create an IIS Express site and configure it for SSL.

You can test the methods manually using an application like Postman.
For the example, VS created the URI https://localhost:44322/api/hospitals for the "get all" method

You can also setup unit tests in the standard way.

For database developement, I would use Entity Framework with just an in-memory provider until the application was built.  
Then you could use EF migrations to create and manage the PostgreSQL schema.



## Option 2 - AWS Template - Serverless Application

Here's a great walkthrough of using the Serverless Application template:
[https://docs.aws.amazon.com/toolkit-for-visual-studio/latest/user-guide/lambda-build-test-severless-app.html]

Why I don't like this method:

- There is a lot of variable lookup and casting vs the dependency injection model of option 1.
- Local debugging through Visual Studio and IISExpress didn't work out of the box.
- You're missing out on a ton of application tooling from Microsoft.

For example, an HTTP 200 response with the AWS template:
```csharp
return APIGatewayProxyResponse
{
    StatusCode = (int)HttpStatusCode.OK,
    Body = "Hello AWS Serverless",
    Headers = new Dictionary<string, string> { { "Content-Type", "text/plain" } }
};
```

The same with ASP.NET
```csharp
return Content("Hello AWS Serverless", "text/plain");
```
