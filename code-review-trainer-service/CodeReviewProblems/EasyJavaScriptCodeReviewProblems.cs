namespace code_review_trainer_service.CodeReviewProblems;

public sealed class EasyJavaScriptCodeReviewProblems : CodeReviewProblems
{
        private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
        {
        // Patch example: original vs patched (Easy JS)
    new ProblemDefinition("Rename parameters and refactor operator usage",
        @"-function sum(a, b) {
+function sum(x, y) {
-    return a + b;
+    var total = x - y;
+    return total;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "sum now subtracts instead of adding", "The commit describes a rename, but the body changed from x + y to x - y. sum(2, 3) returns -1 and the function no longer matches its name.", "critical", 3),
            new StoredReviewIssue("2", "Maintainability", "var used for a value that never changes", "total is assigned once, so const is the right declaration. var also gives the binding function scope, which is the behaviour nobody wants any more.", "low", 1)
        ],
        @"function sum(x, y) {
    return x + y;
}")),

        // Problem 1: Missing semicolon and var keyword misuse
    new ProblemDefinition("Add calculateTotal function to sum item prices",
            @"+function calculateTotal(items) {
+    var total = 0
+    for (var i = 0; i < items.length; i++) {
+        total += items[i].price;
+    }
+    return total;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Style", "Missing semicolon after the total declaration", "var total = 0 has no terminating semicolon and only survives because of automatic semicolon insertion. Relying on ASI is fragile and inconsistent with the rest of the function.", "medium", 2),
            new StoredReviewIssue("2", "Maintainability", "var instead of let/const", "Both var declarations are function scoped and hoisted. total should be let and the loop counter let, so the bindings stop leaking past the block they belong to.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "No guard for missing items or prices", "calculateTotal(undefined) throws and any item without a numeric price turns the running total into NaN with no indication of which item was bad.", "low", 1)
        ],
        @"function calculateTotal(items) {
    if (!Array.isArray(items)) {
        throw new TypeError('items must be an array');
    }

    return items.reduce((total, item) => total + Number(item.price ?? 0), 0);
}")),

        // Problem 2: Using == instead of === and no return statement
    new ProblemDefinition("Add isValidAge function",
            @"+function isValidAge(age) {
+    if (age == 18) {
+        console.log('Valid age');
+    }
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Function never returns a value", "isValidAge always evaluates to undefined, so every caller writing if (isValidAge(x)) takes the false branch. The name promises a boolean.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Loose equality allows coercion", "age == 18 is also true for the string '18' and for [18]. Use === so the check does not depend on JavaScript's coercion rules.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "Only the single value 18 is treated as valid", "A validity check should accept a range of ages. As written, 19 and 42 are not valid and nothing is logged.", "medium", 2)
        ],
        @"function isValidAge(age) {
    return Number.isInteger(age) && age >= 0 && age < 150;
}")),

        // Problem 3: Variable scope issue and undefined variable
    new ProblemDefinition("Add processData function",
            @"+function processData() {
+    for (var i = 0; i < 5; i++) {
+        var result = i * 2;
+    }
+    console.log(result);
+    console.log(counter);
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "counter is never declared", "console.log(counter) throws ReferenceError: counter is not defined the first time the function runs. In strict mode or a module there is no scenario where this works.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "var lets the loop body leak into the function scope", "result is declared inside the loop but read after it, which only works because var is hoisted. With let this would be an error, which is the point - the intent of the code is unclear.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "result is undefined when the loop does not run", "The pattern depends on the loop executing at least once. Any future change to the bound leaves result undefined with no warning.", "low", 1)
        ],
        @"function processData() {
    const results = [];

    for (let i = 0; i < 5; i++) {
        results.push(i * 2);
    }

    return results;
}")),

        // Problem 4: Array modification during iteration
    new ProblemDefinition("Add removeEvenNumbers function",
            @"+function removeEvenNumbers(numbers) {
+    for (let i = 0; i < numbers.length; i++) {
+        if (numbers[i] % 2 === 0) {
+            numbers.splice(i, 1);
+        }
+    }
+    return numbers;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "splice during iteration skips elements", "Removing index i shifts every later element down one, but the loop still increments i. removeEvenNumbers([2, 4, 6]) returns [4] because 4 is never examined.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Caller's array is mutated in place", "The function both mutates its argument and returns it, so callers who keep a reference to the original silently lose data.", "medium", 2)
        ],
        @"function removeEvenNumbers(numbers) {
    return numbers.filter((number) => number % 2 !== 0);
}")),

        // Problem 5: No input validation
    new ProblemDefinition("Add divide function",
            @"+function divide(a, b) {
+    return a / b;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Division by zero is not handled", "divide(1, 0) returns Infinity and divide(0, 0) returns NaN. Both values flow onwards silently and usually surface far from this function.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "No check that the arguments are numbers", "divide('10', 'a') returns NaN rather than failing. A explicit type check makes the contract clear at the call site.", "low", 1)
        ],
        @"function divide(a, b) {
    if (typeof a !== 'number' || typeof b !== 'number') {
        throw new TypeError('Both arguments must be numbers');
    }
    if (b === 0) {
        throw new RangeError('Cannot divide by zero');
    }

    return a / b;
}")),

        // Problem 6: Callback hell and missing error handling
    new ProblemDefinition("Add loadUserData function",
            @"+function loadUserData(userId) {
+    fetchUser(userId, function(user) {
+        fetchProfile(user.id, function(profile) {
+            fetchPreferences(profile.id, function(prefs) {
+                console.log(prefs);
+            });
+        });
+    });
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "No error handling anywhere in the chain", "None of the three callbacks takes an error argument. If fetchUser fails, user is undefined and user.id throws inside a callback where nothing can catch it.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Three levels of nested callbacks", "Each new step indents further and the failure paths cannot be shared. Promises or async/await express the same sequence flat.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "Result is logged rather than returned", "The caller has no way to use the preferences. loadUserData should hand the data back rather than print it.", "medium", 2)
        ],
        @"async function loadUserData(userId) {
    const user = await fetchUser(userId);
    const profile = await fetchProfile(user.id);
    return fetchPreferences(profile.id);
}")),

        // Problem 7: Memory leak potential with event listeners
    new ProblemDefinition("Add setupButton that registers click handler",
            @"+function setupButton() {
+    const button = document.getElementById('myButton');
+    button.addEventListener('click', function() {
+        console.log('Button clicked');
+    });
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Listener is never removed", "The handler is an anonymous function, so there is no reference to pass to removeEventListener. Calling setupButton twice registers two handlers and neither can be detached.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "No check that the element exists", "getElementById returns null when the element is absent, and null.addEventListener throws. This breaks on any page where the markup changed.", "medium", 2)
        ],
        @"function setupButton() {
    const button = document.getElementById('myButton');
    if (!button) {
        return () => {};
    }

    const handleClick = () => console.log('Button clicked');
    button.addEventListener('click', handleClick);

    return () => button.removeEventListener('click', handleClick);
}")),

        // Problem 8: Not handling async operation properly
    new ProblemDefinition("Add getData function using promise chain",
            @"+function getData() {
+    let result;
+    fetch('/api/data')
+        .then(response => response.json())
+        .then(data => {
+            result = data;
+        });
+    return result;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Returns before the fetch resolves", "The return runs synchronously while the promise is still pending, so getData always returns undefined. Assigning to result inside the callback happens after the caller has already moved on.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Promise rejection is unhandled", "There is no catch, so a network failure becomes an unhandled rejection that the caller cannot see or react to.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "HTTP error responses are treated as success", "fetch only rejects on network failure. A 500 still reaches response.json(), which then fails on a non-JSON error body.", "medium", 2)
        ],
        @"async function getData() {
    const response = await fetch('/api/data');
    if (!response.ok) {
        throw new Error(`Request failed: ${response.status}`);
    }

    return response.json();
}")),

        // Problem 9: Mutating function parameters
    new ProblemDefinition("Add updatePrices function",
            @"+function updatePrices(products, discount) {
+    for (let product of products) {
+        product.price = product.price * (1 - discount);
+    }
+    return products;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Caller's product objects are mutated", "The loop writes straight into the objects it was given, so the original prices are lost. Anything else holding a reference to those products sees the discount applied without asking.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "Discount is not validated", "A discount of 2 produces negative prices and a discount of undefined produces NaN. Neither is rejected.", "low", 1)
        ],
        @"function updatePrices(products, discount) {
    if (typeof discount !== 'number' || discount < 0 || discount > 1) {
        throw new RangeError('discount must be between 0 and 1');
    }

    return products.map((product) => ({
        ...product,
        price: product.price * (1 - discount),
    }));
}")),

        // Problem 10: Poor variable naming and no error handling
    new ProblemDefinition("Add calc function",
            @"+function calc(x, y, z) {
+    const a = x + y;
+    const b = a * z;
+    return b / a;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Every name is a single letter", "calc, x, y, z, a and b say nothing about what is being computed. A reader cannot tell whether this function is correct because nobody can say what it is supposed to do.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "Division by zero when x + y is zero", "a is the divisor on the last line, so calc(1, -1, 5) returns NaN. Nothing guards the case.", "medium", 2)
        ],
        @"function calculateWeightedRatio(first, second, multiplier) {
    const total = first + second;
    if (total === 0) {
        throw new RangeError('first and second must not sum to zero');
    }

    return (total * multiplier) / total;
}")),

        // Problem 11: Bad spelling in variable names and comments
    new ProblemDefinition("Add calculateAverage function",
            @" function calculateAverage(numbers) {
-    // Calculate the sum of all numbers
+    // Calculte the sum of all numbers
     let sum = 0;
-    for (let i = 0; i < numbers.length; i++) {
+    for (let i = 0; i < numbers.lenght; i++) {
         sum += numbers[i];
     }
-    return sum / numbers.length;
+    return sum / numbers.lenght;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "numbers.lenght is a typo for numbers.length", "The misspelled property is undefined, so the loop condition i < undefined is false immediately and the function returns 0 / undefined, which is NaN. It fails silently for every input.", "high", 3),
            new StoredReviewIssue("2", "Style", "Comment misspells 'Calculate'", "The comment now reads 'Calculte the sum of all numbers'. Small, but it is the kind of thing a review should catch before it spreads by copy-paste.", "trivial", 1),
            new StoredReviewIssue("3", "Correctness", "Empty input is not handled", "An empty array divides zero by zero and returns NaN rather than reporting that there is nothing to average.", "low", 1)
        ],
        @"function calculateAverage(numbers) {
    if (numbers.length === 0) {
        throw new RangeError('numbers must not be empty');
    }

    // Calculate the sum of all numbers
    const sum = numbers.reduce((total, value) => total + value, 0);
    return sum / numbers.length;
}")),

        // Problem 12: Unclear variable names and missing validation
    new ProblemDefinition("Add processUserData function",
            @" function processUserData(data) {
-    let name = data.name;
-    let age = data.age;
-    let message = name + ' is ' + age + ' years old';
+    let x = data.name;
+    let y = data.age;
+    let z = x + ' is ' + y + ' years old';
     console.log(z);
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Meaningful names replaced with x, y and z", "name, age and message described the values they held. The patch removes that information for no benefit and makes the log statement harder to follow.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "data is dereferenced without any check", "processUserData(undefined) throws on the first line. A missing name or age produces the string 'undefined is undefined years old'.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "let used for values that never change", "All three bindings are assigned once and should be const.", "low", 1)
        ],
        @"function processUserData(data) {
    if (!data?.name || data.age === undefined) {
        throw new TypeError('data must include name and age');
    }

    const message = `${data.name} is ${data.age} years old`;
    console.log(message);
    return message;
}")),

        // Problem 13: Bad logic - off-by-one error in loop
    new ProblemDefinition("Add findMax function",
            @" function findMax(arr) {
     let max = arr[0];
-    for (let i = 1; i < arr.length; i++) {
+    for (let i = 1; i <= arr.length; i++) {
         if (arr[i] > max) {
             max = arr[i];
         }
     }
     return max;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Loop reads one element past the end", "i <= arr.length makes the last iteration evaluate arr[arr.length], which is undefined. The comparison undefined > max is false so the result happens to survive, but the loop is wrong and any change to the body turns it into a real bug.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "Empty array returns undefined", "findMax([]) reads arr[0] as undefined and returns it. The function should say what it does with no input rather than leaking undefined.", "low", 1)
        ],
        @"function findMax(arr) {
    if (arr.length === 0) {
        throw new RangeError('arr must not be empty');
    }

    return arr.reduce((max, value) => (value > max ? value : max), arr[0]);
}")),

        // Problem 14: Terrible performance - nested loops when unnecessary
    new ProblemDefinition("Add checkDuplicates function",
            @" function checkDuplicates(arr1, arr2) {
-    const set = new Set(arr2);
-    for (let item of arr1) {
-        if (set.has(item)) {
-            return true;
-        }
-    }
+    for (let i = 0; i < arr1.length; i++) {
+        for (let j = 0; j < arr2.length; j++) {
+            if (arr1[i] === arr2[j]) {
+                return true;
+            }
+        }
+    }
     return false;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "Linear lookup replaced with a quadratic scan", "The Set gave O(n + m) with O(1) lookups. The nested loops make this O(n * m), so two arrays of 10,000 items go from 20,000 operations to 100,000,000 in the worst case.", "medium", 2),
            new StoredReviewIssue("2", "Maintainability", "More code doing the same job", "Nine lines of index bookkeeping replace five lines that stated the intent directly. There is no reason given for the rewrite.", "low", 1)
        ],
        @"function checkDuplicates(arr1, arr2) {
    const seen = new Set(arr2);
    return arr1.some((item) => seen.has(item));
}")),

        // Problem 15: Could use standard utilities instead of manual implementation
    new ProblemDefinition("Add reverseString function",
            @" function reverseString(str) {
-    return str.split('').reverse().join('');
+    let result = '';
+    for (let i = str.length - 1; i >= 0; i--) {
+        result += str[i];
+    }
+    return result;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Performance", "String concatenation in a loop", "result += str[i] builds a new string each iteration. The one-line version it replaced did the work in a single pass with no intermediate garbage.", "medium", 2),
            new StoredReviewIssue("2", "Maintainability", "Standard library call replaced by a hand-written loop", "split/reverse/join said exactly what the function does. The loop needs a reader to verify the bounds before they can trust it.", "low", 1)
        ],
        @"function reverseString(str) {
    return [...str].reverse().join('');
}")),

        // Problem 16: Type unsafe code - no type checking
    new ProblemDefinition("Add addNumbers function",
            @" function addNumbers(a, b) {
-    if (typeof a !== 'number' || typeof b !== 'number') {
-        throw new Error('Both arguments must be numbers');
-    }
     return a + b;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Type guard removed from a function named addNumbers", "Without the check addNumbers('1', 2) returns the string '12' and addNumbers(undefined, 1) returns NaN. The old code failed loudly; the new code returns a wrong answer quietly.", "medium", 2)
        ],
        @"function addNumbers(a, b) {
    if (typeof a !== 'number' || typeof b !== 'number') {
        throw new TypeError('Both arguments must be numbers');
    }

    return a + b;
}")),

        // Problem 17: Unintentional logic changes - different behavior than expected
    new ProblemDefinition("Add isEven function",
            @" function isEven(num) {
-    return num % 2 === 0;
+    return num % 2 === 1;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Test inverted: the function now detects odd numbers", "isEven(4) returns false and isEven(3) returns true. Every caller gets the opposite of what the name promises.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Comparison against 1 is wrong for negative input", "JavaScript's remainder keeps the sign of the dividend, so -3 % 2 is -1. isEven(-3) therefore returns false as well - the new check is not even a correct isOdd.", "medium", 2)
        ],
        @"function isEven(num) {
    return num % 2 === 0;
}")),

        // Problem 18: Missing validation for null/undefined
    new ProblemDefinition("Add getUserName function",
            @" function getUserName(user) {
-    if (!user || !user.name) {
-        throw new Error('Invalid user object');
-    }
     return user.name.toUpperCase();
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Null guard removed before two dereferences", "getUserName(undefined) now throws TypeError: Cannot read properties of undefined, and a user with no name throws on toUpperCase. The removed check turned both into one clear error.", "high", 3)
        ],
        @"function getUserName(user) {
    if (!user?.name) {
        throw new Error('Invalid user object');
    }

    return user.name.toUpperCase();
}")),

        // Problem 19: Commit code doesn't match the purpose - function does more than described
    new ProblemDefinition("Add formatDate function",
            @" function formatDate(date) {
     const formatted = date.toLocaleDateString();
+    console.log('Date formatted:', formatted);
     return formatted;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Debug logging added to a formatting helper", "The commit says only that formatDate is added, but the patch also writes to the console on every call. In a hot path this floods the log, and it is a side effect nobody reading the commit message would expect.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "No locale or invalid-date handling", "toLocaleDateString depends on the host locale, so the output differs per machine, and an invalid Date silently yields 'Invalid Date'.", "low", 1)
        ],
        @"function formatDate(date) {
    if (Number.isNaN(date?.getTime?.())) {
        throw new TypeError('date must be a valid Date');
    }

    return date.toLocaleDateString('en-US');
}")),

        // Problem 20: Hardcoded values instead of configurable
    new ProblemDefinition("Add calculateTax function",
            @"-function calculateTax(amount, taxRate) {
-    return amount * taxRate;
+function calculateTax(amount) {
+    return amount * 0.08;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Configurable rate replaced with a hardcoded 0.08", "Dropping the taxRate parameter means every caller is stuck with 8%. Any other jurisdiction now needs a second function or an edit to this one.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Magic number with no name", "0.08 appears inline with nothing saying what it is. At minimum it should be a named constant.", "low", 1),
            new StoredReviewIssue("3", "Correctness", "Removing a parameter breaks existing callers", "Anyone calling calculateTax(100, 0.2) now silently gets 8 instead of 20 - the extra argument is ignored rather than reported.", "medium", 2)
        ],
        @"const DEFAULT_TAX_RATE = 0.08;

function calculateTax(amount, taxRate = DEFAULT_TAX_RATE) {
    return amount * taxRate;
}")),

        // Problem 21: Poor code organization - everything in one function
    new ProblemDefinition("Add validateAndSaveUser function",
            @" function validateAndSaveUser(user) {
-    if (!isValidUser(user)) {
-        return false;
-    }
-    saveUserToDatabase(user);
-    return true;
+    if (!user || !user.name || !user.email) {
+        return false;
+    }
+    // Validate email format
+    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
+    if (!emailRegex.test(user.email)) {
+        return false;
+    }
+    // Normalize the record
+    user.name = user.name.trim();
+    user.email = user.email.trim().toLowerCase();
+    // Audit the change
+    console.log('Saving user ' + user.email + ' at ' + new Date().toISOString());
+    // Save to database
+    const connection = openDatabaseConnection();
+    connection.query('INSERT INTO users SET ?', user);
+    connection.close();
+    return true;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Validation, normalization, auditing and persistence in one function", "The patch inlines four separate responsibilities that used to sit behind isValidUser and saveUserToDatabase. Nothing here can be tested or reused on its own, and the function has to change whenever any one of the four concerns does.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "Database connection is not closed on failure", "connection.close() only runs if query succeeds. Any throw leaks the connection, which the extracted helper handled.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "The caller's user object is mutated", "Trimming and lowercasing write back into the argument, so the caller's object changes underneath it.", "low", 1),
            new StoredReviewIssue("4", "Correctness", "All failures collapse into a single false", "Missing fields and a malformed email are indistinguishable to the caller, so no useful message can be shown to the user.", "low", 1)
        ],
        @"function validateAndSaveUser(user) {
    if (!isValidUser(user)) {
        return false;
    }

    saveUserToDatabase(user);
    return true;
}")),

        // Problem 22: Missing error handling in async operation
    new ProblemDefinition("Add fetchUserProfile function",
            @" function fetchUserProfile(userId) {
     fetch(`/api/users/${userId}`)
-        .then(response => {
-            if (!response.ok) {
-                throw new Error('Failed to fetch user profile');
-            }
-            return response.json();
-        })
-        .then(data => console.log(data))
-        .catch(error => console.error('Error:', error));
+        .then(response => response.json())
+        .then(data => console.log(data));
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "catch handler removed", "Without the catch, a network failure becomes an unhandled promise rejection. In Node that can terminate the process; in a browser it is a console error nobody owns.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "response.ok check removed", "fetch resolves for 404 and 500 responses. Those bodies are usually not JSON, so response.json() throws inside a chain that no longer has a catch.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "The promise is not returned", "fetchUserProfile gives the caller nothing to await, so there is no way to know when the work finished or whether it worked.", "medium", 2)
        ],
        @"async function fetchUserProfile(userId) {
    const response = await fetch(`/api/users/${userId}`);
    if (!response.ok) {
        throw new Error('Failed to fetch user profile');
    }

    return response.json();
}"))
        };

        // Providers are constructed by DI; no static instance is required.

        public EasyJavaScriptCodeReviewProblems()
            : base(_problems, Language.JavaScript, "js_easy", DifficultyLevel.Easy)
        {
        }
}
