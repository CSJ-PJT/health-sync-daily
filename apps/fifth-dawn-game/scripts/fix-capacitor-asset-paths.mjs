import { readFileSync, writeFileSync } from "node:fs";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";

const scriptDir = dirname(fileURLToPath(import.meta.url));
const appRoot = resolve(scriptDir, "..");
const distIndexPath = resolve(appRoot, "dist", "index.html");
const androidIndexPath = resolve(appRoot, "android", "app", "src", "main", "assets", "public", "index.html");

const distHtml = readFileSync(distIndexPath, "utf8")
  .replaceAll('src="/assets/', 'src="./assets/')
  .replaceAll('href="/assets/', 'href="./assets/')
  .replaceAll(' crossorigin', '');

writeFileSync(androidIndexPath, distHtml, "utf8");

console.log("Capacitor Android asset index.html paths normalized to relative assets.");
