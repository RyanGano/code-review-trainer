namespace code_review_trainer_service.CodeReviewProblems;

// Static class containing medium-level code review problems
public static class MediumCodeReviewProblems
{
    private static readonly Random _random = new Random();
    
    private static readonly string[] _problems = new string[]
    {
        // Problem 1: Compilation error - missing semicolon and spelling error in comment
        @"public class UserManager
{
    // Retreives user information from the databas
    public User GetUser(int userId)
    {
        var user = Database.FindUser(userId)
        return user;
    }
}",

        // Problem 2: Type mismatch compilation error and logic issue
        @"public class Calculator
{
    public string CalculatePercentage(int value, int total)
    {
        if (total = 0)  // Assignment instead of comparison
        {
            return ""Cannot divide by zero"";
        }
        double percentage = (value / total) * 100;
        return percentage;  // Return type mismatch
    }
}",

        // Problem 3: Spelling error in variable name and off-by-one error
        @"public List<string> GetSubstring(string text, int maxLenght)
{
    var results = new List<string>();
    for (int i = 0; i <= text.Length - maxLenght; i++)  // Off-by-one potential
    {
        results.Add(text.Substring(i, maxLenght));
    }
    return results;
}",

        // Problem 4: Compilation error - undefined variable and logical issue
        @"public bool ValidateInput(string input)
{
    if (string.IsNullOrEmpty(input))
    {
        return false;
    }
    
    // Check if input contains only letters
    foreach (char c in input)
    {
        if (!char.IsLetter(ch))  // 'ch' is undefined
        {
            return false;
        }
    }
    return true;
}",

        // Problem 5: Subtle null reference potential and spelling error
        @"public class OrderProcessor
{
    // Proccess orders and calculate totals
    public decimal ProcessOrder(Order order)
    {
        decimal total = 0;
        foreach (var item in order.Items)  // order.Items could be null
        {
            total += item.Price * item.Quantity;
        }
        return total;
    }
}",

        // Problem 6: Resource leak - missing using statement
        @"public string ReadFileContent(string filePath)
{
    var reader = new StreamReader(filePath);
    string content = reader.ReadToEnd();
    // Missing reader.Dispose() or using statement
    return content;
}",

        // Problem 7: Compilation error and infinite loop potential
        @"public void PrintNumbers(int count)
{
    int i = 0
    while (i < count)  // Missing semicolon above
    {
        Console.WriteLine(i);
        // Missing i++ - infinite loop
    }
}",

        // Problem 8: Type mismatch and spelling error in string
        @"public int GetUserAge(string birthDate)
{
    DateTime birth = DateTime.Parse(birthDate);
    TimeSpan age = DateTime.Now - birth;
    Console.WriteLine(""User is approximatly "" + age.Days / 365 + "" years old"");
    return age.Days / 365;  // Should return int but calculation is imprecise
}",

        // Problem 9: Compilation error - wrong collection type and logic issue
        @"public Dictionary<string, int> CountWords(string text)
{
    var wordCount = new List<string, int>();  // Wrong collection type
    string[] words = text.Split(' ');
    
    foreach (string word in words)
    {
        if (wordCount.ContainsKey(word))
        {
            wordCount[word]++;
        }
        else
        {
            wordCount[word] = 1;
        }
    }
    return wordCount;
}",

        // Problem 10: Subtle array bounds issue and spelling error
        @"public class ArrayProcessor
{
    // Proceses array elements
    public int FindMaxIndex(int[] numbers)
    {
        int maxIndex = 0;
        for (int i = 1; i < numbers.Length; i++)  // Missing null check
        {
            if (numbers[i] > numbers[maxIndex])
            {
                maxIndex = i;
            }
        }
        return maxIndex;  // Could throw NullReferenceException
    }
}",

        // Problem 11: Compilation error - missing return type and logic issue
        @"public class StringUtils
{
    public ReverseString(string input)  // Missing return type
    {
        if (input == null) return null;
        
        char[] chars = input.ToCharArray();
        for (int i = 0; i < chars.Length / 2; i++)
        {
            char temp = chars[i];
            chars[i] = chars[chars.Length - 1 - i];
            chars[chars.Length - 1 - i] = temp;
        }
        return new string(chars);
    }
}",

        // Problem 12: Potential division by zero and spelling error
        @"public class MathOperations
{
    // Calcualtes the average of numbers
    public double CalculateAverage(List<int> numbers)
    {
        int sum = 0;
        foreach (int number in numbers)
        {
            sum += number;
        }
        return sum / numbers.Count;  // Division by zero if empty list
    }
}",

        // Problem 13: Compilation error and concurrency issue
        @"public class Counter
{
    private int count = 0;
    
    public void Increment()
    {
        count++  // Missing semicolon
    }
    
    // Not thread-safe - race condition possible
    public int GetCount()
    {
        return count;
    }
}",

        // Problem 14: Subtle logic error and spelling mistake
        @"public bool IsPalindrome(string text)
{
    // Check if text is a palindrom
    text = text.ToLower().Replace("" "", """");
    
    for (int i = 0; i < text.Length / 2; i++)
    {
        if (text[i] != text[text.Length - i])  // Missing -1, off-by-one error
        {
            return false;
        }
    }
    return true;
}",

        // Problem 15: Memory leak potential and compilation error
        @"public class EventPublisher
{
    public event EventHandler DataChanged;
    
    public void Subscribe(object subscriber)
    {
        DataChanged += (sender, e) => {
            // Anonymous method captures subscriber - potential memory leak
            Console.WriteLine($""Subscriber {subscriber} notified"");
        }  // Missing semicolon
    }
}",

        // Problem 16: Type casting issue and spelling error
        @"public class NumberProcessor
{
    // Converts string to integar
    public int ConvertToInteger(object value)
    {
        string strValue = (string)value;  // Unsafe cast
        return int.Parse(strValue);  // Could throw exception
    }
}",

        // Problem 17: Compilation error and performance issue
        @"public string ConcatenateStrings(List<string> strings)
{
    string result = """"
    foreach (string str in strings)  // Missing semicolon above
    {
        result += str;  // Performance issue - should use StringBuilder
    }
    return result;
}",

        // Problem 18: Subtle boundary condition and spelling error
        @"public class DateUtils
{
    // Calcuates days between two dates
    public int DaysBetween(DateTime start, DateTime end)
    {
        TimeSpan difference = end - start;
        return (int)difference.TotalDays;  // Could be negative, doesn't handle time zones
    }
}",

        // Problem 19: Compilation error and resource management
        @"public void WriteToFile(string fileName, string content)
{
    FileStream stream = new FileStream(fileName, FileMode.Create);
    StreamWriter writer = new StreamWriter(stream);
    writer.Write(content);
    writer.Flush();
    // Missing proper disposal - resource leak
    // Also missing error handling
}",

        // Problem 20: Logic error and spelling mistake in comment
        @"public class Validator
{
    // Validates email adress format
    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return true;  // Should return false
            
        return email.Contains(""@"") && email.Contains(""."");  // Oversimplified validation
    }
}",

        // Problem 21: Compilation error and algorithm issue
        @"public int BinarySearch(int[] array, int target)
{
    int left = 0;
    int right = array.Length;  // Should be array.Length - 1
    
    while (left <= right)
    {
        int mid = (left + right) / 2
        if (array[mid] == target)  // Missing semicolon above
        {
            return mid;
        }
        else if (array[mid] < target)
        {
            left = mid + 1;
        }
        else
        {
            right = mid - 1;
        }
    }
    return -1;
}",

        // Problem 22: Exception handling issue and spelling error
        @"public class DatabaseManager
{
    // Retreive data from databse
    public DataTable GetData(string query)
    {
        try
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(query, connection);
                var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return null;  // Returning null instead of proper error handling
        }
    }
}",

        // Problem 23: Compilation error and logic flaw
        @"public List<T> FilterItems<T>(List<T> items, Func<T, bool> predicate)
{
    var result = new List<T>();
    for (int i = 0; i < items.Count; i++)
    {
        if (predicate(items[i])
        {  // Missing closing parenthesis
            result.Add(items[i]);
        }
    }
    return result;  // Should check for null items list
}",

        // Problem 24: Subtle threading issue and spelling error
        @"public class Cache
{
    private Dictionary<string, object> cache = new Dictionary<string, object>();
    
    // Retreives cached value
    public object Get(string key)
    {
        if (cache.ContainsKey(key))  // Race condition in multi-threaded environment
        {
            return cache[key];  // Key might be removed between check and access
        }
        return null;
    }
    
    public void Set(string key, object value)
    {
        cache[key] = value;  // Not thread-safe
    }
}",

        // Problem 25: Compilation error and performance issue
        @"public bool ContainsDuplicate(List<int> numbers)
{
    for (int i = 0; i < numbers.Count; i++)
    {
        for (int j = i + 1; j < numbers.Count; j++)
        {
            if (numbers[i] == numbers[j])
            {
                return true
            }  // Missing semicolon
        }
    }
    return false;  // O(n²) complexity - could use HashSet for better performance
}",

        // === GOOD CODE EXAMPLES (no issues) ===
        
        // Good Example 1: Proper error handling and resource management
        @"public class FileProcessor
{
    public async Task<string> ReadFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException(""File path cannot be null or empty"", nameof(filePath));
        }
        
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($""File not found: {filePath}"");
        }
        
        using (var reader = new StreamReader(filePath))
        {
            return await reader.ReadToEndAsync();
        }
    }
}",

        // Good Example 2: Thread-safe implementation with proper validation
        @"public class ThreadSafeCounter
{
    private readonly object lockObject = new object();
    private int count = 0;
    
    public void Increment()
    {
        lock (lockObject)
        {
            count++;
        }
    }
    
    public int GetCount()
    {
        lock (lockObject)
        {
            return count;
        }
    }
    
    public void Reset()
    {
        lock (lockObject)
        {
            count = 0;
        }
    }
}",

        // Good Example 3: Proper null checking and input validation
        @"public class StringUtilities
{
    public static bool IsValidEmail(string email)
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
        catch
        {
            return false;
        }
    }
    
    public static string SafeSubstring(string input, int startIndex, int length)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }
        
        if (startIndex < 0 || startIndex >= input.Length)
        {
            return string.Empty;
        }
        
        int maxLength = Math.Min(length, input.Length - startIndex);
        return input.Substring(startIndex, maxLength);
    }
}",

        // Good Example 4: Efficient algorithm with proper error handling
        @"public class SearchUtilities
{
    public static int BinarySearch<T>(T[] array, T target) where T : IComparable<T>
    {
        if (array == null)
        {
            throw new ArgumentNullException(nameof(array));
        }
        
        if (target == null)
        {
            throw new ArgumentNullException(nameof(target));
        }
        
        int left = 0;
        int right = array.Length - 1;
        
        while (left <= right)
        {
            int mid = left + (right - left) / 2; // Prevents overflow
            int comparison = array[mid].CompareTo(target);
            
            if (comparison == 0)
            {
                return mid;
            }
            else if (comparison < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
        
        return -1;
    }
}",

        // Good Example 5: Proper resource management and async patterns
        @"public class DatabaseService
{
    private readonly string connectionString;
    
    public DatabaseService(string connectionString)
    {
        this.connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }
    
    public async Task<List<User>> GetUsersAsync(int pageSize = 50, int pageNumber = 1)
    {
        if (pageSize <= 0)
        {
            throw new ArgumentException(""Page size must be greater than zero"", nameof(pageSize));
        }
        
        if (pageNumber <= 0)
        {
            throw new ArgumentException(""Page number must be greater than zero"", nameof(pageNumber));
        }
        
        using (var connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();
            
            var query = @""
                SELECT Id, Name, Email, CreatedDate 
                FROM Users 
                ORDER BY CreatedDate DESC
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY"";
            
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue(""@Offset"", (pageNumber - 1) * pageSize);
                command.Parameters.AddWithValue(""@PageSize"", pageSize);
                
                var users = new List<User>();
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        users.Add(new User
                        {
                            Id = reader.GetInt32(""Id""),
                            Name = reader.GetString(""Name""),
                            Email = reader.GetString(""Email""),
                            CreatedDate = reader.GetDateTime(""CreatedDate"")
                        });
                    }
                }
                
                return users;
            }
        }
    }
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
            Id = $"medium_{index + 1:D3}",
            Problem = _problems[index]
        };
    }
}