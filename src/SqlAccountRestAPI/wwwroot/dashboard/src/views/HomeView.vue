<template>
  <a-layout has-sider>
    <a-layout-sider :style="{ overflow: 'auto', height: '100vh', position: 'fixed', left: 0, top: 0, bottom: 0 }">
      <site-logo></site-logo>
      <side-bar :selectedKeys="selectedKeys"></side-bar>
    </a-layout-sider>
    <a-layout :style="{ marginLeft: '200px', display: 'flex', flexDirection: 'column', height: '100vh' }">
      <a-layout-content :style="{ margin: '24px 16px 0', overflow: 'auto', flex: 1, display: 'flex',flexDirection: 'column',  }">
        <history-logs></history-logs>
        <a-divider style="margin: 10px 0;" />
        <realtime-log style="flex: 1"></realtime-log>
      </a-layout-content>
      <a-layout-footer :style="{ textAlign: 'center' }">
        SQL Account Rest API ©2024 - Created by HexaSync Inc.
      </a-layout-footer>
    </a-layout>
  </a-layout>
</template>

<script lang="ts" setup>
import { ref } from 'vue';
const selectedKeys = ref<string[]>(['1']);
</script>

<style scoped></style>
