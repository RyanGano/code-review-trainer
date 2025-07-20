namespace code_review_trainer_service.CodeReviewProblems;

// Static class containing easy-level code review problems
public static class EasyCodeReviewProblems
{
    private static readonly Random _random = new Random();
    
    private static readonly string[] _problems = new string[]
    {
        // Problem 1: Bad variable names + confusing logic
        @"public class Calculator
{
    public int x(int a, int b)
    {
        int z = a + b;
        if (z > 10)
        {
            z = z - 1;
        }
        else
        {
            z = z + 1;
        }
        return z;
    }
}",
        
        // Problem 2: Unreachable code + bad formatting
        @"public void ProcessOrder(bool isValid)
{
if(isValid==true){
Console.WriteLine(""Processing order..."");
return;
Console.WriteLine(""Order processed successfully"");
}
else{
Console.WriteLine(""Invalid order"");
}
}",

        // Problem 3: Bad variable names + failing logic
        @"public bool CheckAge(int age)
{
    int x = 18;
    if (age > x)
    {
        return false;
    }
    return true;
}",

        // Problem 4: Bad formatting + confusing logic
        @"public string GetDiscount(double price,int customerType){
double d=0.0;
if(customerType==1){d=0.1;}else if(customerType==2){d=0.15;}else{d=0.05;}
return (price*d).ToString();
}",

        // Problem 5: Bad variable names + unreachable code
        @"public void PrintNumbers()
{
    int i = 1;
    while (i <= 5)
    {
        Console.WriteLine(i);
        i++;
        break;
        Console.WriteLine($""New value: {i}"");
    }
}",

        // Problem 6: Failing logic + bad formatting
        @"public int FindMax(int[]arr){
int max=arr[0];
for(int i=1;i<arr.Length;i++){
if(arr[i]>max){
max=arr[i];
}
}
return max;
}",

        // Problem 7: Confusing logic + bad variable names
        @"public bool IsEven(int num)
{
    bool x = false;
    if (num % 2 == 1)
    {
        x = true;
    }
    return x;
}",

        // Problem 8: Bad variable names + bad formatting
        @"public void DoSomething(string s){
if(s!=null&&s.Length>0){
string t=s.ToUpper();
Console.WriteLine(t);
}
}",

        // Problem 9: Unreachable code + confusing logic
        @"public int Calculate(int x, int y)
{
    if (x > 0)
    {
        return x + y;
    }
    if (x <= 0)
    {
        return x - y;
    }
    return 0;
}",

        // Problem 10: Bad variable names + failing logic
        @"public double CalculateArea(double r)
{
    double pi = 3.14;
    double a = pi * r;
    return a;
}",

        // Problem 11: Bad formatting + confusing logic
        @"public bool ValidatePassword(string pwd){
return pwd.Length>=8&&pwd.Length<=20?true:false;
}",

        // Problem 12: Bad variable names + unreachable code
        @"public void Method1()
{
    int a = 5;
    int b = 10;
    if (a < b)
    {
        Console.WriteLine(""a is smaller"");
        return;
    }
    if (b > a)
    {
        Console.WriteLine(""This will never execute"");
    }
}",

        // Problem 13: Failing logic + bad formatting
        @"public string FormatName(string firstName,string lastName){
if(firstName==null)firstName="""";
if(lastName==null)lastName="""";
return firstName+"" ""+lastName;
}",

        // Problem 14: Confusing logic + bad variable names
        @"public int CountItems(List<string> items)
{
    int c = 0;
    foreach (var i in items)
    {
        if (i != null)
        {
            c--;
        }
    }
    return c;
}",

        // Problem 15: Bad variable names + bad formatting
        @"public void UpdateRecord(int id,string name,double salary){
var r=GetRecord(id);
if(r!=null){
r.Name=name;r.Salary=salary;
SaveRecord(r);
}
}",

        // Problem 16: Unreachable code + failing logic
        @"public int Divide(int x, int y)
{
    if (y == 0)
    {
        throw new DivideByZeroException();
        Console.WriteLine(""Division by zero attempted"");
    }
    return x / y;
}",

        // Problem 17: Bad variable names + confusing logic
        @"public bool IsValid(int n)
{
    bool r = true;
    if (n > 0)
    {
        r = false;
    }
    if (n <= 0)
    {
        r = true;
    }
    return !r;
}",

        // Problem 18: Bad formatting + failing logic
        @"public List<int>GetEvenNumbers(int[]numbers){
List<int>result=new List<int>();
for(int i=0;i<=numbers.Length;i++){
result.Add(numbers[i]);
}
return result;
}",

        // Problem 19: Confusing logic + unreachable code
        @"public string GetGrade(int score)
{
    if (score >= 90) return ""A"";
    else if (score >= 80) return ""B"";
    else if (score >= 70) return ""C"";
    else if (score >= 60) return ""D"";
    else return ""F"";
    
    Console.WriteLine(""Grade calculated"");
}",

        // Problem 20: Bad variable names + bad formatting
        @"public double CalcTax(double amt,double rate){
double t=amt*rate/100;
return t>0?t:0;
}",

        // Problem 21: Failing logic + confusing logic
        @"public bool IsPrime(int number)
{
    if (number <= 1) return true;
    for (int i = 2; i < number; i++)
    {
        if (number % i == 0)
        {
            return false;
        }
    }
    return true;
}",

        // Problem 22: Bad variable names + unreachable code
        @"public void ProcessData()
{
    bool f = true;
    while (f)
    {
        Console.WriteLine(""Processing..."");
        f = false;
        break;
        Console.WriteLine(""Done processing"");
    }
}",

        // Problem 23: Bad formatting + confusing logic
        @"public int GetAbsolute(int num){
if(num<0){
return num*-1;
}else{
return num>0?num:1;
}
}",

        // Problem 24: Bad variable names + failing logic
        @"public string ReverseString(string str)
{
    string r = """";
    for (int i = str.Length; i >= 0; i--)
    {
        r += str[i];
    }
    return r;
}",

        // Problem 25: Multiple issues - bad names, formatting, logic
        @"public void BubbleSort(int[]a){
int n=a.Length;
for(int i=0;i<n-1;i++){
for(int j=0;j<n-i-1;j++){
if(a[j]<a[j+1]){
int temp=a[j];
a[j]=a[j+1];a[j+1]=temp;
}
}
}
}",

        // === GOOD CODE EXAMPLES (no issues to fix) ===
        
        // Good Example 1: Well-written calculator with proper naming and formatting
        @"public class Calculator
{
    public double Add(double firstNumber, double secondNumber)
    {
        return firstNumber + secondNumber;
    }
    
    public double Multiply(double firstNumber, double secondNumber)
    {
        return firstNumber * secondNumber;
    }
}",

        // Good Example 2: Clean string validation with proper error handling
        @"public bool IsValidEmail(string email)
{
    if (string.IsNullOrWhiteSpace(email))
    {
        return false;
    }
    
    return email.Contains(""@"") && email.Contains(""."");
}",

        // Good Example 3: Well-formatted loop with descriptive variable names
        @"public void PrintNumbers(int count)
{
    for (int currentNumber = 1; currentNumber <= count; currentNumber++)
    {
        Console.WriteLine($""Number: {currentNumber}"");
    }
}",

        // Good Example 4: Clean data processing method
        @"public List<string> FilterActiveUsers(List<User> users)
{
    var activeUsers = new List<string>();
    
    foreach (var user in users)
    {
        if (user.IsActive)
        {
            activeUsers.Add(user.Name);
        }
    }
    
    return activeUsers;
}",

        // Good Example 5: Proper null checking and formatting
        @"public string FormatFullName(string firstName, string lastName)
{
    if (string.IsNullOrEmpty(firstName))
    {
        firstName = ""Unknown"";
    }
    
    if (string.IsNullOrEmpty(lastName))
    {
        lastName = ""User"";
    }
    
    return $""{firstName} {lastName}"";
}",

        // Good Example 6: Simple and clean conditional logic
        @"public string GetAgeCategory(int age)
{
    if (age < 13)
    {
        return ""Child"";
    }
    else if (age < 20)
    {
        return ""Teenager"";
    }
    else if (age < 65)
    {
        return ""Adult"";
    }
    else
    {
        return ""Senior"";
    }
}",

        // Good Example 7: Well-structured class with proper encapsulation
        @"public class BankAccount
{
    private decimal balance;
    
    public BankAccount(decimal initialBalance)
    {
        balance = initialBalance;
    }
    
    public void Deposit(decimal amount)
    {
        if (amount > 0)
        {
            balance += amount;
        }
    }
    
    public decimal GetBalance()
    {
        return balance;
    }
}"
    };
    
    public static string GetRandomProblem()
    {
        return _problems[_random.Next(_problems.Length)];
    }
}

// Enum for difficulty levels
public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard,
    ExtraHard
}