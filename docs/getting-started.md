# Getting started

[Documentation index](README.md)

## Requirements

Aurora targets .NET 9. Install the .NET 9 SDK to build from source; running the resulting framework-dependent executable also requires the .NET 9 runtime. Initial package restore needs access to NuGet unless the dependencies are already cached.

Run these commands from the repository root:

```sh
dotnet build Aurora/Aurora.csproj
```

## Your first program

Save this as `hello.aur`:

```aurora
String.create(name="Liam")
Terminal.writeLine("Hello"; name)
```

Run it:

```sh
dotnet run --project Aurora -- hello.aur
```

The program prints:

```text
Hello Liam
```

The `--` separates `dotnet run` options from arguments passed to Aurora. After building, you can also run the DLL directly:

```sh
dotnet Aurora/bin/Debug/net9.0/Aurora.dll hello.aur
```

Scripts conventionally use the `.aur` extension. The interpreter takes a script path; an interactive REPL is not currently provided.

## Command-line options

Use the current executable's help for the installed version:

```sh
dotnet run --project Aurora -- --help
```

| Option | Purpose |
| --- | --- |
| `--version` | Display the executable version. |
| `-v`, `--verbose` | Enable verbose log messages. |
| `-d`, `--debug` | Enable debug log messages. |
| `-w`, `--warn` | Enable warning log messages. |
| `-s`, `--strict` | Enable only the log levels explicitly requested. |
| `--no-console` | Suppress console logging; `Terminal` output still works. |
| `--log-file <path>` | Set the log output file. |
| `--disable-easter-eggs` | Disable interpreter Easter eggs. |
| `--config-file <path>` | Set the configuration path; configuration application is incomplete. |

Without `--strict`, debug also enables verbose logging, and verbose enables warning logging. Strict mode affects logging, not language type checking.

```sh
dotnet run --project Aurora -- hello.aur --verbose --warn
```

See [examples](examples.md) for more programs. Use [status and limitations](status.md) if an older example or command differs from the current interpreter.
