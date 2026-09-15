# Confix.Options

Bind and validate .NET options with `ConfixSection` and `AddConfixOptions<T>`. Uses standard .NET options/configuration dependencies, with no CLI, schema validator, or cloud SDK.

Includes the registration catalog generator as an analyzer; applications need no separate build dependency. Unconditional top-level registrations are picked up automatically; use a shared `IConfixModule` for other setup.

See the code-first guide in the Confix repository for configuration and examples. Requires .NET 10.
