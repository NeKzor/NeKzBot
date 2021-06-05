# Installation

Requires [.NET 5.0](https://dotnet.microsoft.com/download) or higher.

* `git clone --recurse-submodules https://github.com/NeKzor/NeKzBot`
* `cd NeKzBot`
* `./install`
* Configure `private/credentials.json`
  * Discord app bot token
  * speedrun.com API token
* `./build`
* `./run`

## Debugging

* `./build DEBUG`
* `./run DEBUG`
* Command prefix starts with `!` e.g. `!services.?`
