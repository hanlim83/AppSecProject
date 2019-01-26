"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();

connection.on("ReceiveMessage", function ( message) {
    var msg = message.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    var encodedMsg = msg;
    var ul = document.getElementById("messageList");
    var li = document.createElement("li");
    li.textContent = encodedMsg;
    ul.appendChild(li);
   //document.getElementById("messagesList")
    document.getElementById('messageInput').value = "";
});

connection.start().catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
   
    var message = document.getElementById("messageInput").value;
    connection.invoke("SendMessage", message).catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

//group msg ?
document.getElementById("groupmsg").addEventListener("click", async (event) => {
    var groupName = document.getElementById("group-name").value;
    var groupMsg = document.getElementById("group-message-text").value;
    try {
        await connection.invoke("SendMessageToGroup", groupName, groupMsg);
    }
    catch (e) {
        console.error(e.toString());
    }
    event.preventDefault();
});
document.getElementById("join-group").addEventListener("click", async (event) => {
    var groupName = document.getElementById("group-name").value;
    try {
        await connection.invoke("AddToGroup", groupName);
    }
    catch (e) {
        console.error(e.toString());
    }
    event.preventDefault();
});
document.getElementById("leave-group").addEventListener("click", async (event) => {
    var groupName = document.getElementById("group-name").value;
    try {
        await connection.invoke("RemoveFromGroup", groupName);
    }
    catch (e) {
        console.error(e.toString());
    }
    event.preventDefault();
});