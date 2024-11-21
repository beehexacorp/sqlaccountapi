declare module '*.svg' {
    const content: any;
    export default content;
}
declare module '*.svg?component' {
    import { DefineComponent } from 'vue';
    const component: DefineComponent<{}, {}, any>;
    export default component;
}