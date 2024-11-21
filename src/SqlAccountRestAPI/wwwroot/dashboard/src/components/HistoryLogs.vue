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
                        <a href="javascript:void(0)" @click="() => onReadLogDetail(h)">
                            {{ h.displayName }}
                        </a>
                    </div>
                </a-col>
            </a-row>
        </a-card>

        <!-- Drawer for Log Detail -->
        <a-drawer :visible="isDrawerVisible" :width="800" @close="isDrawerVisible = false">
            <template #title>
                <div style="display: flex; justify-content: space-between; align-items: center;">
                    <span>
                        {{ selectedLogDetail?.displayName }} ({{ selectedLogDetail?.filename }})
                    </span>
                    <a-button type="link" style="margin-left: auto;" @click="handleDownload">
                        <DownloadOutlined /> Download
                    </a-button>
                </div>
            </template>
            <pre style="white-space: pre-wrap; text-align: left; width: 100%;">{{ logContent }}</pre>
        </a-drawer>
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import dayjs from 'dayjs';
import advancedFormat from 'dayjs/plugin/advancedFormat';
import { DownloadOutlined } from '@ant-design/icons-vue';
import { message } from 'ant-design-vue';
import {
    fetchHistoryLogs,
    fetchLogDetail,
    downloadLogFile
} from '@/services/logService';
import type { LogEntry } from '@/services/logService';

// Extend Day.js with advanced format for ordinal dates
dayjs.extend(advancedFormat);

const formatDate = (date: dayjs.Dayjs) =>
    date ? date.format('MMM Do, YYYY') : ''; // Example: Jul 24th, 2024

const selectedDate = ref(dayjs());
const defaultDate = dayjs();
const isDrawerVisible = ref(false);
const logContent = ref<string>('');
const selectedLogDetail = ref<LogEntry | null>(null);
const histories = ref<LogEntry[]>([]);

// Helper to display a notification message
const showMessage = (type: 'success' | 'error', content: string) => {
    message[type]({
        content,
        duration: 10
    });
};

// Fetch history logs based on the selected date
const updateHistoryLogs = async () => {
    try {
        const timestamp = selectedDate.value.valueOf();
        histories.value = await fetchHistoryLogs(timestamp);
    } catch (error) {
        showMessage('error', 'Failed to fetch history logs. Please try again later.');
        console.error(error);
        histories.value = [];
    }
};

const onReadLogDetail = async (history: LogEntry) => {
    try {
        const content = await fetchLogDetail(history.filename);
        logContent.value = content;
        selectedLogDetail.value = history;
        isDrawerVisible.value = true;
    } catch (error) {
        showMessage('error', 'Error fetching log detail. Please try again later.');
        console.error(error);
    }
};

const handleDownload = async () => {
    if (!selectedLogDetail.value) return;
    try {
        await downloadLogFile(selectedLogDetail.value.filename);
        showMessage('success', 'File downloaded successfully!');
    } catch (error) {
        showMessage('error', 'Error downloading log file. Please try again later.');
        console.error(error);
    }
};

const onDateChange = () => {
    updateHistoryLogs();
};

// Fetch logs on mount
onMounted(updateHistoryLogs);
</script>

<style scoped>
/* Add any necessary styles */
</style>
