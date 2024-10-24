<template>
    <div class="invoice-container">
        <header class="d-flex justify-content-between align-items-center mb-4">
            <div>
                <h2>Beställningsdokument</h2>
                <p>
                    <strong>Ordernummer:</strong>
                    <input type="text" v-model="orderNumber" class="form-control d-inline-block w-auto" @input="syncData">
                </p>
                <p>
                    <strong>Datum:</strong>
                    <input type="date" v-model="orderDate" class="form-control d-inline-block w-auto" @input="syncData">
                </p>
            </div>
            <div>
                <img src="../assets/logo.png" alt="Företagslogotyp" width="150" class="rounded-circle">
            </div>
        </header>

        <section class="mb-4">
            <div class="row">
                <div class="col-md-6">
                    <h5>Från:</h5>
                    <p>
                        <strong><input type="text" v-model="fromCompany.name" class="form-control" @input="syncData"></strong><br>
                        <input type="text" v-model="fromCompany.address" class="form-control" @input="syncData"><br>
                        <input type="text" v-model="fromCompany.zip" class="form-control w-auto d-inline-block" @input="syncData">
                        <input type="text" v-model="fromCompany.city" class="form-control w-auto d-inline-block" @input="syncData"><br>
                        <input type="text" v-model="fromCompany.country" class="form-control" @input="syncData"><br>
                        Telefon: <input type="text" v-model="fromCompany.phone" class="form-control" @input="syncData"><br>
                        E-post: <input type="text" v-model="fromCompany.email" class="form-control" @input="syncData">
                    </p>
                </div>
                <div class="col-md-6">
                    <h5>Till:</h5>
                    <p>
                        <strong><input type="text" v-model="toCompany.name" class="form-control" @input="syncData"></strong><br>
                        <input type="text" v-model="toCompany.address" class="form-control" @input="syncData"><br>
                        <input type="text" v-model="toCompany.zip" class="form-control w-auto d-inline-block" @input="syncData">
                        <input type="text" v-model="toCompany.city" class="form-control w-auto d-inline-block" @input="syncData"><br>
                        <input type="text" v-model="toCompany.country" class="form-control" @input="syncData"><br>
                        Telefon: <input type="text" v-model="toCompany.phone" class="form-control" @input="syncData"><br>
                        E-post: <input type="text" v-model="toCompany.email" class="form-control" @input="syncData">
                    </p>
                </div>
            </div>
        </section>

        <section class="mb-4">
            <table class="table table-bordered">
                <thead>
                    <tr>
                        <th>Artikel</th>
                        <th>Beskrivning</th>
                        <th>Kvantitet</th>
                        <th>Pris per enhet</th>
                        <th>Total</th>
                    </tr>
                </thead>
                <tbody>
                    <tr v-for="(item, index) in orderItems" :key="index">
                        <td><input type="text" v-model="item.id" class="form-control" @input="syncData"></td>
                        <td><input type="text" v-model="item.description" class="form-control" @input="syncData"></td>
                        <td><input type="number" v-model="item.quantity" class="form-control" @input="updateTotal(index)"></td>
                        <td><input type="number" v-model="item.unitPrice" class="form-control" @input="updateTotal(index)"> SEK</td>
                        <td>{{ item.totalPrice }} SEK</td>
                    </tr>
                </tbody>
                <tfoot>
                    <tr>
                        <td colspan="4" class="text-end"><strong>Subtotal:</strong></td>
                        <td>{{ subtotal }} SEK</td>
                    </tr>
                    <tr>
                        <td colspan="4" class="text-end"><strong>Moms (25%):</strong></td>
                        <td>{{ vat }} SEK</td>
                    </tr>
                    <tr>
                        <td colspan="4" class="text-end"><strong>Totalt:</strong></td>
                        <td><strong>{{ total }} SEK</strong></td>
                    </tr>
                </tfoot>
            </table>
            <div class="text-center mt-3">
                <button class="btn btn-success" @click="addNewItem">Lägg till ny artikel</button>
            </div>
        </section>

        <section>
            <p>
                <strong>Betalningsvillkor:</strong>
                <input type="text" v-model="paymentTerms" class="form-control" @input="syncData">
            </p>
            <p>
                <strong>Leveransvillkor:</strong>
                <input type="text" v-model="deliveryTerms" class="form-control" @input="syncData">
            </p>
        </section>
    </div>
    <div class="text-center mt-4">
        <button class="btn btn-primary" @click="generatePDF">Skapa PDF</button>
    </div>
</template>

<script setup lang="ts">
    import { ref, computed, onMounted, onBeforeUnmount } from 'vue';
    import * as signalR from '@microsoft/signalr';
    import html2pdf from 'html2pdf.js';

    const connection = new signalR.HubConnectionBuilder()
        .withUrl('https://localhost:7065/DocumentHub')
        .withAutomaticReconnect()
        .build();

    const orderNumber = ref('12345');
    const orderDate = ref('2024-10-24');
    const logo = ref('../assets/logo.png');

    const fromCompany = ref({
        name: 'Naturliga växter & kreativa trädgårdar',
        address: 'Växtvägen 3',
        zip: '123 45',
        city: 'Kungsträdgården',
        country: 'Sverige',
        phone: '+46 123 456 789',
        email: 'Kringla@nvkt.se'
    });

    const toCompany = ref({
        name: 'Företaget',
        address: 'Kungsgatan 10',
        zip: '543 21',
        city: 'Djurgården',
        country: 'Sverige',
        phone: '+46 987 654 321',
        email: 'FreeItems@gmail.com'
    });

    const orderItems = ref([
        { id: '001', description: 'Produkt A', quantity: 10, unitPrice: 200, totalPrice: 2000 },
        { id: '002', description: 'Produkt B', quantity: 5, unitPrice: 500, totalPrice: 2500 },
        { id: '003', description: 'Produkt C', quantity: 2, unitPrice: 1000, totalPrice: 2000 }
    ]);

    const paymentTerms = ref('Betalning inom 30 dagar från fakturadatum.');
    const deliveryTerms = ref('Leverans sker inom 5 arbetsdagar efter att ordern är bekräftad.');

    const updateTotal = (index: number) => {
        const item = orderItems.value[index];
        item.totalPrice = item.quantity * item.unitPrice;
        syncData();
    };

    const subtotal = computed(() => {
        return orderItems.value.reduce((sum, item) => sum + item.totalPrice, 0);
    });

    const vat = computed(() => {
        return (subtotal.value * 0.25);
    });

    const total = computed(() => {
        return subtotal.value + vat.value;
    });

    // Funktion för att skicka data till SignalR
    const syncData = () => {
        const data = {
            orderNumber: orderNumber.value,
            orderDate: orderDate.value,
            fromCompany: fromCompany.value,
            toCompany: toCompany.value,
            orderItems: orderItems.value,
            paymentTerms: paymentTerms.value,
            deliveryTerms: deliveryTerms.value,
        };
        connection.invoke("UpdateDocument", data).catch(err => console.error(err));
    };

    // Hantera inkommande data från SignalR
    connection.on("ReceiveDocumentUpdate", (data) => {
        orderNumber.value = data.orderNumber;
        orderDate.value = data.orderDate;
        fromCompany.value = data.fromCompany;
        toCompany.value = data.toCompany;
        orderItems.value = data.orderItems;
        paymentTerms.value = data.paymentTerms;
        deliveryTerms.value = data.deliveryTerms;
    });

    const addNewItem = () => {
        orderItems.value.push({
            id: '',
            description: '',
            quantity: 1,
            unitPrice: 0,
            totalPrice: 0
        });
        syncData(); // Uppdatera synkningen med servern
    };

    const generatePDF = () => {
        const element = document.querySelector('.invoice-container');
        if (element) {
            html2pdf()
                .from(element)
                .save();
        }
    };

    // Starta SignalR-anslutning
    onMounted(async () => {
        try {
            await connection.start();
            console.log("SignalR Connected.");
        } catch (err) {
            console.error("Error connecting to SignalR", err);
        }
    });

    onBeforeUnmount(async () => {
        await connection.stop();
    });
</script>

<style>
    .invoice-container {
        max-width: 900px;
        margin: 20px auto;
        background: #ffffff;
        padding: 30px;
        border-radius: 10px;
        box-shadow: 0 0 20px rgba(0, 0, 0, 0.1);
    }

    .table th, .table td {
        vertical-align: middle;
    }

    .table th {
        background-color: #e9ecef;
    }
</style>
