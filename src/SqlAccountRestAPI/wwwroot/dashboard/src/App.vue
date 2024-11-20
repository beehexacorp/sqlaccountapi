<script setup lang="ts">
import { defineAsyncComponent } from 'vue';
import { RouterLink, RouterView } from 'vue-router'

import 'ant-design-vue/dist/reset.css';
</script>

<template>
  <header>
    <div class="wrapper">
      <nav>
        <RouterLink to="/">Home</RouterLink>
      </nav>
    </div>
  </header>

  <RouterView />
</template>

<style scoped></style>
