<template>
    <div class="container mt-1" style="background-color: forestgreen; color: white;">
        <h1>Växthusets Sensordata och Kontroll</h1>

        <!-- Visar den senaste sensordatan -->
        <div class="sensor-data mb-4">
            <p class="list-group-item">
                {{ sensorData }}
            </p>
        </div>

        <!-- Kontroller för enheter -->
        <div class="mb-3 d-flex justify-content-between align-items-center flex-wrap">
            <div class="d-flex align-items-center">
                <i v-if="!isWindowOpen" class="fas fa-window-close fa-2x me-2"></i>
                <i v-else class="fas fa-window-maximize fa-2x me-2"></i>
                <button v-if="!isWindowOpen" class="btn btn-primary me-2" @click="toggleWindow('open')">Öppna Fönster</button>
                <button v-if="isWindowOpen" class="btn btn-danger me-2" @click="toggleWindow('close')">Stäng Fönster</button>
            </div>
            <div class="d-flex align-items-center">
                <i v-if="!isWatering" class="fas fa-tint-slash fa-2x me-2"></i>
                <i v-else class="fas fa-tint fa-2x me-2"></i>
                <button v-if="!isWatering" class="btn btn-primary me-2" @click="toggleWatering('start')">Starta Bevattning</button>
                <button v-if="isWatering" class="btn btn-danger" @click="toggleWatering('stop')">Stoppa Bevattning</button>
            </div>
        </div>
        <div class="d-flex justify-content-between">
            <h4 v-if="!isWindowOpen">Fönster är stängda</h4>
            <h4 v-if="isWindowOpen">Fönster är öppna</h4>
            <h4 v-if="!isWatering">Bevattning är avstängd</h4>
            <h4 v-if="isWatering">Bevattning är igång</h4>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted, onUnmounted } from 'vue';
    import * as signalR from '@microsoft/signalr';

    // Statusvariabler för att kontrollera knapparnas visning
    const isWindowOpen = ref(false);
    const isWatering = ref(false);

    // Sensordata
    const sensorData = ref<string>('Väntar på sensordata...');
    let connection: signalR.HubConnection | null = null;

    const connectToSignalR = () => {
        connection = new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7065/GreenhouseHub')
            .withAutomaticReconnect()
            .build();

        connection.on('ReceiveSensorData', (data: string) => {
            sensorData.value = data;
        });

        connection.on('UpdateState', (windowState: boolean, wateringState: boolean) => {
            isWindowOpen.value = windowState;
            isWatering.value = wateringState;
        });

        connection.start()
            .then(() => console.log('Ansluten till SignalR-hubben'))
            .catch(err => console.error('SignalR-anslutning misslyckades: ', err));
    };

    // Funktioner för att hantera kommandon och skicka till servern
    const sendCommand = (device: string, command: string) => {
        if (connection) {
            connection.invoke('ControlDevice', device, command)
                .catch(err => console.error(err));
        }
    };

    const toggleWindow = (command: string) => {
        sendCommand('window', command);
    };

    const toggleWatering = (command: string) => {
        sendCommand('watering', command);
    };

    onMounted(() => {
        connectToSignalR();
    });

    onUnmounted(() => {
        if (connection) {
            connection.stop();
        }
    });
</script>

<style>
    .container {
        max-width: 600px;
        height: 265px;
    }

    .sensor-data p {
        font-size: 1.2em;
        padding: 10px;
        background-color: #2c3e50;
        border-radius: 5px;
    }

    .fas {
        color: white;
    }
</style>
