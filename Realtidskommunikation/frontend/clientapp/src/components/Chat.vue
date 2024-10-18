<template>
    <div class="chat-area">
        <!-- Flikbar för att växla mellan chattar -->
        <div class="tab-bar">
            <button :class="{ active: activeTab === 'all' }" @click="activeTab = 'all'">Chatta med alla</button>
            <button :class="{ active: activeTab === 'admin' }" @click="activeTab = 'admin'">
                {{ isAdmin ? 'Hantera användare' : 'Chatta med Admin' }}
            </button>
        </div>

        <!-- Offentlig chatt -->
        <div v-if="activeTab === 'all' && isConnected" class="public-chat">
            <h4>Offentlig Chatt (Alla användare):</h4>
            <div v-for="(message, index) in messages" :key="index" class="message">
                <strong>{{ message.sender }}:</strong> {{ message.content }}
            </div>
            <input type="text" v-model="newMessage" @keyup.enter="sendPublicMessage" placeholder="Skriv ett meddelande..." />
            <button @click="sendPublicMessage">Skicka till alla</button>
        </div>

        <!-- Privat chatt för vanliga användare med Admin -->
        <div v-if="!isAdmin && activeTab === 'admin' && isConnected">
            <h4>Privat Chatt med Admin:</h4>
            <div class="messages">
                <div v-for="(message, index) in privateMessages" :key="index" class="message">
                    <strong>{{ message.sender }}:</strong> {{ message.content }}
                </div>
            </div>
            <input type="text" v-model="newPrivateMessage" @keyup.enter="sendPrivateMessageToAdmin" placeholder="Skriv ett meddelande till Admin..." />
            <button @click="sendPrivateMessageToAdmin">Skicka till Admin</button>
        </div>
        <button v-if="isAdmin" @click="emit('new-private-message', 'TestUser')">Testa skapa chatwidget</button>

        <div v-if="!isConnected" class="connection-status">Connecting to chat...</div>
    </div>
</template>

<script setup>
    import { ref, onMounted, defineEmits } from 'vue';
    import * as signalR from '@microsoft/signalr';

    const props = defineProps({ username: String });
    const emit = defineEmits(['new-private-message']);

    // Data
    const messages = ref([]);
    const privateMessages = ref([]);
    const newMessage = ref('');
    const newPrivateMessage = ref('');
    const connection = ref(null);
    const isConnected = ref(false);
    const activeTab = ref('all');
    const isAdmin = ref(props.username === 'Admin');

    // Anslut till SignalR och hantera inkommande meddelanden
    onMounted(async () => {
        connection.value = new signalR.HubConnectionBuilder()
            .withUrl(`https://localhost:7065/chat-hub?username=${encodeURIComponent(props.username)}`)
            .build();

        // Lyssna på offentliga meddelanden
        connection.value.on('ReceiveMessage', (message, sender) => {
            messages.value.push({ sender, content: message });
            scrollToBottomPublicChat();
        });

        // Lyssna på privata meddelanden
        connection.value.on('ReceivePrivateMessage', (message, sender) => {
            if (isAdmin.value) {
                console.log(`Admin received a private message from ${sender}: ${message}`);
                // Emitera händelse för Admin att skapa en ny widget för användaren
                emit('new-private-message', sender, message);
            } else {
                // Vanliga användare tar emot sina egna meddelanden
                console.log(`User received a private message from ${sender}: ${message}`); // Debug-logg
                privateMessages.value.push({ sender, content: message });
                scrollToBottomPrivateChat();
            }
        });

        try {
            await connection.value.start();
            isConnected.value = true;
        } catch (err) {
            console.error('Failed to connect to SignalR:', err);
        }
    });

    // Offentlig chatt-meddelande
    const sendPublicMessage = async () => {
        if (newMessage.value.trim() !== '') {
            await connection.value.invoke('SendMessage', newMessage.value, props.username);
            messages.value.push({ sender: props.username, content: newMessage.value });
            newMessage.value = '';
            scrollToBottomPublicChat();
        }
    };

    // Privat meddelande till Admin
    const sendPrivateMessageToAdmin = async () => {
        if (newPrivateMessage.value.trim() !== '') {
            await connection.value.invoke('SendMessageToAdmin', newPrivateMessage.value, props.username);
            privateMessages.value.push({ sender: props.username, content: newPrivateMessage.value });
            newPrivateMessage.value = '';
            scrollToBottomPrivateChat();
        }
    };

    // Skrollfunktioner
    const scrollToBottomPublicChat = () => {
        const chatArea = document.querySelector('.public-chat .messages');
        if (chatArea) {
            chatArea.scrollTop = chatArea.scrollHeight;
        }
    };

    const scrollToBottomPrivateChat = () => {
        const chatArea = document.querySelector('.messages');
        if (chatArea) {
            chatArea.scrollTop = chatArea.scrollHeight;
        }
    };

</script>



<style scoped>
    .chat-area {
        flex: 1;
        display: flex;
        flex-direction: column;
    }

    .tab-bar {
        display: flex;
        justify-content: space-around;
        margin-bottom: 10px;
    }

        .tab-bar button {
            padding: 10px;
            background-color: #f0f0f0;
            border: 1px solid #ccc;
            cursor: pointer;
        }

            .tab-bar button.active {
                background-color: #007bff;
                color: white;
            }

    .messages {
        flex: 1;
        overflow-y: auto;
        max-height: 400px;
        padding: 10px;
        border: 1px solid #ddd;
        border-radius: 5px;
        background-color: #f9f9f9;
    }

    .message {
        margin-bottom: 10px;
    }

    .connection-status {
        color: gray;
        font-size: 14px;
    }
</style>