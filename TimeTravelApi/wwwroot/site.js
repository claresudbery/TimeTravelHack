const uri = 'api/moretimerequest';
var time = new Date(); 
var hours; 
var minutes;
var seconds;

$(document).ready(function () {
	getData();
	getTimeToDisplay(this.time);
});

function reactToAlert(puckConnection) {  
    console.log("ALERT!!!");  
    console.log("puck connection: " + puckConnection);  

    var alertToggle = false;
    var heading = document.getElementsByTagName('h1')[0];
    if (puckConnection){
        puckConnection.write("LED2.reset();\n", function() {});
    }

    var started = Date.now();
    // make it loop every 500 milliseconds (ie twice per second)
    var interval = setInterval(function(){      
        // Stop after 5 seconds
        if (Date.now() - started > 5000) {
            console.log("resetting LEDs");
            $("h1").css('background-color', '#fff').css('color', '#000');
            if (puckConnection){
                puckConnection.write("LED1.reset();\n", function() {});
                puckConnection.write("LED3.reset();\n", function() {});
            }
            clearInterval(interval);   
            getData();   
        } else {      
            alertToggle = !alertToggle;
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
    }, 500); // every 500 milliseconds (ie twice per second)
}

function checkForAlert(puckConnection, uniqueId) {
    console.log("Checking for an alert");
    $.ajax({
        type: 'GET',
        url: uri + '/' + uniqueId,
        success: function (data) {
            console.log("API: " + data.alert + ", " + data.newTime);
            if (data.alert === true) {
                reactToAlert(puckConnection);
                time.setMinutes(time.getMinutes() - 20);
            }
            setTimeout(function() {
                checkForAlert(puckConnection, uniqueId);
            }, 5000);
        }
    });
} 

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

function getTimeToDisplay() {
	setInterval(() => {   
   
        var hours;
        var minutes;
        var seconds;
    $.ajax({
        type: 'GET',
        url: uri + '/' + uniqueId,
        success: function (data) {
            console.log('API TIME ' + data.newHours + ":" + data.newMinutes + ":" + data.newSeconds)
            hours = formatTimeDisplay(data.newHours);
            minutes = formatTimeDisplay(data.newMinutes);
            seconds = formatTimeDisplay(data.newSeconds);
            document.querySelector('.clock').innerHTML = `${hours}:${minutes}:${seconds}`;
        }
    })
    }, 1000)

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
        //'lengthInMinutes'
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