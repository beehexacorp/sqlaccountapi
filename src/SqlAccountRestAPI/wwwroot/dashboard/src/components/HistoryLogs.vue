<template>
    <a-card title="History Logs" :bordered="false">
        <a-empty v-if="!histories?.length"></a-empty>
    </a-card>
</template>
<script setup lang="ts">
import { ref } from 'vue';

const histories = ref<any[]>([])
</script>