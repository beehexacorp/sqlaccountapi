<template>
  <a-layout has-sider>
    <a-layout-sider :style="{ overflow: 'auto', height: '100vh', position: 'fixed', left: 0, top: 0, bottom: 0 }">
      <div class="ant-layout-sider-logo">
        <Logo />
      </div>
      <side-bar :selectedKeys="selectedKeys"></side-bar>
    </a-layout-sider>
    <a-layout :style="{ marginLeft: '200px' }">
      <!-- <a-layout-header style="color: var(--color-light); padding:  0 10px; background-color: var(--bg-color-dark);">
        Header
      </a-layout-header> -->
      <a-layout-content :style="{ margin: '24px 16px 0', overflow: 'initial' }">
        <!-- TODO: create component Home.vue and use router -->
        <div :style="{ padding: '24px', textAlign: 'center' }">

          <a-list size="small" bordered :data-source="messages">
            <template #renderItem="{ item }">
              <a-list-item>
                <span style="font-weight: 700;">{{ item.logLevel }}</span>
                <pre style="white-space: pre-wrap;">{{ item.message }}</pre>
              </a-list-item>
            </template>
            <template #header>
              <div>Application Log</div>
            </template>
          </a-list>
          <!-- <pre style="width: 100%; text-align: left;">
  <span v-for="(m, mix) in messages" :key="mix">
    {{ m }}
  </span>
</pre> -->
        </div>
      </a-layout-content>
      <!-- TODO: create component Footer.vue -->
      <a-layout-footer :style="{ textAlign: 'center' }">
        SQL Accounting Rest API ©2024 - Created by HexaSync Inc.
      </a-layout-footer>
    </a-layout>
  </a-layout>
</template>
<script lang="ts" setup>
import { defineAsyncComponent, ref, onMounted } from 'vue';
import { startConnection, onReceiveLog } from '../signalr';
import Logo from '../assets/logo.svg?component'
// const Sidebar = defineAsyncComponent(() => import('../components/Sidebar.vue'))
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

const selectedKeys = ref<string[]>(['1']);
const messages = ref<{ logLevel: string, message: string, ts: number }[]>([])
// TODO: display list of log histories files so people can see the log histories and also the real-time log
// TODO: improve UX for this page
// TODO: use virtual list instead of ant-design list for better performance (please google/chatgpt)
onMounted(async () => {
  await startConnection();
  onReceiveLog((logLevel: string, message: string, ts: number) => {
    messages.value.push({ logLevel, message, ts })
    if (messages.value.length > 10000) {
      messages.value.slice(messages.value.length - 10000);
    }
  })
})
</script>
<style scoped></style>
