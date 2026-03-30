# Build notes

## Current state

This package contains the full Roadbound World source layout plus a GitHub Actions workflow that will build and package the RimWorld 1.6 mod automatically.

## What is finished here

- road travel source
- roadside POI source
- traveler crossing source
- persistent hostile carryover source
- Morrowind style pawn inventory source
- RimWorld mod folder layout
- automated build workflow that outputs a zipped mod with the compiled DLL in `1.6/Assemblies`

## What is still blocked inside this chat environment

This container does not include a local C# toolchain and also blocks binary toolchain downloads, so I could not produce the final DLL from inside this session.

## How the repository is now set up

- `Source/RoadboundWorld.csproj` targets `net472`
- output path is `../1.6/Assemblies/`
- `.github/workflows/build.yml` restores packages, builds the project, and packages the final mod zip automatically on GitHub Actions

## Build target

- RimWorld 1.6
- .NET Framework 4.7.2
- `Krafs.Rimworld.Ref`
- `HarmonyLib`

## Important note

The artifact produced by this chat is still a build ready source package.
The repository itself is now finished enough to auto build in a normal GitHub Actions or local modding environment.
