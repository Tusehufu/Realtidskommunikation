<template>
    <div class="chat-area">
        <!-- Flikbar för att växla mellan chattar -->
        <div class="tab-bar">
            <button :class="{ active: activeTab === 'all' }" @click="activeTab = 'all'">Chatta med alla</button>
            <button :class="{ active: activeTab === 'admin' }" @click="activeTab = 'admin'">
                {{ isAdmin ? 'Hantera användare' : 'Chatta med Admin' }}
            </button>
        </div>

        <!-- Innehåll för vald flik -->
        <div class="messages" v-if="isConnected">
            <!-- Offentlig chatt -->
            <div v-if="activeTab === 'all'">
                <h4>Offentlig Chatt (Alla användare):</h4>
                <div v-for="(message, index) in messages" :key="index" class="message">
                    <strong>{{ message.sender }}:</strong> {{ message.content }}
                </div>
                <input type="text" v-model="newMessage" @keyup.enter="sendPublicMessage" placeholder="Skriv ett meddelande..." />
                <button @click="sendPublicMessage">Skicka till alla</button>
            </div>

            <!-- Privat chatt med Admin eller hantering av användare (för admin) -->
            <div v-if="activeTab === 'admin'">
                <h4 v-if="isAdmin">Hantera användare:</h4>
                <h4 v-else>Privat Chatt med Admin:</h4>

                <div v-if="isAdmin">
                    <!-- Om admin är inloggad, visa listan över användare som skickat privata meddelanden -->
                    <div v-for="(user, index) in privateUsers" :key="index" class="user-tab" @click="selectPrivateChat(user)">
                        {{ user }}
                    </div>
                </div>

                <div v-else>
                    <!-- Om användare är inloggad, visa privata meddelanden med Admin -->
                    <div v-for="(message, index) in privateMessages" :key="index" class="message">
                        <strong>{{ message.sender }}:</strong> {{ message.content }}
                    </div>
                    <input type="text" v-model="newPrivateMessage" @keyup.enter="sendPrivateMessageToAdmin" placeholder="Skriv ett meddelande till Admin..." />
                    <button @click="sendPrivateMessageToAdmin">Skicka till Admin</button>
                </div>
            </div>
        </div>

        <div v-if="!isConnected" class="connection-status">Connecting to chat...</div>
    </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue';
    import * as signalR from '@microsoft/signalr';

    // Props för användarnamn från ChatWidget
    const props = defineProps({ username: String });

    const messages = ref([]);  // Meddelanden från offentlig chatt
    const privateMessages = ref([]);  // Meddelanden från admin-chatt
    const privateUsers = ref([]);  // Lista över användare som admin chattar med
    const newMessage = ref('');  // Inmatning för offentlig chatt
    const newPrivateMessage = ref('');  // Inmatning för privat chatt med admin
    const selectedUser = ref('');  // Den valda användaren som admin chattar med
    const connection = ref(null);  // SignalR-anslutning
    const isConnected = ref(false);  // Status för anslutningen
    const activeTab = ref('all');  // Aktiva fliken (antingen 'all' eller 'admin')
    const isAdmin = ref(props.username === 'Admin');  // Kolla om användaren är admin

    // Hämta meddelanden från sessionStorage när chatten laddas
    onMounted(async () => {
        const savedMessages = JSON.parse(sessionStorage.getItem('chatMessages') || '[]');
        messages.value = savedMessages;

        connection.value = new signalR.HubConnectionBuilder()
            .withUrl(`https://localhost:7065/chat-hub?username=${encodeURIComponent(props.username)}`)
            .build();



        connection.value.on('ReceiveMessage', (message, sender) => {
            messages.value.push({ sender, content: message });
            saveMessagesToSessionStorage();  // Spara meddelanden i sessionStorage
            scrollToBottom();  // Skrolla automatiskt till botten
        });

        connection.value.on('ReceivePrivateMessage', (message, sender) => {
            // Hantera privata meddelanden
            if (isAdmin.value && selectedUser.value === sender) {
                // Om admin har valt denna användare, visa meddelanden
                privateMessages.value.push({ sender, content: message });
            } else if (!isAdmin.value) {
                // Vanliga användare ser sina egna privata meddelanden
                privateMessages.value.push({ sender, content: message });
            }
            saveMessagesToSessionStorage();
            scrollToBottom();
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

    // Skicka offentliga meddelanden till alla
    const sendPublicMessage = async () => {
        if (!isConnected.value) {
            console.error("Cannot send message because SignalR is not connected.");
            return;
        }

        if (newMessage.value.trim() !== '') {
            try {
                await connection.value.invoke('SendMessage', newMessage.value, props.username);  // Skicka meddelandet med användarnamnet
                newMessage.value = '';  // Rensa meddelandefältet efter att meddelandet skickats
                saveMessagesToSessionStorage();  // Spara meddelanden i sessionStorage
            } catch (err) {
                console.error('Failed to send message: ', err);
            }
        }
    };

    // Skicka privata meddelanden till admin eller vald användare (admin)
    const sendPrivateMessageToAdmin = async () => {
        if (!isConnected.value) {
            console.error("Cannot send message because SignalR is not connected.");
            return;
        }

        if (newPrivateMessage.value.trim() !== '') {
            try {
                if (isAdmin.value && selectedUser.value) {
                    await connection.value.invoke('SendMessageToAdmin', newPrivateMessage.value, selectedUser.value);
                } else {
                    await connection.value.invoke('SendMessageToAdmin', newPrivateMessage.value, props.username);
                }
                newPrivateMessage.value = '';
                saveMessagesToSessionStorage();
            } catch (err) {
                console.error('Failed to send private message to admin: ', err);
            }
        }
    };

    // Välj en privat användare (för admin)
    const selectPrivateChat = (user) => {
        selectedUser.value = user;
        privateMessages.value = [];
    };

    // Funktion för att skrolla till botten av chatten när nya meddelanden tas emot
    const scrollToBottom = () => {
        const chatArea = document.querySelector('.messages');
        chatArea.scrollTop = chatArea.scrollHeight;
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