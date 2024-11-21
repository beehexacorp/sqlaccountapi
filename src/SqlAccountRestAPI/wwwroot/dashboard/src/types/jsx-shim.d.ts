/// <reference types="vite/client" />

import { VNode } from 'vue';

declare global {
  namespace JSX {
    interface IntrinsicElements {
      [elemName: string]: any;
    }
  }
}