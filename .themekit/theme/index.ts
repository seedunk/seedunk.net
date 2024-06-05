// https://vitepress.dev/guide/custom-theme
import { h } from 'vue'
import type { Theme } from 'themekit-js'
import DefaultTheme from 'themekit-js/theme'
import './style.css'

import Layout from './Layout.vue'
export default {
  extends: DefaultTheme,
  Layout:Layout,
  enhanceApp({ app, router, siteData }) {
    // ...
  }
} satisfies Theme
