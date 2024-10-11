<template>
    <div class="chat-area">
        <div v-if="!isConnected" class="connection-status">
            Connecting to chat...
        </div>
        <div v-if="isConnected" class="messages">
            <div v-for="(message, index) in messages" :key="index" class="message">
                <strong>{{ message.sender }}:</strong> {{ message.content }}
            </div>
            <input type="text"
                   v-model="newMessage"
                   @keyup.enter="sendMessage"
                   placeholder="Skriv ett meddelande..." />
            <button @click="sendMessage">Skicka</button>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue';
    import * as signalR from '@microsoft/signalr';

    const props = defineProps({
        username: String // Tar emot användarnamnet från ChatWidget
    });

    const messages = ref([]);  // Lista över alla meddelanden
    const newMessage = ref('');  // Nytt meddelande som skrivs av användaren
    const connection = ref(null);  // SignalR-anslutning
    const isConnected = ref(false);  // Status för anslutningen

    // Hämta meddelanden från sessionStorage när chatten laddas
    onMounted(async () => {
        const savedMessages = JSON.parse(sessionStorage.getItem('chatMessages') || '[]');
        messages.value = savedMessages;

        connection.value = new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7065/chat-hub', {})
            .build();

        connection.value.on('ReceiveMessage', (message, sender) => {
            messages.value.push({ sender, content: message });
            saveMessagesToSessionStorage();  // Spara meddelanden i sessionStorage när nya tas emot
        });

        try {
            await connection.value.start();
            isConnected.value = true;  // Sätt statusen till ansluten
            console.log('SignalR connection established.');
        } catch (err) {
            console.error('Failed to connect to SignalR: ', err);
        }
    });

    // Funktion för att spara meddelanden i sessionStorage
    const saveMessagesToSessionStorage = () => {
        sessionStorage.setItem('chatMessages', JSON.stringify(messages.value));
    };

    // Funktion för att skicka meddelanden
    const sendMessage = async () => {
        if (!isConnected.value) {
            console.error("Cannot send message because SignalR is not connected.");
            return;
        }

        if (newMessage.value.trim() !== '') {
            try {
                await connection.value.invoke('SendMessage', newMessage.value, props.username); // Skicka meddelandet med användarnamnet
                newMessage.value = '';  // Rensa meddelandefältet efter att meddelandet skickats
                saveMessagesToSessionStorage();  // Spara meddelandena i sessionStorage
            } catch (err) {
                console.error('Failed to send message: ', err);
            }
        }
    };
</script>

<style scoped>
    .chat-area {
        flex: 1;
        display: flex;
        flex-direction: column;
    }

    .messages {
        flex: 1;
        overflow-y: auto;
    }

    .message {
        margin-bottom: 10px;
    }

    .connection-status {
        color: gray;
        font-size: 14px;
    }
</style>
