
const tokenRegex = /[^\$]+|\$\w+/g;
/** @typedef {{va: boolean, s: string}} Token */
/**
 * @param {string} text
 * @returns {Token[]}
 */
export function parseExpr(text) {
  const tokens = [];
  let match;
  while ((match = tokenRegex.exec(text)) !== null) {
    let s = match[0];
    const va = s[0] === "$";
    if (va) s = s.slice(1);
    tokens.push({va, s});
  }

  return tokens;
}

/** @typedef {{expr: Token[], wheres: Token[][]}} Fact */
/**
 * @param {string} text
 * @returns {Fact[]}
 */
export function parseFacts(text) {
  const facts = /** @type {Fact[]} */([]);
  for (const line of text.split("\n")) {
    if (!line) continue;

    if (line.startsWith("|")) {
      if (!facts.length) throw new Error("Where without fact");
      const expr = parseExpr(line.substring(1).trim());
      facts[facts.length - 1].wheres.push(expr);
    } else {
      const expr = parseExpr(line.trim());
      facts.push({expr, wheres: []});
    }
  }
  return facts;
}

/** @typedef {{children: Result[], line: string}} Result */
/**
 * @param {string} text
 * @returns {{results: Result[], error: boolean}}
 */
export function evaluate(text) {
  return {
    "results": [
      {
        "children": [
          {
            "children": [],
            "line": "Solving 2 * 2 = X",
          },
          {
            "children": [],
            "line": "Applied rule a * b = c Implies a = 2, b = 2, c = 4, X = 4",
          },
          {
            "children": [
              {
                "children": [],
                "line": "Solving x + 1 = 2",
              },
              {
                "children": [],
                "line": "Applied rule 1 + 1 = 2 Implies x = 1",
              },
              {
                "children": [
                  {
                    "children": [],
                    "line": "Rule A = A didn't help because x + 1 is Func but A is Variable",
                  },
                  {
                    "children": [],
                    "line": "Rule 2 + 1 = 3 didn't help because Digit 2 is not 3",
                  },
                  {
                    "children": [],
                    "line": "Rule 3 + 1 = 4 didn't help because Digit 2 is not 4",
                  },
                  {
                    "children": [],
                    "line": "Rule 4 + 1 = 5 didn't help because Digit 2 is not 5",
                  },
                  {
                    "children": [],
                    "line": "Rule 5 + 1 = 6 didn't help because Digit 2 is not 6",
                  },
                  {
                    "children": [],
                    "line": "Rule a + b = c didn't help because y + 1 is Func but b is Variable",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule a * 1 = a didn't help because x + 1 is function + but a * 1 is function *",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule a * b = c didn't help because x + 1 is function + but a * b is function *",
                  },
                ],
                "line": "Rules that didn't help:",
              },
            ],
            "line": "| x + 1 = b Implies x = 1",
          },
          {
            "children": [
              {
                "children": [],
                "line": "Solving 2 * 1 = y",
              },
              {
                "children": [],
                "line": "Applied rule a * 1 = a Implies a = 2, y = 2",
              },
              {
                "children": [
                  {
                    "children": [],
                    "line": "Rule A = A didn't help because 2 * 1 is Func but A is Variable",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule 1 + 1 = 2 didn't help because 2 * 1 is function * but 1 + 1 is function +",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule 2 + 1 = 3 didn't help because 2 * 1 is function * but 2 + 1 is function +",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule 3 + 1 = 4 didn't help because 2 * 1 is function * but 3 + 1 is function +",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule 4 + 1 = 5 didn't help because 2 * 1 is function * but 4 + 1 is function +",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule 5 + 1 = 6 didn't help because 2 * 1 is function * but 5 + 1 is function +",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule a + b = c didn't help because 2 * 1 is function * but a + b is function +",
                  },
                  {
                    "children": [],
                    "line": "Rule a * b = c didn't help because x + 1 is Func but b is Variable",
                  },
                ],
                "line": "Rules that didn't help:",
              },
            ],
            "line": "| a * x = y Implies y = 2",
          },
          {
            "children": [
              {
                "children": [],
                "line": "Solving 2 + 2 = c",
              },
              {
                "children": [],
                "line": "Applied rule a + b = c Implies a = 2, b = 2, c = 4, c = 4",
              },
              {
                "children": [
                  {
                    "children": [],
                    "line": "Solving 2 + 1 = x",
                  },
                  {
                    "children": [],
                    "line": "Applied rule 2 + 1 = 3 Implies x = 3",
                  },
                  {
                    "children": [
                      {
                        "children": [],
                        "line": "Rule A = A didn't help because 2 + 1 is Func but A is Variable",
                      },
                      {
                        "children": [],
                        "line": "Rule 1 + 1 = 2 didn't help because Digit 2 is not 1",
                      },
                      {
                        "children": [],
                        "line": "Rule 3 + 1 = 4 didn't help because Digit 2 is not 3",
                      },
                      {
                        "children": [],
                        "line": "Rule 4 + 1 = 5 didn't help because Digit 2 is not 4",
                      },
                      {
                        "children": [],
                        "line": "Rule 5 + 1 = 6 didn't help because Digit 2 is not 5",
                      },
                      {
                        "children": [],
                        "line":
                          "Rule a + b = c didn't help because a + 1 is Func but d is Variable",
                      },
                      {
                        "children": [],
                        "line":
                          "Rule a * 1 = a didn't help because 2 + 1 is function + but a * 1 is function *",
                      },
                      {
                        "children": [],
                        "line":
                          "Rule a * b = c didn't help because 2 + 1 is function + but a * b is function *",
                      },
                    ],
                    "line": "Rules that didn't help:",
                  },
                ],
                "line": "| a + 1 = x Implies x = 3",
              },
              {
                "children": [
                  {
                    "children": [],
                    "line": "Solving y + 1 = 2",
                  },
                  {
                    "children": [],
                    "line": "Applied rule 1 + 1 = 2 Implies y = 1",
                  },
                  {
                    "children": [
                      {
                        "children": [],
                        "line": "Rule A = A didn't help because y + 1 is Func but A is Variable",
                      },
                      {
                        "children": [],
                        "line": "Rule 2 + 1 = 3 didn't help because Digit 2 is not 3",
                      },
                      {
                        "children": [],
                        "line": "Rule 3 + 1 = 4 didn't help because Digit 2 is not 4",
                      },
                      {
                        "children": [],
                        "line": "Rule 4 + 1 = 5 didn't help because Digit 2 is not 5",
                      },
                      {
                        "children": [],
                        "line": "Rule 5 + 1 = 6 didn't help because Digit 2 is not 6",
                      },
                      {
                        "children": [],
                        "line":
                          "Rule a + b = c didn't help because d + 1 is Func but b is Variable",
                      },
                      {
                        "children": [],
                        "line":
                          "Rule a * 1 = a didn't help because y + 1 is function + but a * 1 is function *",
                      },
                      {
                        "children": [],
                        "line":
                          "Rule a * b = c didn't help because y + 1 is function + but a * b is function *",
                      },
                    ],
                    "line": "Rules that didn't help:",
                  },
                ],
                "line": "| y + 1 = b Implies y = 1",
              },
              {
                "children": [
                  {
                    "children": [],
                    "line": "Solving 3 + 1 = d",
                  },
                  {
                    "children": [],
                    "line": "Applied rule 3 + 1 = 4 Implies d = 4",
                  },
                  {
                    "children": [
                      {
                        "children": [],
                        "line": "Rule A = A didn't help because 3 + 1 is Func but A is Variable",
                      },
                      {
                        "children": [],
                        "line": "Rule 1 + 1 = 2 didn't help because Digit 3 is not 1",
                      },
                      {
                        "children": [],
                        "line": "Rule 2 + 1 = 3 didn't help because Digit 3 is not 2",
                      },
                      {
                        "children": [],
                        "line": "Rule 4 + 1 = 5 didn't help because Digit 3 is not 4",
                      },
                      {
                        "children": [],
                        "line": "Rule 5 + 1 = 6 didn't help because Digit 3 is not 5",
                      },
                      {
                        "children": [],
                        "line":
                          "Rule a + b = c didn't help because a + 1 is Func but x is Variable",
                      },
                      {
                        "children": [],
                        "line":
                          "Rule a * 1 = a didn't help because 3 + 1 is function + but a * 1 is function *",
                      },
                      {
                        "children": [],
                        "line":
                          "Rule a * b = c didn't help because 3 + 1 is function + but a * b is function *",
                      },
                    ],
                    "line": "Rules that didn't help:",
                  },
                ],
                "line": "| x + y = d Implies d = 4",
              },
              {
                "children": [
                  {
                    "children": [],
                    "line": "Rule A = A didn't help because 2 + 2 is Func but A is Variable",
                  },
                  {
                    "children": [],
                    "line": "Rule 1 + 1 = 2 didn't help because Digit 2 is not 1",
                  },
                  {
                    "children": [],
                    "line": "Rule 2 + 1 = 3 didn't help because Digit 2 is not 1",
                  },
                  {
                    "children": [],
                    "line": "Rule 3 + 1 = 4 didn't help because Digit 2 is not 3",
                  },
                  {
                    "children": [],
                    "line": "Rule 4 + 1 = 5 didn't help because Digit 2 is not 4",
                  },
                  {
                    "children": [],
                    "line": "Rule 5 + 1 = 6 didn't help because Digit 2 is not 5",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule a * 1 = a didn't help because 2 + 2 is function + but a * 1 is function *",
                  },
                  {
                    "children": [],
                    "line":
                      "Rule a * b = c didn't help because 2 + 2 is function + but a * b is function *",
                  },
                ],
                "line": "Rules that didn't help:",
              },
            ],
            "line": "| a + y = c Implies c = 4",
          },
          {
            "children": [
              {
                "children": [],
                "line": "Rule A = A didn't help because 2 * 2 is Func but A is Variable",
              },
              {
                "children": [],
                "line":
                  "Rule 1 + 1 = 2 didn't help because 2 * 2 is function * but 1 + 1 is function +",
              },
              {
                "children": [],
                "line":
                  "Rule 2 + 1 = 3 didn't help because 2 * 2 is function * but 2 + 1 is function +",
              },
              {
                "children": [],
                "line":
                  "Rule 3 + 1 = 4 didn't help because 2 * 2 is function * but 3 + 1 is function +",
              },
              {
                "children": [],
                "line":
                  "Rule 4 + 1 = 5 didn't help because 2 * 2 is function * but 4 + 1 is function +",
              },
              {
                "children": [],
                "line":
                  "Rule 5 + 1 = 6 didn't help because 2 * 2 is function * but 5 + 1 is function +",
              },
              {
                "children": [],
                "line":
                  "Rule a + b = c didn't help because 2 * 2 is function * but a + b is function +",
              },
              {
                "children": [],
                "line": "Rule a * 1 = a didn't help because Digit 2 is not 1",
              },
            ],
            "line": "Rules that didn't help:",
          },
        ],
        "line": "4",
      },
    ],
    "error": false,
  };
}
