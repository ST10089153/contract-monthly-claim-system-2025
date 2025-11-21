using System;
using System.IO;
using Microsoft.AspNetCore.Http;

namespace CMCS.Services
{
    public class ValidationService
    {
        private readonly string[] _allowedExtensions = { ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".png", ".jpg", ".jpeg" };
        private readonly long _maxFileSize = 10 * 1024 * 1024; // 10MB

        public (bool IsValid, string Message) ValidateFile(IFormFile file)
        {
            if (file == null)
                return (true, "No file provided"); // File is optional

            // Check file size
            if (file.Length > _maxFileSize)
                return (false, "File size exceeds 10MB limit");

            // Check file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!Array.Exists(_allowedExtensions, ext => ext == extension))
                return (false, "Invalid file type. Allowed types: PDF, Word, Excel, Images");

            return (true, "Valid file");
        }

        public bool ValidateHours(decimal hours)
        {
            return hours > 0 && hours <= 200;
        }

        public bool ValidateRate(decimal rate)
        {
            return rate > 0 && rate <= 10000;
        }
    }
}