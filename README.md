# Roadbound World

Roadbound World is a RimWorld 1.6 mod project with two big feature lines:

- road based tile to tile travel with connected roadside encounters
- a Morrowind inspired replacement for the pawn gear and inventory tab

## Included features

### Road travel systems

- a road that runs across the map and connects to neighboring tiles
- edge travel prompts when pawns reach the connected border
- roadside POI spawning
- traveler groups entering from the map edge and crossing the tile
- hostile carryover records so enemies that leave the map can be met again on the next tile

### Morrowind inspired inventory UI

- custom Inventory, Equipment, and Stats tabs
- paper doll layout with slot mapping
- left aligned equipped strip under the paper doll
- tiled grid inventory on the right panel
- category filtering tabs
- black and gold Morrowind style framing and slot treatment

## Repository layout

- `About/` contains RimWorld metadata
- `Textures/UI/Morrowind/` contains the UI art used by the custom tab
- `1.6/Defs/` contains the world object defs
- `1.6/Languages/` contains keyed strings
- `Source/` contains the C# project and code
- `.github/workflows/build.yml` builds and packages the mod automatically on GitHub Actions

## Honest status

Inside this chat environment I could not generate the compiled DLL because the required binary build toolchain is unavailable here.

What I *did* finish is the full build ready repository, including an automated workflow that compiles the mod and creates a proper install zip in a standard C# environment.
