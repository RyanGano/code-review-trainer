import { useEffect, useRef, useMemo, useState } from "react";
import CodeMirror from "@uiw/react-codemirror";
import { csharp } from "@replit/codemirror-lang-csharp";
import { javascript } from "@codemirror/lang-javascript";
import { ViewPlugin, Decoration } from "@codemirror/view";
import type { DecorationSet } from "@codemirror/view";
import { RangeSetBuilder } from "@codemirror/state";

interface Props {
  original: string | null | undefined;
  patched: string;
  // Optional unified patch string (lines prefixed with --- for removals and +++ for additions)
  patch?: string | null;
  language: "CSharp" | "JavaScript" | "TypeScript" | string;
  purpose?: string;
}

export default function UnifiedMergeView({
  original,
  patched,
  patch,
  language,
  purpose,
}: Props) {
  const leftRef = useRef<HTMLDivElement | null>(null);
  const rightRef = useRef<HTMLDivElement | null>(null);

  // Treat null/undefined/whitespace-only as "no original content"
  const hasOriginal = !!original && original.trim().length > 0;
  const hasPatch = !!patch && patch.trim().length > 0;

  const langExtension =
    language === "JavaScript" || language === "TypeScript"
      ? javascript()
      : csharp();
  useEffect(() => {
    // Keep this hook mounted on every render to preserve hook order.
    // If there's no original content, do nothing.
    if (!hasOriginal) return;

    const leftScroller =
      leftRef.current?.querySelector<HTMLElement>(".cm-scroller");
    const rightScroller =
      rightRef.current?.querySelector<HTMLElement>(".cm-scroller");
    const syncScroll = (src: HTMLElement, dst: HTMLElement | undefined) => {
      if (!dst) return;
      dst.scrollTop = src.scrollTop;
      dst.scrollLeft = src.scrollLeft;
    };

    const attach = (src: HTMLElement | null, dst: HTMLElement | null) => {
      if (!src || !dst) return;
      const handler = () => syncScroll(src, dst);
      src.addEventListener("scroll", handler);
      return () => src.removeEventListener("scroll", handler);
    };

    const leftCleanup = attach(leftScroller ?? null, rightScroller ?? null);
    const rightCleanup = attach(rightScroller ?? null, leftScroller ?? null);

    return () => {
      if (leftCleanup) leftCleanup();
      if (rightCleanup) rightCleanup();
    };
  }, [original, patched, hasOriginal]);

  const unifiedLines = useMemo(() => {
    const lines: { text: string; cls: string }[] = [];
    if (hasPatch) {
      // Parse the provided unified patch text directly
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

    // Fallback: build unified view from original/patched
    const oLines = (original || "").replace(/\r/g, "").split("\n");
    const pLines = patched.replace(/\r/g, "").split("\n");
    const max = Math.max(oLines.length, pLines.length);
    for (let i = 0; i < max; i++) {
      const o = oLines[i];
      const p = pLines[i];
      const oNum = i + 1;
      const pNum = i + 1;
      if (o === p) {
        lines.push({ text: `${oNum}:  ${o ?? ""}`, cls: "equal" });
      } else {
        if (typeof o !== "undefined") {
          lines.push({ text: `${oNum}: -${o}`, cls: "removed" });
        }
        if (typeof p !== "undefined") {
          lines.push({ text: `${pNum}: +${p}`, cls: "added" });
        }
      }
    }
    return lines;
  }, [original, patched, patch, hasPatch]);

  // Two view modes: 'unified' (default) and 'sideBySide' (original/new editors)
  // Three view modes: unified, side-by-side, and top-and-bottom (stacked)
  type ViewMode = "unified" | "sideBySide" | "topAndBottom";
  const viewModes: Record<string, ViewMode> = {
    UNIFIED: "unified",
    SIDE_BY_SIDE: "sideBySide",
    TOP_AND_BOTTOM: "topAndBottom",
  };

  // Always build the unified display text (shows all lines with +/- markers)
  const unifiedDisplayText = unifiedLines.map((l) => l.text ?? "").join("\n");

  // Reconstruct original/new texts from the unifiedLines so side-by-side shows
  // original (equal + removed) on the left and new (equal + added) on the right.
  const reconstructed = useMemo(() => {
    const origLines: string[] = [];
    const newLines: string[] = [];
    for (const l of unifiedLines) {
      const text = l.text ?? "";
      // strip leading numeric prefix like '12: ' if present
      const stripped = text.replace(/^\d+:\s?/, "").replace(/^[-+ ]?/, "");
      if (l.cls === "removed") {
        // left: removed (with '-'), right: omit (as it's not present in new)
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

  // Selected view state (ref + small state tick to force re-render on change)
  const selectedViewState = useRef<ViewMode>(viewModes.UNIFIED);
  const [, setTick] = useState<number>(0);

  const setView = (mode: ViewMode) => {
    selectedViewState.current = mode;
    setTick((t) => t + 1);
  };

  // CodeMirror plugin to add line classes based on line prefixes (--- => removed, +++ => added)
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
      {purpose && (
        <div className="patch-purpose">
          <strong>Purpose:</strong> <em>{purpose}</em>
        </div>
      )}

      <div className="patch-controls">
        <button
          onClick={() => setView(viewModes.UNIFIED)}
          className={
            selectedViewState.current === viewModes.UNIFIED ? "active" : ""
          }
        >
          Unified
        </button>
        <button
          onClick={() => setView(viewModes.SIDE_BY_SIDE)}
          className={
            selectedViewState.current === viewModes.SIDE_BY_SIDE ? "active" : ""
          }
        >
          Side-by-side
        </button>
        <button
          onClick={() => setView(viewModes.TOP_AND_BOTTOM)}
          className={
            selectedViewState.current === viewModes.TOP_AND_BOTTOM
              ? "active"
              : ""
          }
        >
          Top & Bottom
        </button>
      </div>

      {selectedViewState.current === viewModes.SIDE_BY_SIDE &&
      (hasOriginal || hasPatch) ? (
        <div className="merge-editors">
          <div className="editor-pane" ref={leftRef}>
            <div className="pane-title">Original</div>
            {allAdded ? (
              <div className="pane-empty" style={{ padding: 12 }}>
                <em>Code is entirely new</em>
              </div>
            ) : (
              <CodeMirror
                value={
                  hasOriginal ? original ?? "" : reconstructed.originalText
                }
                extensions={[langExtension, diffLineHighlighter]}
                editable={false}
                basicSetup={{ lineNumbers: true }}
              />
            )}
          </div>
          <div className="editor-pane" ref={rightRef}>
            <div className="pane-title">New</div>
            {allRemoved ? (
              <div className="pane-empty" style={{ padding: 12 }}>
                <em>Code was deleted</em>
              </div>
            ) : (
              <CodeMirror
                value={
                  patched && patched.length > 0
                    ? patched
                    : reconstructed.newText
                }
                extensions={[langExtension, diffLineHighlighter]}
                editable={false}
                basicSetup={{ lineNumbers: true }}
              />
            )}
          </div>
        </div>
      ) : selectedViewState.current === viewModes.TOP_AND_BOTTOM &&
        (hasOriginal || hasPatch) ? (
        <div className="merge-editors stacked">
          <div className="editor-pane stacked-top" ref={leftRef}>
            <div className="pane-title">Original</div>
            {allAdded ? (
              <div className="pane-empty" style={{ padding: 12 }}>
                <em>Code is entirely new</em>
              </div>
            ) : (
              <CodeMirror
                value={
                  hasOriginal ? original ?? "" : reconstructed.originalText
                }
                extensions={[langExtension, diffLineHighlighter]}
                editable={false}
                basicSetup={{ lineNumbers: true }}
              />
            )}
          </div>
          <div className="editor-pane stacked-bottom" ref={rightRef}>
            <div className="pane-title">New</div>
            {allRemoved ? (
              <div className="pane-empty" style={{ padding: 12 }}>
                <em>Code was deleted</em>
              </div>
            ) : (
              <CodeMirror
                value={
                  patched && patched.length > 0
                    ? patched
                    : reconstructed.newText
                }
                extensions={[langExtension, diffLineHighlighter]}
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
            extensions={[langExtension, diffLineHighlighter]}
            editable={false}
            basicSetup={{ lineNumbers: true }}
          />
        </div>
      )}
    </div>
  );
}
