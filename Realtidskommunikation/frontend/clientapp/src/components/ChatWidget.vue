<template>
    <div class="chat-widget">
        <button @click="toggleChat" class="toggle-chat-btn">
            {{ isChatOpen ? 'Stäng Chat' : 'Öppna Chat' }}
        </button>

        <div v-if="isChatOpen" class="chat-container">
            <div v-if="!usernameSet" class="username-input">
                <input type="text" v-model="username" placeholder="Välj ett alias" />
                <button @click="setUsername">Starta Chat</button>
            </div>

            <!-- Chat-komponenten visas bara när användarnamn är satt -->
            <Chat v-if="usernameSet" :username="username" @new-private-message="forwardPrivateMessage" />
        </div>
    </div>
</template>

<script setup>
    import { ref, defineEmits } from 'vue';
    import Chat from './Chat.vue'; // Importera din Chat-komponent

    const isChatOpen = ref(false);
    const username = ref('');
    const usernameSet = ref(false);
    const emit = defineEmits(['new-private-message', 'user-set']);

    const toggleChat = () => {
        isChatOpen.value = !isChatOpen.value;
    };

    const setUsername = () => {
        if (username.value.trim() !== '') {
            usernameSet.value = true;
            // Emitera händelsen och meddela app.vue om användaren är Admin eller inte
            emit('user-set', username.value === 'Admin');
        } else {
            alert('Please enter a valid username.');
        }
    };

    // Vidarebefordra `new-private-message`-händelsen till App.vue
    const forwardPrivateMessage = (user, message) => {
        console.log(`Forwarding private message event for user: ${user} with message: ${message}`);
        emit('new-private-message', user, message);
    };
</script>

<style scoped>
    .chat-widget {
        position: fixed;
        bottom: 20px;
        right: 20px;
        z-index: 1000;
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

    .username-input input {
        padding: 10px;
        margin-bottom: 10px;
        width: 100%;
    }

    .username-input button {
        padding: 10px;
        width: 100%;
    }
</style>
