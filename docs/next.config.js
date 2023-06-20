const withNextra = require("nextra")({
  theme: "nextra-theme-docs",
  themeConfig: "./theme.config.tsx",
});

module.exports = withNextra({
  output: "export",
  distDir: "out",
  basePath: "/Confix",
  images: {
    unoptimized: true,
  },
});
