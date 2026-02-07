let classList = ["text-bg-success", "text-bg-warning", "text-bg-danger", "text-bg-dark", "text-bg-primary", "text-bg-light", "text-secondary"];

let mapVoteScaleByClass = new Map([
    [0, "text-bg-light"],
    [1, "text-bg-success"],
    [2, "text-bg-warning"],
    [3, "text-bg-danger"],
    [4, "text-bg-dark"]
]);

function changeConnectionStatus(text, className) {
    
    let connectionStatus =  document.getElementById("my-connection-status");
    
    connectionStatus.classList.remove(...classList);
    connectionStatus.classList.add(className);
    connectionStatus.textContent = text;
}

async function setVote(voteValue, className) {

    let myVote = document.getElementById("my-vote");

    myVote.classList.remove(...classList);
    myVote.classList.add(className);

    myVote.textContent = voteValue;

    await connection.invoke("OnUserVoted", voteValue);
}

function addUser(user, areVotesBeingShowed) {
    
    let userListItemId = "user-list-item-" + user.connectionId;
    let userVoteId = "user-vote-" + user.connectionId;

    let userListItem = document.getElementById(userListItemId);

    let userVote;

    if (!userListItem) {
        userListItem = document.createElement("li");

        userListItem.classList.add("list-group-item");
        userListItem.classList.add("d-flex");
        userListItem.classList.add("justify-content-between");
        userListItem.classList.add("align-items-center");
        userListItem.setAttribute("name", "user-list-item");
        userListItem.id = userListItemId;

        let spanUsername = document.createElement("span");
        spanUsername.id = "username-" + user.connectionId;
        spanUsername.textContent = user.username;

        userListItem.appendChild(spanUsername);

        document.getElementById("user-list").appendChild(userListItem);

        userVote = document.createElement("span");
        userVote.classList.add("badge");
        userVote.classList.add("big-badge");
        userVote.setAttribute("name", "user-vote");
        userVote.id = userVoteId;

        userListItem.appendChild(userVote);
    }else{
        userVote = document.getElementById(userVoteId);
    }
    
    userListItem.addEventListener("dblclick", async function () {
        const ok = confirm("Do you want to kick this user?");

        if (ok) {
            await connection.invoke("KickUserFromSession", user.connectionId);
        }
    });

    if (user.hasVoted) {
        if (areVotesBeingShowed) {
            userVote.textContent = user.currentVote;
            userVote.classList.add(mapVoteScaleByClass.get(user.voteScale));
        } else {
            userVote.classList.add("text-bg-primary");
            userVote.textContent = "OK";
        }
    }
}

function getCsrfToken() {
    return document.querySelector(
        'input[name="__RequestVerificationToken"]'
    ).value;
}