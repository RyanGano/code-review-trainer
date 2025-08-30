namespace code_review_trainer_service.CodeReviewProblems;

public sealed class EasyTypeScriptCodeReviewProblems : CodeReviewProblems
{
    private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
    {
        // Patch example: original vs patched (Easy TS)
        new ProblemDefinition(@"function add(a: number, b: number): number {
    return a + b;
}", @"function add(x: number, y: number): number {
    const result = x - y;
    return result;
}", "Rename parameters and refactor arithmetic operation"),
        // Problem 1: Incorrect variable naming and off-by-one in loop
        new ProblemDefinition(string.Empty,@"function sum(items: number[]): number {
    let total = 0;
    for (let i = 0; i <= items.length; i++) {
        total += items[i];
    }
    return total;
}", "Add sum function"),

        // Problem 2: Using any and missing return for some branches
        new ProblemDefinition(string.Empty,@"function isAdult(age: any) {
    if (age > 18) {
        return true;
    }
}", "Add isAdult function"),

        // Problem 3: Mutating input and poor naming
        new ProblemDefinition(string.Empty,@"function updateUser(user: { name: string; active: boolean }) {
    user.active = !user.active;
    return user;
}", "Add updateUser function"),

        // Problem 4: Unnecessary expensive operation inside loop
        new ProblemDefinition(string.Empty,@"function computeSquares(n: number) {
    const results: number[] = [];
    for (let i = 0; i < n; i++) {
        results.push(Math.pow(i, 2));
        console.log('computed ' + i);
    }
    return results;
}", "Add computeSquares function"),

        // Problem 5: No parameter validation (divide by zero)
        new ProblemDefinition(string.Empty,@"function divide(a: number, b: number): number {
    return a / b;
}", "Add divide function"),

        // Problem 6: Wrong equality operator for types (loose equality)
        new ProblemDefinition(string.Empty,@"function checkId(id: string | number) {
    if (id == '0') {
        return true;
    }
    return false;
}", "Add checkId function"),

        // Problem 7: Inefficient array removal (splice in loop)
        new ProblemDefinition(string.Empty,@"function removeNegatives(arr: number[]) {
    for (let i = 0; i < arr.length; i++) {
        if (arr[i] < 0) {
            arr.splice(i, 1);
        }
    }
    return arr;
}", "Add removeNegatives function"),

        // Problem 8: Poor naming and unreachable code
        new ProblemDefinition(string.Empty,@"function doThing(flag: boolean) {
    if (flag) {
        return 'done';
    }
    return '';
}", "Add doThing function"),

        // Problem 9: Missing await on promise and returning undefined
        new ProblemDefinition(string.Empty,@"async function getValue() {
    let result;
    fetch('/api/value').then(r => r.json()).then(v => result = v);
    return result;
}", "Add getValue async helper"),

        // Problem 10: Poor naming and confusing logic
        new ProblemDefinition(string.Empty,@"function calc(x: number, y: number, z: number) {
    const a = x + y;
    const b = a * z;
    return b / a;
}", "Add calc function"),

        // Problem 11: Spelling error in identifier and wrong type
        new ProblemDefinition(string.Empty,@"function getLenght(s: string): number {
    return s.length + '0';
}", "Add getLenght function"),

        // Problem 12: Using non-strict null checks and potential crash
        new ProblemDefinition(string.Empty,@"function printName(name?: string) {
    if (name.length > 0) {
        console.log(name);
    }
}", "Add printName function"),

        // Problem 13: Inefficient repeated DOM queries
        new ProblemDefinition(string.Empty,@"function highlight(items: string[]) {
    items.forEach(item => {
        const el = document.getElementById(item);
        if (el) el.classList.add('active');
    });
}", "Add highlight utility"),

        // Problem 14: Using var and function-scoped variables incorrectly
        new ProblemDefinition(string.Empty,@"function counter() {
    for (var i = 0; i < 3; i++) {
        setTimeout(() => console.log(i), 10);
    }
}", "Add counter example"),

        // Problem 15: Redundant computation and poor formatting
        new ProblemDefinition(string.Empty,@"function joinStrings(parts: string[]) {
    let out = '';
    for (let i = 0; i < parts.length; i++) {
        out = out + parts[i];
    }
    return out;
}", "Add joinStrings function"),

        // Problem 16: Off-by-one and index error
        new ProblemDefinition(string.Empty,@"function firstAndLast(arr: number[]) {
    return { first: arr[0], last: arr[arr.length] };
}", "Add firstAndLast function"),

        // Problem 17: Reassigning constant and shadowing
        new ProblemDefinition(string.Empty,@"function shadowing() {
    const x = 10;
    if (true) {
        const x = x + 1;
        return x;
    }
    return x;
}", "Add shadowing example"),

        // Problem 18: Inefficient map-then-filter pattern
        new ProblemDefinition(string.Empty,@"function heavy(arr: number[]) {
    return arr.map(x => x * 2).filter(x => x % 2 === 0);
}", "Add heavy function"),

        // Problem 19: Using any to bypass typing
        new ProblemDefinition(string.Empty,@"function parseJson(raw: any) {
    return JSON.parse(raw);
}", "Add parseJson helper"),

        // Problem 20: Missing type on function and implicit any
        new ProblemDefinition(string.Empty,@"function add(a, b) {
    return a + b;
}", "Add add function"),

        // Problem 21: Poor naming and no error handling for parseInt
        new ProblemDefinition(string.Empty,@"function toNumber(s: string) {
    return parseInt(s);
}", "Add toNumber utility"),

        // Problem 22: Possible undefined property access
        new ProblemDefinition(string.Empty,@"function getEmail(user: { profile?: { email?: string } }) {
    return user.profile.email.toLowerCase();
}", "Add getEmail function"),

        // Problem 23: Unnecessary try/catch swallowing errors
        new ProblemDefinition(string.Empty,@"function safeRun(cb: Function) {
    try {
        cb();
    } catch (e) {
        // ignore
    }
}", "Add safeRun wrapper"),

        // Problem 24: Misleading function name and side effects
        new ProblemDefinition(string.Empty,@"function isEmpty(arr: any[]) {
    arr.push(null);
    return arr.length === 0;
}", "Add isEmpty example"),

        // Problem 25: Performance - creating functions inside loop
        new ProblemDefinition(string.Empty,@"function makeHandlers(items: string[]) {
    const handlers: Function[] = [];
    for (let i = 0; i < items.length; i++) {
        handlers.push(function() { return items[i]; });
    }
    return handlers;
}", "Add makeHandlers example"),

        // === GOOD EXAMPLES ===

        // Good 1: Clear, typed, and well-formed function
        new ProblemDefinition(string.Empty,@"function addNumbers(a: number, b: number): number {
    return a + b;
}", "Add addNumbers function"),

        // Good 2: Proper null checks and typing
        new ProblemDefinition(string.Empty,@"function safeGetEmail(user?: { email?: string }): string | undefined {
    return user?.email?.toLowerCase();
}", "Add safeGetEmail function"),

        // Good 3: Efficient iteration
        new ProblemDefinition(string.Empty,@"function sumEfficient(items: number[]): number {
    return items.reduce((s, v) => s + v, 0);
}", "Add sumEfficient function"),

        // Good 4: Proper async/await usage
        new ProblemDefinition(string.Empty,@"async function fetchValue(url: string): Promise<any> {
    const r = await fetch(url);
    return r.json();
}", "Add fetchValue async function"),

        // Good 5: Clear naming and no side effects
        new ProblemDefinition(string.Empty,@"function isArrayEmpty<T>(a: T[]): boolean {
    return a.length === 0;
}", "Add isArrayEmpty function"),
    };

    // Providers are constructed by DI; no static instance is required.

    public EasyTypeScriptCodeReviewProblems()
        : base(_problems, Language.TypeScript, "ts_easy", DifficultyLevel.Easy)
    {
    }
}
