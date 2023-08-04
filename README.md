# ExpLang = Explained Language

Simple math [DSL](https://en.wikipedia.org/wiki/Domain-specific_language) for defining arithmetic operations.

## Language Definition

A base-4 adding program

    1 + 0 = 1
    1 + 1 = 2
    2 + 1 = 3
    3 + 1 = 10

    $a + $b = $c
    | $a + 1 = $x
    | $y + 1 = $b
    | $x + $y = $c

    ---

    2 + 1
    1 + 3

A **program** is multiple lines of *rules* then `---` then some *expressions*.

An **rule** is any text, where `$` followed by an identifier is a variable.

A **where pattern** is `|` *rule*.

### Runtime Behavior

Each expression is evaluated to try to find a number value.

Each rule is checked to see if the expression matches the rule. If multiple rules match, they must find the same value.

A rule matches if every rule and where equality matches. Every variable must have a unique value. Lowercase variables must have a single-digit value. Uppercase variables may have a value of one or more digits.

## Pending work

- [x] website frontend
- [x] github pages with CNAME https://exp.carlwa.com
- [ ] write calc engine
  - [ ] allow spaces
- [ ] favicon
- [ ] Copy notes from OneNote
