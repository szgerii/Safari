# Safari

## Project Origin

This game was developed for our Software Technologies course, meaning the main focus was on software architecture and development methods. We worked on it throughout most of the semester, so it's the result of about 3-4 months of active work (except for exam seasons).

Note that during development the repository was hosted on GitLab, it was only moved here afterwards. Because of this, the commit history might not have every one of our GitHub profiles properly linked, see the next section for seeing who did what (on a surface level). It also means that no GitHub Actions setup exists for our CI/CD pipeline, but the old GitLab-based version can still be checked out through its configuration files in the repo.

The documentation was also originally hosted as a GitLab wiki, so it might look a bit awkward here in a couple places.

## Developed By

- [Szűcs Gergely](https://github.com/szgerii): Engine, Game Logic (animals, rangers, poachers), optimizations (spatial partitioning), CI/CD, Tests, Docs
- [Zelenák András](https://github.com/ZeleAnd): Engine, Game Logic (visitors, cars), Road System, Save/Load System, Day/Night System, Tests, Docs
- [Kiss Gergő](https://github.com/k-geri): Game Logic (park/animal management), All Menu Systems & Other GUIs, Build System, Docs, Tests, Docs

## Solution Structure
- `Safari/Safari`: the main project for the game itself
- `Safari/Engine`: the engine used by the game<br>
    - The engine runs on top of the [MonoGame](https://monogame.net/) framework and is an expansion of an old hobby project started by us. 
- `Safari/GeonBit.UI`: UI library for MonoGame ([GeonBit.UI](https://www.nuget.org/packages/GeonBit.UI/))
- `Safari/SafariTest`: MSTest project for unit testing the game

## Development / Building

Visual Studio is recommended for modifying, building and testing the project, as it is the only IDE that is/was tested by us during development.

It is recommended to install the `MonoGame.Templates.CSharp` NuGet package for development purposes, which should pull in everything needed to configure MonoGame related project data. It also includes a GUI editor for the .mgcb file (the content pipeline configuration).

## CI pipeline

The pipeline consists of three stages: build, test and deploy.
All of these are ran after every commit to master and develop, and the first two also run after every merge request.

### 1. Build

The build stage simply builds the game in Release configuration. If this fails, no other job is executed.
This skips any parts that would require an actual graphics device to work (mainly running the content pipeline for the game assets).

### 2. Test

#### Unit tests

The unit tests contained under the `SafariTest` project are ran, calculating a code coverage percentage (and report) along the way.
The detailed test results are also saved as an artifact report, allowing them to be accessed from the GitLab webpage.

#### SAST

GitLab Static Application Security Testing is also included in the pipeline, with an exception for the weak PRNG rule (as it is not relevant for our use case).

#### Secret Detection, Dependency Scanning

These two GitLab pipeline jobs have also been included on a "better safe than sorry" basis.

#### Static Code Analysis

Static code analysis is implemented through the [Code Quality .NET](https://szofttech.inf.elte.hu/components/code-quality-dotnet) CI component.

### 3. Deploy

Lastly, the pipeline generates the documentation files based on the XML comments from the code. They are generated using Doxygen, in HTML format and are exported as an artifact.
