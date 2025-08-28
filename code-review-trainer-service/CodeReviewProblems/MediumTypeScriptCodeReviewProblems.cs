namespace code_review_trainer_service.CodeReviewProblems;

public static class MediumTypeScriptCodeReviewProblems
{
    private static readonly Random _random = new Random();

    private static readonly string[] _problems = new string[]
    {
        // Problem 1: Async/await misuse and missing error handling
        @"async function processItems(ids: number[]) {
    ids.forEach(async id => {
        const item = await fetchItem(id);
        await save(item);
    });
    return 'done';
}",

        // Problem 2: Prototype abuse via any and direct mutation
        @"function extend(obj: any, src: any) {
    for (const k in src) {
        obj[k] = src[k];
    }
    return obj;
}",

        // Problem 3: Inefficient DOM updates in render loop
        @"class ListView {
    private container = document.getElementById('list');
    refresh(items: string[]) {
        items.forEach(i => {
            const el = document.createElement('div');
            el.textContent = i;
            this.container.appendChild(el);
        });
    }
}",

        // Problem 4: Race condition and cache invalidation
        @"class Cache {
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
}",

        // Problem 5: Unsafe eval-like code and XSS risk
        @"function runUser(code: string) {
    return new Function('return ' + code)();
}",

        // Problem 6: Resource leak - event listeners never removed
        @"class Emitter {
    start() {
        window.addEventListener('resize', () => console.log('resized'));
    }
}",

        // Problem 7: O(n^2) algorithm when hashing would be better
        @"function findCommon(a: number[], b: number[]) {
    const res: number[] = [];
    for (let i = 0; i < a.length; i++) {
        for (let j = 0; j < b.length; j++) {
            if (a[i] === b[j]) res.push(a[i]);
        }
    }
    return res;
}",

        // Problem 8: Incorrect this binding and closures
        @"class Emitter2 {
    listeners: any = {};
    on(event: string, cb: Function) {
        if (!this.listeners[event]) this.listeners[event] = [];
        this.listeners[event].push(cb);
        setTimeout(function() { console.log(this.listeners[event].length); }, 0);
    }
}",

        // Problem 9: Parsing user input without validation
        @"function search(q: string) {
    const params = JSON.parse(q);
    return db.find(params);
}",

        // Problem 10: Promise chains that swallow errors
        @"function workflow(data: any) {
    return step1(data)
        .then(r => step2(r))
        .then(() => 'ok')
        .catch(err => console.error(err));
}",

        // Problem 11: Typo in property access and off-by-one
        @"function getLastChar(s: string) {
    return s[s.length - 0];
}",

        // Problem 12: Mixing sync and async leading to race
        @"function loadAll(urls: string[]) {
    const results = [];
    urls.forEach(u => {
        fetch(u).then(r => r.json()).then(j => results.push(j));
    });
    return results;
}",

        // Problem 13: Using any for event payload and unsafe casts
        @"function handle(evt: any) {
    const payload = evt.payload as string;
    return payload.toUpperCase();
}",

        // Problem 14: Silent failures with empty catch
        @"function readConfig(path: string) {
    try {
        return JSON.parse(fs.readFileSync(path, 'utf8'));
    } catch (e) {
        return null;
    }
}",

        // Problem 15: Slow string concatenation in loop
        @"function build(items: string[]) {
    let out = '';
    for (let i = 0; i < items.length; i++) {
        out += items[i];
    }
    return out;
}",

        // Problem 16: Wrong type narrowing and unsafe assumptions
        @"function len(x: string | string[]) {
    return x.length;
}",

        // Problem 17: Unnecessary JSON stringify/parse causing CPU work
        @"function clone(obj: any) {
    return JSON.parse(JSON.stringify(obj));
}",

        // Problem 18: Missing return type and implicit any in reducer
        @"function reduce(items) {
    return items.reduce((a, b) => a + b, 0);
}",

        // Problem 19: Using == with objects and unexpected coercion
        @"function isEmpty(obj: any) {
    return obj == {};
}",

        // Problem 20: Silent mutation of shared default argument
        @"function pushItem(list: any[] = []) {
    list.push(1);
    return list;
}",

        // Problem 21: Insecure string handling for file paths
        @"function pathJoin(base: string, frag: string) {
    return base + '/' + frag;
}",

        // Problem 22: Using deprecated APIs and non-portable code
        @"function legacy() {
    document.write('<div/>');
}",

        // Problem 23: Expecting synchronous file reads in async flow
        @"async function readThenFetch(path: string) {
    const content = fs.readFileSync(path, 'utf8');
    const r = await fetch('/api', { body: content });
    return r.json();
}",

        // Problem 24: Complex conditional logic and poor naming
        @"function calc(a: number, b: number, c: number) {
    if (a > b) return a - c;
    if (b > c) return b - a;
    return 0;
}",

        // Problem 25: Off-by-one in slice and confusing variable names
        @"function middle(arr: number[]) {
    return arr.slice(1, arr.length);
}",

        // Good examples
        @"function safeAdd(a: number, b: number): number { return a + b; }",
        @"async function fetchJson(url: string): Promise<any> { const r = await fetch(url); return r.json(); }",
        @"function pick<T>(arr: T[], i: number): T | undefined { return arr[i]; }",
        @"function formatName(first: string, last: string) { return `${first} ${last}`; }",
        @"function noop() { }",
    };

    public static string GetRandomProblem()
    {
        return _problems[_random.Next(_problems.Length)];
    }

    public static CodeReviewProblem GetRandomProblemWithId()
    {
        var index = _random.Next(_problems.Length);
        return new CodeReviewProblem
        {
            Id = $"ts_medium_{index + 1:D3}",
            Problem = _problems[index],
            Language = Language.TypeScript
        };
    }

    public static int Count => _problems.Length;
    public static string GetProblemByIndex(int index) => _problems[index];
}
