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
 }"),
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
+}"),

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
 }"),

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
 }"),

        // Problem 4: Compilation error - undefined variable and logical issue - wrong variable name in loop
    new ProblemDefinition("Enhance input validation with additional checks",
            @" public bool ValidateInput(string input)
 {
-    if (string.IsNullOrEmpty(input))
-    {
-        return false;
-    }
-    
-    // Check if input contains only letters
-    foreach (char c in input)
-    {
-        if (!char.IsLetter(c))
-        {
-            return false;
-        }
-    }
-    return true;
+    if (string.IsNullOrEmpty(input))
+    {
+        return false;
+    }
+    
+    // Check if input contains only letters
+    foreach (char c in input)
+    {
+        if (!char.IsLetter(ch))
+        {
+            return false;
+        }
+    }
+    return true;
 }"),

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
 }"),

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
 }"),

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
 }"),

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
 }"),

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
 }"),

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
 }"),

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
 }"),

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
 }"),

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
 }"),

        // Problem 14: Subtle logic error and spelling mistake
    new ProblemDefinition("Add IsPalindrome utility.",
            @"public bool IsPalindrome(string text)
{
    // Check if text is a palindrom
    text = text.ToLower().Replace("" "", """");
    
    for (int i = 0; i < text.Length / 2; i++)
    {
        if (text[i] != text[text.Length - i])
        {
            return false;
        }
    }
    return true;
}"),

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
+}"),

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
+}"),

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
+}"),

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
+}"),

        // Problem 19: Compilation error and resource management
    new ProblemDefinition("Add WriteToFile helper.",
            @"+public void WriteToFile(string fileName, string content)
+{
+    FileStream stream = new FileStream(fileName, FileMode.Create);
+    StreamWriter writer = new StreamWriter(stream);
+    writer.Write(content);
+    writer.Flush();
+}"),

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
+}"),

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
+}"),

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
+}"),

        // Problem 23: Compilation error and logic flaw
    new ProblemDefinition("Add FilterItems generic helper.",
            @"+public List<T> FilterItems<T>(List<T> items, Func<T, bool> predicate)
+{
+    var result = new List<T>();
+    for (int i = 0; i < items.Count; i++)
+    {
+        if (predicate(items[i])
+        {
+            result.Add(items[i]);
+        }
+    }
+    return result;
+}"),

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
+}"),

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
+}"),

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
+}"),

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
+}"),

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
+}"),

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
+}"),

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
+}"),
        };

        public MediumCSharpCodeReviewProblems()
            : base(_problems, Language.CSharp, "cs_medium", DifficultyLevel.Medium)
        {
        }
}