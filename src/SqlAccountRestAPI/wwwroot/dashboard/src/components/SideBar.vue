<template>
    <a-menu :selectedKeys="selectedKeys" theme="dark" mode="inline">
        <a-menu-item key="1">
            <bar-chart-outlined />
            <span class="nav-text"><a href="/dashboard/">Home</a></span>
        </a-menu-item>
        <a-menu-item key="2">
            <video-camera-outlined />
            <span class="nav-text"><a href="/dashboard/documentation">Documentations</a></span>
        </a-menu-item>
    </a-menu>
</template>
<script lang="ts" setup>
import {
    UserOutlined,
    VideoCameraOutlined,
    UploadOutlined,
    BarChartOutlined,
    CloudOutlined,
    AppstoreOutlined,
    TeamOutlined,
    ShopOutlined,
} from '@ant-design/icons-vue';
defineProps({
    selectedKeys: {
        type: Array,
        required: false,
        default: () => { return [] }
    }
})
</script>