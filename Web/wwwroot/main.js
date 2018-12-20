"use strict";

function $(id) { 
  return document.getElementById(id);
}

window.onload = () => {
  $("button").onclick = async () => {
    var request = await fetch("values", {
      method: "POST",
      headers: {
        "Content-Type": "application/json"
      },
      body: JSON.stringify({
        text: $("span").textContent,
      })
    });
    $("span").textContent = await request.text();
  };
}