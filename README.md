# CSM DevKit

A CSM developer toolbox that provides simplified operations and utilities for developers.

## Installation

To install correctly the CMDLine tool, you need to follow the next steps:

1. Add GitHub as package sources:

    ```bash
    dotnet nuget add source --name github --username <Username> --pasword <PAT> --store-password-in-clear-text "https://nuget.pkg.github.com/Cosmos-CSM/index.json"    
2. Install tool as global CMDLine

    ```bash
    dotnet tool install --global CSM.DevKit
3. You're readdy to use it.

    ```bash
    csmdk db
