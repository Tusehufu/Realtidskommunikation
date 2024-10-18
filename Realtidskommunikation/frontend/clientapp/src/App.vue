<template>
    <h1>Naturliga Växter & Kreativa Trädgårdar</h1>
    <router-view />
    <WatherCard />
    <!-- Vanliga användare och Admin använder samma ChatWidget -->
    <ChatWidget @new-private-message="createChatWidget"  @user-set="checkIfAdmin" />

    <!-- Flexbox container för Admins dynamiska chatwidgets -->
    <div v-if="isAdmin" class="admin-chat-widgets">
        <AdminChatWidget v-for="(user, index) in adminPrivateUsers"
                         :key="index"
                         :user="user"
                         :messages="userMessages[user]" />
    </div>
</template>

<script setup>
    import { ref, onMounted } from 'vue';
    import WatherCard from './components/WatherCard.vue';
    import ChatWidget from './components/ChatWidget.vue';
    import AdminChatWidget from './components/AdminChatWidget.vue';

    const isAdmin = ref(false);
    const adminPrivateUsers = ref([]);
    const userMessages = ref({}); // För att hålla reda på meddelanden från varje användare

    // Kontrollera om användaren är Admin när händelsen tas emot
    const checkIfAdmin = (userIsAdmin) => {
        isAdmin.value = userIsAdmin;
        console.log(`User is Admin: ${userIsAdmin}`);
    };

    // Skapa en ny widget för varje användare som kontaktar Admin
    const createChatWidget = (user) => {
        console.log(`Attempting to create new chat widget for user: ${user}`);
        if (!adminPrivateUsers.value.includes(user)) {
            console.log(`Creating new chat widget for user: ${user}`);
            adminPrivateUsers.value.push(user);
            userMessages.value[user] = []; // Initiera meddelandelista för användaren
        } else {
            console.log(`Chat widget for user ${user} already exists.`);
        }
    };

    // Lyssna på inkommande privata meddelanden och lagra dem
    const handleIncomingPrivateMessage = (user, message) => {
        if (!userMessages.value[user]) {
            userMessages.value[user] = [];
        }
        userMessages.value[user].push({ sender: user, content: message });
    };
</script>

<style scoped>
    #app {
        font-family: Avenir, Helvetica, Arial, sans-serif;
        -webkit-font-smoothing: antialiased;
        -moz-osx-font-smoothing: grayscale;
        text-align: center;
        color: #2c3e50;
    }

    h1 {
        font-family: cursive;
    }

    /* Flexbox för Admins chatwidgets */
    .admin-chat-widgets {
        position: fixed;
        bottom: 80px; /* Positionera precis ovanför den vanliga chatwidgeten */
        left: 20px; /* Starta från vänster sida */
        width: auto; /* Flexbox bredd baserat på innehåll */
        display: flex;
        flex-direction: row; /* Liggande placering av widgets */
        flex-wrap: nowrap; /* Förhindra överlappning */
        align-items: flex-start; /* Säkra att de alla placeras på samma linje */
        gap: 10px; /* Mellanrum mellan widgets */
        z-index: 1000;
    }
</style>
