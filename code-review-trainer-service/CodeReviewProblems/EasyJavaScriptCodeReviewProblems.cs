namespace code_review_trainer_service.CodeReviewProblems;

public static class EasyJavaScriptCodeReviewProblems
{
    private static readonly Random _random = new Random();

    private static readonly string[] _problems = new string[]
    {
        // Problem 1: Missing semicolon and var keyword misuse
        @"function calculateTotal(items) {
    var total = 0
    for (var i = 0; i < items.length; i++) {
        total += items[i].price;
    }
    return total;
}",

        // Problem 2: Using == instead of === and no return statement
        @"function isValidAge(age) {
    if (age == 18) {
        console.log('Valid age');
    }
}",

        // Problem 3: Variable scope issue and undefined variable
        @"function processData() {
    for (var i = 0; i < 5; i++) {
        var result = i * 2;
    }
    console.log(result);
    console.log(counter);
}",

        // Problem 4: Array modification during iteration
        @"function removeEvenNumbers(numbers) {
    for (let i = 0; i < numbers.length; i++) {
        if (numbers[i] % 2 === 0) {
            numbers.splice(i, 1);
        }
    }
    return numbers;
}",

        // Problem 5: No input validation
        @"function divide(a, b) {
    return a / b;
}",

        // Problem 6: Callback hell and missing error handling
        @"function loadUserData(userId) {
    fetchUser(userId, function(user) {
        fetchProfile(user.id, function(profile) {
            fetchPreferences(profile.id, function(prefs) {
                console.log(prefs);
            });
        });
    });
}",

        // Problem 7: Memory leak potential with event listeners
        @"function setupButton() {
    const button = document.getElementById('myButton');
    button.addEventListener('click', function() {
        console.log('Button clicked');
    });
}",

        // Problem 8: Not handling async operation properly
        @"function getData() {
    let result;
    fetch('/api/data')
        .then(response => response.json())
        .then(data => {
            result = data;
        });
    return result;
}",

        // Problem 9: Mutating function parameters
        @"function updatePrices(products, discount) {
    for (let product of products) {
        product.price = product.price * (1 - discount);
    }
    return products;
}",

        // Problem 10: Poor variable naming and no error handling
        @"function calc(x, y, z) {
    const a = x + y;
    const b = a * z;
    return b / a;
}"
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
            Id = $"js_easy_{index + 1:D3}",
            Problem = _problems[index],
            Language = Language.JavaScript
        };
    }

    public static int Count => _problems.Length;
    public static string GetProblemByIndex(int index) => _problems[index];
}