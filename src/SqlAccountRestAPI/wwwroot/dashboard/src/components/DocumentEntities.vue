<template>
    <div>
        <a-card title="BizObjects" :bordered="false">
            <a-empty v-if="!bizObjects?.length"></a-empty>
            <a-row v-if="!!bizObjects?.length" style="padding: 20px 0px;">
                <a-col v-for="(b, bix) in bizObjects" :key="bix" :xs="24" class="gutter-row">
                    <div style="padding: 5px">
                        <a href="javascript:void(0)" @click="() => onGetBizObjectDetail(b)">
                            {{ b.name }}
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
                        {{ selectedBizObjectDetail?.name }}
                    </span>
                </div>
            </template>
            <div v-if="selectedBizObjectDetail && selectedBizObjectDetail.datasets">
                <div v-for="(d, didx) in selectedBizObjectDetail.datasets" :key="didx">
                    <strong>{{ d.name }}</strong>
                    <div v-for="(f, fidx) in d.fields" :key="fidx">
                        {{ f }}
                    </div>
                </div>
            </div>
            <a-empty v-else></a-empty>


        </a-drawer>
    </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { message } from 'ant-design-vue';
import type {
    BizObjectDetail,
    BizObjectEntry,
} from '@/services/documentService';

import {
    fetchBizObject,
    fetchBizObjectDetail
} from '@/services/documentService';

const isDrawerVisible = ref(false);
const selectedBizObjectDetail = ref<BizObjectDetail | null>(null);
const bizObjects = ref<BizObjectEntry[]>([]);

// Helper to display a notification message
const showMessage = (type: 'success' | 'error', content: string) => {
    message[type]({
        content,
        duration: 10
    });
};

// Fetch history logs based on the selected date
const listOutEntities = async () => {
    try {
        bizObjects.value = await fetchBizObject();
    } catch (error) {
        showMessage('error', 'Failed to list out entites. Please try again later.');
        console.error(error);
        bizObjects.value = [];
    }
};
const onGetBizObjectDetail = async (b: BizObjectEntry) => {
    try {
        const content = await fetchBizObjectDetail(b.name);
        selectedBizObjectDetail.value = content;
        isDrawerVisible.value = true;
    } catch (error) {
        showMessage('error', 'Error fetching log detail. Please try again later.');
        console.error(error);
    }
}

// Fetch logs on mount
onMounted(listOutEntities);
</script>

<style scoped>
/* Add any necessary styles */
</style>
