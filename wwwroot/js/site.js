"use strict";

let voteButtonClassList = ["text-bg-success", "text-bg-warning", "text-bg-danger", "text-bg-dark", "text-bg-primary"];

var username = document.getElementById("my-username").textContent;

var connection = new signalR.HubConnectionBuilder().withUrl("/pointingpokerhub?username=" + username).build();

connection.on("UserConnected", function (user) {
    addUser(user);
});

connection.on("UserHasVoted", function (connectionId) {

    console.log("user votou --> " + connectionId);
    var userVote = document.getElementById("user-vote-" + connectionId);

    userVote.classList.add("text-bg-primary");
    userVote.textContent = "OK";

    let userListItem = document.getElementById("user-list-item-" + connectionId);

    userListItem.classList.remove("flash");
    void userListItem.offsetWidth;
    userListItem.classList.add("flash");
})

connection.on("UserHasVotedWithShowedVotes", function (user) {
    console.log("Votos abertos: " + user.username + " -- > " + user.currentVote);

    var userVote = document.getElementById("user-vote-" + user.connectionId);

    userVote.classList.add("text-bg-primary");
    userVote.textContent = user.currentVote;

    let userListItem = document.getElementById("user-list-item-" + user.connectionId);

    userListItem.classList.remove("flash");
    void userListItem.offsetWidth;
    userListItem.classList.add("flash");
});

connection.on("SetUserList", function (users) {
    users.forEach(user => {
        addUser(user);
    });
});

connection.on("UserDisconnected", function (user) {
    console.log("UserDisconnected -->" + connection.connectionId);
    var element = document.getElementById("user-list-item-" + user.connectionId);
    element.remove();
});

connection.on("ShowVotes", function (users) {
    console.log("Show Votes!");

    var myConnectionId = document.getElementById("my-connection-id").getAttribute("value");

    users.forEach(user => {

        if (myConnectionId != user.connectionId) {
            let userVote = document.getElementById("user-vote-" + user.connectionId);

            userVote.textContent = user.currentVote;
        }
    });
});

connection.on("ClearVotes", function () {
    console.log("Clear Votes!");

    let userVoteList = document.getElementsByName("user-vote");
    userVoteList.forEach(userVote => {

        userVote.classList.remove(...voteButtonClassList);

        userVote.textContent = "";
    });

    let myVoteSpan = document.getElementById("my-vote");
    myVoteSpan.textContent = "";
});

connection.start().then(function () {
    var myConnectionId = document.getElementById("my-connection-id");
    myConnectionId.setAttribute("value", connection.connectionId);
}).catch(function (err) {
    return alert(err.toString());
});

window.addEventListener("beforeunload", () => {
    connection.invoke("OnClosedTheTab", username);
});

document.getElementsByName("green-vote-button").forEach(btn =>
    btn.addEventListener(("click"), function () {
        setVote(btn.textContent, "text-bg-success");
    })
);

document.getElementsByName("yellow-vote-button").forEach(btn =>
    btn.addEventListener(("click"), function () {
        setVote(btn.textContent, "text-bg-warning");
    })
);

document.getElementsByName("red-vote-button").forEach(btn =>
    btn.addEventListener(("click"), function () {
        setVote(btn.textContent, "text-bg-danger");
    })
);

document.getElementById("empty-vote-button").addEventListener("click", function () {
    setVote("?", "text-bg-dark");
});

document.getElementById("show-votes-button").addEventListener("click", function () {
    connection.invoke("OnShowVotes");
});

document.getElementById("clear-votes-button").addEventListener("click", function () {
    connection.invoke("OnClearVotes");
});

function setVote(voteValue, className) {

    let myVote = document.getElementById("my-vote");

    myVote.classList.remove(...voteButtonClassList);
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
    li.setAttribute("name", "user-list-item");
    li.textContent = user.username;
    li.id = "user-list-item-" + user.connectionId;

    document.getElementById("user-list").appendChild(li);

    var span = document.createElement("span");
    span.classList.add("badge");
    span.classList.add("big-badge");
    span.setAttribute("name", "user-vote");
    span.id = "user-vote-" + user.connectionId;

    if (user.hasVoted) {
        span.classList.add("text-bg-primary");
        span.textContent = "OK";
    }

    li.appendChild(span);
}