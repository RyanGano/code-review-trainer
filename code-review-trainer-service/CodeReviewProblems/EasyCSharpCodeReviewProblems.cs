namespace code_review_trainer_service.CodeReviewProblems;

public sealed class EasyCSharpCodeReviewProblems : CodeReviewProblems
{
    private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
    {
        // Patch example: original vs patched (Easy C#)
    new ProblemDefinition("Change variable names for clarity",
        @"-public int Add(int a, int b)
+public int Add(int x, int y)
 {
-    return a + b;
+    int z = x - y;
+    return z;

 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Addition was silently changed to subtraction", "The commit says the change is a rename for clarity, but the body now computes x - y instead of x + y. Add(2, 3) returns -1. A behaviour change hidden inside a rename is exactly the kind of edit reviewers must catch.", "critical", 3),
            new StoredReviewIssue("2", "Maintainability", "Renames make the parameters less descriptive, not more", "a/b became x/y and the result is stored in z. None of these names say anything about the values, so the stated goal of the commit is not met.", "low", 1),
            new StoredReviewIssue("3", "Style", "Stray blank line before the closing brace", "The patched body leaves an empty line before the closing brace, which no formatter would produce.", "trivial", 1)
        ],
        @"public int Add(int firstValue, int secondValue)
{
    return firstValue + secondValue;
}")),

        // Problem 1: Basic syntax error - missing semicolon
    new ProblemDefinition("Add greet function",
            @" public string GreetUser(string name) {
-    return ""Hello "" + name;
+    return ""Hello "" + name
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Missing semicolon terminating the return statement", "The patched return statement drops its semicolon, so the file no longer compiles. Nothing else in the change matters until this is fixed.", "critical", 3)
        ],
        @"public string GreetUser(string name)
{
    return $""Hello {name}"";
}")),

        // Problem 2: Basic logic error - wrong comparison operator
    new ProblemDefinition("Add isPositive function",
            @" public bool IsPositive(int number) {
-    if (number > 0) {
+    if (number = 0) {
         return true;
     }
     return false;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Assignment used where a comparison was intended", "number = 0 assigns zero to the parameter and yields an int, so the condition does not compile in C#. Even read as intent, testing for zero is the opposite of testing for positive.", "critical", 3)
        ],
        @"public bool IsPositive(int number)
{
    return number > 0;
}")),

        // Problem 3: Basic variable naming - using single letter variables
    new ProblemDefinition("Add calculateArea function",
            @"-public double CalculateRectangleArea(double width, double height) {
-    return width * height;
+public double CalculateRectangleArea(double w, double h) {
+    return w * h;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Descriptive parameter names replaced with single letters", "width and height were renamed to w and h. Callers using named arguments break, and at a call site CalculateRectangleArea(w: 3, h: 4) no longer says what the numbers mean. This is a readability regression with no upside.", "medium", 2)
        ],
        @"public double CalculateRectangleArea(double width, double height)
{
    return width * height;
}")),

        // Problem 4: Basic string concatenation - using + instead of string interpolation
    new ProblemDefinition("Add formatName function",
            @" public string FormatFullName(string first, string last) {
-    return $""{first} {last}"";
+    return first + "" "" + last;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Maintainability", "Interpolation replaced with manual concatenation", "The patch trades $\"{first} {last}\" for first + \" \" + last. The separator is now easy to lose in the noise and the code reads worse, for no benefit.", "medium", 2)
        ],
        @"public string FormatFullName(string first, string last)
{
    return $""{first} {last}"";
}")),

        // Problem 5: Basic array access - wrong index usage
    new ProblemDefinition("Add getFirst function",
            @" public int GetFirstElement(int[] numbers) {
-    return numbers[0];
+    return numbers[1];
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Returns the second element instead of the first", "C# arrays are zero-based, so the first element is numbers[0]. GetFirstElement now returns the wrong value for every input.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Throws on arrays shorter than two elements", "A single-element array is perfectly valid input for GetFirstElement, but numbers[1] raises IndexOutOfRangeException. There is also no null guard.", "medium", 2)
        ],
        @"public int GetFirstElement(int[] numbers)
{
    ArgumentNullException.ThrowIfNull(numbers);

    if (numbers.Length == 0)
    {
        throw new ArgumentException(""Array must not be empty"", nameof(numbers));
    }

    return numbers[0];
}")),

        // Problem 6: Basic type error - returning wrong type
    new ProblemDefinition("Add getLength function",
            @" public int GetStringLength(string text) {
-    return text.Length;
+    return text;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Returns the string itself from a method declared to return int", "return text cannot convert string to int, so the code does not compile. The method was meant to return text.Length.", "critical", 3)
        ],
        @"public int GetStringLength(string text)
{
    return text?.Length ?? 0;
}")),

        // Problem 7: Basic null check - accessing property without check
    new ProblemDefinition("Add getNameLength function",
            @" public int GetNameLength(string name) {
-    return name?.Length ?? 0;
+    return name.Length;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Null-safe access replaced with an unguarded dereference", "The null-conditional operator and its fallback were removed, so GetNameLength(null) now throws NullReferenceException where it used to return 0.", "high", 3)
        ],
        @"public int GetNameLength(string? name)
{
    return name?.Length ?? 0;
}")),

        // Problem 8: Basic variable declaration - using var instead of explicit type
    new ProblemDefinition("Add increment function",
            @" public int IncrementValue(int value) {
-    int result = value + 1;
+    var result = value + 1;
     return result;
 }",
        new StoredReview(ReviewStatus.Approve,
        [
            new StoredReviewIssue("1", "Style", "Explicit type replaced with var in a one-line method", "Swapping int for var hides the declared type in a codebase that spelled it out here. The inferred type is obviously int and nothing breaks, so this is a house-style preference rather than a defect.", "low", 1)
        ],
        @"public int IncrementValue(int value)
{
    return value + 1;
}")),

        // Problem 9: Basic array indexing - off-by-one error
    new ProblemDefinition("Add getLast function",
            @" public int GetLastElement(int[] numbers) {
-    return numbers[numbers.Length - 1];
+    return numbers[numbers.Length];
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Index is one past the end of the array", "Valid indexes run from 0 to Length - 1, so numbers[numbers.Length] throws IndexOutOfRangeException for every input, including a fully populated array.", "critical", 3)
        ],
        @"public int GetLastElement(int[] numbers)
{
    ArgumentNullException.ThrowIfNull(numbers);

    if (numbers.Length == 0)
    {
        throw new ArgumentException(""Array must not be empty"", nameof(numbers));
    }

    return numbers[^1];
}")),

        // Problem 10: Basic constant reassignment - trying to reassign const
    new ProblemDefinition("Add double function",
            @" public int DoubleValue(int value) {
     const int result = value * 2;
+    result = result + 1;
     return result;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Assignment to a const local", "result is declared const, so result = result + 1 is a compile error: the left-hand side of an assignment must be a variable.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "const initialised from a runtime value", "const int result = value * 2 does not compile either, because a const must be a compile-time constant and value is a parameter. This line is pre-existing but the patch builds on top of broken code.", "high", 3),
            new StoredReviewIssue("3", "Correctness", "Result no longer matches the method name", "Even ignoring the compiler, adding one means DoubleValue(4) would return 9. The added line contradicts what the method promises.", "medium", 2)
        ],
        @"public int DoubleValue(int value)
{
    return value * 2;
}")),

        // Problem 11: Basic function declaration - missing return type
    new ProblemDefinition("Add multiply function",
            @"+public Multiply(int a, int b) {
+    return a * b;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Method declaration has no return type", "public Multiply(int a, int b) reads as a constructor for a type called Multiply, so returning a * b does not compile. The declaration needs an explicit int return type.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Product can overflow silently", "Multiplying two ints wraps around unchecked; Multiply(int.MaxValue, 2) returns -2. Worth a deliberate decision even in a helper this small.", "low", 1)
        ],
        @"public int Multiply(int a, int b)
{
    return a * b;
}")),

        // Problem 12: Basic error handling - no try/catch for potential exception
    new ProblemDefinition("Add convertToNumber function",
            @" public int ConvertToInt(string text) {
-    if (int.TryParse(text, out int result)) {
-        return result;
-    }
-    return 0;
+    return int.Parse(text);
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Unhandled parse failure", "int.Parse throws FormatException on non-numeric input, ArgumentNullException on null and OverflowException on values outside int range. The TryParse version handled all three; the caller now has to.", "high", 3)
        ],
        @"public int ConvertToInt(string text)
{
    return int.TryParse(text, out int result) ? result : 0;
}")),

        // Problem 13: Basic boolean logic - wrong logical operator
    new ProblemDefinition("Add isValidAge function",
            @" public bool IsValidAge(int age) {
-    return age > 0 && age < 150;
+    return age > 0 || age < 150;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "&& changed to || makes the check always true", "Every int satisfies at least one of age > 0 and age < 150, so IsValidAge(-5) and IsValidAge(9999) both return true. The validation no longer rejects anything.", "high", 3)
        ],
        @"public bool IsValidAge(int age)
{
    return age > 0 && age < 150;
}")),

        // Problem 14: Basic array access - off-by-one error
    new ProblemDefinition("Get last element from array",
            @" public string GetLastElement(string[] items) {
-    return items[items.Length - 1];
+    return items[items.Length];
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Index is one past the end of the array", "items[items.Length] is always out of range, so the method throws IndexOutOfRangeException for every input instead of returning the last item.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "No guard for an empty or null array", "Even with the index corrected, an empty array has no last element and null throws. The method should say what it does in those cases.", "medium", 2)
        ],
        @"public string GetLastElement(string[] items)
{
    ArgumentNullException.ThrowIfNull(items);

    if (items.Length == 0)
    {
        throw new ArgumentException(""Array must not be empty"", nameof(items));
    }

    return items[^1];
}")),

        // Problem 15: Basic string concatenation - missing space
    new ProblemDefinition("Format full name from first and last",
            @" public string FormatFullName(string firstName, string lastName) {
-    return firstName + "" "" + lastName;
+    return firstName + lastName;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Separator between the names was dropped", "FormatFullName(\"Ada\", \"Lovelace\") now returns \"AdaLovelace\". The space is the entire point of the formatting method.", "medium", 2)
        ],
        @"public string FormatFullName(string firstName, string lastName)
{
    return $""{firstName} {lastName}"";
}")),

        // Problem 16: Basic null check - missing null check
    new ProblemDefinition("Get string length safely",
            @" public int GetStringLength(string text) {
-    if (text != null) {
-        return text.Length;
-    }
-    return 0;
+    return text.Length;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Null guard removed from a method that advertises safety", "The commit calls this getting the length safely, but text.Length now throws NullReferenceException when text is null - the one case the guard existed for.", "high", 3)
        ],
        @"public int GetStringLength(string? text)
{
    return text?.Length ?? 0;
}")),

        // Problem 17: Basic loop - off-by-one in condition
    new ProblemDefinition("Sum all elements in array",
            @" public int SumArray(int[] numbers) {
     int sum = 0;
-    for (int i = 0; i < numbers.Length; i++) {
+    for (int i = 0; i <= numbers.Length; i++) {
         sum += numbers[i];
     }
     return sum;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Loop runs one iteration past the end of the array", "Changing i < numbers.Length to i <= numbers.Length makes the final iteration read numbers[numbers.Length], which throws IndexOutOfRangeException. The method never returns a sum.", "critical", 3)
        ],
        @"public int SumArray(int[] numbers)
{
    ArgumentNullException.ThrowIfNull(numbers);

    return numbers.Sum();
}")),

        // Problem 18: Basic even check - wrong modulo operator
    new ProblemDefinition("Check if number is even",
            @" public bool IsEven(int number) {
-    return number % 2 == 0;
+    return number / 2 == 0;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Division used instead of remainder", "number / 2 == 0 is only true for -1, 0 and 1 because of integer division. IsEven(4) returns false and IsEven(1) returns true - the check is inverted for small odd values and wrong everywhere else.", "high", 3)
        ],
        @"public bool IsEven(int number)
{
    return number % 2 == 0;
}")),

        // Problem 19: Basic comparison - wrong operator
    new ProblemDefinition("Check if person is adult",
            @" public bool IsAdult(int age) {
-    return age >= 18;
+    return age > 18;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Boundary value 18 is now excluded", "Dropping the equals from >= means an 18 year old is not considered an adult. Off-by-one boundary changes like this rarely show up in testing.", "high", 3)
        ],
        @"public bool IsAdult(int age)
{
    return age >= 18;
}")),

        // Problem 20: Basic calculation - wrong arithmetic operator
    new ProblemDefinition("Calculate rectangle area",
            @" public int CalculateArea(int width, int height) {
-    return width * height;
+    return width + height;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Area computed with addition instead of multiplication", "CalculateArea(3, 4) returns 7 instead of 12. The method no longer computes an area at all.", "high", 3)
        ],
        @"public int CalculateArea(int width, int height)
{
    return width * height;
}")),

        // Problem 21: Basic string comparison - wrong operator
    new ProblemDefinition("Check if strings are equal",
            @" public bool AreEqual(string a, string b) {
-    return a == b;
+    return a != b;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Equality operator inverted", "AreEqual now returns true precisely when the two strings differ. Every caller gets the opposite of what the method name promises.", "high", 3)
        ],
        @"public bool AreEqual(string? a, string? b)
{
    return string.Equals(a, b, StringComparison.Ordinal);
}")),

        // Problem 22: Basic boolean logic - wrong negation
    new ProblemDefinition("Check if user is logged in",
            @" public bool IsLoggedIn(bool hasSession) {
-    return hasSession;
+    return !hasSession;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Security", "Login check inverted", "IsLoggedIn now returns true for users with no session and false for users who have one. On an authorization path this hands access to anonymous callers.", "high", 3)
        ],
        @"public bool IsLoggedIn(bool hasSession)
{
    return hasSession;
}")),

        // Problem 23: Basic absolute value - wrong logic
    new ProblemDefinition("Get absolute value",
            @" public int GetAbsolute(int number) {
-    return Math.Abs(number);
+    return number > 0 ? -number : number;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Conditional negates the wrong branch", "The ternary negates positive values and leaves negative ones alone, so GetAbsolute(5) returns -5 and GetAbsolute(-5) returns -5. It computes the negative absolute value.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Hand-rolled replacement for Math.Abs", "Math.Abs already expresses this, is correct at the boundaries and is what a reader expects. Replacing it with arithmetic adds risk for no gain.", "low", 1)
        ],
        @"public int GetAbsolute(int number)
{
    return Math.Abs(number);
}")),

        // Problem 24: Basic string reversal - off-by-one in loop
    new ProblemDefinition("Reverse string",
            @" public string ReverseString(string input) {
-    char[] chars = input.ToCharArray();
-    Array.Reverse(chars);
-    return new string(chars);
+    string result = """";
+    for (int i = input.Length - 1; i > 0; i--) {
+        result += input[i];
+    }
+    return result;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Loop stops before index 0 and drops the first character", "i > 0 never lets the loop read input[0], so ReverseString(\"abc\") returns \"cb\". The condition should be i >= 0.", "high", 3),
            new StoredReviewIssue("2", "Performance", "String concatenation inside the loop", "result += input[i] allocates a new string on every iteration, making the method O(n squared) in both time and garbage. The Array.Reverse version it replaced was linear.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "Working library call replaced with a manual loop", "ToCharArray plus Array.Reverse was shorter, correct and obvious. Hand-rolling the loop is what introduced the off-by-one in the first place.", "low", 1)
        ],
        @"public string ReverseString(string input)
{
    ArgumentNullException.ThrowIfNull(input);

    char[] chars = input.ToCharArray();
    Array.Reverse(chars);
    return new string(chars);
}")),

        // Problem 25: Basic sorting - wrong comparison operator
    new ProblemDefinition("Sort array in ascending order",
            @" public void SortArray(int[] numbers) {
-    Array.Sort(numbers);
+    for (int i = 0; i < numbers.Length - 1; i++) {
+        for (int j = i + 1; j < numbers.Length; j++) {
+            if (numbers[i] < numbers[j]) {
+                int temp = numbers[i];
+                numbers[i] = numbers[j];
+                numbers[j] = temp;
+            }
+        }
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Comparison sorts descending, not ascending", "The swap fires when numbers[i] < numbers[j], which pushes larger values to the front. The commit promises ascending order, so the operator must be >.", "high", 3),
            new StoredReviewIssue("2", "Performance", "O(n squared) selection sort replaces Array.Sort", "Array.Sort is an introsort with O(n log n) behaviour and is already well tested. The hand-written double loop is slower on any real input and is where the bug came from.", "medium", 2)
        ],
        @"public void SortArray(int[] numbers)
{
    ArgumentNullException.ThrowIfNull(numbers);

    Array.Sort(numbers);
}")),

        // Problem 26: Compilation error - undefined variable (typo in loop variable name)

    new ProblemDefinition("Fix typo in input validation loop variable",
            @" public bool ValidateInput(string input)
 {
     if (string.IsNullOrEmpty(input))
     {
         return false;
     }

     // Check if input contains only letters
     foreach (char c in input)
     {
-        if (!char.IsLetter(c))
+        if (!char.IsLetter(ch))
         {
             return false;
         }
     }
     return true;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Loop body references an undeclared variable", "The foreach declares c but the body tests ch, which does not exist in scope. The file does not compile. Note the commit message claims to fix a typo of exactly this kind while introducing one.", "critical", 3)
        ],
        @"public bool ValidateInput(string input)
{
    if (string.IsNullOrEmpty(input))
    {
        return false;
    }

    return input.All(char.IsLetter);
}")),

        // === GOOD CODE EXAMPLES (no issues to fix) ===
        // Good Example 1: Well-written calculator with proper naming and formatting
    new ProblemDefinition("Add Calculator class with Add and Multiply methods",
            @"+public class Calculator
+{
+    public double Add(double firstNumber, double secondNumber)
+    {
+        return firstNumber + secondNumber;
+    }
+
+    public double Multiply(double firstNumber, double secondNumber)
+    {
+        return firstNumber * secondNumber;
+    }
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 2: Clean string validation with proper error handling
    new ProblemDefinition("Add IsValidEmail with null-check and simple format checks",
            @"+public bool IsValidEmail(string email)
+{
+    if (string.IsNullOrWhiteSpace(email))
+    {
+        return false;
+    }
+
+    return email.Contains(""@"") && email.Contains(""."" );
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 3: Well-formatted loop with descriptive variable names
    new ProblemDefinition("Add PrintNumbers method that writes numbered output",
            @"+public void PrintNumbers(int count)
+{
+    for (int currentNumber = 1; currentNumber <= count; currentNumber++)
+    {
+        Console.WriteLine($""Number: {currentNumber}"" );
+    }
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 4: Clean data processing method
    new ProblemDefinition("Add FilterActiveUsers returning names of active users",
            @"+public List<string> FilterActiveUsers(List<User> users)
+{
+    var activeUsers = new List<string>();
+
+    foreach (var user in users)
+    {
+        if (user.IsActive)
+        {
+            activeUsers.Add(user.Name);
+        }
+    }
+
+    return activeUsers;
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 5: Proper null checking and formatting
    new ProblemDefinition("Format full name with defaults for missing names",
            @"+public string FormatFullName(string firstName, string lastName)
+{
+    if (string.IsNullOrEmpty(firstName))
+    {
+        firstName = ""Unknown"";
+    }
+
+    if (string.IsNullOrEmpty(lastName))
+    {
+        lastName = ""User"";
+    }
+
+    return $""{firstName} {lastName}"";
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 6: Simple and clean conditional logic
    new ProblemDefinition("Add GetAgeCategory mapping ages to categories",
            @"+public string GetAgeCategory(int age)
+{
+    if (age < 13)
+    {
+        return ""Child"";
+    }
+    else if (age < 20)
+    {
+        return ""Teenager"";
+    }
+    else if (age < 65)
+    {
+        return ""Adult"";
+    }
+    else
+    {
+        return ""Senior"";
+    }
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 7: Well-structured class with proper encapsulation
    new ProblemDefinition("Add BankAccount with Deposit validation and GetBalance",
            @"+public class BankAccount
+{
+    private decimal balance;
+
+    public BankAccount(decimal initialBalance)
+    {
+        balance = initialBalance;
+    }
+
+    public void Deposit(decimal amount)
+    {
+        if (amount <= 0)
+        {
+            throw new ArgumentException(""Amount must be greater than zero"", nameof(amount));
+        }
+        balance += amount;
+    }
+
+    public decimal GetBalance()
+    {
+        return balance;
+    }
+}",
        new StoredReview(ReviewStatus.Approve, []))
    };

    // Constructor is public so DI can construct and manage lifetime
    public EasyCSharpCodeReviewProblems()
        : base(_problems, Language.CSharp, "cs_easy", DifficultyLevel.Easy)
    {
    }
}
