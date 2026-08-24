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
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "add now subtracts", "The commit describes a rename and a refactor, but the operator changed from + to -. add(2, 3) returns -1 and the function contradicts its own name.", "critical", 3),
            new StoredReviewIssue("2", "Maintainability", "Temporary variable adds nothing", "const result holds the expression for exactly one line. Returning the expression directly is shorter and equally clear.", "trivial", 1)
        ],
        @"function add(x: number, y: number): number {
    return x + y;
}")),

        // Problem 1: Basic syntax error - missing semicolon
    new ProblemDefinition("Add greet function",
            @" function greet(name: string): string {
-    return 'Hello ' + name;
+    return 'Hello ' + name
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Style", "Semicolon removed from the return statement", "The statement only parses because of automatic semicolon insertion, and it now disagrees with every other line in the file. Any lint rule enforcing semicolons fails the build.", "medium", 2),
            new StoredReviewIssue("2", "Maintainability", "Concatenation where a template literal reads better", "'Hello ' + name is the pattern template literals exist to replace.", "trivial", 1)
        ],
        @"function greet(name: string): string {
    return `Hello ${name}`;
}")),

        // Problem 2: Basic logic error - wrong comparison operator
    new ProblemDefinition("Add isPositive function",
            @" function isPositive(num: number): boolean {
-    if (num > 0) {
+    if (num = 0) {
         return true;
     }
     return false;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Assignment used where a comparison was intended", "num = 0 assigns to the parameter and evaluates to a number, so TypeScript rejects it as a condition. Read as intent it is also wrong: testing for zero is not testing for positive.", "critical", 3)
        ],
        @"function isPositive(num: number): boolean {
    return num > 0;
}")),

        // Problem 3: Mutating input and poor naming
    new ProblemDefinition("Add updateUser function",
            @"+function updateUser(u: { name: string; active: boolean }) {
+    u.active = !u.active;
+    return u;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "The caller's object is mutated in place", "updateUser writes straight into its argument and then returns the same reference. Callers who kept the original see it change, and the return value gives the false impression that a new object came back.", "medium", 2),
            new StoredReviewIssue("2", "Maintainability", "Parameter named u", "A single letter says nothing. The parameter is a user, so name it user.", "low", 1),
            new StoredReviewIssue("3", "Maintainability", "No return type and an inline object type", "The shape is worth naming as an interface, and the function should declare what it returns.", "low", 1)
        ],
        @"interface User {
    name: string;
    active: boolean;
}

function updateUser(user: User): User {
    return { ...user, active: !user.active };
}")),

        // Problem 4: Unnecessary expensive operation inside loop
    new ProblemDefinition("Add computeSquares function",
            @"+function computeSquares(n: number) {
+    const results: number[] = [];
+    for (let i = 0; i < n; i++) {
+        results.push(Math.pow(i, 2));
+        console.log('computed ' + i);
+    }
+    return results;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "Console write on every iteration", "console.log is synchronous and comparatively slow. Doing it inside the loop makes the logging, not the arithmetic, the cost of the function, and it floods output for large n.", "medium", 2),
            new StoredReviewIssue("2", "Performance", "Math.pow for squaring", "i * i is a single multiply where Math.pow(i, 2) is a general-purpose call. In a tight loop the difference is measurable and the intent is no clearer.", "low", 1),
            new StoredReviewIssue("3", "Maintainability", "No return type annotation", "The function returns number[] and should say so rather than relying on inference at every call site.", "low", 1)
        ],
        @"function computeSquares(n: number): number[] {
    const results: number[] = [];
    for (let i = 0; i < n; i++) {
        results.push(i * i);
    }

    return results;
}")),

        // Problem 5: Basic variable naming - using single letter variables
    new ProblemDefinition("Add calculateArea function",
            @"-function calculateArea(width: number, height: number): number {
-    return width * height;
+function calculateArea(w: number, h: number): number {
+    return w * h;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Descriptive parameter names replaced with single letters", "width and height documented the argument order at every call site. w and h do not, and the rename is a breaking change for anyone using named destructuring or editor hints.", "medium", 2)
        ],
        @"function calculateArea(width: number, height: number): number {
    return width * height;
}")),

        // Problem 6: Basic string concatenation - using + instead of template literals
    new ProblemDefinition("Add formatName function",
            @" function formatName(first: string, last: string): string {
-    return `${first} ${last}`;
+    return first + ' ' + last;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Template literal replaced with concatenation", "The separator is now a quoted space buried between two plus signs, which is exactly the spot where a missing space goes unnoticed. The original said what it produced.", "medium", 2)
        ],
        @"function formatName(first: string, last: string): string {
    return `${first} ${last}`;
}")),

        // Problem 7: Inefficient array removal (splice in loop)
    new ProblemDefinition("Add removeNegatives function",
            @"+function removeNegatives(arr: number[]) {
+    for (let i = 0; i < arr.length; i++) {
+        if (arr[i] < 0) {
+            arr.splice(i, 1);
+        }
+    }
+    return arr;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "splice inside the loop skips elements", "Removing index i shifts everything down while i still advances, so adjacent negatives are missed. removeNegatives([-1, -2, 3]) returns [-2, 3].", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Input array is mutated and returned", "The caller's array is modified in place, so the original data is gone even though the function looks like it produces a new value.", "medium", 2),
            new StoredReviewIssue("3", "Performance", "splice is O(n) per removal", "Each splice shifts the tail of the array, making the loop quadratic where a single filter pass is linear.", "low", 1)
        ],
        @"function removeNegatives(arr: readonly number[]): number[] {
    return arr.filter((value) => value >= 0);
}")),

        // Problem 8: Basic array access - wrong index usage
    new ProblemDefinition("Add getFirst function",
            @" function getFirst<T>(arr: T[]): T {
-    return arr[0];
+    return arr[1];
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Returns the second element, not the first", "Arrays are zero-based, so getFirst returns the wrong element for every input and returns undefined for a one-element array despite the signature promising T.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Return type lies about empty arrays", "The signature says T but an empty array yields undefined. T | undefined is the honest type unless the function throws.", "medium", 2)
        ],
        @"function getFirst<T>(arr: readonly T[]): T | undefined {
    return arr[0];
}")),

        // Problem 9: Missing await on promise and returning undefined
    new ProblemDefinition("Add getValue async helper",
            @"+async function getValue() {
+    let result;
+    fetch('/api/value').then(r => r.json()).then(v => result = v);
+    return result;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Returns before the promise settles", "Nothing awaits the fetch chain, so the return runs while the request is still in flight and getValue always resolves to undefined.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Rejections are unhandled", "There is no catch and the promise is not awaited, so a failed request becomes an unhandled rejection that no caller can see.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "result is implicitly any and there is no return type", "let result with no initialiser and no annotation defeats the type checker for the whole function.", "low", 1)
        ],
        @"async function getValue<T>(): Promise<T> {
    const response = await fetch('/api/value');
    if (!response.ok) {
        throw new Error(`Request failed: ${response.status}`);
    }

    return (await response.json()) as T;
}")),

        // Problem 10: Poor naming and confusing logic
    new ProblemDefinition("Add calc function",
            @"+function calc(x: number, y: number, z: number) {
+    const a = x + y;
+    const b = a * z;
+    return b / a;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Nothing in the function has a meaningful name", "calc, x, y, z, a and b give a reader no way to decide whether the arithmetic is right, which makes the function effectively unreviewable.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "Divides by x + y without checking for zero", "calc(1, -1, 5) divides by zero and returns NaN. Nothing rejects or reports that input.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "The expression simplifies to z", "b / a is (a * z) / a. Either the function is a very indirect way of returning z, or the formula is wrong - both need an answer before merging.", "medium", 2)
        ],
        @"function calculateRatio(first: number, second: number, multiplier: number): number {
    const total = first + second;
    if (total === 0) {
        throw new RangeError('first and second must not sum to zero');
    }

    return (total * multiplier) / total;
}")),

        // Problem 11: Basic type error - returning wrong type
    new ProblemDefinition("Add getLength function",
            @" function getLength(str: string): number {
-    return str.length;
+    return str;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Returns a string from a function declared to return number", "TypeScript rejects this outright: Type 'string' is not assignable to type 'number'. The .length access was the whole body of the function.", "critical", 3)
        ],
        @"function getLength(str: string): number {
    return str.length;
}")),

        // Problem 12: Basic null check - accessing property without check
    new ProblemDefinition("Add getNameLength function",
            @" function getNameLength(name: string | null): number {
-    return name?.length || 0;
+    return name.length;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Nullable value dereferenced without a check", "The parameter is declared string | null, so under strictNullChecks this fails to compile, and at runtime getNameLength(null) throws. The optional chain and fallback that handled it were removed.", "high", 3)
        ],
        @"function getNameLength(name: string | null): number {
    return name?.length ?? 0;
}")),

        // Problem 13: Inefficient repeated DOM queries
    new ProblemDefinition("Add highlight utility",
            @"+function highlight(items: string[]) {
+    items.forEach(item => {
+        const el = document.getElementById(item);
+        if (el) el.classList.add('active');
+    });
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "One DOM lookup per item", "getElementById runs inside the loop, so highlighting 500 items means 500 separate traversals. A single querySelectorAll, or a lookup map built once, does the same work in one pass.", "medium", 2),
            new StoredReviewIssue("2", "Maintainability", "Missing elements are silently ignored", "if (el) quietly skips ids that do not exist, so a typo in the caller's list produces no highlight and no clue why.", "low", 1),
            new StoredReviewIssue("3", "Maintainability", "No return type annotation", "The function returns void and should say so.", "trivial", 1)
        ],
        @"function highlight(items: readonly string[]): void {
    const selector = items.map((id) => `#${CSS.escape(id)}`).join(',');
    if (!selector) {
        return;
    }

    document.querySelectorAll(selector).forEach((el) => el.classList.add('active'));
}")),

        // Problem 14: Basic variable declaration - using var instead of let/const
    new ProblemDefinition("Add increment function",
            @" function increment(): number {
-    let count = 0;
+    var count = 0;
     count++;
     return count;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "let replaced with var", "var is function scoped and hoisted, which is the behaviour let exists to remove. Modern TypeScript has no reason to reach for it and lint rules generally forbid it.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "The function always returns 1", "count is reset on every call, so increment never increments anything the caller can observe. Either it should take a value or hold state outside the function.", "medium", 2)
        ],
        @"function increment(count: number): number {
    return count + 1;
}")),

        // Problem 15: Redundant computation and poor formatting
    new ProblemDefinition("Add joinStrings function",
            @"+function joinStrings( parts : string[] ) {
+  let out = '';
+    for (let i = 0; i < parts.length; i++) {
+  out = out + parts.map(p => p.trim())[i];
+      }
+ return out;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "The whole array is re-mapped on every iteration", "parts.map(p => p.trim()) builds a complete new array each time round the loop and then throws away all but one element, turning a linear join into O(n squared).", "high", 3),
            new StoredReviewIssue("2", "Style", "Indentation is inconsistent on every line", "The body mixes one, two, four and six space indents and pads the parameter list with spaces. No formatter produces this, so it will churn on the next save.", "medium", 2),
            new StoredReviewIssue("3", "Performance", "String built by repeated concatenation", "out = out + ... allocates a new string per element where Array.join does it in one pass.", "low", 1)
        ],
        @"function joinStrings(parts: readonly string[]): string {
    return parts.map((part) => part.trim()).join('');
}")),

        // Problem 16: Basic array indexing - off-by-one error
    new ProblemDefinition("Add getLast function",
            @" function getLast<T>(arr: T[]): T | undefined {
-    return arr[arr.length - 1];
+    return arr[arr.length];
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Index is one past the end", "Valid indexes stop at length - 1, so arr[arr.length] is always undefined. getLast returns undefined for every input, including a fully populated array, and the declared return type hides the bug from the compiler.", "high", 3)
        ],
        @"function getLast<T>(arr: readonly T[]): T | undefined {
    return arr[arr.length - 1];
}")),

        // Problem 17: Basic constant reassignment - trying to reassign const
    new ProblemDefinition("Add double function",
            @" function double(num: number): number {
     const result = num * 2;
+    result = result + 1;
     return result;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Assignment to a const binding", "result is declared const, so result = result + 1 is a compile error: Cannot assign to 'result' because it is a constant.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "The added line contradicts the function name", "Even if the binding were mutable, double(4) would return 9. Nothing in the commit explains why doubling should also add one.", "medium", 2)
        ],
        @"function double(num: number): number {
    return num * 2;
}")),

        // Problem 18: Inefficient map-then-filter pattern
    new ProblemDefinition("Add heavy function",
            @"+function heavy(arr: number[]) {
+    return arr.map(x => x * 2).filter(x => x % 2 === 0);
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The filter can never remove anything", "Every element has already been multiplied by two, so x % 2 === 0 is true for all of them. Either the filter is dead code or the intended predicate was lost.", "medium", 2),
            new StoredReviewIssue("2", "Performance", "Two passes and an intermediate array", "map followed by filter walks the data twice and allocates an array that is immediately discarded. A single reduce, or dropping the filter entirely, avoids both.", "low", 1),
            new StoredReviewIssue("3", "Maintainability", "Name says nothing and there is no return type", "heavy does not describe doubling a list of numbers, and the return type is left to inference.", "low", 1)
        ],
        @"function doubleAll(arr: readonly number[]): number[] {
    return arr.map((value) => value * 2);
}")),

        // Problem 19: Basic type annotation - missing type for parameter
    new ProblemDefinition("Add square function",
            @"-function square(num: number): number {
+function square(num): number {
     return num * num;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Parameter annotation removed", "num is now an implicit any, which fails the build under noImplicitAny and otherwise lets square('abc') through to return NaN. The signature was the only thing constraining callers.", "medium", 2)
        ],
        @"function square(num: number): number {
    return num * num;
}")),

        // Problem 20: Basic function declaration - missing return type
    new ProblemDefinition("Add multiply function",
            @"-function multiply(a: number, b: number): number {
+function multiply(a: number, b: number) {
     return a * b;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Explicit return type removed", "The type is still inferred correctly today, but the annotation is what stops a future edit from silently changing the public signature of the function. Codebases that enforce explicit-module-boundary-types will fail on this.", "medium", 2)
        ],
        @"function multiply(a: number, b: number): number {
    return a * b;
}")),

        // Problem 21: Basic error handling - no try/catch for parseInt
    new ProblemDefinition("Add convertToNumber function",
            @" function convertToNumber(str: string): number {
-    const num = parseInt(str);
-    return isNaN(num) ? 0 : num;
+    return parseInt(str);
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "NaN is now returned to the caller", "parseInt('abc') is NaN, and the isNaN guard that turned it into 0 was removed. NaN then poisons every arithmetic result downstream while still satisfying the number return type.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "parseInt is called without a radix", "parseInt('08') is fine on modern engines but the missing radix is still a latent trap, and parseInt('12abc') silently returns 12.", "low", 1)
        ],
        @"function convertToNumber(str: string): number {
    const num = Number.parseInt(str, 10);
    return Number.isNaN(num) ? 0 : num;
}")),

        // Problem 22: Basic optional chaining - accessing nested property without checks
    new ProblemDefinition("Add getUserEmail function",
            @" function getUserEmail(user: { profile?: { email?: string } }): string {
-    return user.profile?.email || '';
+    return user.profile.email;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Optional properties dereferenced without a check", "profile and email are both declared optional, so user.profile.email fails to compile under strictNullChecks and throws at runtime for any user without a profile.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Return type no longer matches the value", "The signature promises string, but with both properties optional the expression can produce undefined. The removed || '' was what made the signature true.", "medium", 2)
        ],
        @"function getUserEmail(user: { profile?: { email?: string } }): string {
    return user.profile?.email ?? '';
}")),

        // Problem 23: Unnecessary try/catch swallowing errors
        new ProblemDefinition("Add safeRun wrapper",
            @"+function safeRun(cb: Function) {
+    try {
+        cb();
+    } catch (e) {
+        // ignore
+    }
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Every error is swallowed silently", "The empty catch discards the exception with no log, no metric and no rethrow. A failure inside cb becomes indistinguishable from success, which is the hardest class of bug to track down later.", "medium", 2),
            new StoredReviewIssue("2", "Maintainability", "Function is typed as Function", "Function accepts anything callable and gives no argument or return checking. () => void, or a generic signature, states the contract.", "low", 1),
            new StoredReviewIssue("3", "Correctness", "Async callbacks are not covered", "If cb returns a promise, the rejection happens after the try block has exited, so the wrapper does not make async work any safer despite its name.", "low", 1)
        ],
        @"function safeRun(cb: () => void, onError: (error: unknown) => void = console.error): void {
    try {
        cb();
    } catch (error) {
        onError(error);
    }
}")),

        // Problem 24: Basic boolean logic - wrong logical operator
    new ProblemDefinition("Add isValidAge function",
            @" function isValidAge(age: number): boolean {
-    return age > 0 && age < 150;
+    return age > 0 || age < 150;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "&& changed to || makes the check always true", "Every number satisfies at least one of the two comparisons, so isValidAge(-40) and isValidAge(10000) both return true. The function no longer validates anything.", "high", 3)
        ],
        @"function isValidAge(age: number): boolean {
    return age > 0 && age < 150;
}")),

        // Problem 25: Performance - creating functions inside loop
        new ProblemDefinition("Add makeHandlers example",
            @"+function makeHandlers(items: string[]) {
+    const handlers: Function[] = [];
+    for (let i = 0; i < items.length; i++) {
+        handlers.push(function() { return items[i]; });
+    }
+    return handlers;
+}",
        new StoredReview(ReviewStatus.Approve,
        [
            new StoredReviewIssue("1", "Performance", "A closure is allocated per item", "Each iteration creates a new function object that captures i. For a handful of items this is irrelevant; it is only worth changing if the list is large or this runs on a hot path. Note that the classic capture bug does not apply here - let gives each iteration its own binding, so the handlers return the right values.", "low", 1),
            new StoredReviewIssue("2", "Maintainability", "Handlers are typed as Function", "Function[] loses the return type. (() => string)[] documents what callers get back.", "low", 1)
        ],
        @"function makeHandlers(items: readonly string[]): (() => string)[] {
    return items.map((item) => () => item);
}")),

        // === GOOD EXAMPLES ===

        // Good 1: Clear, typed, and well-formed function
        new ProblemDefinition("Add addNumbers function",
            @"+function addNumbers(a: number, b: number): number {
+    return a + b;
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good 2: Proper null checks and typing
        new ProblemDefinition("Add safeGetEmail function",
            @"+function safeGetEmail(user?: { email?: string }): string | undefined {
+    return user?.email?.toLowerCase();
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good 3: Efficient iteration
        new ProblemDefinition("Add sumEfficient function",
            @"+function sumEfficient(items: number[]): number {
+    return items.reduce((s, v) => s + v, 0);
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good 4: Proper async/await usage
        new ProblemDefinition("Add fetchValue async function",
            @"+async function fetchValue(url: string): Promise<any> {
+    const r = await fetch(url);
+    return r.json();
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good 5: Clear naming and no side effects
        new ProblemDefinition("Add isArrayEmpty function",
            @"+function isArrayEmpty<T>(a: T[]): boolean {
+    return a.length === 0;
+}",
        new StoredReview(ReviewStatus.Approve, [])),
        };

        // Providers are constructed by DI; no static instance is required.

        public EasyTypeScriptCodeReviewProblems()
            : base(_problems, Language.TypeScript, "ts_easy", DifficultyLevel.Easy)
        {
        }
}
