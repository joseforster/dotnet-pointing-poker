"use strict";

var username = document.getElementById("username").textContent;

var connection = new signalR.HubConnectionBuilder().withUrl("/pointingpokerhub?username=" + username).build();

//Disable the send button until connection is established.
//document.getElementById("sendButton").disabled = true;

connection.on("UserHubConnected", function ({ username, connectionId }) {
    console.log("Alguém novo conectou: " + username);
    var li = document.createElement("li");
    document.getElementById("lista-usuarios").appendChild(li);
    li.textContent = username;
    li.id = connectionId;
});

connection.on("UserHubConnectedList", function (users) {
    console.log("Recebi evento UserHubConnectedList " + users);
    users.forEach(user => {
        console.log(user.username);
        var li = document.createElement("li");
        document.getElementById("lista-usuarios").appendChild(li);
        li.textContent = user.username;
        li.id = user.connectionId;
    });
});

connection.on("UserHubDisconnected", function ({ connectionId }) {
    console.log("UserHubDisconnected -->" + connection);
    var element = document.getElementById(connectionId);
    element.remove();
});



connection.start().then(function () {
    //document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

// document.getElementById("sendButton").addEventListener("click", function (event) {
//     var user = document.getElementById("userInput").value;
//     var message = document.getElementById("messageInput").value;
//     connection.invoke("SendMessage", user, message).catch(function (err) {
//         return console.error(err.toString());
//     });
//     event.preventDefault();
// });
