<template>
    <div v-if="locationDenied" class="card text-center shadow-sm border" style="width: 100%; margin: auto; background-color: tomato; color: white;">
        <div class="card-body">
            <p class="display-5 mb-0">Platsåtkomst nekad</p>
            <p>Vi kunde inte hämta väderinformation eftersom platsåtkomsten inte har getts.</p>
        </div>
    </div>
    <div v-else class="card text-center shadow-sm border" style="width: 100%; margin: auto; background-color: forestgreen; color: white;">
        <div class="card-body">
            <!-- Väderinformation -->
            <div class="mb-3">
                <p class="display-1 mb-0">{{ temperature }}°C</p>
                <p>{{ condition }}</p>
                <div class="d-flex justify-content-between">
                    <div>
                        <p class="mb-1">Luftfuktighet</p>
                        <p class="fw-bold">{{ humidity }}%</p>
                    </div>
                    <div>
                        <p class="mb-1">Vindstyrka</p>
                        <p class="fw-bold">{{ windSpeed }} km/h</p>
                    </div>
                </div>
                <hr />
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
    import { ref, onMounted } from 'vue'
    import * as signalR from '@microsoft/signalr'

    // Reactive references för väderdata
    const temperature = ref(0)
    const condition = ref('')
    const humidity = ref(0)
    const windSpeed = ref(0)
    const locationDenied = ref(false)  // Ny variabel för att spåra platsåtkomst

    const translateCondition = (condition: string) => {
        const translations: { [key: string]: string } = {
            "Clear": "Klart",
            "Partially cloudy": "Delvis molnigt",
            "Cloudy": "Molnigt",
            "Overcast": "Mulet",
            "Rain": "Regn",
            "Heavy rain": "Kraftigt regn",
            "Light rain": "Lätt regn",
            "Snow": "Snö",
            "Heavy snow": "Kraftigt snöfall",
            "Light snow": "Lätt snöfall",
            "Thunderstorm": "Åskväder",
            "Fog": "Dimma",
            "Hail": "Hagel",
            "Sleet": "Snöblandat regn",
        };
        return translations[condition] || condition;
    };

    // Funktion för att uppdatera väderdatan i Vue-komponenten
    const updateWeatherData = (weatherData: string) => {
        const data = JSON.parse(weatherData)
        console.log('Received weather data:', weatherData)  // Logga för felsökning
        temperature.value = parseFloat(data.Temperature)
        condition.value = translateCondition(data.Condition)
        humidity.value = parseFloat(data.Humidity)
        windSpeed.value = parseFloat(data.WindSpeed)
    }

    const getUserLocation = async () => {
        return new Promise<{ latitude: number; longitude: number }>((resolve, reject) => {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(
                    (position) => {
                        const { latitude, longitude } = position.coords;
                        console.log(`Coordinates obtained from geolocation: Latitude=${latitude}, Longitude=${longitude}`);
                        resolve({ latitude, longitude });
                    },
                    (error) => {
                        console.error('Geolocation error:', error);
                        locationDenied.value = true;  // Om platsåtkomst nekas
                        reject(error);
                    }
                );
            } else {
                locationDenied.value = true;  // Om geolocation inte stöds
                reject(new Error('Geolocation is not supported by this browser.'));
            }
        });
    }

    // Funktion som hanterar anslutning till SignalR och skickar koordinater
    const connectToSignalR = async () => {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('https://localhost:7065/weatherHub')
            .withAutomaticReconnect()
            .build()

        try {
            await connection.start()
            console.log('SignalR connected!')
        } catch (err) {
            console.error('SignalR connection error:', err)
            return
        }

        try {
            const { latitude, longitude } = await getUserLocation()
            if (connection.state === signalR.HubConnectionState.Connected) {
                await connection.invoke('SendLocation', latitude, longitude)
                console.log('Location sent to server:', { latitude, longitude })
            } else {
                console.warn('Connection is not in Connected state.')
            }
        } catch (err) {
            console.error('Error sending location or getting user location:', err)
        }

        connection.on('ReceiveWeatherUpdate', (weatherData: string) => {
            updateWeatherData(weatherData)
        })
    }

    onMounted(async () => {
        try {
            await connectToSignalR()
        } catch (error) {
            console.error('Error connecting to SignalR:', error)
        }
    })
</script>

<style scoped>
    .card {
        max-width: 100%;
    }
</style>
