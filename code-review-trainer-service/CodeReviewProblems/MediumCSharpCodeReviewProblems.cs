namespace code_review_trainer_service.CodeReviewProblems;

// Static class containing medium-level code review problems
public static class MediumCSharpCodeReviewProblems
{
    private static readonly Random _random = new Random();

    private static readonly ProblemDefinition[] _problems = new ProblemDefinition[]
    {
        // Patch example: original vs patched (Medium C#)
        new ProblemDefinition(@"public bool IsEven(int n)
{
    return n % 2 == 0;
}", @"public bool IsEven(int value)
{
    if (value % 2 == 1)
    {
        return true;
    }
    return false;
}", "Refactor parameter name and conditional logic for clarity"),
        // Problem 1: Compilation error - missing semicolon and spelling error in comment
        new ProblemDefinition(string.Empty, @"public class UserManager
{
    // Retreives user information from the databas
    public User GetUser(int userId)
    {
        var user = Database.FindUser(userId)
        return user;
    }
}", "Add UserManager class scaffold."),

        // Problem 2: Type mismatch compilation error and logic issue
        new ProblemDefinition(string.Empty, @"public class Calculator
{
    public string CalculatePercentage(int value, int total)
    {
        if (total = 0)
        {
            return ""Cannot divide by zero"";
        }
        double percentage = (value / total) * 100;
        return percentage;
    }
}", "Add Calculator.CalculatePercentage implementation."),

        // Problem 3: Spelling error in variable name and off-by-one error
        new ProblemDefinition(string.Empty, @"public List<string> GetSubstring(string text, int maxLenght)
{
    var results = new List<string>();
    for (int i = 0; i <= text.Length - maxLenght; i++)
    {
        results.Add(text.Substring(i, maxLenght));
    }
    return results;
}", "Add substring extraction helper."),

        // Problem 4: Compilation error - undefined variable and logical issue
        new ProblemDefinition(string.Empty, @"public bool ValidateInput(string input)
{
    if (string.IsNullOrEmpty(input))
    {
        return false;
    }
    
    // Check if input contains only letters
    foreach (char c in input)
    {
        if (!char.IsLetter(ch))
        {
            return false;
        }
    }
    return true;
}", "Implement input validation routine."),

        // Problem 5: Subtle null reference potential and spelling error
        new ProblemDefinition(string.Empty, @"public class OrderProcessor
{
    // Proccess orders and calculate totals
    public decimal ProcessOrder(Order order)
    {
        decimal total = 0;
        foreach (var item in order.Items)
        {
            total += item.Price * item.Quantity;
        }
        return total;
    }
}", "Add OrderProcessor.ProcessOrder implementation."),

        // Problem 6: Resource leak - missing using statement
        new ProblemDefinition(string.Empty, @"public string ReadFileContent(string filePath)
{
    var reader = new StreamReader(filePath);
    string content = reader.ReadToEnd();
    return content;
}", "Add ReadFileContent helper."),

        // Problem 7: Compilation error and infinite loop potential
        new ProblemDefinition(string.Empty, @"public void PrintNumbers(int count)
{
    int i = 0
    while (i < count)
    {
        Console.WriteLine(i);
    }
}", "Add PrintNumbers method."),

        // Problem 8: Type mismatch and spelling error in string
        new ProblemDefinition(string.Empty, @"public int GetUserAge(string birthDate)
{
    DateTime birth = DateTime.Parse(birthDate);
    TimeSpan age = DateTime.Now - birth;
    Console.WriteLine(""User is approximatly "" + age.Days / 365 + "" years old"");
    return age.Days / 365;
}", "Add GetUserAge method."),

        // Problem 9: Compilation error - wrong collection type and logic issue
        new ProblemDefinition(string.Empty, @"public Dictionary<string, int> CountWords(string text)
{
    var wordCount = new List<string, int>();
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
}", "Add CountWords utility."),

        // Problem 10: Subtle array bounds issue and spelling error
        new ProblemDefinition(string.Empty, @"public class ArrayProcessor
{
    // Proceses array elements
    public int FindMaxIndex(int[] numbers)
    {
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
}", "Add ArrayProcessor.FindMaxIndex implementation."),

        // Problem 11: Compilation error - missing return type and logic issue
        new ProblemDefinition(string.Empty, @"public class StringUtils
{
    public ReverseString(string input)
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
}", "Add ReverseString method."),

        // Problem 12: Potential division by zero and spelling error
        new ProblemDefinition(string.Empty, @"public class MathOperations
{
    // Calcualtes the average of numbers
    public double CalculateAverage(List<int> numbers)
    {
        int sum = 0;
        foreach (int number in numbers)
        {
            sum += number;
        }
        return sum / numbers.Count;
    }
}", "Add MathOperations.CalculateAverage implementation."),

        // Problem 13: Compilation error and concurrency issue
        new ProblemDefinition(string.Empty, @"public class Counter
{
    private int count = 0;
    
    public void Increment()
    {
        count++
    }
    
    public int GetCount()
    {
        return count;
    }
}", "Add Counter with increment and retrieval."),

        // Problem 14: Subtle logic error and spelling mistake
        new ProblemDefinition(string.Empty, @"public bool IsPalindrome(string text)
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
}", "Add IsPalindrome utility."),

        // Problem 15: Memory leak potential and compilation error
        new ProblemDefinition(string.Empty, @"public class EventPublisher
{
    public event EventHandler DataChanged;
    
    public void Subscribe(object subscriber)
    {
        DataChanged += (sender, e) => {
            Console.WriteLine($""Subscriber {subscriber} notified"");
        }
    }
}", "Add EventPublisher with DataChanged event."),

        // Problem 16: Type casting issue and spelling error
        new ProblemDefinition(string.Empty, @"public class NumberProcessor
{
    // Converts string to integar
    public int ConvertToInteger(object value)
    {
        string strValue = (string)value;
        return int.Parse(strValue);
    }
}", "Add NumberProcessor.ConvertToInteger."),

        // Problem 17: Compilation error and performance issue
        new ProblemDefinition(string.Empty, @"public string ConcatenateStrings(List<string> strings)
{
    string result = """"
    foreach (string str in strings)
    {
        result += str;
    }
    return result;
}", "Add ConcatenateStrings helper."),

        // Problem 18: Subtle boundary condition and spelling error
        new ProblemDefinition(string.Empty, @"public class DateUtils
{
    // Calcuates days between two dates
    public int DaysBetween(DateTime start, DateTime end)
    {
        TimeSpan difference = end - start;
        return (int)difference.TotalDays;
    }
}", "Add DateUtils.DaysBetween utility."),

        // Problem 19: Compilation error and resource management
        new ProblemDefinition(string.Empty, @"public void WriteToFile(string fileName, string content)
{
    FileStream stream = new FileStream(fileName, FileMode.Create);
    StreamWriter writer = new StreamWriter(stream);
    writer.Write(content);
    writer.Flush();
}", "Add WriteToFile helper."),

        // Problem 20: Logic error and spelling mistake in comment
        new ProblemDefinition(string.Empty, @"public class Validator
{
    // Validates email adress format
    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return true;
            
        return email.Contains(""@"") && email.Contains(""."");
    }
}", "Add Validator.IsValidEmail implementation."),

        // Problem 21: Compilation error and algorithm issue
        new ProblemDefinition(string.Empty, @"public int BinarySearch(int[] array, int target)
{
    int left = 0;
    int right = array.Length;
    
    while (left <= right)
    {
        int mid = (left + right) / 2
        if (array[mid] == target)
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
}", "Add BinarySearch implementation."),

        // Problem 22: Exception handling issue and spelling error
        new ProblemDefinition(string.Empty, @"public class DatabaseManager
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
            return null;
        }
    }
}", "Add DatabaseManager.GetData method."),

        // Problem 23: Compilation error and logic flaw
        new ProblemDefinition(string.Empty, @"public List<T> FilterItems<T>(List<T> items, Func<T, bool> predicate)
{
    var result = new List<T>();
    for (int i = 0; i < items.Count; i++)
    {
        if (predicate(items[i])
        {
            result.Add(items[i]);
        }
    }
    return result;
}", "Add FilterItems generic helper."),

        // Problem 24: Subtle threading issue and spelling error
        new ProblemDefinition(string.Empty, @"public class Cache
{
    private Dictionary<string, object> cache = new Dictionary<string, object>();
    
    // Retreives cached value
    public object Get(string key)
    {
        if (cache.ContainsKey(key))
        {
            return cache[key];
        }
        return null;
    }
    
    public void Set(string key, object value)
    {
        cache[key] = value;
    }
}", "Add Cache simple in-memory store."),

        // Problem 25: Compilation error and performance issue
        new ProblemDefinition(string.Empty, @"public bool ContainsDuplicate(List<int> numbers)
{
    for (int i = 0; i < numbers.Count; i++)
    {
        for (int j = i + 1; j < numbers.Count; j++)
        {
            if (numbers[i] == numbers[j])
            {
                return true
            }
        }
    }
    return false;
}", "Add ContainsDuplicate check."),

        // === GOOD CODE EXAMPLES (no issues) ===
        
        // Good Example 1: Proper error handling and resource management
        new ProblemDefinition(string.Empty, @"public class FileProcessor
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
}", "Add FileProcessor.ReadFileAsync example."),

        // Good Example 2: Thread-safe implementation with proper validation
        new ProblemDefinition(string.Empty, @"public class ThreadSafeCounter
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
}", "Add ThreadSafeCounter example."),

        // Good Example 3: Proper null checking and input validation
        new ProblemDefinition(string.Empty, @"public class StringUtilities
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
}", "Add StringUtilities example."),

        // Good Example 4: Efficient algorithm with proper error handling
        new ProblemDefinition(string.Empty, @"public class SearchUtilities
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
}", "Add SearchUtilities.BinarySearch example."),

        // Good Example 5: Proper resource management and async patterns
        new ProblemDefinition(string.Empty, @"public class DatabaseService
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
}", "Add DatabaseService.GetUsersAsync example."),
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
            Id = $"cs_medium_{index + 1:D3}",
            Problem = def.Updated,
            Language = Language.CSharp,
            Original = def.Original ?? string.Empty,
            Purpose = def.Purpose ?? string.Empty
        };
    }

    public static int Count => _problems.Length;
    public static string GetProblemByIndex(int index) => _problems[index].Updated;
}