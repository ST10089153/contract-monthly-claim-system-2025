-- Create the database
CREATE DATABASE ContractMonthlyClaimDB;
GO

USE ContractMonthlyClaimDB;
GO

-- Users table
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL CHECK (Role IN ('Lecturer', 'Coordinator', 'Manager', 'HR')),
    FirstName NVARCHAR(50) NOT NULL,
    LastName NVARCHAR(50) NOT NULL,
    CreatedDate DATETIME DEFAULT GETDATE()
);

-- Claims table
CREATE TABLE Claims (
    ClaimId INT PRIMARY KEY IDENTITY(1,1),
    LecturerId INT NOT NULL,
    HoursWorked DECIMAL(10,2) NOT NULL,
    HourlyRate DECIMAL(10,2) NOT NULL,
    TotalAmount DECIMAL(10,2) NOT NULL,
    SubmissionDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Pending',
    Notes NVARCHAR(500),
    FOREIGN KEY (LecturerId) REFERENCES Users(UserId)
);

-- Documents table
CREATE TABLE Documents (
    DocumentId INT PRIMARY KEY IDENTITY(1,1),
    ClaimId INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    UploadDate DATETIME DEFAULT GETDATE(),
    FileSize BIGINT,
    FOREIGN KEY (ClaimId) REFERENCES Claims(ClaimId) ON DELETE CASCADE
);

-- ApprovalHistory table
CREATE TABLE ApprovalHistory (
    ApprovalId INT PRIMARY KEY IDENTITY(1,1),
    ClaimId INT NOT NULL,
    ApproverId INT NOT NULL,
    Action NVARCHAR(20) NOT NULL,
    Comments NVARCHAR(500),
    ActionDate DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (ClaimId) REFERENCES Claims(ClaimId) ON DELETE CASCADE,
    FOREIGN KEY (ApproverId) REFERENCES Users(UserId)
);

-- Invoices table for HR
CREATE TABLE Invoices (
    InvoiceId INT PRIMARY KEY IDENTITY(1,1),
    ClaimId INT NOT NULL,
    InvoiceNumber NVARCHAR(50) UNIQUE NOT NULL,
    GeneratedDate DATETIME DEFAULT GETDATE(),
    DueDate DATETIME NOT NULL,
    Amount DECIMAL(10,2) NOT NULL,
    Status NVARCHAR(20) DEFAULT 'Generated',
    FOREIGN KEY (ClaimId) REFERENCES Claims(ClaimId)
);

-- Insert sample data
INSERT INTO Users (Username, Password, Email, Role, FirstName, LastName) VALUES
('lecturer1', 'password', 'lecturer1@example.com', 'Lecturer', 'John', 'Doe'),
('lecturer2', 'password', 'lecturer2@example.com', 'Lecturer', 'Jane', 'Smith'),
('coordinator1', 'password', 'coord1@example.com', 'Coordinator', 'Mike', 'Johnson'),
('manager1', 'password', 'manager1@example.com', 'Manager', 'Sarah', 'Williams'),
('hr1', 'password', 'hr1@example.com', 'HR', 'Tom', 'Brown');
GO