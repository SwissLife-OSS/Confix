import { DocsThemeConfig } from "nextra-theme-docs";
import { Logo } from "./components/logo";
import React from "react";

const config: DocsThemeConfig = {
  logo: <Logo height="36"/>,
  project: {
    link: "https://github.com/swisslife-oss/confix",
  },
  docsRepositoryBase: "https://github.com/swisslife-oss/confix",
  useNextSeoProps() {
    return {
      titleTemplate: "%s - Confix",
    };
  },
  sidebar: { defaultMenuCollapseLevel: 1 },
};

export default config;
