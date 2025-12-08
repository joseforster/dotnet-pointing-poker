document.getElementsByName("green-vote-button").forEach(btn =>
    btn.addEventListener(("click"), async () => {
        await setVote(btn.textContent, "text-bg-success");
    })
);

document.getElementsByName("yellow-vote-button").forEach(btn =>
    btn.addEventListener(("click"), async () => {
        await setVote(btn.textContent, "text-bg-warning");
    })
);

document.getElementsByName("red-vote-button").forEach(btn =>
    btn.addEventListener(("click"), async () => {
        await setVote(btn.textContent, "text-bg-danger");
    })
);

document.getElementById("empty-vote-button").addEventListener("click", async () => {
    await setVote("?", "text-bg-dark");
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

function exitSession() {
    connection.invoke("ExitSession");
}