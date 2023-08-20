---
created: 2017-07-23
---

# Natural Numbers

## Successor
```
0+1=0
1+1=2
2+1=3
...
8+1=9

1+2 => 1+1+1 => 2+1 => 3
```

## Add

- [ ] Learn to memoize all `N+N` single digit

Memoize `9+1=10`

## Digits

`10+1=11`

## Subtraction

```
3-1
lookup _+1=3
       2+1=3
solved: 3-1=2
```

## Negative
```
-1+1=-
-2+1=-1
```

- [ ] How to avoid hardcoding?

## Inequality
```
3-1 ?<=>? 0
3-1 => 2 =>
2 > 0

(parse "(a:expr) <=>> (b:expr))
  => 1) simplify a
     2) simplify b -- [ ] subtract to 0?
```

Idea: Memoize by some caching algorithm for facts
Idea: PERF: track how expensive each database scan is
