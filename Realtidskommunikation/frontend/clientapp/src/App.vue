<template>
    <header>
        <h1 class="text-center">Naturliga Växter & Kreativa Trädgårdar</h1>
    </header>
    <main>
        <section>
            <router-view />
        </section>
        <section>
            <WatherCard />
        </section>
        <aside>
            <!-- Vanliga användare och Admin använder samma ChatWidget -->
            <ChatWidget @new-private-message="createChatWidget" @user-set="checkIfAdmin" />
        </aside>
    </main>
    <!-- Flexbox container för Admins dynamiska chatwidgets -->
    <section v-if="isAdmin" class="admin-chat-widgets">
        <AdminChatWidget v-for="(user, index) in adminPrivateUsers"
                         :key="index"
                         :user="user"
                         :messages="userMessages[user]" />
    </section>
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

    // Skapa en ny widget för varje användare som kontaktar Admin och lagra meddelandet
    const createChatWidget = (user, message) => {
        console.log(`Attempting to create new chat widget for user: ${user}`);
        if (!adminPrivateUsers.value.includes(user)) {
            console.log(`Creating new chat widget for user: ${user}`);
            adminPrivateUsers.value.push(user);

            // Lägg till första meddelandet direkt i listan för användaren
            userMessages.value = {
                ...userMessages.value,
                [user]: [{ sender: user, content: message }]
            };

            console.log(`First message added for ${user}:`, { sender: user, content: message });
        } else {
            // Om widget redan finns, lägg till meddelandet i listan
            userMessages.value[user].push({ sender: user, content: message });
            console.log(`Added subsequent message for ${user}:`, { sender: user, content: message });
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
