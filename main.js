var fs = require('fs');

function readLines(input, func, callback) {
  var remaining = '';

  input.on('data', function(data) {
    remaining += data;
    var index = remaining.indexOf('\n');
    var last  = 0;
    while (index > -1) {
      var line = remaining.substring(last, index);
      last = index + 1;
      func(line);
      index = remaining.indexOf('\n', last);
    }

    remaining = remaining.substring(last);
  });

  input.on('end', function() {
    if (remaining.length > 0) {
      func(remaining);
    }
    callback();
  });
}

var letter = {};
var abc = 'abcdefghijklmnopqrstuvwxyz';
for (var i = 0; i < abc.length; ++i) {
  var c = abc[i];
  letter[c] = true;
  letter[c.toUpperCase()] = true;
}
function isLetter(c) {
  return letter[c] || false;
}

function getCount(s) {
  var counts = {};
  for (var i = 0; i < s.length; ++i) {
    if (isLetter(s[i])) {
      var A = s[i].toLowerCase();
      counts[A] = (counts[A] || 0) + 1;
    }
  }
  var ans = "";
  for (var i = 0; i < abc.length; ++i) {
    ans += counts[abc[i]] || "0";
  }
  return ans;
}

var lines = [];
// readLines(fs.createReadStream('words.txt'), 
//   (line) => lines.push(line),
//   () => readLines(fs.createReadStream('words2.txt'), 
//     (line) => lines.push(line),
//     () => readLines(fs.createReadStream('words3.txt'), 
//       (line) => lines.push(line),
//       () => complete)));

readLines(fs.createReadStream('words.txt'), 
  (line) => lines.push(line),
  complete);

// readLines(fs.createReadStream('smallwords.txt'), 
//   (line) => lines.push(line),
//   complete);

function complete() {
      console.log("read " + lines.length + " lines");

      var index = {};
      lines.forEach(line => {
        var hash = getCount(line);
        if (!index[hash]) index[hash] = [];
        index[hash].push(line);
      });

      console.log("Completed indexing");
      console.log();

      var toFind = [
        "done",
      ];
      toFind.forEach(f => {
        console.log("Looking for matches for: " + f);
        var matches = index[getCount(f)];
        console.log("Found " + matches.length + " matches: ");
        console.log(matches.join("\r\n"));
        console.log();
      })
    }
