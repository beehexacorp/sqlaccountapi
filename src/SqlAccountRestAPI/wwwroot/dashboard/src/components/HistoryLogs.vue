<template>
    <div>
        <a-card title="History Logs" :bordered="false">
            <a-date-picker v-model:value="selectedDate" :size="`large`" :format="formatDate"
                :default-value="defaultDate" @change="onDateChange" />
            <a-empty v-if="!histories?.length"></a-empty>
            <a-row v-if="!!histories?.length" style="padding: 20px 0px;">
                <a-col v-for="(h, hix) in histories" :key="hix" :xs="24" :sm="12" :md="12" :lg="6" :xl="4"
                    class="gutter-row">
                    <div style="padding: 5px">
                        <a href="javascript:void(0)" @click="() => onReadLogDetail(h)">{{ h.displayName }}</a>
                    </div>
                </a-col>
            </a-row>
        </a-card>

        <!-- Drawer for Log Detail -->
        <a-drawer :title="`${selectedLogDetail?.displayName} (${selectedLogDetail?.filename})`"
            :visible="isDrawerVisible" :width="800" @close="isDrawerVisible = false">
            <pre style="text-align: left; width: 100%;">{{ logContent }}</pre>
        </a-drawer>
    </div>

</template>
<script setup lang="ts">
import { onMounted, ref } from 'vue';
import type { Dayjs } from 'dayjs';
import dayjs from 'dayjs';
import msgpack from "msgpack-lite";
import advancedFormat from 'dayjs/plugin/advancedFormat';

// Extend Day.js with advanced format for ordinal dates
dayjs.extend(advancedFormat);

const formatDate = (date: dayjs.Dayjs) => {
    if (!date) return '';
    return date.format('MMM Do, YYYY'); // Example: Jul 24th, 2024
};
const selectedDate = ref<Dayjs>(dayjs());
const defaultDate = dayjs();
// Drawer states
const isDrawerVisible = ref(false);
const logContent = ref<string>('');
const selectedLogDetail = ref<{ filename: string; displayName: string }>()

// History logs (array of { filename: string, displayName: string })
const histories = ref<{ filename: string; displayName: string }[]>([]);

// Fetch history logs based on the selected date
const fetchHistoryLogs = async () => {
    try {
        const unixTimestamp = selectedDate.value.valueOf(); // Unix timestamp in milliseconds
        const endpoint = `api/history/logs?ts=${unixTimestamp}`

        // @ts-ignore
        const apiUrl = import.meta.env.VITE_SQL_ACCOUNT_API_URL ? `${import.meta.env.VITE_SQL_ACCOUNT_API_URL}/${endpoint}` : `/${endpoint}`;

        const response = await fetch(apiUrl, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            }
        });

        if (!response.ok) {
            throw new Error(`Error fetching logs: ${response.statusText}`);
        }

        const data: { filename: string }[] = await response.json() as any;

        // Transform filenames into the desired display format
        histories.value = data.map((log) => {
            const match = log.filename.match(
                /^log(\d{4})(\d{2})(\d{2})(\d{2})\.txt$/
            );
            if (match) {
                const [, year, month, day, hour] = match;
                const logDate = dayjs(`${year}-${month}-${day}T${hour}:00`);
                return {
                    filename: log.filename,
                    displayName: logDate.format("MMM Do, YYYY h A"),
                };
            }
            return {
                filename: log.filename,
                displayName: log.filename, // Fallback if filename doesn't match pattern
            };
        });
    } catch (error) {
        console.error("Failed to fetch history logs:", error);
        histories.value = [];
    }
};
const onReadLogDetail = async (history: { filename: string; displayName: string }) => {
    const endpoint = `api/history/log-detail?fn=${encodeURIComponent(history.filename)}`

    // @ts-ignore
    const apiUrl = import.meta.env.VITE_SQL_ACCOUNT_API_URL ? `${import.meta.env.VITE_SQL_ACCOUNT_API_URL}/${endpoint}` : `/${endpoint}`;
    const response = await fetch(apiUrl, {
        headers: {
            Accept: "application/x-msgpack",
        },
    });

    if (!response.ok) {
        throw new Error(`Failed to fetch log file: ${response.statusText}`);
    }

    // Parse the MessagePack response
    const buffer = await response.arrayBuffer();
    const decodedContent = msgpack.decode(new Uint8Array(buffer));

    // Set the content and open the drawer
    logContent.value = decodedContent as string;
    isDrawerVisible.value = true;
    selectedLogDetail.value = history
    return decodedContent;
}
const onDateChange = (date: dayjs.Dayjs) => {
    if (!date) {
        return;
    }
    selectedDate.value = date; // Update the selected date
    fetchHistoryLogs(); // Fetch logs for the selected date
};
// Lifecycle hook
onMounted(() => {
    fetchHistoryLogs(); // Fetch logs on mount
});
</script>