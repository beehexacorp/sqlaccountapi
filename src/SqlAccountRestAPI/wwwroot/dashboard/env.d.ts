/// <reference types="vite/client" />
interface ImportMetaEnv {
    readonly BASE_URL: string;
    readonly VITE_API_URL: string; // Add your custom environment variables here
    // Add more variables as needed
}

interface ImportMeta {
    readonly env: ImportMetaEnv;
}


declare module '*.svg?component' {
    import { FunctionalComponent, SVGAttributes } from 'vue';
    const src: FunctionalComponent<SVGAttributes>;
    export default src;
}

declare module '*.svg' {
    const src: string;
    export default src;
}