---
created: 2017-12-10
---

# CharConcat

Math: Lex, pretty print, pattern match

variables, `$` and `_` e.g. `$a` is expression variable, `_a` is char variable

```
2 = 1+1
3 = 2+1
...
9 = 8+1
10 = 9+1
11 = 10+1
1_d+1 = 1{_d+1}
  where _d+1 = _A
_a_b+1 = _a{_B}
  where _B = _b+1
_a_b+1 =_A0 ???
  where _b+1=10
```

Mixing tabs and spaces
Search for both `A=B` and `B=A`
```
$a + $b = $A + $B
  where $A = $a + $B
  where $b = $B + 1

$A - $B = $C => $A = $B + $C
```

`=>` means prefer term on right
`1 + 1 => 2` could use symbol `:=`?

Define axioms, then auto-generate cache of facts based on practice problems.
Some way to cache results
Some way to invalidate cache if facts change
