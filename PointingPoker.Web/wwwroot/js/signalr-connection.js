class AlwaysRetryReconnectPolicy {
    nextRetryDelayInMilliseconds(retryContext){
        
        let retrySuffix = retryContext.previousRetryCount == 1 ? "try" : "tries";
        
        let msg = `reconnecting (${retryContext.previousRetryCount} ${retrySuffix})`;

        changeConnectionStatus(msg, "text-bg-warning");
        
        return 2000;
    }
}

let connection = new signalR.HubConnectionBuilder()
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
    userListItem.offsetWidth;
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
    voteCard.offsetWidth;
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
    users.forEach(user => {

        if (connection.connectionId !== user.connectionId) {
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

connection.on("UserHasReconnected", function (oldConnectionId, newConnectionId) {
    let userListItem = document.getElementById("user-list-item-" + oldConnectionId);

    userListItem.id = "user-list-item-" + newConnectionId;

    let userVote = document.getElementById("user-vote-" + oldConnectionId);
    userVote.id = "user-vote-" + newConnectionId;
});

connection.on("SessionToWatchError", function (error) {
    alert(error);
})

connection.on("SessionToWatchConnected", function (sessionId) {
    let watchSessionId = document.getElementById("watch-session-id");
    watchSessionId.textContent = `session ${sessionId}`;
    
    let watchSessionVotes = document.getElementById("watch-session-votes");
    watchSessionVotes.textContent = "waiting";
});

connection.on("SetVoteResultOnWatchSession", function (voteModel) {
    let watchSessionVotes = document.getElementById("watch-session-votes");

    if (voteModel.voteResult == "") {
        return;
    }

    watchSessionVotes.textContent = voteModel.voteResult;

    let watchSessionVotesCard = document.getElementById("watch-session-votes-card");

    watchSessionVotesCard.classList.remove(...classList);

    let newClass = mapVoteScaleByClass.get(voteModel.voteScale);
    watchSessionVotesCard.classList.add(newClass);
});

connection.on("ClearVotesOnWatchSession", function () {
    let watchSessionVotes = document.getElementById("watch-session-votes");
    watchSessionVotes.textContent = "waiting";

    let watchSessionVotesCard = document.getElementById("watch-session-votes-card");
    watchSessionVotesCard.classList.remove(...classList);
    watchSessionVotesCard.classList.add("text-bg-light");
    watchSessionVotesCard.classList.add("text-secondary");
})

connection.on("KickedFromSession", async function () {
    await fetch("/?handler=ExitSession", {
        method: "POST",
        headers: {
            "RequestVerificationToken": getCsrfToken()
        },
        credentials: "include"
    }).then(async () => {
        
        await exitSession();
        
        window.location.reload();
    }).catch((error) => {
        alert("Someone tried to kick you from session, but this error occurred: " + error.message);
    });
});

connection.on("UserKickedFromSession", async function (userWhoKicked, userThatWasKicked) {
    alert(`${userWhoKicked} kicked ${userThatWasKicked} from session.`);
})

connection.onclose(function () {
    changeConnectionStatus("disconnected", "text-bg-danger");
});

connection.onreconnected(function () {
    changeConnectionStatus("connected", "text-bg-success");
});

connection.onreconnecting(function () {
    changeConnectionStatus("reconnecting", "text-bg-warning");
});

connection.start();