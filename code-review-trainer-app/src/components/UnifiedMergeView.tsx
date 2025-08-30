import { useEffect, useRef, useMemo } from "react";
import CodeMirror from "@uiw/react-codemirror";
import { csharp } from "@replit/codemirror-lang-csharp";
import { javascript } from "@codemirror/lang-javascript";

interface Props {
  original: string | null | undefined;
  patched: string;
  language: "CSharp" | "JavaScript" | "TypeScript" | string;
  purpose?: string;
}

export default function UnifiedMergeView({
  original,
  patched,
  language,
  purpose,
}: Props) {
  const leftRef = useRef<HTMLDivElement | null>(null);
  const rightRef = useRef<HTMLDivElement | null>(null);

  // Treat null/undefined/whitespace-only as "no original content"
  const hasOriginal = !!original && original.trim().length > 0;

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
    const oLines = (original || "").replace(/\r/g, "").split("\n");
    const pLines = patched.replace(/\r/g, "").split("\n");
    const max = Math.max(oLines.length, pLines.length);
    const lines: { text: string; cls: string }[] = [];
    for (let i = 0; i < max; i++) {
      const o = oLines[i];
      const p = pLines[i];
      const oNum = i + 1;
      const pNum = i + 1;
      if (o === p) {
        lines.push({ text: `${oNum}:  ${o ?? ""}`, cls: "equal" });
      } else {
        if (typeof o !== "undefined") {
          lines.push({ text: `${oNum}: ---${o}`, cls: "removed" });
        }
        if (typeof p !== "undefined") {
          lines.push({ text: `${pNum}: +++${p}`, cls: "added" });
        }
      }
    }
    return lines;
  }, [original, patched]);

  // If there's no original content, fall back to the old single-editor UI.
  if (!hasOriginal) {
    return (
      <div className="merge-view">
        {purpose && (
          <div className="patch-purpose">
            <strong>Purpose:</strong> <em>{purpose}</em>
          </div>
        )}
        <div className="merge-editors">
          <div className="editor-pane single-pane">
            <div className="pane-title">Code</div>
            <CodeMirror
              value={patched}
              extensions={[langExtension]}
              editable={false}
              basicSetup={{
                lineNumbers: true,
                foldGutter: true,
                dropCursor: false,
                allowMultipleSelections: false,
                indentOnInput: false,
                bracketMatching: true,
                closeBrackets: false,
                autocompletion: false,
                highlightSelectionMatches: false,
                searchKeymap: false,
              }}
            />
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="merge-view">
      {purpose && (
        <div className="patch-purpose">
          <strong>Purpose:</strong> <em>{purpose}</em>
        </div>
      )}
      <div className="merge-editors">
        <div className="editor-pane" ref={leftRef}>
          <div className="pane-title">Original</div>
          <CodeMirror
            value={original ?? ""}
            extensions={[langExtension]}
            editable={false}
            basicSetup={{ lineNumbers: true }}
          />
        </div>
        <div className="editor-pane" ref={rightRef}>
          <div className="pane-title">Changed</div>
          <CodeMirror
            value={patched}
            extensions={[langExtension]}
            editable={false}
            basicSetup={{ lineNumbers: true }}
          />
        </div>
      </div>

      <div className="merge-unified">
        <div className="pane-title">Unified Diff</div>
        <div className="unified-diff" role="region" aria-label="Unified diff">
          {unifiedLines.map((l, idx) => (
            <div key={idx} className={`unified-line ${l.cls}`}>
              <span className="line-text">{l.text}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
