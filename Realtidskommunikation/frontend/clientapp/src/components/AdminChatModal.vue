<template>
    <div>
        <h2>Admin Chat Hantering</h2>
        <!-- Här visar vi alla chattmodals som öppnas dynamiskt för varje användare -->
        <div v-for="(user, index) in privateUsers" :key="index" class="chat-modal">
            <h3>Chatta med {{ user }}</h3>
            <div class="messages">
                <div v-for="(message, index) in userChats[user]" :key="index" class="message">
                    <strong>{{ message.sender }}:</strong> {{ message.content }}
                </div>
            </div>
            <input type="text" v-model="newPrivateMessages[user]" @keyup.enter="sendPrivateMessage(user)" placeholder="Skriv ett meddelande till {{ user }}" />
            <button @click="sendPrivateMessage(user)">Skicka till {{ user }}</button>
        </div>
    </div>
</template>

<script setup>
import { ref, onMounted } from 'vue';
import * as signalR from '@microsoft/signalr';

const props = defineProps({ username: String });

const privateUsers = ref([]);  // Lista över alla användare som har skickat privata meddelanden
const userChats = ref({});  // En objekt där varje användare har sina meddelanden
const newPrivateMessages = ref({});  // En objekt där vi sparar input för nya meddelanden
const connection = ref(null);
const isConnected = ref(false);

// Anslut till SignalR
onMounted(async () => {
  connection.value = new signalR.HubConnectionBuilder()
    .withUrl(`https://localhost:7065/chat-hub?username=${encodeURIComponent(props.username)}`)
    .build();

  connection.value.on('ReceivePrivateMessage', (message, sender) => {
    // Om Admin får ett privat meddelande, öppna en ny chattmodal om den inte redan finns
    if (!privateUsers.value.includes(sender)) {
      privateUsers.value.push(sender);  // Lägg till användaren i listan av privata användare
      userChats.value[sender] = [];  // Initiera meddelandelistan för den nya användaren
      newPrivateMessages.value[sender] = '';  // Initiera inputfältet för nya meddelanden
    }

    // Lägg till meddelandet i den specifika användarens chattmodal
    userChats.value[sender].push({ sender, content: message });
    scrollToBottom(sender);
  });

  try {
    await connection.value.start();
    isConnected.value = true;
  } catch (err) {
    console.error('Failed to connect to SignalR:', err);
  }
});

// Skicka ett privat meddelande till en specifik användare
const sendPrivateMessage = async (user) => {
  if (newPrivateMessages.value[user].trim() !== '') {
    await connection.value.invoke('SendMessageToAdmin', newPrivateMessages.value[user], props.username);
    userChats.value[user].push({ sender: 'Admin', content: newPrivateMessages.value[user] });
    newPrivateMessages.value[user] = '';  // Rensa inputfältet efter skickat meddelande
    scrollToBottom(user);
  }
};

// Skrolla till botten av chatten för en specifik användare
const scrollToBottom = (user) => {
  const chatArea = document.querySelector(`.chat-modal .messages[data-user="${user}"]`);
  if (chatArea) {
    chatArea.scrollTop = chatArea.scrollHeight;
  }
};
</script>

<style scoped>
    .chat-modal {
        border: 1px solid #ccc;
        padding: 10px;
        margin-bottom: 20px;
        width: 300px;
        background-color: white;
    }

    .messages {
        max-height: 200px;
        overflow-y: auto;
        border: 1px solid #ddd;
        padding: 10px;
        background-color: #f9f9f9;
    }

    .message {
        margin-bottom: 10px;
    }
</style>
