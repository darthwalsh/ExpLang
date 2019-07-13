# ExpLang = Explained Language

[![Build status](https://darthwalsh.visualstudio.com/ExpLang/_apis/build/status/ExpLang-Azure%20Web%20App%20for%20ASP.NET-CI)](https://darthwalsh.visualstudio.com/ExpLang/_build/latest?definitionId=3)

Simple math [DSL](https://en.wikipedia.org/wiki/Domain-specific_language) for defining arithmetic operations.

Try it online: https://explang.azurewebsites.net/

## Running Locally

Building will download and execute [Grammatica](https://github.com/cederberg/grammatica) to generate source code.

## Deploying

Run `az webapp deployment list-publishing-profiles --name explang --resource-group darthwalsh` to get `userPWD`
