import {parseExpr} from "../calc.js";

/** @param {string} s */
function lit(s) {
  return {va: false, s};
}

describe("Parses", function () {
  it("literal text", function () {
    expect(parseExpr("")).toEqual([]);
    expect(parseExpr("a")).toEqual([lit("a")]);
    expect(parseExpr("1")).toEqual([lit("1")]);
    expect(parseExpr("a + 1")).toEqual([lit("a + 1")]);
    expect(parseExpr("ch(1)")).toEqual([lit("ch(1)")]);
  });

  it("variables", function () {
    expect(parseExpr("$a")).toEqual([{va: true, s: "a"}]);
    expect(parseExpr("$X9")).toEqual([{va: true, s: "X9"}]);
    expect(parseExpr("$a + $X")).toEqual([{va: true, s: "a"}, lit(" + "), {va: true, s: "X"}]);
  });
});
