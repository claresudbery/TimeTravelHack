const uri = 'api/moretimerequest';
var puckConnection = null;
var workingWithoutPuck = false;
var connection;
var uniqueId = "dummy id";
var hours;
var minutes;
var seconds;
var newHours;
var newMinutes;
var alert = false;
var firstTime = true;

// Make sure your mouse cursor turns into a hand when over the tardis, and gray it out
var img = document.getElementsByTagName('img')[0];
img.style="cursor:pointer;fill:#BBB";

$(document).ready(function () {
    getData();
    updateClockData();
});

function uuidv4() {
    return ([1e7]+-1e3+-4e3+-8e3+-1e11).replace(/[018]/g, c =>
        (c ^ crypto.getRandomValues(new Uint8Array(1))[0] & 15 >> c / 4).toString(16)
    )
}

// Called when we get a line of data - this might mean the button has been clicked
function onLine(v) {
  if (v.includes("click\r")) {
    console.log("BUTTON CLICKED");
    addTimeRequest(uniqueId);
  }
}

function handleNoPuckConnection() {   
  if (confirm("Couldn't connect to puck! Would you like to add a time request every time you click on the tardis?")) {
    uniqueId = uuidv4();
    console.log("new id: " + uniqueId);
    addTimeRequest(uniqueId);
    workingWithoutPuck = true;
  }
}

function handleDataFromPuck() {      
  // Handle the data we get back, and call 'onLine'
  // whenever we get a line
  var buf = "";
  puckConnection.on("data", function(d) {
    buf += d;
    var i = buf.indexOf("\n");
    while (i>=0) {
      onLine(buf.substr(0,i));
      buf = buf.substr(i+1);
      i = buf.indexOf("\n");
    }
  });
}

function setupPuckEvents() {
  // First, reset Puck.js
  puckConnection.write("reset();\n", function() {
    // Wait for it to reset itself
    setTimeout(function() {
      // Now tell it to set up an event handler which will send back a 'click'
      // message whenever the puck button is clicked
      puckConnection.write("setWatch(function(e) {Bluetooth.println('click');LED2.set();setTimeout(function(){LED2.reset();},500);}, BTN, {edge:'falling', debounce:50, repeat:true});\n",
        function() { console.log("Ready..."); });
    }, 1500);
  });
}

function createPuckConnection() {      
    Puck.connect(function(c) {
      puckConnection = c;
      if (puckConnection) {
          uniqueId = uuidv4();
          console.log("new id: " + uniqueId);
      }

      if (!puckConnection) {
        handleNoPuckConnection();
      } else {
        handleDataFromPuck();
        setupPuckEvents();
      }
    });
}

// When clicked, connect or disconnect
img.addEventListener("click", function() {
  if (connection) {
    connection.close();
    connection = undefined;
  }

  if (workingWithoutPuck) {
    addTimeRequest(uniqueId);
  } else {
    createPuckConnection();
  }
});

function getData() {
    $.ajax({
        type: 'GET',
        url: uri,
        success: function (data) {
            $('#timerequests').empty();
            $.each(data, function (key, item) {
                const expired = item.expired ? 'checked' : '';
                const alerted = item.alerted ? 'checked' : '';

                $('<tr><td><input disabled="true" type="checkbox" ' + expired + '></td>' +
                    '<td><input disabled="true" type="checkbox" ' + alerted + '></td>' +
                    '<td>' + item.requestTimeStamp + '</td>' +
                    '<td>' + item.lengthInMinutes + '</td>' +
                    '<td>' + item.userId + '</td>' +
                    '</tr>').appendTo($('#timerequests'));
            });
        }
    });
}

function doAlertFlashing(alertToggle) {  
    console.log("flashing colours after alert");
    if (alertToggle) {
        $("h1").css('background-color', '#FF0000').css('color', '#FFFF00');
        if (puckConnection){
            puckConnection.write("LED3.reset();\n", function() {});
            puckConnection.write("LED1.set();\n", function() {});
        }
    } else {
        $("h1").css('background-color', '#FFFF00').css('color', '#FF0000');
        if (puckConnection){
            puckConnection.write("LED1.reset();\n", function() {});
            puckConnection.write("LED3.set();\n", function() {});
        }
    }
}

function stopAlerting(interval) {
    console.log("resetting LEDs");
    $("h1").css('background-color', '#fff').css('color', '#000');
    if (puckConnection){
        puckConnection.write("LED1.reset();\n", function() {});
        puckConnection.write("LED3.reset();\n", function() {});
    }
    clearInterval(interval);   
    getData();  
}

function reactToAlert(puckConnection) {  
    console.log("puck connection: " + puckConnection);  
    if (puckConnection){
        puckConnection.write("LED2.reset();\n", function() {});
    }

    var alertToggle = false;
    var started = Date.now();
    // make it loop every 500 milliseconds (ie twice per second)
    var interval = setInterval(function(){      
        // Stop after 5 seconds
        if ((Date.now() - started) > 5000) {
            stopAlerting(interval);
        } else {    
            alertToggle = !alertToggle;
            doAlertFlashing(alertToggle);
        }
    }, 500); // every 500 milliseconds (ie twice per second)
}

function getTimeFromApi() {
    $.ajax({
        type: 'GET',
        url: uri + '/time/' + uniqueId,
        success: function (data) {
            console.log("API TIME " 
                + data.newHours + ":" + data.newMinutes + ":" + data.newSeconds
                + " (won't update until seconds reach :59)");
            newHours = data.newHours;
            newMinutes = data.newMinutes;
            if (firstTime) {
                firstTime = false;
                hours = newHours;
                minutes = newMinutes;
            }
        }
    });
}

function checkForAlerts() {
    $.ajax({
        type: 'GET',
        url: uri + '/alert/' + uniqueId,
        success: function (data) {
            if (data.alert === true) {
                alert = true;
                console.log("ALERT!!! (but won't appear in browser until seconds tick over the minute threshold)");  
            }
        }
    });
}

function tickOverFromOneMinuteToTheNext() {
    seconds = 0;
    // Don't update hours and minutes until seconds tick back over to 0, otherwise
    // you get weird things like moving from 12:34:59 to 12:34:00 (instead of 12:35:00)
    hours = newHours;
    minutes = newMinutes;
    // Only react to alerts in here, so they coincide with the time changing.
    if (alert === true) {
        alert = false;
        reactToAlert(puckConnection);
    }
}

function updateClockDisplay() {
    var formattedHours = formatTimeDisplay(hours);
    var formattedMinutes = formatTimeDisplay(minutes);
    var formattedSeconds = formatTimeDisplay(seconds);
    document.querySelector('.clock').innerHTML = `${formattedHours}:${formattedMinutes}:${formattedSeconds}`;
}

function setTimeTenSecondsBehindSoApiCatchesUp(timeNow) {    
    seconds = timeNow.getSeconds() - 10;
    if (seconds < 0) {
        seconds = seconds + 60;
    }
}

function getRealTime() {
    var timeNow = new Date(); // for now
    hours = timeNow.getHours(); 
    minutes = timeNow.getMinutes();
    setTimeTenSecondsBehindSoApiCatchesUp(timeNow);
    newHours = hours;
    newMinutes = minutes;
    updateClockDisplay(); 
}

function updateClockData() {
    getRealTime();
    getTimeFromApi();
	setInterval(() => { 
        // Only make API calls every 10 seconds
        if (seconds % 10 === 0) {
            checkForAlerts();
            getTimeFromApi();
        }
        // Because of API delays and infrequent API requests, it's better to handle the seconds separately from the hours and minutes.
        seconds = seconds + 1;
        if (seconds === 60) {
            tickOverFromOneMinuteToTheNext();
        }
        updateClockDisplay();
    }, 1000) // The interval goes off once per second, but the API call is made less often (see above).
}

function formatTimeDisplay(number) {
    if(number < 10) {
        number = '0' + number
    }
    return number
}

function addTimeRequest(uniqueId) {
    const item = {
        'userId': uniqueId,
        'lengthInMinutes': document.forms[0].elements['RequestedTimeInMinutes'].value
    };

    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: uri,
        contentType: 'application/json',
        data: JSON.stringify(item),
        error: function (jqXHR, textStatus, errorThrown) {
            alert("'Can't add new items - received error from API (Is the API running?)");
        },
        success: function (result) {
            getData();
            $('#add-name').val('');
        }
    });
}

function closeInput() {
    $('#spoiler').css({ 'display': 'none' });
}