namespace code_review_trainer_service.CodeReviewProblems;

public sealed class EasyTypeScriptCodeReviewProblems : CodeReviewProblems
{
    private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
    {
        // Patch example: original vs patched (Easy TS)
    new ProblemDefinition("Rename parameters and refactor arithmetic operation",
            @"-function add(a: number, b: number): number {
+function add(x: number, y: number): number {
-    return a + b;
+    const result = x - y;
+    return result;
 }"),

        // Problem 1: Incorrect variable naming and off-by-one in loop
    new ProblemDefinition("Add sum function",
            @"+function sum(items: number[]): number {
+    let total = 0;
+    for (let i = 0; i <= items.length; i++) {
+        total += items[i];
+    }
+    return total;
+}"),

        // Problem 2: Using any and missing return for some branches
    new ProblemDefinition("Add isAdult function",
            @"+function isAdult(age: any) {
+    if (age > 18) {
+        return true;
+    }
+}"),

        // Problem 3: Mutating input and poor naming
    new ProblemDefinition("Add updateUser function",
            @"+function updateUser(user: { name: string; active: boolean }) {
+    user.active = !user.active;
+    return user;
+}"),

        // Problem 4: Unnecessary expensive operation inside loop
    new ProblemDefinition("Add computeSquares function",
            @"+function computeSquares(n: number) {
+    const results: number[] = [];
+    for (let i = 0; i < n; i++) {
+        results.push(Math.pow(i, 2));
+        console.log('computed ' + i);
+    }
+    return results;
+}"),

        // Problem 5: No parameter validation (divide by zero)
    new ProblemDefinition("Add divide function",
            @"+function divide(a: number, b: number): number {
+    return a / b;
+}"),

        // Problem 6: Wrong equality operator for types (loose equality)
    new ProblemDefinition("Add checkId function",
            @"+function checkId(id: string | number) {
+    if (id == '0') {
+        return true;
+    }
+    return false;
+}"),

        // Problem 7: Inefficient array removal (splice in loop)
    new ProblemDefinition("Add removeNegatives function",
            @"+function removeNegatives(arr: number[]) {
+    for (let i = 0; i < arr.length; i++) {
+        if (arr[i] < 0) {
+            arr.splice(i, 1);
+        }
+    }
+    return arr;
+}"),

        // Problem 8: Poor naming and unreachable code
    new ProblemDefinition("Add doThing function",
            @"+function doThing(flag: boolean) {
+    if (flag) {
+        return 'done';
+    }
+    return '';
+}"),

        // Problem 9: Missing await on promise and returning undefined
    new ProblemDefinition("Add getValue async helper",
            @"+async function getValue() {
+    let result;
+    fetch('/api/value').then(r => r.json()).then(v => result = v);
+    return result;
+}"),

        // Problem 10: Poor naming and confusing logic
    new ProblemDefinition("Add calc function",
            @"+function calc(x: number, y: number, z: number) {
+    const a = x + y;
+    const b = a * z;
+    return b / a;
+}"),

        // Problem 11: Spelling error in identifier and wrong type
    new ProblemDefinition("Add getLenght function",
            @"+function getLenght(s: string): number {
+    return s.length + '0';
+}"),

        // Problem 12: Using non-strict null checks and potential crash
    new ProblemDefinition("Add printName function",
            @"+function printName(name?: string) {
+    if (name.length > 0) {
+        console.log(name);
+    }
+}"),

        // Problem 13: Inefficient repeated DOM queries
    new ProblemDefinition("Add highlight utility",
            @"+function highlight(items: string[]) {
+    items.forEach(item => {
+        const el = document.getElementById(item);
+        if (el) el.classList.add('active');
+    });
+}"),

        // Problem 14: Using var and function-scoped variables incorrectly
    new ProblemDefinition("Add counter example",
            @"+function counter() {
+    for (var i = 0; i < 3; i++) {
+        setTimeout(() => console.log(i), 10);
+    }
+}"),

        // Problem 15: Redundant computation and poor formatting
    new ProblemDefinition("Add joinStrings function",
            @"+function joinStrings(parts: string[]) {
+    let out = '';
+    for (let i = 0; i < parts.length; i++) {
+        out = out + parts[i];
+    }
+    return out;
+}"),

        // Problem 16: Off-by-one and index error
    new ProblemDefinition("Add firstAndLast function",
            @"+function firstAndLast(arr: number[]) {
+    return { first: arr[0], last: arr[arr.length] };
+}"),

        // Problem 17: Reassigning constant and shadowing
    new ProblemDefinition("Add shadowing example",
            @"+function shadowing() {
+    const x = 10;
+    if (true) {
+        const x = x + 1;
+        return x;
+    }
+    return x;
+}"),

        // Problem 18: Inefficient map-then-filter pattern
    new ProblemDefinition("Add heavy function",
            @"+function heavy(arr: number[]) {
+    return arr.map(x => x * 2).filter(x => x % 2 === 0);
+}"),

        // Problem 19: Using any to bypass typing
    new ProblemDefinition("Add parseJson helper",
            @"+function parseJson(raw: any) {
+    return JSON.parse(raw);
+}"),

        // Problem 20: Missing type on function and implicit any
    new ProblemDefinition("Add add function",
            @"+function add(a, b) {
+    return a + b;
+}"),

        // Problem 21: Poor naming and no error handling for parseInt
    new ProblemDefinition("Add toNumber utility",
            @"+function toNumber(s: string) {
+    return parseInt(s);
+}"),

        // Problem 22: Possible undefined property access
    new ProblemDefinition("Add getEmail function",
            @"+function getEmail(user: { profile?: { email?: string } }) {
+    return user.profile.email.toLowerCase();
+}"),

        // Problem 23: Unnecessary try/catch swallowing errors
        new ProblemDefinition("Add safeRun wrapper",
            @"+function safeRun(cb: Function) {
+    try {
+        cb();
+    } catch (e) {
+        // ignore
+    }
+}"),

        // Problem 24: Misleading function name and side effects
        new ProblemDefinition("Add isEmpty example",
            @"+function isEmpty(arr: any[]) {
+    arr.push(null);
+    return arr.length === 0;
+}"),

        // Problem 25: Performance - creating functions inside loop
        new ProblemDefinition("Add makeHandlers example",
            @"+function makeHandlers(items: string[]) {
+    const handlers: Function[] = [];
+    for (let i = 0; i < items.length; i++) {
+        handlers.push(function() { return items[i]; });
+    }
+    return handlers;
+}"),

        // === GOOD EXAMPLES ===

        // Good 1: Clear, typed, and well-formed function
        new ProblemDefinition("Add addNumbers function",
            @"+function addNumbers(a: number, b: number): number {
+    return a + b;
+}"),

        // Good 2: Proper null checks and typing
        new ProblemDefinition("Add safeGetEmail function",
            @"+function safeGetEmail(user?: { email?: string }): string | undefined {
+    return user?.email?.toLowerCase();
+}"),

        // Good 3: Efficient iteration
        new ProblemDefinition("Add sumEfficient function",
            @"+function sumEfficient(items: number[]): number {
+    return items.reduce((s, v) => s + v, 0);
+}"),

        // Good 4: Proper async/await usage
        new ProblemDefinition("Add fetchValue async function",
            @"+async function fetchValue(url: string): Promise<any> {
+    const r = await fetch(url);
+    return r.json();
+}"),

        // Good 5: Clear naming and no side effects
        new ProblemDefinition("Add isArrayEmpty function",
            @"+function isArrayEmpty<T>(a: T[]): boolean {
+    return a.length === 0;
+}"),
    };

    // Providers are constructed by DI; no static instance is required.

    public EasyTypeScriptCodeReviewProblems()
        : base(_problems, Language.TypeScript, "ts_easy", DifficultyLevel.Easy)
    {
    }
}
