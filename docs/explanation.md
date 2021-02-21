# Build Explanation 


## Core Compile Process

rules_dotnet rules collect build inputs provided from the `srcs` and `deps` attributes and ultimately pass them to `csc.exe` mimicking msbuild as much as possible [(see emit_assembly_core_csharp)](../dotnet/private/actions/assembly_core.bzl#L76) 

Rules compile to standalone executables using [/nostdlib+](https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/nostdlib-compiler-option). One notable impact of this is all possibly referenced system dlls are copied into the build output directory next to the target assembly. [See Runtime considerations](./runtime.md). This results in *many* more .dlls in the output directory than a standard call to `dotnet build`.

## Output

Output for `bazel build //foo/bar:bar.exe` given the input target defined in `<workspaceroot>/foo/bar/BUILD`
``` python 
csharp_binary(
    name = "bar.exe",
    srcs = ["Program.cs"],
    deps = ["@core_sdk_stdlib//:libraryset"]
)
```

- `bazel-bin/foo/`
  - `bar.exe.param` These are the parameters passed to [csc.exe](../dotnet/private/actions/assembly_core.bzl#L170) to compile `bazel-bin/foo/bar.exe/bar.exe`
  - `bar.exe/` (yes, this is a folder)
    - `bar.exe_0.exe`: can be executed directly: `./bar.exe_0.exe` 
    - `bar.exe`: can be executed with: `dotnet bar.exe`
      - May produce an error regarding `bar.deps.json` not existing
    - `bar.exe_0.exe.runfiles`: runfiles?
    - `bar.exe_0.exe.runfiles_manifest`
    - *many* system dlls
    - nuget dependencies


### Justification for `exe_0`
IDK


## Generation of System Assemblies


## Unit Test Rules
Test rules depend on the [nuget packages](../dotnet/private/deps/nuget.bzl).


# Tweaks to other docs

## Setup of .bazelrc
```
build --host_platform @io_bazel_rules_dotnet//dotnet/toolchain:windows_amd64_3.1.100
build --platforms @io_bazel_rules_dotnet//dotnet/toolchain:windows_amd64_3.1.100
```

## nuget2bazel

> Nuget2Bazel auto-upgrades your pacakge versions! Create an issue for not allowing multiple package versions?

### Reccommended way to register a nuget package with bazel
```powershell
mkdir <workspaceroot>/deps/nuget.bzl
touch <workspaceroot>/deps/BUILD
```

`<workspaceroot>/deps/nuget.bzl`
```python
"""Defines nuget pacakges for the workspace"""
load("@io_bazel_rules_dotnet//dotnet:defs.bzl", "nuget_package")

def nuget_packages():
    pass # remove this line after adding first package
```
`<workspaceroot>/WORKSPACE`
```python
load("//deps:nuget.bzl", "nuget_packages")
nuget_packages()
```

```powershell
bazel run @io_bazel_rules_dotnet//tools/nuget2bazel:nuget2bazel.exe -- add -p $PWD CommandLineParser 2.6.0 -b deps/nuget.bzl -i 
```

> **Side Effect**: this command creates a `packages` folder in the current directory. I'm not sure if this is required by NuGet or not. It may be safely deleted.


`@commandlineparser//:lib` may now be listed as a dependency of a target for access to the nuget package.

`<workspaceroot>/foo/bar/BUILD`
``` python
load("@io_bazel_rules_dotnet//dotnet:defs.bzl", "csharp_binary")

csharp_binary(
    name = "bar.exe",
    srcs = [
        "Program.cs",
    ],
    deps = [
        "@core_sdk_stdlib//:libraryset",
        "@commandlineparser//:lib"
    ],
)

```
