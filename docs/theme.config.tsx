import { DocsThemeConfig } from "nextra-theme-docs";
import Image from "next/image";
import Logo from "../images/logo.png";
import React from "react";

const config: DocsThemeConfig = {
  logo: <Image src={Logo} alt="" height={48} />,
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
