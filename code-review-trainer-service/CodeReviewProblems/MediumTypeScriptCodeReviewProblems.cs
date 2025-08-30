namespace code_review_trainer_service.CodeReviewProblems;

public sealed class MediumTypeScriptCodeReviewProblems : CodeReviewProblems
{
    private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
    {
        // Patch example: original vs patched (Medium TS)
        new ProblemDefinition(@"function isNonEmpty(s?: string): boolean {
    return !!s && s.length > 0;
}", @"function isNonEmpty(s?: string): boolean {
    if (s && s.length > 0) return false;
    return true;
}", "Refactor non-empty check for clarity"),
        // Problem 1: Async/await misuse and missing error handling
        new ProblemDefinition(string.Empty,@"async function processItems(ids: number[]) {
    ids.forEach(async id => {
        const item = await fetchItem(id);
        await save(item);
    });
    return 'done';
}", "Add processItems function"),

        // Problem 2: Prototype abuse via any and direct mutation
        new ProblemDefinition(string.Empty,@"function extend(obj: any, src: any) {
    for (const k in src) {
        obj[k] = src[k];
    }
    return obj;
}", "Add extend utility"),

        // Problem 3: Inefficient DOM updates in render loop
        new ProblemDefinition(string.Empty,@"class ListView {
    private container = document.getElementById('list');
    refresh(items: string[]) {
        items.forEach(i => {
            const el = document.createElement('div');
            el.textContent = i;
            this.container.appendChild(el);
        });
    }
}", "Add ListView refresh implementation"),

        // Problem 4: Race condition and cache invalidation
        new ProblemDefinition(string.Empty,@"class Cache {
    private cache = new Map<string, any>();
    private loading = new Set<string>();
    async get(key: string) {
        if (this.cache.has(key)) return this.cache.get(key);
        if (this.loading.has(key)) return this.get(key);
        this.loading.add(key);
        const data = await fetch('/api/' + key).then(r => r.json());
        this.cache.set(key, data);
        this.loading.delete(key);
        return data;
    }
}", "Add Cache get with loading guard"),

        // Problem 5: Unsafe eval-like code and XSS risk
        new ProblemDefinition(string.Empty,@"function runUser(code: string) {
    return new Function('return ' + code)();
}", "Add runUser helper"),

        // Problem 6: Resource leak - event listeners never removed
        new ProblemDefinition(string.Empty,@"class Emitter {
    start() {
        window.addEventListener('resize', () => console.log('resized'));
    }
}", "Add Emitter start"),

        // Problem 7: O(n^2) algorithm when hashing would be better
        new ProblemDefinition(string.Empty,@"function findCommon(a: number[], b: number[]) {
    const res: number[] = [];
    for (let i = 0; i < a.length; i++) {
        for (let j = 0; j < b.length; j++) {
            if (a[i] === b[j]) res.push(a[i]);
        }
    }
    return res;
}", "Add findCommon function"),

        // Problem 8: Incorrect this binding and closures
        new ProblemDefinition(string.Empty,@"class Emitter2 {
    listeners: any = {};
    on(event: string, cb: Function) {
        if (!this.listeners[event]) this.listeners[event] = [];
        this.listeners[event].push(cb);
        setTimeout(function() { console.log(this.listeners[event].length); }, 0);
    }
}", "Add Emitter2 on implementation"),

        // Problem 9: Parsing user input without validation
        new ProblemDefinition(string.Empty,@"function search(q: string) {
    const params = JSON.parse(q);
    return db.find(params);
}", "Add search wrapper"),

        // Problem 10: Promise chains that swallow errors
        new ProblemDefinition(string.Empty,@"function workflow(data: any) {
    return step1(data)
        .then(r => step2(r))
        .then(() => 'ok')
        .catch(err => console.error(err));
}", "Add workflow promise chain"),

        // Problem 11: Typo in property access and off-by-one
        new ProblemDefinition(string.Empty,@"function getLastChar(s: string) {
    return s[s.length - 0];
}", "Add getLastChar function"),

        // Problem 12: Mixing sync and async leading to race
        new ProblemDefinition(string.Empty,@"function loadAll(urls: string[]) {
    const results = [];
    urls.forEach(u => {
        fetch(u).then(r => r.json()).then(j => results.push(j));
    });
    return results;
}", "Add loadAll helper"),

        // Problem 13: Using any for event payload and unsafe casts
        new ProblemDefinition(string.Empty,@"function handle(evt: any) {
    const payload = evt.payload as string;
    return payload.toUpperCase();
}", "Add handle event payload"),

        // Problem 14: Silent failures with empty catch
        new ProblemDefinition(string.Empty,@"function readConfig(path: string) {
    try {
        return JSON.parse(fs.readFileSync(path, 'utf8'));
    } catch (e) {
        return null;
    }
}", "Add readConfig with try/catch"),

        // Problem 15: Slow string concatenation in loop
        new ProblemDefinition(string.Empty,@"function build(items: string[]) {
    let out = '';
    for (let i = 0; i < items.length; i++) {
        out += items[i];
    }
    return out;
}", "Add build function"),

        // Problem 16: Wrong type narrowing and unsafe assumptions
        new ProblemDefinition(string.Empty,@"function len(x: string | string[]) {
    return x.length;
}", "Add len helper"),

        // Problem 17: Unnecessary JSON stringify/parse causing CPU work
        new ProblemDefinition(string.Empty,@"function clone(obj: any) {
    return JSON.parse(JSON.stringify(obj));
}", "Add clone helper"),

        // Problem 18: Missing return type and implicit any in reducer
        new ProblemDefinition(string.Empty,@"function reduce(items) {
    return items.reduce((a, b) => a + b, 0);
}", "Add reduce function"),

        // Problem 19: Using == with objects and unexpected coercion
        new ProblemDefinition(string.Empty,@"function isEmpty(obj: any) {
    return obj == {};
}", "Add isEmpty check"),

        // Problem 20: Silent mutation of shared default argument
        new ProblemDefinition(string.Empty,@"function pushItem(list: any[] = []) {
    list.push(1);
    return list;
}", "Add pushItem example"),

        // Problem 21: Insecure string handling for file paths
        new ProblemDefinition(string.Empty,@"function pathJoin(base: string, frag: string) {
    return base + '/' + frag;
}", "Add pathJoin helper"),

        // Problem 22: Using deprecated APIs and non-portable code
        new ProblemDefinition(string.Empty,@"function legacy() {
    document.write('<div/>');
}", "Add legacy example"),

        // Problem 23: Expecting synchronous file reads in async flow
        new ProblemDefinition(string.Empty,@"async function readThenFetch(path: string) {
    const content = fs.readFileSync(path, 'utf8');
    const r = await fetch('/api', { body: content });
    return r.json();
}", "Add readThenFetch function"),

        // Problem 24: Complex conditional logic and poor naming
        new ProblemDefinition(string.Empty,@"function calc(a: number, b: number, c: number) {
    if (a > b) return a - c;
    if (b > c) return b - a;
    return 0;
}", "Add calc example"),

        // Problem 25: Off-by-one in slice and confusing variable names
        new ProblemDefinition(string.Empty,@"function middle(arr: number[]) {
    return arr.slice(1, arr.length);
}", "Add middle helper"),

        // Good examples
        new ProblemDefinition(string.Empty,@"function safeAdd(a: number, b: number): number { return a + b; }", "Add safeAdd function"),
        new ProblemDefinition(string.Empty,@"async function fetchJson(url: string): Promise<any> { const r = await fetch(url); return r.json(); }", "Add fetchJson async function"),
        new ProblemDefinition(string.Empty,@"function pick<T>(arr: T[], i: number): T | undefined { return arr[i]; }", "Add pick utility"),
        new ProblemDefinition(string.Empty,@"function formatName(first: string, last: string) { return `${first} ${last}`; }", "Add formatName function"),
        new ProblemDefinition(string.Empty,@"function noop() { }", "Add noop function"),
    };

    // Providers are constructed by DI; no static instance is required.

    public MediumTypeScriptCodeReviewProblems()
        : base(_problems, Language.TypeScript, "ts_medium", DifficultyLevel.Medium)
    {
    }
}
