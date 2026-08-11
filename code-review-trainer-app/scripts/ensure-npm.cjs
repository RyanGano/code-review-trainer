#!/usr/bin/env node
/*
 * Fails the command when anything other than npm is used to drive this project.
 *
 * Runs from `preinstall`/`prebuild` and from yarn's `yarn-path`, which means it
 * executes before node_modules exists. It must therefore have zero imports and
 * stay CommonJS (.cjs) — package.json sets "type": "module".
 */

// npm, pnpm and bun all set npm_config_user_agent to "<manager>/<version> ...".
// yarn invoked via `yarn-path` sets no user agent at all, so an empty value must
// be treated as "not npm" rather than assumed safe.
const agent = process.env.npm_config_user_agent || "";
const manager = agent.split("/")[0] || "yarn";

if (manager === "npm") {
  process.exit(0);
}

const attempted = process.argv.slice(2).join(" ").trim();

const equivalents = {
  install: "npm install",
  add: "npm install <package>       (add -D  ->  npm install -D <package>)",
  remove: "npm uninstall <package>",
  upgrade: "npm update",
  dlx: "npx <package>",
};

const verb = process.argv[2] || "install";
const suggestion =
  equivalents[verb] ||
  (verb ? `npm run ${verb}` : "npm install");

process.stderr.write(
  [
    "",
    `  This project uses npm. Detected package manager: ${manager}`,
    "",
    attempted ? `  You ran:   ${manager} ${attempted}` : `  You ran:   ${manager}`,
    `  Instead:   ${suggestion}`,
    "",
    "  Why this is enforced:",
    "    yarn 1.x is unmaintained and emits DEP0169/DEP0040 deprecation noise on",
    "    Node 22 that cannot be fixed. More importantly, Azure Static Web Apps",
    "    picks its package manager from the committed lockfile, so a stray",
    "    yarn.lock changes what actually gets deployed.",
    "",
    "  The repo commits package-lock.json and ignores yarn.lock.",
    "",
    "  If you are deliberately switching package managers, change these:",
    "    code-review-trainer-app/package.json   (packageManager, preinstall, prebuild)",
    "    code-review-trainer-app/.yarnrc        (yarn-path guard)",
    "    .yarnrc and .npmrc                     (repo root)",
    "    .gitignore                             (which lockfile is ignored)",
    "",
  ].join("\n") + "\n"
);

process.exit(1);
