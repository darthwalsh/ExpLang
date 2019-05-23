"use strict";

function $(id) { 
    return document.getElementById(id);
}

async function run() {
  try {
      var request = await fetch("values", {
          method: "POST",
          headers: {
              "Content-Type": "application/json"
          },
          body: JSON.stringify({
              text: $("input").value
          })
      });
      const result = await request.json();
      $("output").textContent = result.result;
      $("output").style.color = result.error ? "red" : "black";
  } catch (ex) {
      $("output").textContent = `js error: ${ex}`;
      $("output").style.color = "red";
  }
}

window.onload = () => {
  if (localStorage.getItem("in")) {
    $("input").value = localStorage.getItem("in");
  }
  $("input").addEventListener("input", () =>
    localStorage.setItem("in", $("input").value));
  $("input").addEventListener("keydown", e => {
    if ((e.ctrlKey || e.metaKey) && (e.keyCode === 13 || e.keyCode === 10)) {
      run();
    }
  });

  $("run").onclick = run;

  $("issue").onclick = () => {
    const codeBlock = "```";
    const body = `**Description of the problem:**

**Expected results:**

**Actual results:**
${codeBlock}
${$("output").value}
${codeBlock}

**Code:**
${codeBlock}
${$("input").value}
${codeBlock}`;
    const url = `https://github.com/darthwalsh/ExpLang/issues/new?body=${encodeURIComponent(body)}`;
    var win = window.open(url, '_blank');
    win.focus();
  };
};