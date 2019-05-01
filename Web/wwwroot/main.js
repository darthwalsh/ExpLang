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
                  text: $("input").value,
              })
          });
          const result = await request.json();
          $("output").textContent = result.result;
          $("output").style.color = result.error ? "red" : "black";
      } catch (ex) {
          $("output").textContent = `js error: ${ex}`;
          $("output").style.color = "red";
      }
  };
};