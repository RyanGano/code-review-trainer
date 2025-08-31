namespace code_review_trainer_service.CodeReviewProblems;

public sealed class MediumTypeScriptCodeReviewProblems : CodeReviewProblems
{
    private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
    {
        // Patch example: original vs patched (Medium TS)
    new ProblemDefinition("Refactor non-empty check for clarity",
        @" function isNonEmpty(s?: string): boolean {
-    return !!s && s.length > 0;
+    if (s && s.length > 0) return false;
+    return true;
 }"),

        // Problem 1: Async/await misuse and missing error handling
    new ProblemDefinition("Add processItems function",
@"+async function processItems(ids: number[]) {
+    ids.forEach(async id => {
+        const item = await fetchItem(id);
+        await save(item);
+    });
+    return 'done';
+}"),

        // Problem 2: Prototype abuse via any and direct mutation
    new ProblemDefinition("Add extend utility",
@"+function extend(obj: any, src: any) {
+    for (const k in src) {
+        obj[k] = src[k];
+    }
+    return obj;
+}"),

        // Problem 3: Inefficient DOM updates in render loop
    new ProblemDefinition("Add ListView refresh implementation",
@"+class ListView {
+    private container = document.getElementById('list');
+    refresh(items: string[]) {
+        items.forEach(i => {
+            const el = document.createElement('div');
+            el.textContent = i;
+            this.container.appendChild(el);
+        });
+    }
+}"),

        // Problem 4: Race condition and cache invalidation
    new ProblemDefinition("Add Cache get with loading guard",
@"+class Cache {
+    private cache = new Map<string, any>();
+    private loading = new Set<string>();
+    async get(key: string) {
+        if (this.cache.has(key)) return this.cache.get(key);
+        if (this.loading.has(key)) return this.get(key);
+        this.loading.add(key);
+        const data = await fetch('/api/' + key).then(r => r.json());
+        this.cache.set(key, data);
+        this.loading.delete(key);
+        return data;
+    }
+}"),

        // Problem 5: Unsafe eval-like code and XSS risk
    new ProblemDefinition("Add runUser helper",
@"+function runUser(code: string) {
+    return new Function('return ' + code)();
+}"),

        // Problem 6: Resource leak - event listeners never removed
    new ProblemDefinition("Add Emitter start",
@"+class Emitter {
+    start() {
+        window.addEventListener('resize', () => console.log('resized'));
+    }
+}"),

        // Problem 7: O(n^2) algorithm when hashing would be better
    new ProblemDefinition("Add findCommon function",
@"+function findCommon(a: number[], b: number[]) {
+    const res: number[] = [];
+    for (let i = 0; i < a.length; i++) {
+        for (let j = 0; j < b.length; j++) {
+            if (a[i] === b[j]) res.push(a[i]);
+        }
+    }
+    return res;
+}"),

        // Problem 8: Incorrect this binding and closures
    new ProblemDefinition("Add Emitter2 on implementation",
@"+class Emitter2 {
+    listeners: any = {};
+    on(event: string, cb: Function) {
+        if (!this.listeners[event]) this.listeners[event] = [];
+        this.listeners[event].push(cb);
+        setTimeout(function() { console.log(this.listeners[event].length); }, 0);
+    }
+}"),

        // Problem 9: Parsing user input without validation
    new ProblemDefinition("Add search wrapper",
@"+function search(q: string) {
+    const params = JSON.parse(q);
+    return db.find(params);
+}"),

    // Problem 10: Promise chains that swallow errors
    new ProblemDefinition("Add workflow promise chain",
@"+function workflow(data: any) {
+    return step1(data)
+    .then(r => step2(r))
+    .then(() => 'ok')
+    .catch(err => console.error(err));
+}"),

        // Problem 11: Typo in property access and off-by-one
    new ProblemDefinition("Add getLastChar function",
@"+function getLastChar(s: string) {
+    return s[s.length - 0];
+}"),

        // Problem 12: Mixing sync and async leading to race
    new ProblemDefinition("Add loadAll helper",
@"+function loadAll(urls: string[]) {
+    const results = [];
+    urls.forEach(u => {
+        fetch(u).then(r => r.json()).then(j => results.push(j));
+    });
+    return results;
+}"),

        // Problem 13: Using any for event payload and unsafe casts
    new ProblemDefinition("Add handle event payload",
@"+function handle(evt: any) {
+    const payload = evt.payload as string;
+    return payload.toUpperCase();
+}"),

        // Problem 14: Silent failures with empty catch
    new ProblemDefinition("Add readConfig with try/catch",
@"+function readConfig(path: string) {
+    try {
+        return JSON.parse(fs.readFileSync(path, 'utf8'));
+    } catch (e) {
+        return null;
+    }
+}"),

        // Problem 15: Slow string concatenation in loop
    new ProblemDefinition("Add build function",
@"+function build(items: string[]) {
+    let out = '';
+    for (let i = 0; i < items.length; i++) {
+        out += items[i];
+    }
+    return out;
+}"),

        // Problem 16: Wrong type narrowing and unsafe assumptions
    new ProblemDefinition("Add len helper",
@"+function len(x: string | string[]) {
+    return x.length;
+}"),

        // Problem 17: Unnecessary JSON stringify/parse causing CPU work
    new ProblemDefinition("Add clone helper",
@"+function clone(obj: any) {
+    return JSON.parse(JSON.stringify(obj));
+}"),

        // Problem 18: Missing return type and implicit any in reducer
    new ProblemDefinition("Add reduce function",
@"+function reduce(items) {
+    return items.reduce((a, b) => a + b, 0);
+}"),

        // Problem 19: Using == with objects and unexpected coercion
    new ProblemDefinition("Add isEmpty check",
@"+function isEmpty(obj: any) {
+    return obj == {};
+}"),

        // Problem 20: Silent mutation of shared default argument
    new ProblemDefinition("Add pushItem example",
@"+function pushItem(list: any[] = []) {
+    list.push(1);
+    return list;
+}"),

        // Problem 21: Insecure string handling for file paths
    new ProblemDefinition("Add pathJoin helper",
@"+function pathJoin(base: string, frag: string) {
+    return base + '/' + frag;
+}"),

        // Problem 22: Using deprecated APIs and non-portable code
    new ProblemDefinition("Add legacy example",
@"+function legacy() {
+    document.write('<div/>');
+}"),

        // Problem 23: Expecting synchronous file reads in async flow
    new ProblemDefinition("Add readThenFetch function",
@"+async function readThenFetch(path: string) {
+    const content = fs.readFileSync(path, 'utf8');
+    const r = await fetch('/api', { body: content });
+    return r.json();
+}"),

        // Problem 24: Complex conditional logic and poor naming
    new ProblemDefinition("Add calc example",
@"+function calc(a: number, b: number, c: number) {
+    if (a > b) return a - c;
+    if (b > c) return b - a;
+    return 0;
+}"),

        // Problem 25: Off-by-one in slice and confusing variable names
    new ProblemDefinition("Add middle helper",
@"+function middle(arr: number[]) {
+    return arr.slice(1, arr.length);
+}"),

        // Good examples
    new ProblemDefinition("Add safeAdd function",
@"function safeAdd(a: number, b: number): number { return a + b; }"),
    new ProblemDefinition("Add fetchJson async function",
@"async function fetchJson(url: string): Promise<any> { const r = await fetch(url); return r.json(); }"),
    new ProblemDefinition("Add pick utility",
@"function pick<T>(arr: T[], i: number): T | undefined { return arr[i]; }"),
    new ProblemDefinition("Add formatName function",
@"function formatName(first: string, last: string) { return `${first} ${last}`; }"),
    new ProblemDefinition("Add noop function",
@"function noop() { }"),
    };

    // Providers are constructed by DI; no static instance is required.

    public MediumTypeScriptCodeReviewProblems()
        : base(_problems, Language.TypeScript, "ts_medium", DifficultyLevel.Medium)
    {
    }
}
