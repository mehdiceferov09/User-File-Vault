using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security.Cryptography;


string filePath = "users.txt";
string currentUser;
string rootFolder = "user_files";
string userFolderPath;

Directory.CreateDirectory(rootFolder);

static (string saltB64, string hashB64, int iterations) HashPassword(string password)
{
    int iterations = 100_000;
    byte[] salt = RandomNumberGenerator.GetBytes(16);

    using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
    byte[] hash = pbkdf2.GetBytes(32);

    return (Convert.ToBase64String(salt), Convert.ToBase64String(hash), iterations);
}

static bool VerifyPassword(string password, string saltB64, string hashB64, int iterations)
{
    byte[] salt = Convert.FromBase64String(saltB64);
    byte[] expectedHash = Convert.FromBase64String(hashB64);

    using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA256);
    byte[] actualHash = pbkdf2.GetBytes(32);

    return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
}

static bool IsValid(string text)
{
    bool isValid = Regex.IsMatch(text, @"^[a-zA-Z0-9_]+$");
    return isValid;
}

static string ReadHiddenPassword()
{
    string password = "";

    ConsoleKeyInfo passwordKey;

    do
    {
        passwordKey = Console.ReadKey(true);

        if (password.Length != 0 && passwordKey.Key == ConsoleKey.Backspace)
        {
            password = password.Substring(0, password.Length - 1);
            Console.Write("\b \b");
            continue;
        }

        if (passwordKey.Key != ConsoleKey.Enter)
        {
            password += passwordKey.KeyChar;
            Console.Write("*");
        }

    } while (passwordKey.Key != ConsoleKey.Enter);

    Console.WriteLine();
    return password;
}
static bool MainMenu(string filePath, out string currentUser)
{
    currentUser = "";

    while (true)
    {
        Console.Clear();

        Console.WriteLine("[1] Log in");
        Console.WriteLine("[2] Sign up");
        Console.WriteLine("[3] Exit");

        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1)
        {
            Console.Clear();

            if (!File.Exists(filePath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No users found. Please sign up first.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                continue;
            }

            string[] users = File.ReadAllLines(filePath);

            Console.Write("Username: ");
            string loginUser = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(loginUser) || !IsValid(loginUser))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wrong username.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                continue;
            }

            Console.Write("Password: ");
            string loginPass = ReadHiddenPassword();

            bool found = false;

            foreach (string user in users)
            {
                string[] parts = user.Split(':');
                if (parts.Length != 4) continue;

                string u = parts[0];
                string saltB64 = parts[1];
                string hashB64 = parts[2];

                if (!int.TryParse(parts[3], out int iter)) continue;

                if (u == loginUser && VerifyPassword(loginPass, saltB64, hashB64, iter))
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Logged in.");
                currentUser = loginUser;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();

                return true;
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Wrong username or password");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine();
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }
        }
        else if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2)
        {
            bool usernameExists;
            while (true)
            {
                Console.Clear();

                if (!File.Exists(filePath))
                {
                    File.Create(filePath).Close();
                }

                string[] users = File.ReadAllLines(filePath);

                Console.Write("Username: ");
                string username = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(username) || !IsValid(username))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Username can only contain letters, numbers, underscore and can't be empty.");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                    continue;
                }

                usernameExists = false;

                foreach (string user in users)
                {
                    int index = user.IndexOf(':');
                    if (index == -1) continue;

                    string existingUsername = user.Substring(0, index);

                    if (existingUsername == username)
                    {
                        usernameExists = true;
                        break;
                    }
                }

                if (usernameExists)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("There is already a user with this username.");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Press any key to try again.");
                    Console.ReadKey();
                }
                else
                {
                    Console.Write("Password: ");

                    string password = ReadHiddenPassword();

                    if (string.IsNullOrEmpty(password) || password.Length < 8)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("\nPassword can't be empty or less than 8 symbols.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        continue;
                    }

                    var (saltB64, hashB64, iter) = HashPassword(password);
                    string line = $"{username}:{saltB64}:{hashB64}:{iter}";

                    File.AppendAllText(filePath, line + Environment.NewLine);

                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Sign up is successfull!");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine();
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();

                    break;
                }
            }
        }
        else if (key == ConsoleKey.D3 || key == ConsoleKey.NumPad3)
        {
            return false;
        }
        else
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Please enter a valid key");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
    }
}

bool loggedIn = MainMenu(filePath, out currentUser);

if (loggedIn)
{
    Console.Clear();

    userFolderPath = Path.Combine(rootFolder, currentUser);

    while (true)
    {
        if (!Directory.Exists(userFolderPath))
        {
            Directory.CreateDirectory(userFolderPath);
        }

        Console.Clear();
        Console.WriteLine("[1] Back to menu");
        Console.WriteLine("[2] Write text");
        Console.WriteLine("[3] Open text file");
        Console.WriteLine("[4] Exit");

        var key = Console.ReadKey(true).Key;

        if (key == ConsoleKey.D1 || key == ConsoleKey.NumPad1)
        {
            Console.Clear();

            loggedIn = MainMenu(filePath, out currentUser);
            if (!loggedIn)
                break;
            userFolderPath = Path.Combine(rootFolder, currentUser);
        }
        else if (key == ConsoleKey.D4 || key == ConsoleKey.NumPad4)
        {
            Console.Clear();

            break;
        }

        else if (key == ConsoleKey.D2 || key == ConsoleKey.NumPad2)
        {
            Console.Clear();

            Console.Write("Enter the file name: ");

            string fileName = Console.ReadLine() ?? "";

            if (!IsValid(fileName) || string.IsNullOrWhiteSpace(fileName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("File name can only contain letters, numbers, underscore and can't be empty");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
                continue;
            }

            fileName += ".txt";
            string fullPath = Path.Combine(userFolderPath, fileName);

            if (!File.Exists(fullPath))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(fileName + " created successfully.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
            }

            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("There is already a file with this name.");
                Console.ForegroundColor = ConsoleColor.White;
                Console.ReadKey();
                continue;
            }

            Console.Clear();

            Console.Write(fileName);
            Console.WriteLine();

            List<string> lines = new List<string>();

            while (true)
            {
                string line = Console.ReadLine();
                if (string.IsNullOrEmpty(line))
                    break;

                lines.Add(line);
            }

            File.WriteAllLines(fullPath, lines);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Text file saved successfully.");
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
            Console.Clear();
            continue;
        }

        else if (key == ConsoleKey.D3 || key == ConsoleKey.NumPad3)
        {
            Console.Clear();

            string[] files = Directory.GetFiles(userFolderPath, "*.txt");

            if (files.Length == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("You don't have any files yet.");
                Console.ForegroundColor = ConsoleColor.White;

                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                continue;
            }

            else
            {
                int page = 0;
                int pageSize = 9;
                int totalPages = (files.Length + pageSize - 1) / pageSize;

                while (true)
                {
                    Console.Clear();

                    int start = page * pageSize;
                    int end = Math.Min(start + pageSize, files.Length);

                    for (int i = start; i < end; i++)
                    {
                        Console.WriteLine($"[{i - start + 1}] {Path.GetFileName(files[i])}");
                    }

                    Console.WriteLine("<- Prev | -> Next | 1-9 Open");
                    Console.WriteLine($"[Esc] Exit");

                    var k = Console.ReadKey(true).Key;

                    if (k == ConsoleKey.Escape)
                    {
                        break;
                    }

                    else if (k == ConsoleKey.RightArrow && page < totalPages - 1)
                    {
                        page++;
                        continue;
                    }
                    else if (k == ConsoleKey.LeftArrow && page > 0)
                    {
                        page--;
                        continue;
                    }
                    else if (k == ConsoleKey.LeftArrow && page == 0)
                    {
                        continue;
                    }

                    int choice = -1;

                    if (k >= ConsoleKey.D1 && k <= ConsoleKey.D9)
                        choice = k - ConsoleKey.D0;

                    else if (k >= ConsoleKey.NumPad1 && k <= ConsoleKey.NumPad9)
                        choice = k - ConsoleKey.NumPad0;

                    int countOnPage = end - start;

                    if (choice == -1)
                    {
                        continue;
                    }

                    else if (choice > countOnPage)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("There is no option with that number.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();

                        continue;
                    }

                    else
                    {
                        int selectedIndex = start + (choice - 1);
                        string selectedPath = files[selectedIndex];
                        string text = File.ReadAllText(selectedPath);
                        Console.Clear();
                        Console.WriteLine(Path.GetFileName(files[selectedIndex]));
                        Console.WriteLine(text);
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        continue;
                    }
                }
            }
        }
    }
}