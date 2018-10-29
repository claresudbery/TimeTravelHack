const uri = 'api/moretimerequest';
var hours;
var minutes;
var seconds;
var newHours;
var newMinutes;
var alert = false;
var dataReceivedFromAPI = false;

$(document).ready(function () {
    getData();
    seconds = 0;
	updateClockData();
});

function getData() {
    $.ajax({
        type: 'GET',
        url: uri,
        success: function (data) {
            $('#timerequests').empty();
            $.each(data, function (key, item) {
                const checked = item.expired ? 'checked' : '';

                $('<tr><td><input disabled="true" type="checkbox" ' + checked + '></td>' +
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

function getTimeAndAlertData() {
    $.ajax({
        type: 'GET',
        url: uri + '/' + uniqueId,
        success: function (data) {
            if(puckConnection != null) {
                if (data.alert === true) {
                    alert = true;
                    console.log("ALERT!!! (but won't appear in browser until seconds tick over the minute threshold)");  
                }
            }
            console.log('API TIME ' + data.newHours + ":" + data.newMinutes + ":" + data.newSeconds);
            newHours = data.newHours;
            newMinutes = data.newMinutes;
            if (!dataReceivedFromAPI) {
                dataReceivedFromAPI = true;
                hours = newHours;
                minutes = newMinutes;
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

function updateClockData() {
	setInterval(() => { 
        // Only make API calls every 20 seconds
        if (seconds === 0 || seconds === 20 || seconds === 40)
        {
            getTimeAndAlertData();
        }
        // Because of API delays and infrequent API requests, it's better to handle the seconds separately from the hours and minutes.
        seconds = seconds + 1;
        if (seconds === 60) {
            tickOverFromOneMinuteToTheNext();
        }
        if (dataReceivedFromAPI) {
            updateClockDisplay();
        }
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