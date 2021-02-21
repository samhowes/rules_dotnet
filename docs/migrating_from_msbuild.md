# Migrating from MsBuild

## Core conversion concepts
- `.csproj` => `BUILD`
- `<PackageReference>` => `dep`
    - Packages just become yet another [target](https://docs.bazel.build/versions/4.0.0/build-ref.html#targets) that your target can depend on
    - `nuget2bazel` converts `<PackageReference Include="PackageName" Version="X.Y.Z"/>` to `@packagename//:lib` where `@packagename` is defined as a package using [`nuget_package`](api.md#nuget_package) with `:lib` exposing the core `.dll` as a bazel target.
