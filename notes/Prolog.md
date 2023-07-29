## Variable matching

ExpLang is a declarative programming language with the goal that program execution is easy to trace and understand.

A difference between ExpLang and more complicated logic programming languages like Prolog is unification doesn't allow aliasing between variables.

As an exploration, a subset of ExpLang expressions should return the same answers as a more complete Prolog implementation:

A **string** is made of *characters* (digits and letters) and can be thought of as a regular expression with capture groups and backreferences.

* **Digits** are literal characters that must match other digits: `3` would be `3` in RegEx
* **Lowercase letters** are variables that equal one digits: `a` would be `.` in RegEx
* **Uppercase letters** are variables that match one or more digits: `A` would be `.+` in RegEx

| ExpLang string | RegEx    | Matches | Captures        | Doesn't Match       |
| -------------- | -------- | ------- | --------------- | ------------------- |
| `123`          | `123`    | `123`   |                 | `124`, `12`, `1234` |
| `aba`          | `(.).\1` | `121`   | a: `1`, b: `2`  | `123`               |
| `A3B`          | `.+3.+`  | `1234`  | A: `12`, B: `4` | `3`, `123`, `1245`  |

Repeated variables need to contain the same value.

An **expression** is two strings: *string* `=` *string*

Each string must match the same string. This can be thought of using the RegEx *and* operator to say that two different capture strings must match some test input, and finding all possible capture groups for any test string.

| ExpLang expression | Captures                                                 |
| ------------------ | -------------------------------------------------------- |
| `a2c` = `1b3`      | a: `1`, b: `2`, c: `3`                                   |
| `12` = `34`        | *doesn't match because 1 != 3*                           |
| `a` = `b`          | *doesn't match because a doesn't resolve to a literal*   |
| `A3` = `12b`       | A: `12`, b: `3`                                          |
| `AB` = `123`       | A: `12`, B: `3` *or* A: `1`, B: `23`                     |
| `Ab` = `b23`       | A: `32`, b: `3`                                          |
| `AbbC` = `1223445` | A: `1`, b: `2`, C: `3445` *or* A: `1223`, b: `4`, C: `5` |
