import { fileURLToPath, URL } from 'node:url';
import { defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import vueJsx from '@vitejs/plugin-vue-jsx';
import vueDevTools from 'vite-plugin-vue-devtools';
import nightwatchPlugin from 'vite-plugin-nightwatch';
import Components from 'unplugin-vue-components/vite';
import tsconfigPaths from 'vite-tsconfig-paths';
import svgLoader from 'vite-svg-loader';
import { AntDesignVueResolver } from 'unplugin-vue-components/resolvers';
import * as dotenv from 'dotenv';

// Load environment variables manually
dotenv.config();

console.log("Base Directory", process.env.VITE_BASE || '/dashboard/dist/')
console.log("Port", Number(process.env.VITE_PORT) || 3000)
console.log(fileURLToPath(new URL('./src', import.meta.url)));

// https://vite.dev/config/
export default defineConfig({
  base: '/dashboard',
  server: {
    port: Number(process.env.VITE_PORT) || 3000,
  },
  plugins: [
    vue(),
    vueJsx(),
    tsconfigPaths(),
    svgLoader(),
    vueDevTools(),
    nightwatchPlugin(),
    Components({
      resolvers: [
        AntDesignVueResolver({
          importStyle: false, // css in js
        }),
      ],
    }),
  ],
  build: {
    rollupOptions: {
      onwarn(warning, warn) {
        // Suppress specific warnings
        if (warning.code === 'UNRESOLVED_IMPORT' || warning.message.includes('@microsoft/signalr')) {
          return;
        }
        warn(warning); // Default behavior for other warnings
      },
      output: {
        manualChunks(id) {
          if (id.includes('node_modules')) {
            
            // Handle shared utilities of ant-design-vue
            // Dynamically split ant-design-vue components
            if (id.includes('ant-design-vue')) {
              return 'ant-design-vue';
            }

            // Force splitting of Vue core libraries
            if (id.includes('@vue')) {
              return 'vue-core';
            }

            // Force splitting of other large dependencies
            if (id.includes('moment')) {
              return 'moment';
            }
            if (id.includes('lodash')) {
              return 'lodash';
            }
            if (id.includes('axios')) {
              return 'axios';
            }

            // Group dependencies from the same namespace
            const namespaceMatch = id.match(/node_modules\/@([^/]+)\//);
            if (namespaceMatch) {
              return `vendor-${namespaceMatch[1]}`;
            }

            // Group other libraries by their folder
            const libraryMatch = id.match(/node_modules\/([^/]+)\//);
            if (libraryMatch) {
              return `vendor-${libraryMatch[1]}`;
            }

            // Default vendor chunk for remaining libraries
            return 'vendor';
          }
        },
        chunkFileNames: 'chunks/[name]-[hash].js', // Specify the output folder for chunks
      },
    },
    outDir: 'dist', // Ensure the output folder matches the base
    assetsDir: 'assets',
  },
  optimizeDeps: {
    include: ['ant-design-vue'],
    exclude: ['@ant-design/icons-vue'],
  },
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
});