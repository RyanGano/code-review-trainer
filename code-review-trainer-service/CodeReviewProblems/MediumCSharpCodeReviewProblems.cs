namespace code_review_trainer_service.CodeReviewProblems;

public sealed class MediumCSharpCodeReviewProblems : CodeReviewProblems
{
        private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
        {
        // Patch example: original vs patched (Medium C#)
    new ProblemDefinition("Refactor parameter name and conditional logic for clarity",
         @"-public bool IsEven(int n)
+public bool IsEven(int value)
 {
-    return n % 2 == 0;
+    if (value % 2 == 1)
+    {
+        return true;
+    }
+    return false;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The test now matches odd numbers", "value % 2 == 1 is true for odd input, so IsEven(4) returns false and IsEven(3) returns true. The commit calls this a clarity refactor, but it inverts the result.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Comparing the remainder to 1 is wrong for negative values", "In C# -3 % 2 is -1, so the condition is false for negative odd numbers too. Even read as an isOdd check it is broken.", "high", 3),
            new StoredReviewIssue("3", "Maintainability", "if/return true/return false replaces a boolean expression", "Five lines now do what one expression did. Returning the comparison directly is the clearer form the commit claims to be aiming for.", "low", 1)
        ],
        @"public bool IsEven(int value)
{
    return value % 2 == 0;
}")),
        // Problem 1: Compilation error - missing semicolon and spelling error in comment
    new ProblemDefinition("Add UserManager class scaffold.",
            @"+public class UserManager
+{
+    // Retreives user information from the databas
+    public User GetUser(int userId)
+    {
+        var user = Database.FindUser(userId)
+        return user;
+    }
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Missing semicolon after the Database.FindUser call", "var user = Database.FindUser(userId) has no terminating semicolon, so the class does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Style", "Two spelling mistakes in the doc comment", "'Retreives' should be 'Retrieves' and 'databas' should be 'database'. Comments are the first thing the next reader trusts, so typos in them spread.", "trivial", 1),
            new StoredReviewIssue("3", "Correctness", "No handling for a user that does not exist", "FindUser can return null and the method hands that straight back with a non-nullable User return type, pushing the failure onto every caller.", "low", 1)
        ],
        @"public class UserManager
{
    // Retrieves user information from the database
    public User? GetUser(int userId)
    {
        return Database.FindUser(userId);
    }
}")),

        // Problem 2: Type mismatch compilation error and logic issue - assignment instead of comparison, wrong return type
    new ProblemDefinition("Refactor Calculator.CalculatePercentage to handle edge cases",
            @" public class Calculator
 {
-    public double CalculatePercentage(int value, int total)
-    {
-        if (total == 0) return 0.0;
-        return (double)value / total * 100.0;
-    }
+    public string CalculatePercentage(int value, int total)
+    {
+        if (total = 0)
+        {
+            return ""Cannot divide by zero"";
+        }
+        double percentage = (value / total) * 100;
+        return percentage;
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Assignment used as a condition", "if (total = 0) assigns zero to total and yields an int, which is not a bool. It does not compile, and if it did it would set the divisor to zero on every call.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Returns a double from a method declared to return string", "return percentage cannot convert double to string. The method signature and the body disagree.", "critical", 3),
            new StoredReviewIssue("3", "Correctness", "Integer division truncates the result", "value and total are both int, so (value / total) is computed with integer division before the multiply. CalculatePercentage(1, 4) yields 0 instead of 25. The removed version cast to double first.", "high", 3),
            new StoredReviewIssue("4", "Maintainability", "Error signalled by returning a message string", "Changing the return type from double to string forces every caller to parse a number back out and to string-match on the error text. An exception or a nullable double keeps the contract usable.", "medium", 2)
        ],
        @"public class Calculator
{
    public double CalculatePercentage(int value, int total)
    {
        if (total == 0)
        {
            throw new ArgumentException(""Total must not be zero"", nameof(total));
        }

        return (double)value / total * 100.0;
    }
}")),

        // Problem 3: Spelling error in variable name and off-by-one error - incorrect substring bounds
    new ProblemDefinition("Optimize substring extraction for better performance",
            @" public List<string> GetSubstring(string text, int maxLength)
 {
-    var results = new List<string>();
-    for (int i = 0; i < text.Length - maxLength; i++)
-    {
-        results.Add(text.Substring(i, maxLength));
-    }
-    return results;
+    var results = new List<string>();
+    for (int i = 0; i <= text.Length - maxLenght; i++)
+    {
+        results.Add(text.Substring(i, maxLenght));
+    }
+    return results;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "maxLenght is not a declared identifier", "The parameter is maxLength. Both uses of maxLenght are misspelled, so the method does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Loop bound changed from < to <=", "With the spelling fixed, i <= text.Length - maxLength makes the last iteration call Substring at the exact end of the string plus one character, throwing ArgumentOutOfRangeException.", "high", 3),
            new StoredReviewIssue("3", "Maintainability", "The commit claims an optimisation that is not there", "Nothing in the patch is faster than what it replaced; the only changes are the misspelling and the bound. A commit message that does not match the diff makes history untrustworthy.", "low", 1)
        ],
        @"public List<string> GetSubstring(string text, int maxLength)
{
    ArgumentNullException.ThrowIfNull(text);

    var results = new List<string>();
    for (int i = 0; i + maxLength <= text.Length; i++)
    {
        results.Add(text.Substring(i, maxLength));
    }

    return results;
}")),

        // Problem 5: Subtle null reference potential and spelling error - no null check for order.Items
    new ProblemDefinition("Simplify order processing logic",
            @" public class OrderProcessor
 {
-    public decimal ProcessOrder(Order order)
-    {
-        if (order == null || order.Items == null)
-        {
-            return 0;
-        }
-
-        decimal total = 0;
-        foreach (var item in order.Items)
-        {
-            total += item.Price * item.Quantity;
-        }
-        return total;
-    }
+    // Proccess orders and calculate totals
+    public decimal ProcessOrder(Order order)
+    {
+        decimal total = 0;
+        foreach (var item in order.Items)
+        {
+            total += item.Price * item.Quantity;
+        }
+        return total;
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Null guards removed before dereferencing order.Items", "ProcessOrder(null) throws NullReferenceException, and so does an order whose Items collection was never populated. The removed check turned both into a total of zero.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Removing validation is not simplification", "The commit says simplify, but the only thing removed is error handling. That is a behaviour change and it belongs in the message.", "medium", 2),
            new StoredReviewIssue("3", "Style", "'Proccess' is misspelled in the new comment", "The comment added by this patch spells Process with two Cs.", "trivial", 1)
        ],
        @"public class OrderProcessor
{
    // Processes orders and calculates totals
    public decimal ProcessOrder(Order order)
    {
        if (order?.Items is null)
        {
            return 0;
        }

        return order.Items.Sum(item => item.Price * item.Quantity);
    }
}")),

        // Problem 6: Resource leak - missing using statement and dispose
    new ProblemDefinition("Optimize file reading performance",
            @" public string ReadFileContent(string filePath)
 {
-    using (var reader = new StreamReader(filePath))
-    {
-        return reader.ReadToEnd();
-    }
+    var reader = new StreamReader(filePath);
+    string content = reader.ReadToEnd();
+    return content;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "StreamReader is never disposed", "Dropping the using leaves the file handle open until the finalizer runs. The file stays locked on Windows, and repeated calls exhaust handles.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "The commit claims a performance win", "Removing the using does not make reading faster. The message describes an optimisation the diff does not contain.", "low", 1),
            new StoredReviewIssue("3", "Correctness", "No handling for a missing or unreadable file", "A bad path throws FileNotFoundException straight out of a method whose signature suggests it just returns text.", "low", 1)
        ],
        @"public string ReadFileContent(string filePath)
{
    using var reader = new StreamReader(filePath);
    return reader.ReadToEnd();
}")),

        // Problem 7: Compilation error and infinite loop potential - missing semicolon and increment
    new ProblemDefinition("Improve number printing with better formatting",
            @" public void PrintNumbers(int count)
 {
-    for (int i = 0; i < count; i++)
-    {
-        Console.WriteLine(i);
-    }
+    int i = 0
+    while (i < count)
+    {
+        Console.WriteLine(i);
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Missing semicolon after int i = 0", "The declaration has no terminator, so the method does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "The loop counter is never incremented", "The for loop's i++ was lost in the conversion to while, so once it compiles the method prints 0 forever and never returns.", "critical", 3),
            new StoredReviewIssue("3", "Maintainability", "Nothing about the formatting improved", "The commit promises better formatting; the diff swaps a correct for loop for a broken while loop and changes no output format.", "medium", 2)
        ],
        @"public void PrintNumbers(int count)
{
    for (int i = 0; i < count; i++)
    {
        Console.WriteLine(i);
    }
}")),

        // Problem 8: Type mismatch and spelling error in string - wrong calculation and spelling
    new ProblemDefinition("Enhance age calculation with better precision",
            @" public int GetUserAge(string birthDate)
 {
-    DateTime birth = DateTime.Parse(birthDate);
-    TimeSpan age = DateTime.Now - birth;
-    return (int)(age.TotalDays / 365.25);
+    DateTime birth = DateTime.Parse(birthDate);
+    TimeSpan age = DateTime.Now - birth;
+    Console.WriteLine(""User is approximatly "" + age.Days / 365 + "" years old"");
+    return age.Days / 365;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Dividing by 365 loses precision the commit promised to improve", "365.25 accounted for leap years; 365 drifts by a day every four years and eventually reports the wrong age around a birthday. The commit message claims the opposite of what the diff does.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Console output added to a calculation method", "GetUserAge now writes to stdout on every call. That is an unannounced side effect and it makes the method unusable in a service.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "DateTime.Parse without a culture or failure path", "Parse throws FormatException on bad input and interprets ambiguous dates using the current culture, so the same string means different things on different machines.", "medium", 2),
            new StoredReviewIssue("4", "Style", "'approximatly' is misspelled", "The message written to the console should read 'approximately'.", "trivial", 1)
        ],
        @"public int GetUserAge(string birthDate)
{
    if (!DateTime.TryParse(birthDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out var birth))
    {
        throw new ArgumentException(""Birth date is not a valid date"", nameof(birthDate));
    }

    var today = DateTime.Today;
    var age = today.Year - birth.Year;
    if (birth.Date > today.AddYears(-age))
    {
        age--;
    }

    return age;
}")),

        // Problem 9: Compilation error - wrong collection type and logic issue - wrong generic parameters
    new ProblemDefinition("Optimize word counting algorithm",
            @" public Dictionary<string, int> CountWords(string text)
 {
-    var wordCount = new Dictionary<string, int>();
-    string[] words = text.Split(' ');
-
-    foreach (string word in words)
-    {
-        if (wordCount.ContainsKey(word))
-        {
-            wordCount[word]++;
-        }
-        else
-        {
-            wordCount[word] = 1;
-        }
-    }
-    return wordCount;
+    var wordCount = new Dictionary<string>();
+    string[] words = text.Split(' ');
+
+    foreach (string word in words)
+    {
+        if (wordCount.ContainsKey(word))
+        {
+            wordCount[word]++;
+        }
+        else
+        {
+            wordCount[word] = 1;
+        }
+    }
+    return wordCount;
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Dictionary declared with one type argument", "Dictionary<string> does not exist - the type takes a key and a value. The method does not compile and cannot return the declared Dictionary<string, int>.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Splitting on a single space mis-counts real text", "Split(' ') produces empty entries for runs of spaces and keeps punctuation attached, so 'the  cat.' counts an empty string and 'cat.' as separate words. Nothing in the patch optimises anything either.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "ContainsKey followed by an indexer hashes twice", "CollectionsMarshal or TryGetValue avoids the double lookup, which matters for the large inputs the commit claims to be optimising.", "low", 1)
        ],
        @"public Dictionary<string, int> CountWords(string text)
{
    ArgumentNullException.ThrowIfNull(text);

    var wordCount = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
    foreach (var word in text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
    {
        wordCount.TryGetValue(word, out var count);
        wordCount[word] = count + 1;
    }

    return wordCount;
}")),

        // Problem 10: Subtle array bounds issue and spelling error - no bounds check for empty array
    new ProblemDefinition("Improve array processing efficiency",
            @" public class ArrayProcessor
 {
-    public int FindMaxIndex(int[] numbers)
-    {
-        if (numbers == null || numbers.Length == 0)
-        {
-            return -1;
-        }
-
-        int maxIndex = 0;
-        for (int i = 1; i < numbers.Length; i++)
-        {
-            if (numbers[i] > numbers[maxIndex])
-            {
-                maxIndex = i;
-            }
-        }
-        return maxIndex;
-    }
+    // Proceses array elements
+    public int FindMaxIndex(int[] numbers)
+    {
+        int maxIndex = 0;
+        for (int i = 1; i < numbers.Length; i++)
+        {
+            if (numbers[i] > numbers[maxIndex])
+            {
+                maxIndex = i;
+            }
+        }
+        return maxIndex;
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Null and empty guards removed", "FindMaxIndex(null) throws NullReferenceException on numbers.Length, and an empty array now returns 0 - an index that does not exist - instead of the -1 the removed guard produced.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "The commit promises efficiency but only deletes checks", "The loop is unchanged. Removing validation is a behaviour change dressed up as an optimisation.", "medium", 2),
            new StoredReviewIssue("3", "Style", "'Proceses' is misspelled in the new comment", "The comment added by this patch should read 'Processes'.", "trivial", 1)
        ],
        @"public class ArrayProcessor
{
    // Processes array elements
    public int FindMaxIndex(int[] numbers)
    {
        if (numbers is null || numbers.Length == 0)
        {
            return -1;
        }

        int maxIndex = 0;
        for (int i = 1; i < numbers.Length; i++)
        {
            if (numbers[i] > numbers[maxIndex])
            {
                maxIndex = i;
            }
        }

        return maxIndex;
    }
}")),

        // Problem 11: Compilation error - missing return type and logic issue - wrong method signature
    new ProblemDefinition("Refactor string reversal for better readability",
            @" public class StringUtils
 {
-    public static string ReverseString(string input)
-    {
-        if (string.IsNullOrEmpty(input))
-        {
-            return input;
-        }
-
-        char[] chars = input.ToCharArray();
-        for (int i = 0; i < chars.Length / 2; i++)
-        {
-            char temp = chars[i];
-            chars[i] = chars[chars.Length - 1 - i];
-            chars[chars.Length - 1 - i] = temp;
-        }
-        return new string(chars);
-    }
+    public ReverseString(string input)
+    {
+        if (input == null) return null;
+
+        char[] chars = input.ToCharArray();
+        for (int i = 0; i < chars.Length / 2; i++)
+        {
+            char temp = chars[i];
+            chars[i] = chars[chars.Length - 1 - i];
+            chars[chars.Length - 1 - i] = temp;
+        }
+        return new string(chars);
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Method declaration lost its return type", "public ReverseString(string input) parses as a constructor for a type named ReverseString inside StringUtils, so both return statements are errors and the class does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "static modifier removed from a utility method", "Every existing StringUtils.ReverseString(...) call site breaks, and callers now need an instance of a class that holds no state.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "Nothing about readability improved", "The commit says readability; the diff changes the signature and the null handling. The manual swap loop that made the method hard to read is untouched.", "low", 1)
        ],
        @"public class StringUtils
{
    public static string ReverseString(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        char[] chars = input.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
}")),

        // Problem 12: Potential division by zero and spelling error - no check for empty list
    new ProblemDefinition("Streamline average calculation",
            @" public class MathOperations
 {
-    public double CalculateAverage(List<int> numbers)
-    {
-        if (numbers == null || numbers.Count == 0)
-        {
-            return 0.0;
-        }
-
-        int sum = 0;
-        foreach (int number in numbers)
-        {
-            sum += number;
-        }
-        return (double)sum / numbers.Count;
-    }
+    // Calcualtes the average of numbers
+    public double CalculateAverage(List<int> numbers)
+    {
+        int sum = 0;
+        foreach (int number in numbers)
+        {
+            sum += number;
+        }
+        return sum / numbers.Count;
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "The double cast was dropped, so the average is integer division", "sum / numbers.Count divides two ints and truncates before the result is widened. The average of 1 and 2 comes back as 1.0, not 1.5.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "Null and empty guards removed", "A null list throws NullReferenceException and an empty list throws DivideByZeroException on the integer division. The removed guard returned 0.0 for both.", "high", 3),
            new StoredReviewIssue("3", "Style", "'Calcualtes' is misspelled in the new comment", "The comment added by this patch should read 'Calculates'.", "trivial", 1)
        ],
        @"public class MathOperations
{
    // Calculates the average of numbers
    public double CalculateAverage(List<int> numbers)
    {
        if (numbers is null || numbers.Count == 0)
        {
            return 0.0;
        }

        return numbers.Average();
    }
}")),

        // Problem 13: Compilation error and concurrency issue - missing semicolon and no thread safety
    new ProblemDefinition("Simplify counter implementation",
            @" public class Counter
 {
-    private readonly object lockObject = new object();
-    private int count = 0;
-
-    public void Increment()
-    {
-        lock (lockObject)
-        {
-            count++;
-        }
-    }
-
-    public int GetCount()
-    {
-        lock (lockObject)
-        {
-            return count;
-        }
-    }
+    private int count = 0;
+
+    public void Increment()
+    {
+        count++
+    }
+
+    public int GetCount()
+    {
+        return count;
+    }
 }",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Missing semicolon after count++", "The statement has no terminator, so the class does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Locking removed from a shared counter", "count++ is a read, an add and a write. Two threads can interleave and lose an increment, and GetCount can now observe a stale value because there is no memory barrier. Interlocked.Increment is the cheap fix if the lock was the thing being simplified away.", "high", 3)
        ],
        @"public class Counter
{
    private int count;

    public void Increment()
    {
        Interlocked.Increment(ref count);
    }

    public int GetCount()
    {
        return Volatile.Read(ref count);
    }
}")),

        // Problem 14: Subtle logic error and spelling mistake
    new ProblemDefinition("Add IsPalindrome utility.",
            @"+public bool IsPalindrome(string text)
+{
+    // Check if text is a palindrom
+    text = text.ToLower().Replace("" "", """");
+
+    for (int i = 0; i < text.Length / 2; i++)
+    {
+        if (text[i] != text[text.Length - i])
+        {
+            return false;
+        }
+    }
+    return true;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Mirror index is off by one", "The character opposite index i is text[text.Length - 1 - i]. As written, i = 0 reads text[text.Length], which throws IndexOutOfRangeException for every non-empty input.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Null input throws before the loop", "text.ToLower() dereferences the parameter with no guard, so IsPalindrome(null) throws NullReferenceException.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "ToLower is culture sensitive", "ToLower() uses the current culture, which changes the answer for some inputs on Turkish locales. ToLowerInvariant is what this comparison wants.", "low", 1),
            new StoredReviewIssue("4", "Style", "'palindrom' is misspelled in the comment", "The comment above the normalisation should read 'palindrome'. It is the only description of what the method does, so it is worth getting right.", "trivial", 1)
        ],
        @"public bool IsPalindrome(string text)
{
    ArgumentNullException.ThrowIfNull(text);

    // Check if text is a palindrome
    var normalized = text.ToLowerInvariant().Replace("" "", """");

    for (int i = 0; i < normalized.Length / 2; i++)
    {
        if (normalized[i] != normalized[normalized.Length - 1 - i])
        {
            return false;
        }
    }

    return true;
}")),

        // Problem 15: Memory leak potential and compilation error
    new ProblemDefinition("Add EventPublisher with DataChanged event.",
            @"+public class EventPublisher
+{
+    public event EventHandler DataChanged;
+
+    public void Subscribe(object subscriber)
+    {
+        DataChanged += (sender, e) => {
+            Console.WriteLine($""Subscriber {subscriber} notified"");
+        }
+    }
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Missing semicolon after the lambda", "The += statement ends with the lambda's closing brace and no terminator, so the class does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Subscribers can never be removed", "The handler is an anonymous lambda, so nothing holds a reference that could be passed to -=. Each Subscribe call adds another handler that captures subscriber and keeps it alive for the lifetime of the publisher.", "high", 3),
            new StoredReviewIssue("3", "Maintainability", "Subscribe ignores the object it is given", "The parameter is only used inside a log message; the subscriber never receives the event. The method does not do what its name says.", "medium", 2)
        ],
        @"public class EventPublisher
{
    public event EventHandler? DataChanged;

    public IDisposable Subscribe(EventHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);

        DataChanged += handler;
        return new Unsubscriber(() => DataChanged -= handler);
    }

    private sealed class Unsubscriber(Action unsubscribe) : IDisposable
    {
        public void Dispose() => unsubscribe();
    }
}")),

        // Problem 16: Type casting issue and spelling error
    new ProblemDefinition("Add NumberProcessor.ConvertToInteger.",
            @"+public class NumberProcessor
+{
+    // Converts string to integar
+    public int ConvertToInteger(object value)
+    {
+        string strValue = (string)value;
+        return int.Parse(strValue);
+    }
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Unchecked cast from object to string", "The parameter accepts any object, so passing an int or a null throws InvalidCastException or produces a null that int.Parse rejects. A pattern match or Convert.ToInt32 handles the cases the signature allows.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "int.Parse has no failure path", "Non-numeric text throws FormatException and large values throw OverflowException, both straight out of a method that gives no indication it can fail.", "medium", 2),
            new StoredReviewIssue("3", "Style", "'integar' is misspelled in the comment", "The summary comment should read 'Converts a value to an integer'. Misspellings in the one line describing a public method tend to be copied into the next one.", "trivial", 1)
        ],
        @"public class NumberProcessor
{
    // Converts a value to an integer
    public int ConvertToInteger(object value)
    {
        return value switch
        {
            int number => number,
            string text when int.TryParse(text, out var parsed) => parsed,
            _ => throw new ArgumentException(""Value is not convertible to an integer"", nameof(value))
        };
    }
}")),

        // Problem 17: Compilation error and performance issue
    new ProblemDefinition("Add ConcatenateStrings helper.",
            @"+public string ConcatenateStrings(List<string> strings)
+{
+    string result = """"
+    foreach (string str in strings)
+    {
+        result += str;
+    }
+    return result;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Missing semicolon after the result declaration", "string result = \"\" has no terminator, so the method does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Performance", "Quadratic string concatenation", "Strings are immutable, so result += str allocates and copies the whole accumulated string on every iteration. For n items this is O(n squared) in both time and allocations; string.Concat or StringBuilder is linear.", "medium", 2)
        ],
        @"public string ConcatenateStrings(List<string> strings)
{
    ArgumentNullException.ThrowIfNull(strings);

    return string.Concat(strings);
}")),

        // Problem 18: Subtle boundary condition and spelling error
    new ProblemDefinition("Add DateUtils.DaysBetween utility.",
            @"+public class DateUtils
+{
+    // Calcuates days between two dates
+    public int DaysBetween(DateTime start, DateTime end)
+    {
+        TimeSpan difference = end - start;
+        return (int)difference.TotalDays;
+    }
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Time of day makes the result truncate unexpectedly", "TotalDays is fractional and the cast rounds toward zero, so 1 January 23:00 to 2 January 01:00 reports 0 days. Comparing the Date components is what callers of a DaysBetween helper expect.", "medium", 2),
            new StoredReviewIssue("2", "Correctness", "Kind and offset are ignored", "Subtracting a Utc DateTime from a Local one produces a meaningless span, and nothing here rejects or normalises that.", "medium", 2),
            new StoredReviewIssue("3", "Correctness", "Negative results are undocumented", "Passing end before start returns a negative number. Whether that is intended should be stated or guarded.", "low", 1),
            new StoredReviewIssue("4", "Style", "'Calcuates' is misspelled in the comment", "The comment above DaysBetween should read 'Calculates days between two dates', which is also the place to state whether the result can be negative.", "trivial", 1)
        ],
        @"public class DateUtils
{
    // Calculates whole days between two dates
    public int DaysBetween(DateOnly start, DateOnly end)
    {
        return end.DayNumber - start.DayNumber;
    }
}")),

        // Problem 19: Compilation error and resource management
    new ProblemDefinition("Add WriteToFile helper.",
            @"+public void WriteToFile(string fileName, string content)
+{
+    FileStream stream = new FileStream(fileName, FileMode.Create);
+    StreamWriter writer = new StreamWriter(stream);
+    writer.Write(content)
+    writer.Flush();
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Missing semicolon after writer.Write(content)", "The call has no terminator, so the method does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "Neither the writer nor the stream is disposed", "Flush pushes the buffer out but leaves both handles open, so the file stays locked and the handle is only released when the finalizer eventually runs. A using declaration on each disposable fixes it.", "high", 3)
        ],
        @"public void WriteToFile(string fileName, string content)
{
    using var stream = new FileStream(fileName, FileMode.Create);
    using var writer = new StreamWriter(stream);
    writer.Write(content);
}")),

        // Problem 20: Logic error and spelling mistake in comment
    new ProblemDefinition("Add Validator.IsValidEmail implementation.",
            @"+public class Validator
+{
+    // Validates email adress format
+    public bool IsValidEmail(string email)
+    {
+        if (string.IsNullOrEmpty(email))
+            return true;
+
+        return email.Contains(""@"") && email.Contains(""."");
+    }
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Empty input is reported as valid", "The guard returns true for null and empty, so IsValidEmail(\"\") passes. This is the exact inverse of what the check is for, and it lets blank addresses through every caller.", "high", 3),
            new StoredReviewIssue("2", "Correctness", "The format check is far too permissive", "'@.' satisfies both Contains calls. At minimum the @ must appear before the final dot and neither may be at the ends of the string.", "low", 1),
            new StoredReviewIssue("3", "Style", "'adress' is misspelled in the comment", "The comment above IsValidEmail should read 'Validates email address format'. Worth fixing while the inverted guard on the next line is being corrected anyway.", "trivial", 1)
        ],
        @"public class Validator
{
    // Validates email address format
    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return false;
        }

        try
        {
            var mailAddress = new System.Net.Mail.MailAddress(email);
            return mailAddress.Address == email;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}")),

        // Problem 21: Compilation error and algorithm issue
    new ProblemDefinition("Add BinarySearch implementation.",
            @"+public int BinarySearch(int[] array, int target)
+{
+    int left = 0;
+    int right = array.Length;
+
+    while (left <= right)
+    {
+        int mid = (left + right) / 2
+        if (array[mid] == target)
+        {
+            return mid;
+        }
+        else if (array[mid] < target)
+        {
+            left = mid + 1;
+        }
+        else
+        {
+            right = mid - 1;
+        }
+    }
+    return -1;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Missing semicolon after the mid calculation", "int mid = (left + right) / 2 has no terminator, so the method does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "right starts one past the last index", "With right = array.Length and an inclusive while (left <= right), the first probe of a search for a value larger than everything reads array[array.Length] and throws IndexOutOfRangeException. right must start at array.Length - 1.", "high", 3),
            new StoredReviewIssue("3", "Correctness", "left + right can overflow", "For very large arrays the sum exceeds int.MaxValue and mid goes negative. left + (right - left) / 2 avoids it.", "low", 1)
        ],
        @"public int BinarySearch(int[] array, int target)
{
    ArgumentNullException.ThrowIfNull(array);

    int left = 0;
    int right = array.Length - 1;

    while (left <= right)
    {
        int mid = left + (right - left) / 2;
        if (array[mid] == target)
        {
            return mid;
        }

        if (array[mid] < target)
        {
            left = mid + 1;
        }
        else
        {
            right = mid - 1;
        }
    }

    return -1;
}")),

        // Problem 22: Exception handling issue and spelling error
    new ProblemDefinition("Add DatabaseManager.GetData method.",
            @"+public class DatabaseManager
+{
+    // Retreive data from databse
+    public DataTable GetData(string query)
+    {
+        try
+        {
+            using (var connection = new SqlConnection(connectionString))
+            {
+                connection.Open();
+                var command = new SqlCommand(query, connection);
+                var adapter = new SqlDataAdapter(command);
+                var dataTable = new DataTable();
+                adapter.Fill(dataTable);
+                return dataTable;
+            }
+        }
+        catch (Exception ex)
+        {
+            Console.WriteLine(ex.Message);
+            return null;
+        }
+    }
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Every exception is swallowed and null is returned", "catch (Exception) turns a connection failure, a syntax error and an out-of-memory condition into the same silent null. Callers cannot tell failure from an empty result, and the only record is a Console.WriteLine.", "high", 3),
            new StoredReviewIssue("2", "Security", "Caller-supplied SQL is executed verbatim", "GetData takes a raw query string and runs it. If any caller builds that string from user input this is a SQL injection hole; the method should take parameters instead.", "high", 3),
            new StoredReviewIssue("3", "Correctness", "connectionString is not declared in this class", "Nothing in the patch defines the field, so the class does not compile as written.", "medium", 2),
            new StoredReviewIssue("4", "Style", "'Retreive' and 'databse' are misspelled", "The comment should read 'Retrieve data from database'.", "trivial", 1)
        ],
        @"public class DatabaseManager(string connectionString)
{
    // Retrieves data from the database
    public async Task<DataTable> GetDataAsync(string query, params SqlParameter[] parameters)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new SqlCommand(query, connection);
        command.Parameters.AddRange(parameters);

        var dataTable = new DataTable();
        using var reader = await command.ExecuteReaderAsync();
        dataTable.Load(reader);
        return dataTable;
    }
}")),

        // Problem 23: Compilation error and logic flaw
    new ProblemDefinition("Add FilterItems generic helper.",
            @"+public List<T> FilterItems<T>(List<T> items, Func<T, bool> predicate)
+{
+    var result = new List<T>();
+    for (int i = 0; i < items.Count - 1; i++)
+    {
+        if (predicate(items[i])
+        {
+            result.Add(items[i]);
+        }
+    }
+    return result;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Unbalanced parenthesis in the if condition", "if (predicate(items[i]) is missing its closing bracket, so the method does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Correctness", "The loop never examines the last item", "i < items.Count - 1 stops one element early, so a matching final item is silently dropped. FilterItems on a single-item list always returns empty.", "high", 3),
            new StoredReviewIssue("3", "Maintainability", "Reimplements Enumerable.Where", "The whole body is what Where already does, lazily and without the index arithmetic that introduced the bug.", "low", 1)
        ],
        @"public List<T> FilterItems<T>(List<T> items, Func<T, bool> predicate)
{
    ArgumentNullException.ThrowIfNull(items);
    ArgumentNullException.ThrowIfNull(predicate);

    return items.Where(predicate).ToList();
}")),

        // Problem 24: Subtle threading issue and spelling error
    new ProblemDefinition("Add Cache simple in-memory store.",
            @"+public class Cache
+{
+    private Dictionary<string, object> cache = new Dictionary<string, object>();
+
+    // Retreives cached value
+    public object Get(string key)
+    {
+        if (cache.ContainsKey(key))
+        {
+            return cache[key];
+        }
+        return null;
+    }
+
+    public void Set(string key, object value)
+    {
+        cache[key] = value;
+    }
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Dictionary is not safe for concurrent use", "A cache is shared by definition, but Dictionary allows one writer or many readers, never both. A Set that resizes the buckets while another thread is in Get can corrupt the instance or spin forever. ConcurrentDictionary is the drop-in fix.", "high", 3),
            new StoredReviewIssue("2", "Maintainability", "Nothing ever evicts an entry", "The cache grows without bound for the lifetime of the process. There is no size limit, no expiry and no way to remove a key.", "medium", 2),
            new StoredReviewIssue("3", "Maintainability", "ContainsKey followed by the indexer looks the key up twice", "TryGetValue does it once and removes the window where another thread removes the key between the two calls.", "low", 1),
            new StoredReviewIssue("4", "Style", "'Retreives' is misspelled in the comment", "The comment above Get should read 'Retrieves a cached value'. The same misspelling appears in other classes in this set, which is how it spreads.", "trivial", 1)
        ],
        @"public class Cache
{
    private readonly ConcurrentDictionary<string, object> cache = new();

    // Retrieves a cached value
    public object? Get(string key)
    {
        return cache.TryGetValue(key, out var value) ? value : null;
    }

    public void Set(string key, object value)
    {
        cache[key] = value;
    }

    public bool Remove(string key)
    {
        return cache.TryRemove(key, out _);
    }
}")),

        // Problem 25: Compilation error and performance issue
    new ProblemDefinition("Add ContainsDuplicate check.",
            @"+public bool ContainsDuplicate(List<int> numbers)
+{
+    for (int i = 0; i < numbers.Count; i++)
+    {
+        for (int j = i + 1; j < numbers.Count; j++)
+        {
+            if (numbers[i] == numbers[j])
+            {
+                return true
+            }
+        }
+    }
+    return false;
+}",
        new StoredReview(ReviewStatus.Reject,
        [
            new StoredReviewIssue("1", "Correctness", "Missing semicolon after return true", "The statement has no terminator, so the method does not compile.", "critical", 3),
            new StoredReviewIssue("2", "Performance", "Quadratic scan where a hash set is linear", "The nested loops compare every pair, so a list of 10,000 numbers performs about 50 million comparisons. Adding to a HashSet and checking the return value answers the same question in one pass.", "medium", 2)
        ],
        @"public bool ContainsDuplicate(List<int> numbers)
{
    ArgumentNullException.ThrowIfNull(numbers);

    var seen = new HashSet<int>(numbers.Count);
    return numbers.Any(number => !seen.Add(number));
}")),

        // === GOOD CODE EXAMPLES (no issues) ===

        // Good Example 1: Proper error handling and resource management
    new ProblemDefinition("Add FileProcessor.ReadFileAsync example.",
            @"+public class FileProcessor
+{
+    public async Task<string> ReadFileAsync(string filePath)
+    {
+        if (string.IsNullOrEmpty(filePath))
+        {
+            throw new ArgumentException(""File path cannot be null or empty"", nameof(filePath));
+        }
+
+        if (!File.Exists(filePath))
+        {
+            throw new FileNotFoundException($""File not found: {filePath}"");
+        }
+
+        using (var reader = new StreamReader(filePath))
+        {
+            return await reader.ReadToEndAsync();
+        }
+    }
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 2: Thread-safe implementation with proper validation
    new ProblemDefinition("Add ThreadSafeCounter example.",
            @"+public class ThreadSafeCounter
+{
+    private readonly object lockObject = new object();
+    private int count = 0;
+
+    public void Increment()
+    {
+        lock (lockObject)
+        {
+            count++;
+        }
+    }
+
+    public int GetCount()
+    {
+        lock (lockObject)
+        {
+            return count;
+        }
+    }
+
+    public void Reset()
+    {
+        lock (lockObject)
+        {
+            count = 0;
+        }
+    }
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 3: Proper null checking and input validation
    new ProblemDefinition("Add StringUtilities example.",
            @"+public class StringUtilities
+{
+    public static bool IsValidEmail(string email)
+    {
+        if (string.IsNullOrWhiteSpace(email))
+        {
+            return false;
+        }
+
+        try
+        {
+            var mailAddress = new System.Net.Mail.MailAddress(email);
+            return mailAddress.Address == email;
+        }
+        catch
+        {
+            return false;
+        }
+    }
+
+    public static string SafeSubstring(string input, int startIndex, int length)
+    {
+        if (string.IsNullOrEmpty(input))
+        {
+            return string.Empty;
+        }
+
+        if (startIndex < 0 || startIndex >= input.Length)
+        {
+            return string.Empty;
+        }
+
+        int maxLength = Math.Min(length, input.Length - startIndex);
+        return input.Substring(startIndex, maxLength);
+    }
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 4: Efficient algorithm with proper error handling
    new ProblemDefinition("Add SearchUtilities.BinarySearch example.",
            @"+public class SearchUtilities
+{
+    public static int BinarySearch<T>(T[] array, T target) where T : IComparable<T>
+    {
+        if (array == null)
+        {
+            throw new ArgumentNullException(nameof(array));
+        }
+
+        if (target == null)
+        {
+            throw new ArgumentNullException(nameof(target));
+        }
+
+        int left = 0;
+        int right = array.Length - 1;
+
+        while (left <= right)
+        {
+            int mid = left + (right - left) / 2; // Prevents overflow
+            int comparison = array[mid].CompareTo(target);
+
+            if (comparison == 0)
+            {
+                return mid;
+            }
+            else if (comparison < 0)
+            {
+                left = mid + 1;
+            }
+            else
+            {
+                right = mid - 1;
+            }
+        }
+
+        return -1;
+    }
+}",
        new StoredReview(ReviewStatus.Approve, [])),

        // Good Example 5: Proper resource management and async patterns
    new ProblemDefinition("Add DatabaseService.GetUsersAsync example.",
            @"+public class DatabaseService
+{
+    private readonly string connectionString;
+
+    public DatabaseService(string connectionString)
+    {
+        this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
+    }
+
+    public async Task<List<User>> GetUsersAsync(int pageSize = 50, int pageNumber = 1)
+    {
+        if (pageSize <= 0)
+        {
+            throw new ArgumentException(""Page size must be greater than zero"", nameof(pageSize));
+        }
+
+        if (pageNumber <= 0)
+        {
+            throw new ArgumentException(""Page number must be greater than zero"", nameof(pageNumber));
+        }
+
+        using (var connection = new SqlConnection(connectionString))
+        {
+            await connection.OpenAsync();
+
+            var query = @""
+                SELECT Id, Name, Email, CreatedDate
+                FROM Users
+                ORDER BY CreatedDate DESC
+                OFFSET @Offset ROWS
+                FETCH NEXT @PageSize ROWS ONLY"";
+
+            using (var command = new SqlCommand(query, connection))
+            {
+                command.Parameters.AddWithValue(""@Offset"", (pageNumber - 1) * pageSize);
+                command.Parameters.AddWithValue(""@PageSize"", pageSize);
+
+                var users = new List<User>();
+                using (var reader = await command.ExecuteReaderAsync())
+                {
+                    while (await reader.ReadAsync())
+                    {
+                        users.Add(new User
+                        {
+                            Id = reader.GetInt32(""Id""),
+                            Name = reader.GetString(""Name""),
+                            Email = reader.GetString(""Email""),
+                            CreatedDate = reader.GetDateTime(""CreatedDate"")
+                        });
+                    }
+                }
+
+                return users;
+            }
+        }
+    }
+}",
        new StoredReview(ReviewStatus.Approve, [])),
        };

        public MediumCSharpCodeReviewProblems()
            : base(_problems, Language.CSharp, "cs_medium", DifficultyLevel.Medium)
        {
        }
}
