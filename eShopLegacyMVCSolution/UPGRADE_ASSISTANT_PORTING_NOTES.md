# MS Upgrade Assistant

https://github.com/dotnet/upgrade-assistant#installation

## Dependencies

### Install visual studio community

Depends on MSBuild, and VS2019 is recommend, so:

    choco install visualstudio2019community -y

### install upgrade assistant

    dotnet tool install -g upgrade-assistant

error installing, so also tried:

    dotnet tool install -g upgrade-assistant --ignore-failed-sources

```
PS C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC> dotnet tool install -g upgrade-assistant --ignore-failed-sources
error NU1100: Unable to resolve 'upgrade-assistant (>= 0.0.0)' for 'net5.0'.
error NU1100: Unable to resolve 'upgrade-assistant (>= 0.0.0)' for 'net5.0/any'.
The tool package could not be restored.
Tool 'upgrade-assistant' failed to install. This failure may have been caused by:

* You are attempting to install a preview release and did not use the --version option to specify the version.
* A package by this name was found, but it was not a .NET tool.
* The required NuGet feed cannot be accessed, perhaps because of an Internet connection problem.
* You mistyped the name of the tool.

For more reasons, including package naming enforcement, visit https://aka.ms/failure-installing-tool
```

Tried again with latest source:

    dotnet tool install -g upgrade-assistant --add-source https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-tools/nuget/v3/index.json

This time worked.

## Giving the analyze a test spin:

```
    u upgrade-assistant analyze .\eShopLegacyMVC.sln
```


```
-----------------------------------------------------------------------------------------------------------------
Microsoft .NET Upgrade Assistant v0.2.242001+4be4778cb767292f63833ee56322a50fe1041a0b

We are interested in your feedback! Please use the following link to open a survey: https://aka.ms/DotNetUASurvey
-----------------------------------------------------------------------------------------------------------------

[04:50:59 INF] Loaded 5 extensions

Telemetry
----------
The .NET tools collect usage data in order to help us improve your experience. The data is collected by Microsoft and shared with the community. You can opt-out of telemetry by setting the DOTNET_UPGRADEASSISTANT_TELEMTRY_OPTOUT environment variable to '1' or 'true' using your favorite shell.

Read more about Upgrade Assistant telemetry: https://aka.ms/upgrade-assistant-telemetry
Read more about .NET CLI Tools telemetry: https://aka.ms/dotnet-cli-telemetry

[04:51:01 WRN] Did not find a Visual Studio instance
[04:51:01 INF] Found candidate MSBuild instances: C:\Program Files\dotnet\sdk\5.0.400\
[04:51:01 INF] MSBuild registered from C:\Program Files\dotnet\sdk\5.0.400\
[04:51:02 INF] Registered MSBuild at C:\Program Files\dotnet\sdk\5.0.400\
[04:51:05 ERR] Could not load project. Please ensure the selected VS instance has the correct workloads installed for your projects. If Upgrade Assistant selected the incorrect VS version, please pass the argument --vs-path with the appropriate path. You can see the Visual Studio instances searched by running with the --verbose flag.
```

Perhaps it needs .NET Framework 4.7.2?

## Open in Visual Studio

Prompts to install the "ASP.NET and web development workload" - prompt accepted and let installer run.

What exactly is it installing???

Installation successful...

## Try and build

Fails with failure to restore nuget packages.

Unable to find version '4.9.1' of pacakge 'Autofac'.

Only has 'offline packages' as a package source - see https://social.msdn.microsoft.com/Forums/vstudio/en-US/86aad234-8c71-4f76-a751-2fd6b98d493b/vs-2019-manage-nuget-packages-has-only-offline-packages?forum=vssetup

Trying repairing the installation using Visual Studio Installer

No dice.

Manually added nuget.org package source with https://api.nuget.org/v3/index.json - not sure why this was necessary!!

Most packages installed, except log4net seems to not be present, leading to build failures with missing packages.  Was on 2.0.10.

Update to log4net 2.0.12.

Still no dice.

Downgrade to log4net 2.0.8 - see https://github.com/dotnet-architecture/eShopModernizing/issues/57 and https://github.com/dotnet-architecture/eShopModernizing/issues/62

```patch
--- a/eShopLegacyMVCSolution/src/eShopLegacyMVC/packages.config
+++ b/eShopLegacyMVCSolution/src/eShopLegacyMVC/packages.config
@@ -7,7 +7,7 @@
   <package id="EntityFramework" version="6.2.0" targetFramework="net472" />
   <package id="jQuery" version="3.3.1" targetFramework="net472" />
   <package id="jQuery.Validation" version="1.17.0" targetFramework="net472" />
-  <package id="log4net" version="2.0.10" targetFramework="net472" />
+  <package id="log4net" version="2.0.8" targetFramework="net472" />
   <package id="Microsoft.ApplicationInsights" version="2.9.1" targetFramework="net472" />
   <package id="Microsoft.ApplicationInsights.Agent.Intercept" version="2.4.0" targetFramework="net472" />
   <package id="Microsoft.ApplicationInsights.DependencyCollector" version="2.9.1" targetFramework="net472" />
```


Woo hoo! it runs!

(By default, it runs using 'mock data', so, turned off mock data in appsettings in web.config, and ran again).

```patch
--- a/eShopLegacyMVCSolution/src/eShopLegacyMVC/Web.config
+++ b/eShopLegacyMVCSolution/src/eShopLegacyMVC/Web.config
@@ -16,7 +16,7 @@
     <add key="webpages:Enabled" value="false" />
     <add key="ClientValidationEnabled" value="true" />
     <add key="UnobtrusiveJavaScriptEnabled" value="true" />
-    <add key="UseMockData" value="true" />
+    <add key="UseMockData" value="false" />
     <add key="UseCustomizationData" value="false" />
   </appSettings>
   <!--
```


Checking again by connecting to localdb:

```pwsh
sqlcmd -S "(localdb)\MSSQLLocalDB"
```

sql script:

```sql
select name from sys.databases;
go
use [Microsoft.eShopOnContainers.Services.CatalogDb];
go
select name from sys.tables;
go
```

output:

```
name                                                                                                                    
--------------------------------------------------------------------------------------------------------------------------------
CatalogBrand                                                                                                            
Catalog                                                                                                                 
CatalogType                                                                                                             
__MigrationHistory
```

## Retry analyze:

With the app proven to be working (all necessary dependencies, etc.)

```
upgrade-assistant analyze .\eShopLegacyMVC.sln
```

```
-----------------------------------------------------------------------------------------------------------------
Microsoft .NET Upgrade Assistant v0.2.242001+4be4778cb767292f63833ee56322a50fe1041a0b

We are interested in your feedback! Please use the following link to open a survey: https://aka.ms/DotNetUASurvey
-----------------------------------------------------------------------------------------------------------------

[06:23:32 INF] Loaded 5 extensions
[06:23:33 INF] Found Visual Studio v16.11.31613.86 [C:\Program Files (x86)\Microsoft Visual Studio\2019\Community]
[06:23:33 INF] Found candidate MSBuild instances: C:\Program Files\dotnet\sdk\5.0.400\
[06:23:33 INF] MSBuild registered from C:\Program Files\dotnet\sdk\5.0.400\
[06:23:34 INF] Registered MSBuild at C:\Program Files\dotnet\sdk\5.0.400\
[06:23:36 INF] Recommending executable TFM net5.0 because the project builds to a web app
[06:23:38 INF] Marking assembly reference System.Web.DynamicData for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Adding framework reference Microsoft.AspNetCore.App based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.Entity for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.ApplicationServices for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.Abstractions for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.Extensions for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.Helpers for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.Mvc for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.Optimization for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.Razor for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.Routing for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.WebPages for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.WebPages.Deployment for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Web.WebPages.Razor for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Configuration for removal based on package mapping configuration System.Configuration
[06:23:38 INF] Adding package System.Configuration.ConfigurationManager based on package mapping configuration System.Configuration
[06:23:38 INF] Marking assembly reference System.Web.Services for removal based on package mapping configuration ASP.NET
[06:23:38 INF] Marking assembly reference System.Net.Http.WebRequest for removal based on package mapping configuration HTTP packages
[06:23:38 INF] Adding package System.Net.Http based on package mapping configuration HTTP packages
[06:23:40 INF] Reference to .NET Upgrade Assistant analyzer package (Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers, version 0.2.241603) needs added
[06:23:40 INF] Running analyzers on eShopLegacyMVC
[06:23:42 INF] Identified 16 diagnostics in project eShopLegacyMVC
```

contents of log.txt for the run:

```
2021-08-23 06:23:32.252 +00:00 [INF] Loaded 5 extensions
2021-08-23 06:23:33.353 +00:00 [INF] Found Visual Studio v16.11.31613.86 [C:\Program Files (x86)\Microsoft Visual Studio\2019\Community]
2021-08-23 06:23:33.907 +00:00 [INF] Found candidate MSBuild instances: C:\Program Files\dotnet\sdk\5.0.400\
2021-08-23 06:23:33.909 +00:00 [INF] MSBuild registered from C:\Program Files\dotnet\sdk\5.0.400\
2021-08-23 06:23:34.193 +00:00 [INF] Registered MSBuild at C:\Program Files\dotnet\sdk\5.0.400\
2021-08-23 06:23:36.875 +00:00 [INF] Recommending executable TFM net5.0 because the project builds to a web app
2021-08-23 06:23:38.514 +00:00 [INF] Marking assembly reference System.Web.DynamicData for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.523 +00:00 [INF] Adding framework reference Microsoft.AspNetCore.App based on package mapping configuration ASP.NET
2021-08-23 06:23:38.525 +00:00 [INF] Marking assembly reference System.Web.Entity for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.529 +00:00 [INF] Marking assembly reference System.Web.ApplicationServices for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.532 +00:00 [INF] Marking assembly reference System.Web for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.534 +00:00 [INF] Marking assembly reference System.Web.Abstractions for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.536 +00:00 [INF] Marking assembly reference System.Web.Extensions for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.538 +00:00 [INF] Marking assembly reference System.Web.Helpers for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.539 +00:00 [INF] Marking assembly reference System.Web.Mvc for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.541 +00:00 [INF] Marking assembly reference System.Web.Optimization for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.544 +00:00 [INF] Marking assembly reference System.Web.Razor for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.545 +00:00 [INF] Marking assembly reference System.Web.Routing for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.547 +00:00 [INF] Marking assembly reference System.Web.WebPages for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.549 +00:00 [INF] Marking assembly reference System.Web.WebPages.Deployment for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.551 +00:00 [INF] Marking assembly reference System.Web.WebPages.Razor for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.553 +00:00 [INF] Marking assembly reference System.Configuration for removal based on package mapping configuration System.Configuration
2021-08-23 06:23:38.556 +00:00 [INF] Adding package System.Configuration.ConfigurationManager based on package mapping configuration System.Configuration
2021-08-23 06:23:38.559 +00:00 [INF] Marking assembly reference System.Web.Services for removal based on package mapping configuration ASP.NET
2021-08-23 06:23:38.561 +00:00 [INF] Marking assembly reference System.Net.Http.WebRequest for removal based on package mapping configuration HTTP packages
2021-08-23 06:23:38.564 +00:00 [INF] Adding package System.Net.Http based on package mapping configuration HTTP packages
2021-08-23 06:23:40.525 +00:00 [INF] Reference to .NET Upgrade Assistant analyzer package (Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers, version 0.2.241603) needs added
2021-08-23 06:23:40.548 +00:00 [INF] Running analyzers on eShopLegacyMVC
2021-08-23 06:23:42.749 +00:00 [INF] Identified 16 diagnostics in project eShopLegacyMVC
```

## Giving the interactive upgrade a test spin

See https://docs.microsoft.com/en-us/dotnet/core/porting/upgrade-assistant-aspnetmvc for a walkthrough of an upgrade process.


Step 1
```
Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Next step] Back up project
2. Convert project file to SDK style
3. Clean up NuGet package references
4. Update TFM
5. Update NuGet Packages
6. Add template files
7. Upgrade app config files
    a. Convert Application Settings
    b. Convert Connection Strings
    c. Disable unsupported configuration sections
8. Update Razor files
    a. Apply code fixes to Razor documents
    b. Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0002: Types should be upgraded
    b. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    c. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    d. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Back up project)
   2. Skip next step (Back up project)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:26:59 INF] Applying upgrade step Back up project
Please choose a backup path
   1. Use default path [C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution.backup]
   2. Enter custom path
> 1
[06:27:02 INF] Backing up C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC to C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution.backup\eShopLegacyMVC
[06:27:10 INF] Project backed up to C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution.backup\eShopLegacyMVC
[06:27:10 INF] Upgrade step Back up project applied successfully
```

Step 2:
```
[06:27:21 INF] Initializing upgrade step Convert project file to SDK style

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Next step] Convert project file to SDK style
3. Clean up NuGet package references
4. Update TFM
5. Update NuGet Packages
6. Add template files
7. Upgrade app config files
    a. Convert Application Settings
    b. Convert Connection Strings
    c. Disable unsupported configuration sections
8. Update Razor files
    a. Apply code fixes to Razor documents
    b. Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0002: Types should be upgraded
    b. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    c. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    d. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Convert project file to SDK style)
   2. Skip next step (Convert project file to SDK style)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:27:30 INF] Applying upgrade step Convert project file to SDK style
[06:27:30 INF] Converting project file format with try-convert, version 0.9.232202
[06:27:31 INF] [dotnet] C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj contains a reference to System.Web, which is not supported on .NET Core. You may have significant work ahead of you to fully port this project.
[06:27:31 INF] [dotnet] 'C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj' is a legacy web project and/or references System.Web. Legacy Web projects and System.Web are unsupported on .NET Core. You will need to rewrite your application or find a way to not depend on System.Web to convert this project.
[06:27:33 INF] [dotnet] This project has an unrecognized custom import which may need reviewed after conversion: Microsoft.ApplicationInsights.DependencyCollector.targets
[06:27:33 INF] [dotnet] This project has an unrecognized custom import which may need reviewed after conversion: Microsoft.ApplicationInsights.PerfCounterCollector.targets
[06:27:33 INF] [dotnet] This project has an unrecognized custom import which may need reviewed after conversion: Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel.targets
[06:27:33 INF] [dotnet] This project has an unrecognized custom import which may need reviewed after conversion: Microsoft.ApplicationInsights.WindowsServer.targets
[06:27:33 INF] [dotnet] This project has an unrecognized custom import which may need reviewed after conversion: Microsoft.ApplicationInsights.Web.targets
[06:27:36 INF] [dotnet] Conversion complete!
[06:27:38 INF] Project file converted successfully! The project may require additional changes to build successfully against the new .NET target.
```

Step 3:
```
[06:28:08 INF] Initializing upgrade step Clean up NuGet package references
[06:28:09 INF] Marking package Microsoft.ApplicationInsights for removal because it appears to be a transitive dependency
[06:28:09 INF] Marking package Microsoft.ApplicationInsights.Agent.Intercept for removal because it appears to be a transitive dependency
[06:28:09 INF] Marking package Microsoft.ApplicationInsights.DependencyCollector for removal because it appears to be a transitive dependency
[06:28:09 INF] Marking package Microsoft.ApplicationInsights.PerfCounterCollector for removal because it appears to be a transitive dependency
[06:28:09 INF] Marking package Microsoft.ApplicationInsights.WindowsServer for removal because it appears to be a transitive dependency
[06:28:09 INF] Marking package Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel for removal because it appears to be a transitive dependency
[06:28:09 INF] Marking package Microsoft.AspNet.TelemetryCorrelation for removal because it appears to be a transitive dependency
[06:28:10 INF] Marking package System.Buffers for removal because it appears to be a transitive dependency
[06:28:10 INF] Marking package System.IO.Pipelines for removal because it appears to be a transitive dependency
[06:28:10 INF] Marking package System.Memory for removal because it appears to be a transitive dependency
[06:28:10 INF] Marking package System.Numerics.Vectors for removal because it appears to be a transitive dependency
[06:28:10 INF] Marking package System.Runtime.CompilerServices.Unsafe for removal because it appears to be a transitive dependency
[06:28:10 INF] Marking package System.Threading.Tasks.Extensions for removal because it appears to be a transitive dependency
[06:28:10 INF] Marking assembly reference System.Configuration for removal based on package mapping configuration System.Configuration
[06:28:10 INF] Adding package System.Configuration.ConfigurationManager based on package mapping configuration System.Configuration
[06:28:11 INF] Reference to .NET Upgrade Assistant analyzer package (Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers, version 0.2.241603) needs added
[06:28:11 INF] References to be removed: Operation { Item = System.Configuration, OperationDetails = OperationDetails { Risk = Medium, Details = System.Collections.Generic.List`1[System.String] } }
[06:28:11 INF] Packages to be removed: Operation { Item = Microsoft.ApplicationInsights, Version=2.9.1, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = Microsoft.ApplicationInsights.Agent.Intercept, Version=2.4.0, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = Microsoft.ApplicationInsights.DependencyCollector, Version=2.9.1, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = Microsoft.ApplicationInsights.PerfCounterCollector, Version=2.9.1, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = Microsoft.ApplicationInsights.WindowsServer, Version=2.9.1, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel, Version=2.9.1, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = Microsoft.AspNet.TelemetryCorrelation, Version=1.0.5, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = System.Buffers, Version=4.4.0, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = System.IO.Pipelines, Version=4.5.1, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = System.Memory, Version=4.5.1, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = System.Numerics.Vectors, Version=4.4.0, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = System.Runtime.CompilerServices.Unsafe, Version=4.5.0, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = System.Threading.Tasks.Extensions, Version=4.5.1, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
[06:28:11 INF] Packages to be added: Operation { Item = System.Configuration.ConfigurationManager, Version=5.0.0, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers, Version=0.2.241603, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Next step] Clean up NuGet package references
4. Update TFM
5. Update NuGet Packages
6. Add template files
7. Upgrade app config files
    a. Convert Application Settings
    b. Convert Connection Strings
    c. Disable unsupported configuration sections
    d. Convert system.web.webPages.razor/pages/namespaces
8. Update Razor files
    a. Apply code fixes to Razor documents
    b. Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Clean up NuGet package references)
   2. Skip next step (Clean up NuGet package references)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:28:35 INF] Applying upgrade step Clean up NuGet package references
[06:28:35 INF] Removing outdated assembly reference: System.Configuration
[06:28:35 INF] Removing outdated package reference: Microsoft.ApplicationInsights, Version=2.9.1
[06:28:35 INF] Removing outdated package reference: Microsoft.ApplicationInsights.Agent.Intercept, Version=2.4.0
[06:28:35 INF] Removing outdated package reference: Microsoft.ApplicationInsights.DependencyCollector, Version=2.9.1
[06:28:35 INF] Removing outdated package reference: Microsoft.ApplicationInsights.PerfCounterCollector, Version=2.9.1
[06:28:35 INF] Removing outdated package reference: Microsoft.ApplicationInsights.WindowsServer, Version=2.9.1
[06:28:35 INF] Removing outdated package reference: Microsoft.ApplicationInsights.WindowsServer.TelemetryChannel, Version=2.9.1
[06:28:35 INF] Removing outdated package reference: Microsoft.AspNet.TelemetryCorrelation, Version=1.0.5
[06:28:35 INF] Removing outdated package reference: System.Buffers, Version=4.4.0
[06:28:35 INF] Removing outdated package reference: System.IO.Pipelines, Version=4.5.1
[06:28:35 INF] Removing outdated package reference: System.Memory, Version=4.5.1
[06:28:35 INF] Removing outdated package reference: System.Numerics.Vectors, Version=4.4.0
[06:28:35 INF] Removing outdated package reference: System.Runtime.CompilerServices.Unsafe, Version=4.5.0
[06:28:35 INF] Removing outdated package reference: System.Threading.Tasks.Extensions, Version=4.5.1
[06:28:35 INF] Adding package reference: System.Configuration.ConfigurationManager, Version=5.0.0
[06:28:35 INF] Adding package reference: Microsoft.DotNet.UpgradeAssistant.Extensions.Default.Analyzers, Version=0.2.241603
[06:28:40 INF] Upgrade step Clean up NuGet package references applied successfully
```

Step 4:
```
[06:28:56 INF] Initializing upgrade step Update TFM
[06:28:56 INF] Recommending executable TFM net5.0 because the project builds to an executable
[06:28:56 INF] TFM needs updated to net5.0

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Next step] Update TFM
5. Update NuGet Packages
6. Add template files
7. Upgrade app config files
    a. Convert Application Settings
    b. Convert Connection Strings
    c. Disable unsupported configuration sections
    d. Convert system.web.webPages.razor/pages/namespaces
8. Update Razor files
    a. Apply code fixes to Razor documents
    b. Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Update TFM)
   2. Skip next step (Update TFM)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:29:10 INF] Applying upgrade step Update TFM
[06:29:10 INF] Recommending executable TFM net5.0 because the project builds to an executable
[06:29:35 INF] Updated TFM to net5.0
[06:29:35 INF] Upgrade step Update TFM applied successfully
```

Step 5:
```
[06:30:08 INF] Initializing upgrade step Update NuGet Packages
[06:30:10 INF] Marking package System.IO.Compression for removal because it appears to be a transitive dependency
[06:30:10 INF] Marking package System.Configuration.ConfigurationManager for removal because it appears to be a transitive dependency
[06:30:10 INF] Marking package Antlr for removal based on package mapping configuration Antlr
[06:30:10 INF] Adding package Antlr4 based on package mapping configuration Antlr
[06:30:10 INF] Marking package Autofac.Mvc5 for removal based on package mapping configuration Autofac.Mvc
[06:30:10 INF] Marking package Microsoft.AspNet.SessionState.SessionStateModule for removal based on package mapping configuration ASP.NET
[06:30:10 INF] Adding framework reference Microsoft.AspNetCore.App based on package mapping configuration ASP.NET
[06:30:10 INF] Marking package Microsoft.Net.Compilers for removal based on package mapping configuration Microsoft.Net.Compilers
[06:30:10 INF] Marking package WebGrease for removal based on package mapping configuration WebGrease
[06:30:11 INF] Marking package Autofac.Mvc5, Version=4.0.2 for removal because it doesn't support the target framework but a newer version (5.0.0) does
[06:30:11 WRN] Package Autofac.Mvc5 needs to be upgraded across major versions (4.0.2 -> 5.0.0) which may introduce breaking changes
[06:30:11 INF] Marking package EntityFramework, Version=6.2.0 for removal because it doesn't support the target framework but a newer version (6.4.4) does
[06:30:12 WRN] No version of Microsoft.ApplicationInsights.Web found that supports ["net5.0"]; leaving unchanged
[06:30:14 WRN] No version of Autofac.Mvc5 found that supports ["net5.0"]; leaving unchanged
[06:30:15 INF] Reference to Newtonsoft package (Microsoft.AspNetCore.Mvc.NewtonsoftJson, version 5.0.9) needs added
[06:30:15 INF] Packages to be removed: Operation { Item = System.IO.Compression, Version=4.3.0, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = System.Configuration.ConfigurationManager, Version=5.0.0, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = Antlr, Version=3.5.0.2, OperationDetails = OperationDetails { Risk = Medium, Details = System.Collections.Generic.List`1[System.String] } }
Operation { Item = Autofac.Mvc5, Version=4.0.2, OperationDetails = OperationDetails { Risk = Medium, Details = System.Collections.Generic.List`1[System.String] } }
Operation { Item = Microsoft.AspNet.SessionState.SessionStateModule, Version=1.1.0, OperationDetails = OperationDetails { Risk = Medium, Details = System.Collections.Generic.List`1[System.String] } }
Operation { Item = Microsoft.Net.Compilers, Version=2.10.0, OperationDetails = OperationDetails { Risk = Medium, Details = System.Collections.Generic.List`1[System.String] } }
Operation { Item = WebGrease, Version=1.6.0, OperationDetails = OperationDetails { Risk = Medium, Details = System.Collections.Generic.List`1[System.String] } }
Operation { Item = Autofac.Mvc5, Version=4.0.2, OperationDetails = OperationDetails { Risk = None, Details = System.Collections.Generic.List`1[System.String] } }
Operation { Item = EntityFramework, Version=6.2.0, OperationDetails = OperationDetails { Risk = None, Details = System.Collections.Generic.List`1[System.String] } }
[06:30:15 INF] Packages to be added: Operation { Item = Antlr4, Version=4.6.6, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
Operation { Item = Autofac.Mvc5, Version=5.0.0, OperationDetails = OperationDetails { Risk = Medium, Details = System.Collections.Generic.List`1[System.String] } }
Operation { Item = EntityFramework, Version=6.4.4, OperationDetails = OperationDetails { Risk = Low, Details = System.Collections.Generic.List`1[System.String] } }
Operation { Item = Microsoft.AspNetCore.Mvc.NewtonsoftJson, Version=5.0.9, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }
[06:30:15 INF] Framework references to be added: Operation { Item = Microsoft.AspNetCore.App, OperationDetails = OperationDetails { Risk = None, Details = System.Linq.EmptyPartition`1[System.String] } }

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Next step] Update NuGet Packages
6. Add template files
7. Upgrade app config files
    a. Convert Application Settings
    b. Convert Connection Strings
    c. Disable unsupported configuration sections
    d. Convert system.web.webPages.razor/pages/namespaces
8. Update Razor files
    a. Apply code fixes to Razor documents
    b. Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Update NuGet Packages)
   2. Skip next step (Update NuGet Packages)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
1
[06:30:31 INF] Applying upgrade step Update NuGet Packages
[06:30:31 INF] Removing outdated package reference: Antlr, Version=3.5.0.2
[06:30:31 INF] Removing outdated package reference: Autofac.Mvc5, Version=4.0.2
[06:30:31 INF] Removing outdated package reference: EntityFramework, Version=6.2.0
[06:30:31 INF] Removing outdated package reference: Microsoft.AspNet.SessionState.SessionStateModule, Version=1.1.0
[06:30:31 INF] Removing outdated package reference: Microsoft.Net.Compilers, Version=2.10.0
[06:30:31 INF] Removing outdated package reference: System.IO.Compression, Version=4.3.0
[06:30:31 INF] Removing outdated package reference: WebGrease, Version=1.6.0
[06:30:31 INF] Removing outdated package reference: System.Configuration.ConfigurationManager, Version=5.0.0
[06:30:31 INF] Adding package reference: Antlr4, Version=4.6.6
[06:30:31 INF] Adding package reference: Autofac.Mvc5, Version=5.0.0
[06:30:31 INF] Adding package reference: EntityFramework, Version=6.4.4
[06:30:31 INF] Adding package reference: Microsoft.AspNetCore.Mvc.NewtonsoftJson, Version=5.0.9
[06:30:31 INF] Adding framework reference: Microsoft.AspNetCore.App
[06:30:40 INF] Marking package Microsoft.CSharp for removal because it appears to be a transitive dependency
[06:30:40 INF] Marking package Newtonsoft.Json for removal because it appears to be a transitive dependency
[06:30:40 INF] Marking package Autofac.Mvc5 for removal based on package mapping configuration Autofac.Mvc
[06:30:41 WRN] No version of Microsoft.ApplicationInsights.Web found that supports ["net5.0"]; leaving unchanged
[06:30:42 WRN] No version of Autofac.Mvc5 found that supports ["net5.0"]; leaving unchanged
[06:30:42 INF] Removing outdated package reference: Microsoft.CSharp, Version=4.7.0
[06:30:42 INF] Removing outdated package reference: Newtonsoft.Json, Version=12.0.1
[06:30:42 INF] Removing outdated package reference: Autofac.Mvc5, Version=5.0.0
[06:30:46 WRN] No version of Microsoft.ApplicationInsights.Web found that supports ["net5.0"]; leaving unchanged
[06:30:46 INF] Upgrade step Update NuGet Packages applied successfully
```

Step 6:
```
[06:31:07 INF] Initializing upgrade step Add template files
[06:31:07 INF] 4 expected template items needed

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Next step] Add template files
7. Upgrade app config files
    a. Convert Application Settings
    b. Convert Connection Strings
    c. Disable unsupported configuration sections
    d. Convert system.web.webPages.razor/pages/namespaces
8. Update Razor files
    a. Apply code fixes to Razor documents
    b. Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Add template files)
   2. Skip next step (Add template files)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:31:20 INF] Applying upgrade step Add template files
[06:31:20 INF] Added template file Program.cs
[06:31:20 INF] Added template file Startup.cs
[06:31:20 INF] Added template file appsettings.json
[06:31:20 INF] Added template file appsettings.Development.json
[06:31:21 INF] 4 template items added
[06:31:21 INF] Upgrade step Add template files applied successfully
```

Step 7.a:
```
[06:31:53 INF] Initializing upgrade step Upgrade app config files
[06:31:53 INF] Found 6 app settings for upgrade: webpages:Version, webpages:Enabled, ClientValidationEnabled, UnobtrusiveJavaScriptEnabled, UseMockData, UseCustomizationData
[06:31:53 INF] Found 1 connection strings for upgrade: CatalogDBContext
[06:31:53 INF] 1 web page namespace imports need upgraded: eShopLegacyMVC

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. Upgrade app config files
    a. [Next step] Convert Application Settings
    b. Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. Convert system.web.webPages.razor/pages/namespaces
8. Update Razor files
    a. Apply code fixes to Razor documents
    b. Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Convert Application Settings)
   2. Skip next step (Convert Application Settings)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:32:14 INF] Applying upgrade step Convert Application Settings
[06:32:14 INF] Upgrade step Convert Application Settings applied successfully
```

Step 7.b
```
Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. Upgrade app config files
    a. [Complete] Convert Application Settings
    b. [Next step] Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. Convert system.web.webPages.razor/pages/namespaces
8. Update Razor files
    a. Apply code fixes to Razor documents
    b. Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Convert Connection Strings)
   2. Skip next step (Convert Connection Strings)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:32:50 INF] Applying upgrade step Convert Connection Strings
[06:32:50 INF] Upgrade step Convert Connection Strings applied successfully
```

Step 7.d:
```
Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. Upgrade app config files
    a. [Complete] Convert Application Settings
    b. [Complete] Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. [Next step] Convert system.web.webPages.razor/pages/namespaces
8. Update Razor files
    a. Apply code fixes to Razor documents
    b. Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Convert system.web.webPages.razor/pages/namespaces)
   2. Skip next step (Convert system.web.webPages.razor/pages/namespaces)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:33:17 INF] Applying upgrade step Convert system.web.webPages.razor/pages/namespaces
[06:33:17 INF] View imports written to C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Views\_ViewImports.cshtml
[06:33:17 INF] Upgrade step Convert system.web.webPages.razor/pages/namespaces applied successfully
[06:33:17 INF] Applying upgrade step Upgrade app config files
[06:33:17 INF] Upgrade step Upgrade app config files applied successfully
```

Step 8.a:
```
[06:33:48 INF] Initializing upgrade step Update Razor files
[06:33:53 INF] Identified 3 diagnostics in Razor files in project eShopLegacyMVC
[06:33:53 INF]   3 diagnostics need fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Views\Shared\_Layout.cshtml
[06:33:53 INF] Found @helper functions in 0 documents

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. [Complete] Upgrade app config files
    a. [Complete] Convert Application Settings
    b. [Complete] Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. [Complete] Convert system.web.webPages.razor/pages/namespaces
8. Update Razor files
    a. [Next step] Apply code fixes to Razor documents
    b. [Complete] Replace @helper syntax in Razor files
9. Update source code
    a. Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Apply code fixes to Razor documents)
   2. Skip next step (Apply code fixes to Razor documents)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:34:08 INF] Applying upgrade step Apply code fixes to Razor documents
[06:34:12 INF] Updating source code in Razor document C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Views\Shared\_Layout.cshtml at line 35
[06:34:12 INF] Updating source code in Razor document C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Views\Shared\_Layout.cshtml at line 0
[06:34:13 INF] Upgrade step Apply code fixes to Razor documents applied successfully
[06:34:13 INF] Applying upgrade step Update Razor files
[06:34:13 INF] Upgrade step Update Razor files applied successfully
```

Step 9.a
```
[06:34:34 INF] Initializing upgrade step Update source code
[06:34:34 INF] Running analyzers on eShopLegacyMVC
[06:34:34 INF] Identified 41 diagnostics in project eShopLegacyMVC

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. [Complete] Upgrade app config files
    a. [Complete] Convert Application Settings
    b. [Complete] Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. [Complete] Convert system.web.webPages.razor/pages/namespaces
8. [Complete] Update Razor files
    a. [Complete] Apply code fixes to Razor documents
    b. [Complete] Replace @helper syntax in Razor files
9. Update source code
    a. [Next step] Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. [Complete] Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. [Complete] Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. [Complete] Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. [Complete] Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. [Complete] Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. [Complete] Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces)
   2. Skip next step (Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:34:53 INF] Applying upgrade step Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\App_Start\BundleConfig.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\App_Start\FilterConfig.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\App_Start\RouteConfig.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Controllers\CatalogController.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Controllers\PicController.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Models\CatalogBrand.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Models\CatalogItemHiLoGenerator.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Models\CatalogType.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Models\Infrastructure\CatalogDBInitializer.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Models\Infrastructure\PreconfiguredData.cs
[06:34:53 INF] Running analyzers on eShopLegacyMVC
[06:34:53 INF] Identified 30 diagnostics in project eShopLegacyMVC
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\App_Start\BundleConfig.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\App_Start\FilterConfig.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\App_Start\RouteConfig.cs
[06:34:53 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs
[06:34:54 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Models\Infrastructure\CatalogDBInitializer.cs
[06:34:54 INF] Running analyzers on eShopLegacyMVC
[06:34:54 INF] Identified 25 diagnostics in project eShopLegacyMVC
[06:34:54 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\App_Start\RouteConfig.cs
[06:34:54 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs
[06:34:54 INF] Running analyzers on eShopLegacyMVC
[06:34:54 INF] Identified 23 diagnostics in project eShopLegacyMVC
[06:34:54 INF] Diagnostic UA0001 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs
[06:34:54 INF] Running analyzers on eShopLegacyMVC
[06:34:55 INF] Identified 22 diagnostics in project eShopLegacyMVC
[06:34:55 INF] Upgrade step Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces applied successfully
```

Step 9.b:
```
Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. [Complete] Upgrade app config files
    a. [Complete] Convert Application Settings
    b. [Complete] Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. [Complete] Convert system.web.webPages.razor/pages/namespaces
8. [Complete] Update Razor files
    a. [Complete] Apply code fixes to Razor documents
    b. [Complete] Replace @helper syntax in Razor files
9. Update source code
    a. [Complete] Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. [Next step] Apply fix for UA0002: Types should be upgraded
    c. Apply fix for UA0005: Do not use HttpContext.Current
    d. [Complete] Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. [Complete] Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. [Complete] Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. [Complete] Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. [Complete] Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. [Complete] Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Apply fix for UA0002: Types should be upgraded)
   2. Skip next step (Apply fix for UA0002: Types should be upgraded)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:35:34 INF] Applying upgrade step Apply fix for UA0002: Types should be upgraded
[06:35:34 INF] Diagnostic UA0002 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Controllers\CatalogController.cs
[06:35:34 INF] Diagnostic UA0002 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Controllers\PicController.cs
[06:35:34 INF] Running analyzers on eShopLegacyMVC
[06:35:35 INF] Identified 9 diagnostics in project eShopLegacyMVC
[06:35:35 INF] Diagnostic UA0002 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Controllers\PicController.cs
[06:35:35 INF] Diagnostic UA0002 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Controllers\CatalogController.cs
[06:35:35 INF] Running analyzers on eShopLegacyMVC
[06:35:35 INF] Identified 7 diagnostics in project eShopLegacyMVC
[06:35:35 INF] Diagnostic UA0002 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Controllers\CatalogController.cs
[06:35:35 INF] Running analyzers on eShopLegacyMVC
[06:35:35 INF] Identified 6 diagnostics in project eShopLegacyMVC
[06:35:35 INF] Diagnostic UA0002 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Controllers\CatalogController.cs
[06:35:35 INF] Running analyzers on eShopLegacyMVC
[06:35:36 INF] Identified 5 diagnostics in project eShopLegacyMVC
[06:35:36 INF] Upgrade step Apply fix for UA0002: Types should be upgraded applied successfully
```

Step 9.c
```
Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. [Complete] Upgrade app config files
    a. [Complete] Convert Application Settings
    b. [Complete] Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. [Complete] Convert system.web.webPages.razor/pages/namespaces
8. [Complete] Update Razor files
    a. [Complete] Apply code fixes to Razor documents
    b. [Complete] Replace @helper syntax in Razor files
9. Update source code
    a. [Complete] Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. [Complete] Apply fix for UA0002: Types should be upgraded
    c. [Next step] Apply fix for UA0005: Do not use HttpContext.Current
    d. [Complete] Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. [Complete] Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. [Complete] Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. Apply fix for UA0010: Attributes should be upgraded
    h. [Complete] Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. [Complete] Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. [Complete] Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Apply fix for UA0005: Do not use HttpContext.Current)
   2. Skip next step (Apply fix for UA0005: Do not use HttpContext.Current)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:36:04 INF] Applying upgrade step Apply fix for UA0005: Do not use HttpContext.Current
[06:36:04 INF] Diagnostic UA0005 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs
[06:36:04 INF] Running analyzers on eShopLegacyMVC
[06:36:04 INF] Identified 4 diagnostics in project eShopLegacyMVC
[06:36:04 INF] Diagnostic UA0005 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs
[06:36:04 INF] Running analyzers on eShopLegacyMVC
[06:36:05 INF] Identified 3 diagnostics in project eShopLegacyMVC
[06:36:05 INF] Diagnostic UA0005 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs
[06:36:05 INF] Running analyzers on eShopLegacyMVC
[06:36:05 INF] Identified 2 diagnostics in project eShopLegacyMVC
[06:36:05 INF] Diagnostic UA0005 fixed in C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs
[06:36:05 INF] Running analyzers on eShopLegacyMVC
[06:36:06 INF] Identified 1 diagnostics in project eShopLegacyMVC
[06:36:06 INF] Upgrade step Apply fix for UA0005: Do not use HttpContext.Current applied successfully
```

Step 9.g:
```
Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. [Complete] Upgrade app config files
    a. [Complete] Convert Application Settings
    b. [Complete] Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. [Complete] Convert system.web.webPages.razor/pages/namespaces
8. [Complete] Update Razor files
    a. [Complete] Apply code fixes to Razor documents
    b. [Complete] Replace @helper syntax in Razor files
9. Update source code
    a. [Complete] Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. [Complete] Apply fix for UA0002: Types should be upgraded
    c. [Complete] Apply fix for UA0005: Do not use HttpContext.Current
    d. [Complete] Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. [Complete] Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. [Complete] Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. [Next step] Apply fix for UA0010: Attributes should be upgraded
    h. [Complete] Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. [Complete] Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. [Complete] Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Apply fix for UA0010: Attributes should be upgraded)
   2. Skip next step (Apply fix for UA0010: Attributes should be upgraded)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:36:35 INF] Applying upgrade step Apply fix for UA0010: Attributes should be upgraded
[06:36:35 INF] Upgrade step Apply fix for UA0010: Attributes should be upgraded applied successfully
```

Step 9:
```
Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. [Complete] Upgrade app config files
    a. [Complete] Convert Application Settings
    b. [Complete] Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. [Complete] Convert system.web.webPages.razor/pages/namespaces
8. [Complete] Update Razor files
    a. [Complete] Apply code fixes to Razor documents
    b. [Complete] Replace @helper syntax in Razor files
9. [Next step] Update source code
    a. [Complete] Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. [Complete] Apply fix for UA0002: Types should be upgraded
    c. [Complete] Apply fix for UA0005: Do not use HttpContext.Current
    d. [Complete] Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. [Complete] Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. [Complete] Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. [Complete] Apply fix for UA0010: Attributes should be upgraded
    h. [Complete] Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. [Complete] Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. [Complete] Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. Move to next project

Choose a command:
   1. Apply next step (Update source code)
   2. Skip next step (Update source code)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:37:04 INF] Applying upgrade step Update source code
[06:37:05 INF] Source updates complete with 1 diagnostics remaining which require manual updates
[06:37:05 WRN] Manual updates needed to address: UA0013_C@SourceFile(C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\App_Start\BundleConfig.cs[211..227)): Script and style bundling works differently in ASP.NET Core. BundleCollection should be replaced by alternative bundling technologies. https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
[06:37:05 INF] Upgrade step Update source code applied successfully
```

Step 10:
```
[06:37:33 INF] Initializing upgrade step Move to next project

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj
Current Project: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

1. [Complete] Back up project
2. [Complete] Convert project file to SDK style
3. [Complete] Clean up NuGet package references
4. [Complete] Update TFM
5. [Complete] Update NuGet Packages
6. [Complete] Add template files
7. [Complete] Upgrade app config files
    a. [Complete] Convert Application Settings
    b. [Complete] Convert Connection Strings
    c. [Complete] Disable unsupported configuration sections
    d. [Complete] Convert system.web.webPages.razor/pages/namespaces
8. [Complete] Update Razor files
    a. [Complete] Apply code fixes to Razor documents
    b. [Complete] Replace @helper syntax in Razor files
9. [Complete] Update source code
    a. [Complete] Apply fix for UA0001: ASP.NET Core projects should not reference ASP.NET namespaces
    b. [Complete] Apply fix for UA0002: Types should be upgraded
    c. [Complete] Apply fix for UA0005: Do not use HttpContext.Current
    d. [Complete] Apply fix for UA0006: HttpContext.DebuggerEnabled should be replaced with System.Diagnostics.Debugger.IsAttached
    e. [Complete] Apply fix for UA0007: HtmlHelper should be replaced with IHtmlHelper
    f. [Complete] Apply fix for UA0008: UrlHelper should be replaced with IUrlHelper
    g. [Complete] Apply fix for UA0010: Attributes should be upgraded
    h. [Complete] Apply fix for UA0012: 'UnsafeDeserialize()' does not exist
    i. [Complete] Apply fix for UA0014: .NET MAUI projects should not reference Xamarin.Forms namespaces
    j. [Complete] Apply fix for UA0015: .NET MAUI projects should not reference Xamarin.Essentials namespaces
10. [Next step] Move to next project

Choose a command:
   1. Apply next step (Move to next project)
   2. Skip next step (Move to next project)
   3. See more step details
   4. Configure logging
   5. Exit
>
[06:37:41 INF] Applying upgrade step Move to next project
[06:37:41 INF] Upgrade step Move to next project applied successfully
```

Step final:
```
[06:37:49 INF] Recommending executable TFM net5.0 because the project builds to an executable
[06:37:49 INF] Recommending executable TFM net5.0 because the project builds to an executable
[06:37:49 INF] Recommending executable TFM net5.0 because the project builds to an executable
[06:37:49 INF] Initializing upgrade step Finalize upgrade

Upgrade Steps

Entrypoint: C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj

[06:37:49 INF] Recommending executable TFM net5.0 because the project builds to an executable
1. [Next step] Finalize upgrade

Choose a command:
   1. Apply next step (Finalize upgrade)
   2. Skip next step (Finalize upgrade)
   3. See more step details
   4. Configure logging
   5. Exit
> 1
[06:38:09 INF] Applying upgrade step Finalize upgrade
[06:38:09 INF] Upgrade step Finalize upgrade applied successfully
```

```
[06:38:20 INF] Upgrade has completed. Please review any changes.
PS C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution> git status
On branch master
Your branch is up to date with 'origin/master'.

Changes not staged for commit:
  (use "git add/rm <file>..." to update what will be committed)
  (use "git restore <file>..." to discard changes in working directory)
        modified:   src/eShopLegacyMVC/App_Start/BundleConfig.cs
        modified:   src/eShopLegacyMVC/App_Start/FilterConfig.cs
        modified:   src/eShopLegacyMVC/App_Start/RouteConfig.cs
        modified:   src/eShopLegacyMVC/Controllers/CatalogController.cs
        modified:   src/eShopLegacyMVC/Controllers/PicController.cs
        modified:   src/eShopLegacyMVC/Global.asax.cs
        modified:   src/eShopLegacyMVC/Models/CatalogBrand.cs
        modified:   src/eShopLegacyMVC/Models/CatalogItemHiLoGenerator.cs
        modified:   src/eShopLegacyMVC/Models/CatalogType.cs
        modified:   src/eShopLegacyMVC/Models/Infrastructure/CatalogDBInitializer.cs
        modified:   src/eShopLegacyMVC/Models/Infrastructure/PreconfiguredData.cs
        modified:   src/eShopLegacyMVC/Views/Shared/_Layout.cshtml
        modified:   src/eShopLegacyMVC/Web.config
        modified:   src/eShopLegacyMVC/eShopLegacyMVC.csproj
        deleted:    src/eShopLegacyMVC/packages.config

Untracked files:
  (use "git add <file>..." to include in what will be committed)
        ../eShopLegacyMVCSolution.backup/
        AnalysisReport.sarif
        log.txt
        src/eShopLegacyMVC/HttpContextHelper.cs
        src/eShopLegacyMVC/Program.cs
        src/eShopLegacyMVC/Startup.cs
        src/eShopLegacyMVC/Views/_ViewImports.cshtml
        src/eShopLegacyMVC/appsettings.Development.json
        src/eShopLegacyMVC/appsettings.json

no changes added to commit (use "git add" and/or "git commit -a")
PS C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution>
```

Moment of truth - attempt to build and run (not likely!)

Of course - tonnes of build errors.  Work through one by one, or...?

## Working through the errors and warnings

```
Severity	Code	Description	Project	File	Line	Suppression State
Error	CS0234	The type or namespace name 'Integration' does not exist in the namespace 'Autofac' (are you missing an assembly reference?)	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	2	Active
Error	CS0234	The type or namespace name 'HttpApplication' does not exist in the namespace 'System.Web' (are you missing an assembly reference?)	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	17	Active
Error	CS0103	The name 'AreaRegistration' does not exist in the current context	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	26	Active
Error	CS0103	The name 'GlobalFilters' does not exist in the current context	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	27	Active
Error	CS0103	The name 'RouteTable' does not exist in the current context	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	28	Active
Error	CS0103	The name 'BundleTable' does not exist in the current context	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	29	Active
Error	CS0021	Cannot apply indexing with [] to an expression of type 'ISession'	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	38	Active
Error	CS0021	Cannot apply indexing with [] to an expression of type 'ISession'	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	39	Active
Error	CS1061	'ContainerBuilder' does not contain a definition for 'RegisterControllers' and no accessible extension method 'RegisterControllers' accepting a first argument of type 'ContainerBuilder' could be found (are you missing a using directive or an assembly reference?)	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	59	Active
Error	CS0103	The name 'DependencyResolver' does not exist in the current context	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	65	Active
Error	CS0246	The type or namespace name 'AutofacDependencyResolver' could not be found (are you missing a using directive or an assembly reference?)	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	65	Active
Error	CS1061	'HttpRequest' does not contain a definition for 'RawUrl' and no accessible extension method 'RawUrl' accepting a first argument of type 'HttpRequest' could be found (are you missing a using directive or an assembly reference?)	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	99	Active
Error	CS1061	'HttpRequest' does not contain a definition for 'UserAgent' and no accessible extension method 'UserAgent' accepting a first argument of type 'HttpRequest' could be found (are you missing a using directive or an assembly reference?)	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	99	Active
Warning	HttpContextCurrent	'HttpContextHelper.Current' is obsolete: 'Prefer accessing HttpContext via injection'	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	38	Active
Warning	HttpContextCurrent	'HttpContextHelper.Current' is obsolete: 'Prefer accessing HttpContext via injection'	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	39	Active
Warning	HttpContextCurrent	'HttpContextHelper.Current' is obsolete: 'Prefer accessing HttpContext via injection'	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	99	Active
Warning	HttpContextCurrent	'HttpContextHelper.Current' is obsolete: 'Prefer accessing HttpContext via injection'	eShopLegacyMVC	C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\Global.asax.cs	99	Active
Warning	MSB4011	"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Microsoft\VisualStudio\v16.0\TypeScript\Microsoft.TypeScript.Default.props" cannot be imported again. It was already imported at "C:\Program Files\dotnet\sdk\5.0.400\Sdks\Microsoft.NET.Sdk.Web.ProjectSystem\targets\Microsoft.NET.Sdk.Web.ProjectSystem.props (25,3)". This is most likely a build authoring error. This subsequent import will be ignored. 	eShopLegacyMVC		2	
Warning	MSB4011	"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Microsoft\VisualStudio\v16.0\TypeScript\Microsoft.TypeScript.targets" cannot be imported again. It was already imported at "C:\Users\Administrator\Desktop\eShopModernizing\eShopLegacyMVCSolution\src\eShopLegacyMVC\eShopLegacyMVC.csproj (73,3)". This is most likely a build authoring error. This subsequent import will be ignored. 	eShopLegacyMVC		14	
Warning	NETSDK1086	A FrameworkReference for 'Microsoft.AspNetCore.App' was included in the project. This is implicitly referenced by the .NET SDK and you do not typically need to reference it from your project. For more information, see https://aka.ms/sdkimplicitrefs	eShopLegacyMVC	C:\Program Files\dotnet\sdk\5.0.400\Sdks\Microsoft.NET.Sdk\targets\Microsoft.NET.Sdk.FrameworkReferenceResolution.targets	65	
```

https://docs.microsoft.com/en-us/dotnet/architecture/porting-existing-aspnet-apps/example-migration-eshop provides manual guidance on porting.

Working through errors one by one using guidance from the above, and got the app to run as a .net 5.0 app.  See branch commit history for specific changes.

## Moving to docker

Right-click on project, "Add Docker Support..." - select linux.

Adds a default docker file.

Attempting to run warns that Docker is configured for windows containers - unable to test further on windows.

Will pull on a linux machine and use docker build.