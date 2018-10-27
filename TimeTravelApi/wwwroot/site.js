const uri = 'api/moretimerequest';
let todos = null;
function getCount(data) {
    const el = $('#counter');
    let name = 'to-do';
    if (data) {
        if (data > 1) {
            name = 'to-dos';
        }
        el.text(data + ' ' + name);
    } else {
        el.html('No ' + name);
    }
}

$(document).ready(function () {
    getData();
});

function reactToAlert(puckConnection) {  
    console.log("ALERT!!!");  
    console.log("puck connection: " + puckConnection);  

    var alertToggle = false;
    var path = document.getElementsByTagName('svg')[0];
    puckConnection.write("LED2.reset();\n", function() {});
    addItem("ALERT!!");

    var started = Date.now();
    // make it loop every 500 milliseconds (ie twice per second)
    var interval = setInterval(function(){      
        // Stop after 5 seconds
        if (Date.now() - started > 5000) {
            console.log("resetting LEDs");
            path.style.fill="rgb(200,0,200)";
            puckConnection.write("LED1.reset();\n", function() {});
            puckConnection.write("LED3.reset();\n", function() {});
            clearInterval(interval);      
        } else {      
            alertToggle = !alertToggle;
            console.log("flashing colours after alert");
            if (alertToggle) {
                path.style.fill="rgb(0,150,150)";
                puckConnection.write("LED3.reset();\n", function() {});
                puckConnection.write("LED1.set();\n", function() {});
            } else {
                path.style.fill="rgb(0,150,0)";
                puckConnection.write("LED1.reset();\n", function() {});
                puckConnection.write("LED3.set();\n", function() {});
            }
        }
    }, 500); // every 500 milliseconds (ie twice per second)
}

function checkForAlert(puckConnection) {
    console.log("Checking for an alert");
    $.ajax({
        type: 'GET',
        url: uri + '/alertquery',
        success: function (data) {
            console.log("API: " + data);
            if (data === "alert") {
                reactToAlert(puckConnection);
            }
            setTimeout(function() {
                checkForAlert(puckConnection);
            }, 1000);
        }
    });
} 

function getData() {
    $.ajax({
        type: 'GET',
        url: uri,
        success: function (data) {
            $('#todos').empty();
            getCount(data.length);
            $.each(data, function (key, item) {
                const checked = item.expired ? 'checked' : '';

                $('<tr><td><input disabled="true" type="checkbox" ' + checked + '></td>' +
                    '<td>' + item.requestTimeStamp + '</td>' +
                    '</tr>').appendTo($('#todos'));
            });

            todos = data;
        }
    });
}

function addItem() {

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

$('.my-form').on('submit', function () {
    const item = {
        'name': $('#edit-name').val(),
        'isComplete': $('#edit-isComplete').is(':checked'),
        'id': $('#edit-id').val()
    };

    $.ajax({
        url: uri + '/' + $('#edit-id').val(),
        type: 'PUT',
        accepts: 'application/json',
        contentType: 'application/json',
        data: JSON.stringify(item),
        success: function (result) {
            getData();
        }
    });

    closeInput();
    return false;
});

function closeInput() {
    $('#spoiler').css({ 'display': 'none' });
}