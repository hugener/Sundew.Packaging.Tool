# Sundew.Packaging.Update

## **1. Description**
Alternative NuGet client for bulk updating NuGet packages in csproj, fsproj and vbproj projects.

## **2. Install**
dotnet tool install -g Sundew.Packaging.Update

## **3. Usage**

```
Help
 Arguments:
  -id | --package-ids    | The package(s) to update. (* Wildcards supported)                    | Default: *
                           Format: Id[.Version] or "Id[ Version]" (Pinning version is optional)
  -p  | --projects       | The project(s) to update (* Wildcards supported)                     | Default: *
  -s  | --source         | The source or source name to search for packages (All supported)     | Default: NuGet.config: defaultPushSource
      | --version        | Pins the NuGet package version.                                      | Default: Latest version
  -d  | --root-directory | The directory to search to projects                                  | Default: Current directory
  -l  | --local          | Forces the source to "Local-Sundew"
  -pr | --prerelease     | Allow updating to latest prerelease version
  -v  | --verbose        | Verbose
```

## **3. Examples**
Open Package Manager Console in Visual Studio or a similar command line.
1. ```spu``` - Update all packages in all projects to the latest stable version.
2. ```spu -id Serilog*``` - Updates all Serilog packages to the latest stable version for all projects (That reference Serilog)
3. ```spu -id TransparentMoq -pr -l``` - Updates TransparentMoq to the latest prerelease from the "Local-Sundew" source (Useful together with Sundew.Packaging.Publish for local development).

Sundew.Packaging.Publish is available on NuGet at: https://www.nuget.org/packages/Sundew.Packaging.Publish