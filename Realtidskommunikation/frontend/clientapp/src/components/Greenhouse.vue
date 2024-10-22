<template>
    <div class="container mt-5s" style="background-color: forestgreen; color: white; ">
        <h1 class="text-center">Växthus</h1>

        <!-- Visar sensordata -->
        <ul class="list-group mb-4">
            <li class="list-group-item" v-for="(data, index) in sensorData" :key="index">
                {{ data }}
            </li>
        </ul>

        <!-- Kontroller för enheter -->
        <div class="mb-3 d-flex justify-content-between align-items-center flex-wrap">
            <div class="d-flex">
                <button class="btn btn-primary me-2">Öppna Fönster</button>
                <button class="btn btn-danger me-2">Stäng Fönster</button>
            </div>
            <div class="d-flex">
                <button class="btn btn-primary me-2">Starta Bevattning</button>
                <button class="btn btn-danger">Stoppa Bevattning</button>
            </div>
        </div>

    </div>
</template>

<script setup lang="ts">

</script>

<style>
    .container {
        max-width: 600px;
    }
</style>
