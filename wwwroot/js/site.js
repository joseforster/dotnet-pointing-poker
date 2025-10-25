"use strict";

var username = document.getElementById("my-username").textContent;

var connection = new signalR.HubConnectionBuilder().withUrl("/pointingpokerhub?username=" + username).build();

connection.on("UserHubConnected", function (user) {
    addUser(user);
});

connection.on("UserHasVoted", function (connectionId) {

    console.log("user votou --> " + connectionId);
    var span = document.getElementById("vote-" + connectionId);

    span.classList.add("text-bg-primary");
    span.textContent = "OK";

    let li = document.getElementById("username-" + connectionId);

    li.classList.remove("flash");
    void li.offsetWidth; // force reflow to restart animation
    li.classList.add("flash");
})

connection.on("UserHubConnectedList", function (users) {
    users.forEach(user => {
        addUser(user);
    });
});

connection.on("UserHubDisconnected", function (user) {
    console.log("UserHubDisconnected -->" + connection.connectionId);
    var element = document.getElementById("username-" + user.connectionId);
    element.remove();
});

connection.start().then(function () {
}).catch(function (err) {
    return console.error(err.toString());
});

window.addEventListener("beforeunload", () => {
    connection.invoke("OnClosedTheTab", username);
});

document.getElementsByName("vote-button-success").forEach(btn =>
    btn.addEventListener(("click"), function () {
        addVote(btn.textContent, "text-bg-success");
    })
);

document.getElementsByName("vote-button-warning").forEach(btn =>
    btn.addEventListener(("click"), function () {
        addVote(btn.textContent, "text-bg-warning");
    })
);

document.getElementsByName("vote-button-danger").forEach(btn =>
    btn.addEventListener(("click"), function () {
        addVote(btn.textContent, "text-bg-danger");
    })
);

document.getElementById("empty-vote-button").addEventListener("click", function () {
    addVote("?", "text-bg-dark");
});


function addVote(voteValue, className) {
    let list = ["text-bg-success", "text-bg-warning", "text-bg-danger", "text-bg-dark"];

    let myVote = document.getElementById("my-vote");

    myVote.classList.remove(...list);
    myVote.classList.add(className);
    myVote.textContent = voteValue;

    connection.invoke("OnUserVoted", voteValue);
}

function addUser(user) {

    console.log("Alguém novo conectou: " + user.username);

    var li = document.createElement("li");

    li.classList.add("list-group-item");
    li.classList.add("d-flex");
    li.classList.add("justify-content-between");
    li.classList.add("align-items-center");
    li.textContent = user.username;
    li.id = "username-" + user.connectionId;

    document.getElementById("user-list").appendChild(li);

    var span = document.createElement("span");
    span.classList.add("badge");
    span.classList.add("big-badge");
    span.id = "vote-" + user.connectionId;

    if (user.hasVoted) {
        span.classList.add("text-bg-primary");
        span.textContent = "OK";
    }

    li.appendChild(span);
}