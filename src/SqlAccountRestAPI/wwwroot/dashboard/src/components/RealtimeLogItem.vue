<template>
    <a-row :wrap="false" class="log-item">
        <a-col flex="none" style="text-align: left; padding: 0px 20px;">
            <div>
                <span style="font-weight: 700;">{{ source.logLevel }}</span>
            </div>
            <div>{{ moment(source.ts).format("MMM DD, YYYY HH:mm:ss.SSS A") }}</div>
        </a-col>
        <a-col flex="auto" style="padding: 0px 20px;">
            <div>
                <pre style="text-align: left; width: 100%;">
                    {{ source.message }}
                </pre>
            </div>
        </a-col>
    </a-row>
</template>

<script setup lang="ts">
import moment from 'moment';
import { defineProps } from 'vue';

// Define props for the custom item component
defineProps({
    source: {
        type: Object,
        default: () => ({}),
    },
});
</script>
<style lang="scss" scoped>
.log-item {
    border-bottom: 1px solid #e7e7e7;
    padding: 10px 0px;
}
</style>