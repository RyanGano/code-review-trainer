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
+}"),

        // Problem 1: Missing semicolon and var keyword misuse
    new ProblemDefinition("Add calculateTotal function to sum item prices",
            @"+function calculateTotal(items) {
+    var total = 0
+    for (var i = 0; i < items.length; i++) {
+        total += items[i].price;
+    }
+    return total;
+}"),

        // Problem 2: Using == instead of === and no return statement
    new ProblemDefinition("Add isValidAge function",
            @"+function isValidAge(age) {
+    if (age == 18) {
+        console.log('Valid age');
+    }
+}"),

        // Problem 3: Variable scope issue and undefined variable
    new ProblemDefinition("Add processData function",
            @"+function processData() {
+    for (var i = 0; i < 5; i++) {
+        var result = i * 2;
+    }
+    console.log(result);
+    console.log(counter);
+}"),

        // Problem 4: Array modification during iteration
    new ProblemDefinition("Add removeEvenNumbers function",
            @"+function removeEvenNumbers(numbers) {
+    for (let i = 0; i < numbers.length; i++) {
+        if (numbers[i] % 2 === 0) {
+            numbers.splice(i, 1);
+        }
+    }
+    return numbers;
+}"),

        // Problem 5: No input validation
    new ProblemDefinition("Add divide function",
            @"+function divide(a, b) {
+    return a / b;
+}"),

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
+}"),

        // Problem 7: Memory leak potential with event listeners
    new ProblemDefinition("Add setupButton that registers click handler",
            @"+function setupButton() {
+    const button = document.getElementById('myButton');
+    button.addEventListener('click', function() {
+        console.log('Button clicked');
+    });
+}"),

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
+}"),

        // Problem 9: Mutating function parameters
    new ProblemDefinition("Add updatePrices function",
            @"+function updatePrices(products, discount) {
+    for (let product of products) {
+        product.price = product.price * (1 - discount);
+    }
+    return products;
+}"),

        // Problem 10: Poor variable naming and no error handling
    new ProblemDefinition("Add calc function",
            @"+function calc(x, y, z) {
+    const a = x + y;
+    const b = a * z;
+    return b / a;
+}"),

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
 }"),

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
 }"),

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
 }"),

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
 }"),

        // Problem 15: Could use standard utilities instead of manual implementation
    new ProblemDefinition("Add reverseString function",
            @" function reverseString(str) {
-    return str.split('').reverse().join('');
+    let result = '';
+    for (let i = str.length - 1; i >= 0; i--) {
+        result += str[i];
+    }
+    return result;
 }"),

        // Problem 16: Type unsafe code - no type checking
    new ProblemDefinition("Add addNumbers function",
            @" function addNumbers(a, b) {
-    if (typeof a !== 'number' || typeof b !== 'number') {
-        throw new Error('Both arguments must be numbers');
-    }
     return a + b;
 }"),

        // Problem 17: Unintentional logic changes - different behavior than expected
    new ProblemDefinition("Add isEven function",
            @" function isEven(num) {
-    return num % 2 === 0;
+    return num % 2 == 0;
 }"),

        // Problem 18: Missing validation for null/undefined
    new ProblemDefinition("Add getUserName function",
            @" function getUserName(user) {
-    if (!user || !user.name) {
-        throw new Error('Invalid user object');
-    }
     return user.name.toUpperCase();
 }"),

        // Problem 19: Commit code doesn't match the purpose - function does more than described
    new ProblemDefinition("Add formatDate function",
            @" function formatDate(date) {
     const formatted = date.toLocaleDateString();
+    console.log('Date formatted:', formatted);
     return formatted;
 }"),

        // Problem 20: Hardcoded values instead of configurable
    new ProblemDefinition("Add calculateTax function",
            @" function calculateTax(amount) {
-    const TAX_RATE = 0.08;
-    return amount * TAX_RATE;
+    const taxRate = 0.08;
+    return amount * taxRate;
 }"),

        // Problem 21: Poor code organization - everything in one function
    new ProblemDefinition("Add validateAndSaveUser function",
            @" function validateAndSaveUser(user) {
-    if (!user || !user.name || !user.email) {
-        return false;
-    }
-    // Validate email format
-    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
-    if (!emailRegex.test(user.email)) {
-        return false;
-    }
-    // Save to database
-    saveUserToDatabase(user);
-    return true;
+    if (user.name && user.email) {
+        // Save to database
+        console.log('User saved');
+        return true;
+    }
+    return false;
 }"),

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
 }")
        };

        // Providers are constructed by DI; no static instance is required.

        public EasyJavaScriptCodeReviewProblems()
            : base(_problems, Language.JavaScript, "js_easy", DifficultyLevel.Easy)
        {
        }
}