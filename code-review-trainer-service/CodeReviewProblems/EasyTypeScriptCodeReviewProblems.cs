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

        // Problem 1: Basic syntax error - missing semicolon
    new ProblemDefinition("Add greet function",
            @" function greet(name: string): string {
-    return 'Hello ' + name
+    return 'Hello ' + name;
 }"),

        // Problem 2: Basic logic error - wrong comparison operator
    new ProblemDefinition("Add isPositive function",
            @" function isPositive(num: number): boolean {
-    if (num = 0) {
-        return false;
-    }
+    if (num > 0) {
+        return true;
+    }
     return false;
 }"),

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

        // Problem 5: Basic variable naming - using single letter variables
    new ProblemDefinition("Add calculateArea function",
            @" function calculateArea(w: number, h: number): number {
-    return w * h;
+    return width * height;
 }"),

        // Problem 6: Basic string concatenation - using + instead of template literals
    new ProblemDefinition("Add formatName function",
            @" function formatName(first: string, last: string): string {
-    return first + ' ' + last;
+    return `${first} ${last}`;
 }"),

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

        // Problem 8: Basic array access - wrong index usage
    new ProblemDefinition("Add getFirst function",
            @" function getFirst<T>(arr: T[]): T {
-    return arr[1];
+    return arr[0];
 }"),

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

        // Problem 11: Basic type error - returning wrong type
    new ProblemDefinition("Add getLength function",
            @" function getLength(str: string): number {
-    return str;
+    return str.length;
 }"),

        // Problem 12: Basic null check - accessing property without check
    new ProblemDefinition("Add getNameLength function",
            @" function getNameLength(name: string | null): number {
-    return name.length;
+    return name?.length || 0;
 }"),

        // Problem 13: Inefficient repeated DOM queries
    new ProblemDefinition("Add highlight utility",
            @"+function highlight(items: string[]) {
+    items.forEach(item => {
+        const el = document.getElementById(item);
+        if (el) el.classList.add('active');
+    });
+}"),

        // Problem 14: Basic variable declaration - using var instead of let/const
    new ProblemDefinition("Add increment function",
            @" function increment(): number {
-    var count = 0;
+    let count = 0;
     count++;
     return count;
 }"),

        // Problem 15: Redundant computation and poor formatting
    new ProblemDefinition("Add joinStrings function",
            @"+function joinStrings(parts: string[]) {
+    let out = '';
+    for (let i = 0; i < parts.length; i++) {
+        out = out + parts[i];
+    }
+    return out;
+}"),

        // Problem 16: Basic array indexing - off-by-one error
    new ProblemDefinition("Add getLast function",
            @" function getLast<T>(arr: T[]): T | undefined {
-    return arr[arr.length];
+    return arr[arr.length - 1];
 }"),

        // Problem 17: Basic constant reassignment - trying to reassign const
    new ProblemDefinition("Add double function",
            @" function double(num: number): number {
-    const result = num * 2;
-    result = result + 1;
+    const result = num * 2;
+    return result;
 }"),

        // Problem 18: Inefficient map-then-filter pattern
    new ProblemDefinition("Add heavy function",
            @"+function heavy(arr: number[]) {
+    return arr.map(x => x * 2).filter(x => x % 2 === 0);
+}"),

        // Problem 19: Basic type annotation - missing type for parameter
    new ProblemDefinition("Add square function",
            @" function square(num: number): number {
-    return num * num;
+    return value * value;
 }"),

        // Problem 20: Basic function declaration - missing return type
    new ProblemDefinition("Add multiply function",
            @" function multiply(a: number, b: number) {
-    return a * b;
+    return a * b;
 }"),

        // Problem 21: Basic error handling - no try/catch for parseInt
    new ProblemDefinition("Add convertToNumber function",
            @" function convertToNumber(str: string): number {
-    return parseInt(str);
+    const num = parseInt(str);
+    return isNaN(num) ? 0 : num;
 }"),

        // Problem 22: Basic optional chaining - accessing nested property without checks
    new ProblemDefinition("Add getUserEmail function",
            @" function getUserEmail(user: { profile?: { email?: string } }): string {
-    return user.profile.email;
+    return user.profile?.email || '';
 }"),

        // Problem 23: Unnecessary try/catch swallowing errors
        new ProblemDefinition("Add safeRun wrapper",
            @"+function safeRun(cb: Function) {
+    try {
+        cb();
+    } catch (e) {
+        // ignore
+    }
+}"),

        // Problem 24: Basic boolean logic - wrong logical operator
    new ProblemDefinition("Add isValidAge function",
            @" function isValidAge(age: number): boolean {
-    return age > 0 && age < 150;
+    return age > 0 || age < 150;
 }"),

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
