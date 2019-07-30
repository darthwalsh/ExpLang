"use strict";

function $(id) {
  return document.getElementById(id);
}

const expanded = "▼";
const collapsed = "▶";
/**
 * @param {object} e MouseEvent
 * @param {HTMLElement} e.target triangle
 */
function triangleClick(e) {
  const triangle = e.target;
  const div = triangle.nextElementSibling.nextElementSibling;
  const expand = triangle.textContent !== expanded;

  triangle.textContent = expand ? expanded : collapsed;
  triangle.style.transform = expand ? "" : "scale(0.7) translateX(-0.4ch)"; // Unicode characters are different sizes...
  div.style.display = expand ? "" : "none";
}

function addResult(e, result, indent) {
  const explainChecked = $("explain").checked;
  let triangle = null;
  if (result.children.length) {
    triangle = document.createElement("pre");
    triangle.className = "triangle";
    triangle.style.marginLeft = indent * 4 + "ch";
    triangle.textContent = expanded;
    triangle.onclick = triangleClick;
    e.appendChild(triangle);
  }

  const line = document.createElement("pre");
  line.textContent = result.line;
  line.style.marginLeft = 1 + indent * 4 + "ch";
  e.appendChild(line);

  if (result.children.length) {
    const div = document.createElement("div");
    result.children.forEach(r => addResult(div, r, indent + 1));
    e.appendChild(div);
  
    if (!explainChecked) {
      triangleClick({ target: triangle });
    }
  }
}

async function run() {
  let result = "Oops";
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
    result = await request.json();
  } catch (ex) {
    $("output").textContent = `js error: ${ex}`;
    $("output").style.color = "red";
  }
  
  while ($("output").firstChild) {
    $("output").removeChild($("output").firstChild);
  }
  result.results.forEach(r => addResult($("output"), r, 0));

  $("output").style.color = result.error ? "red" : "black";
}

window.onload = () => {
  if (localStorage.getItem("in")) {
    $("input").value = localStorage.getItem("in");
  }
  if (localStorage.getItem("explain")) {
    $("explain").checked = true;
  }
    
  $("explain").addEventListener("click", () => {
    const checked = $("explain").checked;
    localStorage.setItem("explain", checked ? "true" : "");
    [...document.getElementsByClassName("triangle")].forEach(triangle => {
      const isExpanded = triangle.textContent === expanded;
      if (isExpanded !== checked) {
        triangleClick({ target: triangle });
      }
    });
  });

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
