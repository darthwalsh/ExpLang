import {parseExpr, parseFacts} from "../calc.js";

/**
 * @typedef {import('../calc.js').Token} Token
 * @typedef {import('../calc.js').Fact} Fact
 */

/** @param {string} s */
function lit(s) {
  return {va: false, s};
}

/** @param {string} s */
function va(s) {
  return {va: true, s};
}

describe("parseExpr", function () {
  it("literal text", function () {
    expect(parseExpr("")).toEqual([]);
    expect(parseExpr("a")).toEqual([lit("a")]);
    expect(parseExpr("1")).toEqual([lit("1")]);
    expect(parseExpr("a + 1")).toEqual([lit("a + 1")]);
    expect(parseExpr("ch(1)")).toEqual([lit("ch(1)")]);
  });

  it("variables", function () {
    expect(parseExpr("$a")).toEqual([va("a")]);
    expect(parseExpr("$X9")).toEqual([va("X9")]);
    expect(parseExpr("$a + $X")).toEqual([va("a"), lit(" + "), va("X")]);
  });
});

/**
 * @param {Token[]} expr
 * @param {Token[][]} wheres
 * @returns {Fact}
 */
function fact(expr, ...wheres) {
  return {expr, wheres};
}
describe("parseFacts", function () {
  it("parses", function () {
    const expr = `
1=1
|2=2

3=3
| 4=4
|5=5
6=6
`
    expect(parseFacts(expr)).toEqual([
      fact([lit("1=1")], [lit("2=2")]),
      fact([lit("3=3")], [lit("4=4")], [lit("5=5")]),
      fact([lit("6=6")]),
    ]);
  });
});
