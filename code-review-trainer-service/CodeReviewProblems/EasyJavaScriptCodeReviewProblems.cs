namespace code_review_trainer_service.CodeReviewProblems;

public static class EasyJavaScriptCodeReviewProblems
{
    private static readonly Random _random = new Random();

    private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
    {
        // Patch example: original vs patched (Easy JS)
        new ProblemDefinition(@"function sum(a, b) {
    return a + b;
}", @"function sum(x, y) {
    var total = x - y;
    return total;
}", "Rename parameters and refactor operator usage"),
        // Problem 1: Missing semicolon and var keyword misuse
        new ProblemDefinition(string.Empty,@"function calculateTotal(items) {
    var total = 0
    for (var i = 0; i < items.length; i++) {
        total += items[i].price;
    }
    return total;
}", "Add calculateTotal function to sum item prices"),

        // Problem 2: Using == instead of === and no return statement
        new ProblemDefinition(string.Empty,@"function isValidAge(age) {
    if (age == 18) {
        console.log('Valid age');
    }
}", "Add isValidAge function"),

        // Problem 3: Variable scope issue and undefined variable
        new ProblemDefinition(string.Empty,@"function processData() {
    for (var i = 0; i < 5; i++) {
        var result = i * 2;
    }
    console.log(result);
    console.log(counter);
}", "Add processData function"),

        // Problem 4: Array modification during iteration
        new ProblemDefinition(string.Empty,@"function removeEvenNumbers(numbers) {
    for (let i = 0; i < numbers.length; i++) {
        if (numbers[i] % 2 === 0) {
            numbers.splice(i, 1);
        }
    }
    return numbers;
}", "Add removeEvenNumbers function"),

        // Problem 5: No input validation
        new ProblemDefinition(string.Empty,@"function divide(a, b) {
    return a / b;
}", "Add divide function"),

        // Problem 6: Callback hell and missing error handling
        new ProblemDefinition(string.Empty,@"function loadUserData(userId) {
    fetchUser(userId, function(user) {
        fetchProfile(user.id, function(profile) {
            fetchPreferences(profile.id, function(prefs) {
                console.log(prefs);
            });
        });
    });
}", "Add loadUserData function"),

        // Problem 7: Memory leak potential with event listeners
        new ProblemDefinition(string.Empty,@"function setupButton() {
    const button = document.getElementById('myButton');
    button.addEventListener('click', function() {
        console.log('Button clicked');
    });
}", "Add setupButton that registers click handler"),

        // Problem 8: Not handling async operation properly
        new ProblemDefinition(string.Empty,@"function getData() {
    let result;
    fetch('/api/data')
        .then(response => response.json())
        .then(data => {
            result = data;
        });
    return result;
}", "Add getData function using promise chain"),

        // Problem 9: Mutating function parameters
        new ProblemDefinition(string.Empty,@"function updatePrices(products, discount) {
    for (let product of products) {
        product.price = product.price * (1 - discount);
    }
    return products;
}", "Add updatePrices function"),

        // Problem 10: Poor variable naming and no error handling
        new ProblemDefinition(string.Empty,@"function calc(x, y, z) {
    const a = x + y;
    const b = a * z;
    return b / a;
}", "Add calc function")
    };

    public static string GetRandomProblem()
    {
        return _problems[_random.Next(_problems.Length)].Updated;
    }

    public static CodeReviewProblem GetRandomProblemWithId()
    {
        var index = _random.Next(_problems.Length);
        var def = _problems[index];
        return new CodeReviewProblem
        {
            Id = $"js_easy_{index + 1:D3}",
            Problem = def.Updated,
            Language = Language.JavaScript,
            Original = def.Original ?? string.Empty,
            Purpose = def.Purpose ?? string.Empty
        };
    }

    public static int Count => _problems.Length;
    public static string GetProblemByIndex(int index) => _problems[index].Updated;
}