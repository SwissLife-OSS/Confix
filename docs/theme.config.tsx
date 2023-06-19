import { DocsThemeConfig } from "nextra-theme-docs";
import React from "react";

const config: DocsThemeConfig = {
  logo: <span>Confix</span>,
  project: {
    link: "https://github.com/swisslife-oss/confix",
  },
  docsRepositoryBase: "https://github.com/swisslife-oss/confix",
  useNextSeoProps() {
    return {
      titleTemplate: "%s - Confix",
    };
  },
};

export default config;
