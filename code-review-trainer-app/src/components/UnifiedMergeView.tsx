import { useMemo, useState } from "react";
import CodeMirror from "@uiw/react-codemirror";
import { csharp } from "@replit/codemirror-lang-csharp";
import { javascript } from "@codemirror/lang-javascript";
import { oneDark } from "@codemirror/theme-one-dark";
import { ViewPlugin, Decoration, EditorView } from "@codemirror/view";
import type { DecorationSet } from "@codemirror/view";
import { RangeSetBuilder } from "@codemirror/state";

interface Props {
  // Patch string (lines prefixed with - for removals and + for additions)
  patch?: string | null;
  language: "CSharp" | "JavaScript" | "TypeScript" | string;
  purpose?: string;
}

export default function UnifiedMergeView({ patch, language, purpose }: Props) {
  const hasPatch = !!patch && patch.trim().length > 0;
  // Detect dark mode
  const isDark = window.matchMedia("(prefers-color-scheme: dark)").matches;

  const langExtension =
    language === "JavaScript" || language === "TypeScript"
      ? javascript()
      : csharp();

  // Create a theme extension that provides consistent syntax highlighting for all diff lines
  const diffAwareTheme = useMemo(
    () =>
      EditorView.theme({
        // Apply consistent syntax highlighting colors to all unified diff lines
        ".unified-line .ͼb": { color: "#60a5fa" }, // Type color: light blue (consistent across all lines)
        ".unified-line .ͼc": { color: "#fbbf24" }, // Keywords: yellow
        ".unified-line .ͼd": { color: "#34d399" }, // Strings: green
        ".unified-line .ͼe": { color: "#9ca3af" }, // Comments: gray
        ".unified-line .ͼf": { color: "#e5e7eb" }, // Variables: light gray
      }),
    []
  );

  const unifiedLines = useMemo(() => {
    const lines: { text: string; cls: string }[] = [];
    if (hasPatch) {
      const raw = (patch || "").replace(/\r/g, "").split("\n");
      for (const l of raw) {
        if (l.startsWith("-")) {
          lines.push({ text: l, cls: "removed" });
        } else if (l.startsWith("+")) {
          lines.push({ text: l, cls: "added" });
        } else {
          lines.push({ text: l, cls: "equal" });
        }
      }
      return lines;
    }
    return [];
  }, [patch, hasPatch]);

  const reconstructed = useMemo(() => {
    const origLines: string[] = [];
    const newLines: string[] = [];
    for (const l of unifiedLines) {
      const text = l.text ?? "";
      const stripped = text.replace(/^[-+ ]?/, "");
      if (l.cls === "removed") {
        // left: removed (with '-'), right: omit
        origLines.push("-" + stripped);
      } else if (l.cls === "added") {
        // left: omit, right: added (with '+')
        newLines.push("+" + stripped);
      } else {
        // equal: appear in both with a leading space marker
        origLines.push(" " + stripped);
        newLines.push(" " + stripped);
      }
    }
    return { originalText: origLines.join("\n"), newText: newLines.join("\n") };
  }, [unifiedLines]);

  // Detect if the unified view represents only added lines (entirely new file)
  const allAdded = useMemo(() => {
    return (
      unifiedLines.length > 0 && unifiedLines.every((l) => l.cls === "added")
    );
  }, [unifiedLines]);

  // Detect if the unified view represents only removed lines (file deletion)
  const allRemoved = useMemo(() => {
    return (
      unifiedLines.length > 0 && unifiedLines.every((l) => l.cls === "removed")
    );
  }, [unifiedLines]);

  const [selectedView, setSelectedView] = useState<
    "unified" | "sideBySide" | "topAndBottom"
  >("unified");

  const unifiedDisplayText = unifiedLines.map((l) => l.text ?? "").join("\n");

  const diffLineHighlighter = useMemo(() => {
    return ViewPlugin.fromClass(
      class {
        // Decorations set for the plugin
        decorations: DecorationSet | undefined;
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        constructor(view: any) {
          this.decorations = this.buildDecorations(view as unknown);
        }
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        update(update: any) {
          if (update.docChanged || update.viewportChanged) {
            this.decorations = this.buildDecorations(update.view as unknown);
          }
        }
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        buildDecorations(view: any) {
          const builder = new RangeSetBuilder<Decoration>();
          for (const range of view.visibleRanges) {
            let pos = range.from;
            while (pos <= range.to) {
              const line = view.state.doc.lineAt(pos);
              const text = line.text || "";
              if (text.startsWith("-")) {
                builder.add(
                  line.from,
                  line.from,
                  Decoration.line({ class: "unified-line removed" })
                );
              } else if (text.startsWith("+")) {
                builder.add(
                  line.from,
                  line.from,
                  Decoration.line({ class: "unified-line added" })
                );
              } else {
                builder.add(
                  line.from,
                  line.from,
                  Decoration.line({ class: "unified-line equal" })
                );
              }
              pos = line.to + 1;
            }
          }
          return builder.finish();
        }
      },
      {
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        decorations: (v: any) => v.decorations,
      }
    );
    // Intentionally no dependencies here; plugin rebuilds when editor content changes
  }, []);

  return (
    <div className="merge-view">
      {!hasPatch && (
        <div className="error-message">
          Error: No code patch data available. Please try loading a new code
          sample.
        </div>
      )}

      {purpose && (
        <div className="patch-purpose">
          <strong>Purpose:</strong> <em>{purpose}</em>
        </div>
      )}

      {hasPatch && (
        <>
          <div className="patch-controls">
            <button
              onClick={() => setSelectedView("unified")}
              className={selectedView === "unified" ? "active" : ""}
            >
              Unified
            </button>
            <button
              onClick={() => setSelectedView("sideBySide")}
              className={selectedView === "sideBySide" ? "active" : ""}
            >
              Side-by-side
            </button>
            <button
              onClick={() => setSelectedView("topAndBottom")}
              className={selectedView === "topAndBottom" ? "active" : ""}
            >
              Top & Bottom
            </button>
          </div>

          {selectedView === "sideBySide" ? (
            <div className="merge-editors">
              <div className="editor-pane">
                <div className="pane-title">Original</div>
                {allAdded ? (
                  <div className="pane-empty" style={{ padding: 12 }}>
                    <em>Code is entirely new</em>
                  </div>
                ) : (
                  <CodeMirror
                    value={reconstructed.originalText}
                    extensions={[
                      langExtension,
                      diffLineHighlighter,
                      diffAwareTheme,
                    ]}
                    theme={isDark ? oneDark : undefined}
                    editable={false}
                    basicSetup={{ lineNumbers: true }}
                  />
                )}
              </div>
              <div className="editor-pane">
                <div className="pane-title">New</div>
                {allRemoved ? (
                  <div className="pane-empty" style={{ padding: 12 }}>
                    <em>Code was deleted</em>
                  </div>
                ) : (
                  <CodeMirror
                    value={reconstructed.newText}
                    extensions={[
                      langExtension,
                      diffLineHighlighter,
                      diffAwareTheme,
                    ]}
                    theme={isDark ? oneDark : undefined}
                    editable={false}
                    basicSetup={{ lineNumbers: true }}
                  />
                )}
              </div>
            </div>
          ) : selectedView === "topAndBottom" ? (
            <div className="merge-editors stacked">
              <div className="editor-pane stacked-top">
                <div className="pane-title">Original</div>
                {allAdded ? (
                  <div className="pane-empty" style={{ padding: 12 }}>
                    <em>Code is entirely new</em>
                  </div>
                ) : (
                  <CodeMirror
                    value={reconstructed.originalText}
                    extensions={[
                      langExtension,
                      diffLineHighlighter,
                      diffAwareTheme,
                    ]}
                    theme={isDark ? oneDark : undefined}
                    editable={false}
                    basicSetup={{ lineNumbers: true }}
                  />
                )}
              </div>
              <div className="editor-pane stacked-bottom">
                <div className="pane-title">New</div>
                {allRemoved ? (
                  <div className="pane-empty" style={{ padding: 12 }}>
                    <em>Code was deleted</em>
                  </div>
                ) : (
                  <CodeMirror
                    value={reconstructed.newText}
                    extensions={[
                      langExtension,
                      diffLineHighlighter,
                      diffAwareTheme,
                    ]}
                    theme={isDark ? oneDark : undefined}
                    editable={false}
                    basicSetup={{ lineNumbers: true }}
                  />
                )}
              </div>
            </div>
          ) : (
            <div className="merge-single-editor">
              <CodeMirror
                value={unifiedDisplayText}
                extensions={[
                  langExtension,
                  diffLineHighlighter,
                  diffAwareTheme,
                ]}
                theme={isDark ? oneDark : undefined}
                editable={false}
                basicSetup={{ lineNumbers: true }}
              />
            </div>
          )}
        </>
      )}
    </div>
  );
}
