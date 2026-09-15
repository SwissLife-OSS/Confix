# Confix.CodeGeneration

Build-only activation catalog generator for Confix. This is not a published package: it ships as an analyzer inside `Confix.Options`, so applications reference that package only.

A catalog is generated for executables; libraries referencing `Confix.Options` are unaffected. Unconditional top-level registrations are picked up automatically; use a shared Confix module for other setup.

See the code-first guide in the Confix repository for configuration and examples. Requires .NET 10.
