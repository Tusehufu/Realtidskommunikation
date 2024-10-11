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
            <Chat v-if="usernameSet" :username="username" />
        </div>
    </div>
</template>

<script setup>
    import { ref } from 'vue';
    import Chat from './Chat.vue'; // Importera din Chat-komponent

    const isChatOpen = ref(false);
    const username = ref('');
    const usernameSet = ref(false);

    const toggleChat = () => {
        isChatOpen.value = !isChatOpen.value;
    };

    const setUsername = () => {
        if (username.value.trim() !== '') {
            usernameSet.value = true;
        } else {
            alert('Please enter a valid username.');
        }
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
