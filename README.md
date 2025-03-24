# Gerik - Szafari

## Project Structure
- `Safari/Safari`: the main project for the game itself
- `Safari/Engine`: the engine used by the game, powered by [MonoGame](https://monogame.net/)
- `Safari/GeonBit.UI`: UI library for MonoGame ([GeonBit.UI](https://www.nuget.org/packages/GeonBit.UI/))
- `Safari/SafariTest`: MSTest project for unit testing the game

## Development / Building
Visual Studio is recommended for modifying, building and testing the project, as it is the only IDE that is/was tested by us during development.

The `MonoGame.Templates.CSharp` NuGet package should pull in everything needed to build. It also includes a GUI editor for the .mgcb file (the content pipeline configuration).

## CI pipeline

WIP