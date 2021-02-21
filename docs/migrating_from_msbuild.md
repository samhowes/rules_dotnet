# Migrating from MsBuild

## Core conversion concepts
- `.csproj` => `BUILD`
- `<PackageReference>` => `dep`
    - Packages just become yet another [target](https://docs.bazel.build/versions/4.0.0/build-ref.html#targets) that your target can depend on
    - `nuget2bazel` converts `<PackageReference Include="PackageName" Version="X.Y.Z"/>` to `@packagename//:lib` where `@packagename` is defined as a package using [`nuget_package`](api.md#nuget_package) with `:lib` exposing the core `.dll` as a bazel target.

## Deps

In a `.csproj` file, "dependencies" are spread out to multiple different attributes:

- `<Project Sdk="Microsoft.NET.SDK">`: This is simply syntactic sugar, at build time, [MsBuild replaces these with two `<Import>` elements](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview#project-files). 
- `<PackageReference Include="Contoso.Utility.UsefulStuff" Version="3.6.0" />` This is a hook that triggers [NuGet to download that pacakage](https://docs.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files) (if needed) and make those assemblies availabe for linking
- `<ProjectReference Include="..\lib2\lib2.csproj" />` is a hook that tells [MsBuild to compile `lib2.csproj`](https://github.com/dotnet/msbuild/blob/master/documentation/ProjectReference-Protocol.md#the-projectreference-protocol) before compiling the current project.

In Bazel, each of these elements simply becomes a [`label`](https://docs.bazel.build/versions/4.0.0/build-ref.html#labels) that gets listed in the `deps` attribute of a build rule:
``` python
# //myapp/BUILD
csharp_binary(
    name = "myapp.exe",
    srcs = ["Program.cs"],
    deps = [
        "@core_sdk_stdlib//:libraryset",
        "@contoso.utility.usefulstuff//:lib",
        "//lib2:lib2.dll",
    ],
)
```

This tells bazel to "build" `"@core_sdk_stdlib//:libraryset","@contoso.utility.usefulstuff//:lib"` and `"//lib2:lib2.dll"` before building `myapp.exe`. So long as those labels are defined for bazel, they will be built using whatever rules they are defined with.