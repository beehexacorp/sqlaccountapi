<template>
    <a-card title="Application Logs" class="realtime-log" :bordered="false">
        <a-empty v-if="!messages?.length"></a-empty>
        <div v-if="!!messages?.length" class="scrollable-container">
            <VirtualScrollList
                class="scrollable-list"
                :data-key="'id'"
                :data-sources="messages"
                :keeps="20"
                :data-component="Item"
                ref="virtualScroll"
            ></VirtualScrollList>
        </div>
    </a-card>
</template>

<script setup lang="ts">
import { onMounted, onBeforeUnmount, ref, defineAsyncComponent, nextTick } from "vue";
import { startConnection, onReceiveLog, stopConnection } from "../signalr";

import VirtualScrollList from "vue3-virtual-scroll-list";
const Item = defineAsyncComponent(() => import('./RealtimeLogItem.vue'));

const messages = ref<{ id: number; logLevel: string; message: string; ts: number }[]>([]);
const virtualScroll = ref<InstanceType<typeof VirtualScrollList> | null>(null);

const scrollToBottom = async () => {
    await nextTick(); // Wait for DOM updates
    if (virtualScroll.value) {
        virtualScroll.value.scrollToBottom(); // Scroll to the last item
    }
};

onMounted(async () => {
    await startConnection();
    onReceiveLog((logLevel: string, message: string, ts: number) => {
        messages.value.push({ id: Date.now(), logLevel, message, ts });
        scrollToBottom(); // Scroll to the bottom when a new message is added
    });
});

onBeforeUnmount(() => {
    stopConnection();
});
</script>

<style scoped lang="scss">
.realtime-log {
    display: flex;
    flex-direction: column;
    height: 100%;
    overflow: hidden;

    :deep(.ant-card-body) {
        display: flex;
        flex-direction: column;
        height: 100%;
    }
}

.scrollable-container {
    flex-grow: 1; /* Fill the remaining space */
    overflow: hidden; /* Prevent unnecessary scrolling of the container */
    display: flex;
    flex-direction: column;
    margin-bottom: 40px;
}

.scrollable-list {
    flex-grow: 1; /* Allow the list to expand */
    overflow-y: auto; /* Enable vertical scrolling */
    border: 1px solid #e7e7e7;
    border-radius: 4px;
}

.virtual-list-item {
    padding: 10px;
    border-bottom: 1px solid #f0f0f0;
}
</style>
