import {setVote} from "./shared.js";
import {connection} from "./signalr-connection.js";

document.getElementsByName("green-vote-button").forEach(btn =>
    btn.addEventListener(("click"), async () => {
        setVote(btn.textContent, "text-bg-success");
        await connection.invoke("OnUserVoted", btn.textContent);
    })
);

document.getElementsByName("yellow-vote-button").forEach(btn =>
    btn.addEventListener(("click"), async () => {
        setVote(btn.textContent, "text-bg-warning");
        await connection.invoke("OnUserVoted", btn.textContent);
    })
);

document.getElementsByName("red-vote-button").forEach(btn =>
    btn.addEventListener(("click"), async () => {
        setVote(btn.textContent, "text-bg-danger");
        await connection.invoke("OnUserVoted", btn.textContent);
    })
);

document.getElementById("empty-vote-button").addEventListener("click", async () => {
    setVote("?", "text-bg-dark");
    await connection.invoke("OnUserVoted", btn.textContent);
});

document.getElementById("show-votes-button").addEventListener("click", async () => {
    await connection.invoke("OnShowVotes");
});

document.getElementById("clear-votes-button").addEventListener("click", async () => {
    await connection.invoke("OnClearVotes");
});

window.addEventListener("beforeunload", async () => {
    await connection.invoke("ExitSession");
});

export function exitSession() {
    connection.invoke("ExitSession");
}