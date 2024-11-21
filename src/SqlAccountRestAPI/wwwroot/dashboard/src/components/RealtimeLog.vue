<template>
    <a-card title="Application Logs" :bordered="false">
        <a-empty v-if="!messages?.length"></a-empty>
        <VirtualScrollList v-if="!!messages?.length" class="scrollable-list" :data-key="'id'" :data-sources="messages"
            :keeps="20" style="max-height: 400px; overflow-y: auto;" :data-component="Item" ref="virtualScroll">
        </VirtualScrollList>
    </a-card>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref, defineAsyncComponent, nextTick } from "vue";
import { startConnection, onReceiveLog, stopConnection } from "../signalr";

import VirtualScrollList from "vue3-virtual-scroll-list";
const Item = defineAsyncComponent(() => import('./RealtimeLogItem.vue'))

const messages = ref<{ id: number; logLevel: string; message: string; ts: number }[]>([]);
const virtualScroll = ref<InstanceType<typeof VirtualScrollList> | null>(null);

// Scroll to the bottom of the list
const scrollToBottom = async () => {
    await nextTick(); // Wait for DOM updates
    virtualScroll.value.scrollToBottom()
};

onMounted(async () => {
    await startConnection();
    onReceiveLog((logLevel: string, message: string, ts: number) => {
        messages.value.push({ id: Date.now(), logLevel, message, ts });
        // Scroll to the bottom when a new message is added
        scrollToBottom();
    });
});

onBeforeUnmount(async () => {
    stopConnection();
});
</script>

<style scoped>
.scrollable-list {
    max-height: 400px;
    overflow-y: auto;
    border: 1px solid #d9d9d9;
    border-radius: 4px;
}

.virtual-list-item {
    padding: 10px;
    border-bottom: 1px solid #f0f0f0;
}
</style>
