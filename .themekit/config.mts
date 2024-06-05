import { defineConfig } from 'themekit-js'

 
const siteBase="/seedunk.net/"
export default defineConfig({
  title: "Seedunk.Net",  
  base:siteBase,
  themeName: "default theme + customization",
  description: "A VitePress Site",
  
  themeConfig: { 
    nav: [ ], 
    socialLinks: [
      { icon: 'github', link: 'https://github.com/seedunk/seedunk.net/' }
      ],
    footer: {
      message: '',
      copyright: 'Copyright © 2019-2024 Seedunk'
    }
  }
})
