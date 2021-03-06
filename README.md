# ExpLang = Explained Language

[![Build status](https://darthwalsh.visualstudio.com/ExpLang/_apis/build/status/ExpLang-Azure%20Web%20App%20for%20ASP.NET-CI)](https://darthwalsh.visualstudio.com/ExpLang/_build/latest?definitionId=3)

Simple math [DSL](https://en.wikipedia.org/wiki/Domain-specific_language) for defining arithmetic operations.

Try it online: https://explang.azurewebsites.net/

## Running Locally

Building will download and execute [Grammatica](https://github.com/cederberg/grammatica) to generate source code.

## Language

A base-4 adding program

    1 + 0 = 1
    1 + 1 = 2
    2 + 1 = 3
    3 + 1 = 10

    a + b = c
    | a + 1 = x
    | y + 1 = b
    | x + y = c

    2 + 1
    1 + 3

A **program** is multiple lines of *rules* and *expressions*.

An **expression** is a

* number e.g. `3`
* lowercase variable for single characters e.g. `a`
* uppercase variables for any number of characters e.g. `A`
* simple arithmetic operation of numbers or variables e.g. `3 + ab`

A **rule** is *expression* `=` *expression*, optionally followed by where patterns.

A **where pattern** is `|` *rule*.

### Runtime Behavior

Each expression is evaluated to try to find a number value.

Each rule is checked to see if the expression matches the rule. If multiple rules match, they must find the same value.

A rule matches if every rule and where equality matches. Every variable must have a unique value. Lowercase variables must have a single-digit value. Uppercase variables may have a value of one or more digits.

## Deploying

Run `az webapp deployment list-publishing-profiles --name explang --resource-group darthwalsh` to get `userPWD`
