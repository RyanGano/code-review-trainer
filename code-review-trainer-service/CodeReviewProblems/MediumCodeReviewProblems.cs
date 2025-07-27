namespace code_review_trainer_service.CodeReviewProblems;

// Static class containing medium-level code review problems
public static class MediumCodeReviewProblems
{
    private static readonly Random _random = new Random();
    
    private static readonly string[] _problems = new string[]
    {
        // Problem 1
        @"public class UserManager
{
    // Retreives user information from the databas
    public User GetUser(int userId)
    {
        var user = Database.FindUser(userId)
        return user;
    }
}",

        // Problem 2
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

        // Problem 3
        @"public List<string> GetSubstring(string text, int maxLenght)
{
    var results = new List<string>();
    for (int i = 0; i <= text.Length - maxLenght; i++)  // Off-by-one potential
    {
        results.Add(text.Substring(i, maxLenght));
    }
    return results;
}",

        // Problem 4
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

        // Problem 5
        @"public class OrderProcessor
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
}",

        // Problem 6
        @"public string ReadFileContent(string filePath)
{
    var reader = new StreamReader(filePath);
    string content = reader.ReadToEnd();
    return content;
}",

        // Problem 7
        @"public void PrintNumbers(int count)
{
    int i = 0
    while (i < count)  // Missing semicolon above
    {
        Console.WriteLine(i);
    }
}",

        // Problem 8
        @"public int GetUserAge(string birthDate)
{
    DateTime birth = DateTime.Parse(birthDate);
    TimeSpan age = DateTime.Now - birth;
    Console.WriteLine(""User is approximatly "" + age.Days / 365 + "" years old"");
    return age.Days / 365;  // Should return int but calculation is imprecise
}",

        // Problem 9
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

        // Problem 10
        @"public class ArrayProcessor
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
}",

        // Problem 11
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

        // Problem 12
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
        return sum / numbers.Count;
    }
}",

        // Problem 13
        @"public class Counter
{
    private int count = 0;
    
    public void Increment()
    {
        count++  // Missing semicolon
    }
    
    public int GetCount()
    {
        return count;
    }
}",

        // Problem 14
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
}",

        // Problem 15
        @"public class EventPublisher
{
    public event EventHandler DataChanged;
    
    public void Subscribe(object subscriber)
    {
        DataChanged += (sender, e) => {
            Console.WriteLine($""Subscriber {subscriber} notified"");
        }  // Missing semicolon
    }
}",

        // Problem 16
        @"public class NumberProcessor
{
    // Converts string to integar
    public int ConvertToInteger(object value)
    {
        string strValue = (string)value;
        return int.Parse(strValue);
    }
}",

        // Problem 17
        @"public string ConcatenateStrings(List<string> strings)
{
    string result = """"
    foreach (string str in strings)  // Missing semicolon above
    {
        result += str;
    }
    return result;
}",

        // Problem 18
        @"public class DateUtils
{
    // Calcuates days between two dates
    public int DaysBetween(DateTime start, DateTime end)
    {
        TimeSpan difference = end - start;
        return (int)difference.TotalDays;
    }
}",

        // Problem 19
        @"public void WriteToFile(string fileName, string content)
{
    FileStream stream = new FileStream(fileName, FileMode.Create);
    StreamWriter writer = new StreamWriter(stream);
    writer.Write(content);
    writer.Flush();
}",

        // Problem 20
        @"public class Validator
{
    // Validates email adress format
    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
            return true;
            
        return email.Contains(""@"") && email.Contains(""."");
    }
}",

        // Problem 21
        @"public int BinarySearch(int[] array, int target)
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
}",

        // Problem 22
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
            return null;
        }
    }
}",

        // Problem 23
        @"public List<T> FilterItems<T>(List<T> items, Func<T, bool> predicate)
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
}",

        // Problem 24
        @"public class Cache
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
}",

        // Problem 25
        @"public bool ContainsDuplicate(List<int> numbers)
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
}",

        // === GOOD CODE EXAMPLES (no issues) ===
        
        // Good Example 1
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

        // Good Example 2
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

        // Good Example 3
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

        // Good Example 4
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

        // Good Example 5
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