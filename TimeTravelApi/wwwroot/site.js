const uri = 'api/moretimerequest';

$(document).ready(function () {
    getData();
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

function checkForAlert(puckConnection) {
    console.log("Checking for an alert");
    $.ajax({
        type: 'GET',
        url: uri + '/true',
        success: function (data) {
            console.log("API: " + data);
            if (data === true) {
                reactToAlert(puckConnection);
            }
            setTimeout(function() {
                checkForAlert(puckConnection);
            }, 20000);
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
                    '</tr>').appendTo($('#timerequests'));
            });
        }
    });
}

function addTimeRequest() {

    $.ajax({
        type: 'POST',
        accepts: 'application/json',
        url: uri,
        contentType: 'application/json',
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