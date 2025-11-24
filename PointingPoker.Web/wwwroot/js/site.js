"use strict";

var classList = ["text-bg-success", "text-bg-warning", "text-bg-danger", "text-bg-dark", "text-bg-primary", "text-bg-light", "text-secondary"];

var mapVoteScaleByClass = new Map();
mapVoteScaleByClass.set(0, "text-bg-light");
mapVoteScaleByClass.set(1, "text-bg-success");
mapVoteScaleByClass.set(2, "text-bg-warning");
mapVoteScaleByClass.set(3, "text-bg-danger");
mapVoteScaleByClass.set(4, "text-bg-dark");

class AlwaysRetryReconnectPolicy {
    nextRetryDelayInMilliseconds(retryContext) {
        return 5000;
    }
}

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/pointingpokerhub")
    .withAutomaticReconnect(new AlwaysRetryReconnectPolicy())
    .build();

connection.on("NewUserHasConnected", function (user) {
    addUser(user);
});

connection.on("UserHasVoted", function (connectionId) {

    let userVote = document.getElementById("user-vote-" + connectionId);

    userVote.classList.add("text-bg-primary");
    userVote.textContent = "OK";

    let userListItem = document.getElementById("user-list-item-" + connectionId);

    userListItem.classList.remove("flash");
    void userListItem.offsetWidth;
    userListItem.classList.add("flash");
})

connection.on("UserHasVotedWithShowedVotes", function (user) {

    let userVote = document.getElementById("user-vote-" + user.connectionId);

    userVote.classList.remove(...classList);

    var newClass = mapVoteScaleByClass.get(user.voteScale);
    userVote.classList.add(newClass);
    userVote.textContent = user.currentVote;

    let userListItem = document.getElementById("user-list-item-" + user.connectionId);

    userListItem.classList.remove("flash");
    void userListItem.offsetWidth;
    userListItem.classList.add("flash");
});

connection.on("SetVoteResult", function (voteModel) {
    let voteResult = document.getElementById("vote-result");

    if (voteModel.voteResult == "") {
        return;
    }

    voteResult.textContent = voteModel.voteResult;

    let voteCard = document.getElementById("vote-card");

    voteCard.classList.remove(...classList);

    var newClass = mapVoteScaleByClass.get(voteModel.voteScale);
    voteCard.classList.add(newClass);

    voteCard.classList.remove("flash");
    void voteCard.offsetWidth;
    voteCard.classList.add("flash");
});

connection.on("SetUserList", function (users, areVotesBeingShowed) {
    users.forEach(user => {
        addUser(user, areVotesBeingShowed);
    });
});

connection.on("UserDisconnected", function (user) {
    let element = document.getElementById("user-list-item-" + user.connectionId);
    element.remove();
});

connection.on("ShowVotes", function (users) {
    let myConnectionId = document.getElementById("my-connection-id").getAttribute("value");

    users.forEach(user => {

        if (myConnectionId != user.connectionId) {
            let userVote = document.getElementById("user-vote-" + user.connectionId);

            userVote.textContent = user.currentVote;
            userVote.classList.remove(...classList);
            userVote.classList.add(mapVoteScaleByClass.get(user.voteScale));
        }
    });
});

connection.on("ClearVotes", function () {

    let userVoteList = document.getElementsByName("user-vote");
    
    userVoteList.forEach(userVote => {
        userVote.classList.remove(...classList);
    });

    let myVoteSpan = document.getElementById("my-vote");
    myVoteSpan.textContent = "--";
    myVoteSpan.classList.remove(...classList);
    myVoteSpan.classList.add("text-secondary");

    let voteResult = document.getElementById("vote-result");
    voteResult.textContent = "waiting";

    let voteCard = document.getElementById("vote-card");
    voteCard.classList.remove(...classList);
    voteCard.classList.add("text-bg-light");
    voteCard.classList.add("text-secondary");
});

connection.on("UserOffline", function (user) {
    
    let userListItem = document.getElementsByName("user-list-item-" + user.connectionId);
    userListItem.classList.add("bg-danger-subtle");
    
    let username = document.getElementById("username-" + user.connectionId);
    username.textContent += " (OFFLINE)";
});

connection.start().then(function () {
    let myConnectionId = document.getElementById("my-connection-id");
    myConnectionId.setAttribute("value", connection.connectionId);
}).catch(function (err) {
    return alert(err.toString());
});

window.addEventListener("beforeunload", () => {
    connection.invoke("ExitSession");
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

    myVote.classList.remove(...classList);
    myVote.classList.add(className);

    myVote.textContent = voteValue;

    connection.invoke("OnUserVoted", voteValue);
}

function addUser(user, areVotesBeingShowed) {

    let listItemId = "user-list-item-" + user.connectionId;

    let userListItem = document.getElementById(listItemId);
    
    if (!userListItem) {
        let listItem = document.createElement("li");

        listItem.classList.add("list-group-item");
        listItem.classList.add("d-flex");
        listItem.classList.add("justify-content-between");
        listItem.classList.add("align-items-center");
        listItem.setAttribute("name", "user-list-item");
        listItem.id = listItemId;

        let spanUsername = document.createElement("span");
        spanUsername.id = "username-" + user.connectionId;
        spanUsername.textContent = user.username;

        listItem.appendChild(spanUsername);

        document.getElementById("user-list").appendChild(listItem);

        let spanUserVote = document.createElement("span");
        spanUserVote.classList.add("badge");
        spanUserVote.classList.add("big-badge");
        spanUserVote.setAttribute("name", "user-vote");
        spanUserVote.id = "user-vote-" + user.connectionId;

        if (user.hasVoted) {
            if (areVotesBeingShowed) {
                spanUserVote.textContent = user.currentVote;
                spanUserVote.classList.add(mapVoteScaleByClass.get(user.voteScale));
            } else {
                spanUserVote.classList.add("text-bg-primary");
                spanUserVote.textContent = "OK";
            }
        }

        listItem.appendChild(spanUserVote);
    }else if(userListItem.classList.contains("bg-danger-subtle")) {
        userListItem.classList.remove("bg-danger-subtle");

        let spanUsername = document.getElementById("username-" + user.connectionId);
        spanUsername.textContent = user.usernamespanUsername.textContent.replace("(OFFLINE)", "");
    }
}

function exitSession() {
    connection.invoke("ExitSession");
}