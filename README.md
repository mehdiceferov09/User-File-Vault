# SecureVault

SecureVault is a C# console application that provides secure user authentication and private file management within a command-line environment.

The application simulates a simple secure storage system where each user has protected access to their own personal files.

# Features

User Registration and Login
Passwords are secured using salted hashing with PBKDF2.

Private User Storage
Each user has an individual folder for storing personal files.

File Management
Users can create and read text files through a console-based interface.

Input Validation
Username and file name validation helps prevent invalid or unsafe entries.

# Technologies Used

C#

.NET Console Application

File I/O

Secure Password Hashing (PBKDF2)

# Project Purpose

This project was developed to practice authentication logic, file handling, and secure password storage in a console-based application. It focuses on implementing fundamental security principles and structured file management without using external databases or frameworks.

# How It Works

Users create an account or log in.

Passwords are hashed and stored securely.

After authentication, users can manage text files inside their personal directory.

All operations are performed through a console interface.

# Learning Outcomes

Through this project, the following concepts were practiced:

Implementing secure password storage

Managing user-specific directories

Working with file input and output in C#

Designing a simple authentication system

Validating user input in console applications
