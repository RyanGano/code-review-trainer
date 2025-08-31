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
 }"),

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
}"),

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
+}")
    };

    // Providers are constructed by DI; no static instance is required.

    public EasyJavaScriptCodeReviewProblems()
        : base(_problems, Language.JavaScript, "js_easy", DifficultyLevel.Easy)
    {
    }
}