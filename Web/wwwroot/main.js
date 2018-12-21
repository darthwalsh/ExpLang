"use strict";

function $(id) { 
    return document.getElementById(id);
}

window.onload = () => {
  $("button").onclick = async () => {
      try {
          var request = await fetch("values", {
              method: "POST",
              headers: {
                  "Content-Type": "application/json"
              },
              body: JSON.stringify({
                  text: $("input").textContent,
              })
          });
          const result = await request.json();
          $("output").textContent = result.result;
      } catch (ex) {
          $("output").textContent = `js error: ${ex}`;
      }
  };
};