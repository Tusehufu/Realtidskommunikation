<template>
    <div class="chat-widget">
        <button @click="toggleChat" class="toggle-chat-btn">
            {{ isChatOpen ? `Stäng Chat med ${user}` : `Öppna Chat med ${user}` }}
        </button>

        <div v-if="isChatOpen" class="chat-container">
            <div class="messages">
                <div v-for="(message, index) in props.messages" :key="index" class="message">
                    <strong>{{ message.sender }}:</strong> {{ message.content }}
                </div>
            </div>
            <input type="text" v-model="newMessage" @keyup.enter="sendMessageToUser" :placeholder="`Skriv ett meddelande till ${user}`" />
            <button @click="sendMessageToUser">Skicka</button>
        </div>
    </div>
</template>

<script setup>
    import { ref, onMounted, computed, watch } from 'vue';
    import * as signalR from '@microsoft/signalr';

    const props = defineProps({
        user: String,
        messages: Array // Tar emot meddelanden från props
    });

    const isChatOpen = ref(false);
    const newMessage = ref('');
    const connection = ref(null);
    const chatMessages = computed(() => props.messages);

    // Debug-logg för att verifiera reaktivitet av meddelanden
    watch(props.messages, (newMessages) => {
        console.log("Messages updated:", newMessages);
    });

    const toggleChat = () => {
        isChatOpen.value = !isChatOpen.value;
    };

    // Skicka meddelandet från Admin till den specifika användaren via SignalR
    const sendMessageToUser = async () => {
        if (newMessage.value.trim()) {
            // Lägg till Admins meddelande i den lokala meddelandelistan
            props.messages.push({ sender: 'Admin', content: newMessage.value });

            try {
                // Skicka meddelandet via SignalR
                await connection.value.invoke("SendMessageToUser", props.user, newMessage.value);
                console.log(`Sent message to user ${props.user}: ${newMessage.value}`);
            } catch (error) {
                console.error("Failed to send message:", error);
            }

            newMessage.value = '';
        }
    };

    // Anslut till SignalR när komponenten monteras
    onMounted(async () => {
        connection.value = new signalR.HubConnectionBuilder()
            .withUrl("https://localhost:7065/chat-hub?username=Admin")
            .build();

        connection.value.on("ReceivePrivateMessage", (message, sender) => {
            if (sender === props.user) {
                // Lägg till användarens meddelande till den lokala listan
                props.messages.push({ sender, content: message });
            }
        });

        try {
            await connection.value.start();
            console.log("Connected to SignalR");
        } catch (error) {
            console.error("Failed to connect to SignalR:", error);
        }
    });
</script>

<style scoped>
    .chat-widget {
        width: 300px; /* Behåll enhetlig bredd för widgets */
        flex: 0 0 auto; /* Tillåt widgets att placeras horisontellt utan att krympa */
    }

    .toggle-chat-btn {
        background-color: #0056b3;
        color: white;
        padding: 10px 20px;
        border: none;
        cursor: pointer;
    }

    .chat-container {
        width: 300px;
        height: 400px;
        background-color: white;
        box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
        padding: 10px;
        display: flex;
        flex-direction: column;
    }

    .message {
        margin-bottom: 10px;
    }
</style>
