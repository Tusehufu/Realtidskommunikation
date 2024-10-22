<template>
    <div class="container mt-5" style="background-color: forestgreen; color: white;">
        <h1>Växthusets Sensordata och Kontroll</h1>

        <!-- Visar den senaste sensordatan -->
        <div class="sensor-data mb-4">
            <p class="list-group-item">
                {{ sensorData }}
            </p>
        </div>

        <!-- Kontroller för enheter -->
        <div class="mb-3 d-flex justify-content-between align-items-center flex-wrap">
            <div class="d-flex">
                <button class="btn btn-primary me-2" @click="sendCommand('window', 'open')">Öppna Fönster</button>
                <button class="btn btn-danger me-2" @click="sendCommand('window', 'close')">Stäng Fönster</button>
            </div>
            <div class="d-flex">
                <button class="btn btn-primary me-2" @click="sendCommand('watering', 'start')">Starta Bevattning</button>
                <button class="btn btn-danger" @click="sendCommand('watering', 'stop')">Stoppa Bevattning</button>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted, onUnmounted } from 'vue';
    import * as signalR from '@microsoft/signalr';

    // Ändra sensorData till en sträng för att endast hålla den senaste uppdateringen
    const sensorData = ref<string>('Väntar på sensordata...');
    let connection: signalR.HubConnection | null = null;

    const connectToSignalR = () => {
        connection = new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7065/GreenhouseHub')
            .withAutomaticReconnect()
            .build();

        // Hantera inkommande sensordata och uppdatera endast det befintliga elementet
        connection.on('ReceiveSensorData', (data: string) => {
            sensorData.value = data;
        });

        // Hantera feedback från kommandon
        connection.on('DeviceControl', (device: string, command: string) => {
            console.log(`Enhet: ${device}, Kommando: ${command}`);
        });

        connection.start()
            .then(() => console.log('Ansluten till SignalR-hubben'))
            .catch(err => console.error('SignalR-anslutning misslyckades: ', err));
    };

    // Skicka kommandon till backend
    const sendCommand = (device: string, command: string) => {
        if (connection) {
            connection.invoke('ControlDevice', device, command)
                .catch(err => console.error(err));
        }
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
    }

    .sensor-data p {
        font-size: 1.2em;
        padding: 10px;
        background-color: #2c3e50;
        border-radius: 5px;
    }
</style>
