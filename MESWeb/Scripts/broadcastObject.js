$(function () {
    // Reference the auto-generated proxy for the hub.  
    var notification = $.connection.notificationHub;
    // Create a function that the hub can call back to display messages.
    
    // DISPLAY STATUS OF DEVICE
    notification.client.addNewMessageToPage = function (mac, machine, atom, worker, state, message) {

        var _title = 'Device State: ' + message.toUpperCase();
        var _msg = '<b>MAC address:</b> ' + mac +
            '<br><b>Machine Id:</b> ' + machine +
            '<br><b>ATOM:</b> ' + atom;

        abNotify(_title, _msg, false);

        var _html =
            '<tr class="odd gradeX">' +
                '<td>' + mac + '</td>' +
                '<td>' + machine + '</td>' +
                '<td>' + atom + '</td>' +
                '<td>' + worker + '</td>' +
                '<td>' + message + '</td>' +
            '</tr>';
        $('#notificationModalContent tbody').append(_html);

    };

    // DISPLAY MESSAGE FROM SERVER
    notification.client.showMessageServer = function (company, factory, message) {
        var _title = 'Message From Server';
        var _msg = '<b>Company:</b> ' + company + '<br><b>Factory:</b> ' + factory + '<br><b>Message:</b> ' + message;

        abNotify(_title, _msg, false);

        var msgItem = '<li class="clearfix"><div class="txt"><span class="label label-info">' + company + ' - ' + factory + '</span>&nbsp;&nbsp;' + message + '</div></li>';
        $('#messageServerModalContent ul').append(msgItem);
    };

    //notification.client.showMessageServer1 = function (message) {

    //};

    $.connection.hub.start().done(function () {
        //REGISTER EVENT TO CALL SERVER HERE
        //$('#sendmessage').click(function () {
        //});
    });
});