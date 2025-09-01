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

 }"),

        // Problem 1: Basic syntax error - missing semicolon
    new ProblemDefinition("Add greet function",
            @" public string GreetUser(string name) {
-    return ""Hello "" + name
+    return ""Hello "" + name;
 }"),

        // Problem 2: Basic logic error - wrong comparison operator
    new ProblemDefinition("Add isPositive function",
            @" public bool IsPositive(int number) {
-    if (number = 0) {
-        return false;
-    }
+    if (number > 0) {
+        return true;
+    }
     return false;
 }"),

        // Problem 3: Basic variable naming - using single letter variables
    new ProblemDefinition("Add calculateArea function",
            @" public double CalculateRectangleArea(double w, double h) {
-    return w * h;
+    return width * height;
 }"),

        // Problem 4: Basic string concatenation - using + instead of string interpolation
    new ProblemDefinition("Add formatName function",
            @" public string FormatFullName(string first, string last) {
-    return first + "" "" + last;
+    return $""{first} {last}"";
 }"),

        // Problem 5: Basic array access - wrong index usage
    new ProblemDefinition("Add getFirst function",
            @" public int GetFirstElement(int[] numbers) {
-    return numbers[1];
+    return numbers[0];
 }"),

        // Problem 6: Basic type error - returning wrong type
    new ProblemDefinition("Add getLength function",
            @" public int GetStringLength(string text) {
-    return text;
+    return text.Length;
 }"),

        // Problem 7: Basic null check - accessing property without check
    new ProblemDefinition("Add getNameLength function",
            @" public int GetNameLength(string name) {
-    return name.Length;
+    return name?.Length ?? 0;
 }"),

        // Problem 8: Basic variable declaration - using var instead of explicit type
    new ProblemDefinition("Add increment function",
            @" public int IncrementValue(int value) {
-    var result = value + 1;
+    int result = value + 1;
     return result;
 }"),

        // Problem 9: Basic array indexing - off-by-one error
    new ProblemDefinition("Add getLast function",
            @" public int GetLastElement(int[] numbers) {
-    return numbers[numbers.Length];
+    return numbers[numbers.Length - 1];
 }"),

        // Problem 10: Basic constant reassignment - trying to reassign const
    new ProblemDefinition("Add double function",
            @" public int DoubleValue(int value) {
     const int result = value * 2;
-    result = result + 1;
+    return result;
 }"),

        // Problem 11: Basic function declaration - missing return type
    new ProblemDefinition("Add multiply function",
            @" public int Multiply(int a, int b) {
     return a * b;
 }"),

        // Problem 12: Basic error handling - no try/catch for potential exception
    new ProblemDefinition("Add convertToNumber function",
            @" public int ConvertToInt(string text) {
-    return int.Parse(text);
+    if (int.TryParse(text, out int result)) {
+        return result;
+    }
+    return 0;
 }"),

        // Problem 13: Basic boolean logic - wrong logical operator
    new ProblemDefinition("Add isValidAge function",
            @" public bool IsValidAge(int age) {
-    return age > 0 && age < 150;
+    return age > 0 || age < 150;
 }"),

        // Problem 14: Basic array access - off-by-one error
    new ProblemDefinition("Get last element from array",
            @" public string GetLastElement(string[] items) {
-    return items[items.Length - 1];
+    return items[items.Length];
 }"),

        // Problem 15: Basic string concatenation - missing space
    new ProblemDefinition("Format full name from first and last",
            @" public string FormatFullName(string firstName, string lastName) {
-    return firstName + "" "" + lastName;
+    return firstName + lastName;
 }"),

        // Problem 16: Basic null check - missing null check
    new ProblemDefinition("Get string length safely",
            @" public int GetStringLength(string text) {
-    if (text != null) {
-        return text.Length;
-    }
-    return 0;
+    return text.Length;
 }"),

        // Problem 17: Basic loop - off-by-one in condition
    new ProblemDefinition("Sum all elements in array",
            @" public int SumArray(int[] numbers) {
     int sum = 0;
-    for (int i = 0; i < numbers.Length; i++) {
+    for (int i = 0; i <= numbers.Length; i++) {
         sum += numbers[i];
     }
     return sum;
 }"),

        // Problem 18: Basic even check - wrong modulo operator
    new ProblemDefinition("Check if number is even",
            @" public bool IsEven(int number) {
-    return number % 2 == 0;
+    return number / 2 == 0;
 }"),

        // Problem 19: Basic comparison - wrong operator
    new ProblemDefinition("Check if person is adult",
            @" public bool IsAdult(int age) {
-    return age >= 18;
+    return age > 18;
 }"),

        // Problem 20: Basic calculation - wrong arithmetic operator
    new ProblemDefinition("Calculate rectangle area",
            @" public int CalculateArea(int width, int height) {
-    return width * height;
+    return width + height;
 }"),

        // Problem 21: Basic string comparison - wrong operator
    new ProblemDefinition("Check if strings are equal",
            @" public bool AreEqual(string a, string b) {
-    return a == b;
+    return a.Equals(b);
 }"),

        // Problem 22: Basic boolean logic - wrong negation
    new ProblemDefinition("Check if user is logged in",
            @" public bool IsLoggedIn(bool hasSession) {
-    return hasSession;
+    return !hasSession;
 }"),

        // Problem 23: Basic absolute value - wrong logic
    new ProblemDefinition("Get absolute value",
            @" public int GetAbsolute(int number) {
-    return Math.Abs(number);
+    return number < 0 ? -number : number;
 }"),

        // Problem 24: Basic string reversal - off-by-one in loop
    new ProblemDefinition("Reverse string",
            @" public string ReverseString(string input) {
-    char[] chars = input.ToCharArray();
-    Array.Reverse(chars);
-    return new string(chars);
+    string result = """";
+    for (int i = input.Length - 1; i >= 0; i--) {
+        result += input[i];
+    }
+    return result;
 }"),

        // Problem 25: Basic sorting - wrong comparison operator
    new ProblemDefinition("Sort array in ascending order",
            @" public void SortArray(int[] numbers) {
-    Array.Sort(numbers);
+    for (int i = 0; i < numbers.Length - 1; i++) {
+        for (int j = i + 1; j < numbers.Length; j++) {
+            if (numbers[i] > numbers[j]) {
+                int temp = numbers[i];
+                numbers[i] = numbers[j];
+                numbers[j] = temp;
+            }
+        }
+    }
 }"),

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
 }"),

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
+}"),

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
+}"),

        // Good Example 3: Well-formatted loop with descriptive variable names
    new ProblemDefinition("Add PrintNumbers method that writes numbered output",
            @"+public void PrintNumbers(int count)
+{
+    for (int currentNumber = 1; currentNumber <= count; currentNumber++)
+    {
+        Console.WriteLine($""Number: {currentNumber}"" );
+    }
+}"),

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
+}"),

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
+}"),

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
+}"),

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
+}" )
    };

    // Constructor is public so DI can construct and manage lifetime
    public EasyCSharpCodeReviewProblems()
        : base(_problems, Language.CSharp, "cs_easy", DifficultyLevel.Easy)
    {
    }
}