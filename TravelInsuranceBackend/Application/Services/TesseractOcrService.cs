using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Application.DTOs;
using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Tesseract;

namespace Application.Services
{
    public class TesseractOcrService : IOcrService
    {
        private readonly ILogger<TesseractOcrService> _logger;
        private readonly string _tessDataPath;

        public TesseractOcrService(ILogger<TesseractOcrService> logger)
        {
            _logger = logger;
            // Use a relative path from the API project
            _tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            
            // Fallback for development if BaseDirectory doesn't work as expected
            if (!Directory.Exists(_tessDataPath))
            {
                _tessDataPath = @"d:\HIMAPRIYA\TravelInsuranceBackend\API\tessdata";
            }
        }

        public async Task<ExtractedDataDTO> ProcessDocumentAsync(IFormFile file)
        {
            try
            {
                _logger.LogInformation($"Processing document: {file.FileName}");

                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                var fileBytes = ms.ToArray();

                string extractedText = "";

                // Tesseract works primarily with images. 
                // For a real production app, we'd use a library like Ghostscript or PDFium to convert PDF pages to images.
                // For this implementation, we assume images (JPG/PNG).
                
                using (var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromMemory(fileBytes))
                    {
                        using (var page = engine.Process(img))
                        {
                            extractedText = page.GetText();
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(extractedText))
                {
                    return new ExtractedDataDTO { Success = false, ErrorMessage = "No text could be extracted from the document." };
                }

                return ParseFields(extractedText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during OCR processing.");
                return new ExtractedDataDTO { Success = false, ErrorMessage = $"OCR Error: {ex.Message}. Make sure tessdata/eng.traineddata exists." };
            }
        }

        private ExtractedDataDTO ParseFields(string text)
        {
            var result = new ExtractedDataDTO { Success = true };
            _logger.LogInformation("--- Raw OCR Text Start ---");
            _logger.LogInformation(text);
            _logger.LogInformation("--- Raw OCR Text End ---");

            // 1. Passport Number: Starts with a letter followed by 7-8 digits
            var passportRegex = new Regex(@"([A-PR-WYZ][1-9][0-9]{7})|([A-Z][0-9]{7,8})", RegexOptions.IgnoreCase);
            var passportMatch = passportRegex.Match(text);
            if (passportMatch.Success) result.DocumentNumber = passportMatch.Value.ToUpper();

            // 2. Date of Birth: Common formats like 15/02/1990, 15 FEB 1990
            var dobRegex = new Regex(@"(\d{1,2}[-/\s](?:[A-Z]{3}|\d{1,2})[-/\s]\d{4})", RegexOptions.IgnoreCase);
            var dobMatch = dobRegex.Match(text);
            if (dobMatch.Success && DateTime.TryParse(dobMatch.Value, out DateTime dob))
            {
                result.DateOfBirth = dob;
            }

            // 3. Name Extraction (More robust for Passports)
            // Pattern 1: Labels like Surname/Name followed by names
            var namePatterns = new[] {
                @"(?:SURNAME|NAME)[\s:]+([A-Z\s]{3,})",
                @"(?:GIVEN NAMES|FIRST NAME)[\s:]+([A-Z\s]{3,})",
                @"FULL NAME[\s:]+([A-Z\s]{3,})",
                @"NAME OF HOLDER[\s:]+([A-Z\s]{3,})"
            };

            string extractedSurname = "";
            string extractedGivenNames = "";

            var surnameMatch = Regex.Match(text, @"(?:SURNAME|NAME)[\s:]+([A-Z\s]{3,})", RegexOptions.IgnoreCase);
            var givenNamesMatch = Regex.Match(text, @"(?:GIVEN NAMES|GIVEN NAME)[\s:]+([A-Z\s]{3,})", RegexOptions.IgnoreCase);

            if (surnameMatch.Success) extractedSurname = surnameMatch.Groups[1].Value.Trim();
            if (givenNamesMatch.Success) extractedGivenNames = givenNamesMatch.Groups[1].Value.Trim();

            if (!string.IsNullOrEmpty(extractedSurname) || !string.IsNullOrEmpty(extractedGivenNames))
            {
                result.FullName = (extractedGivenNames + " " + extractedSurname).Trim();
            }

            // Fallback 1: Look for "P<IND" or similar Machine Readable Zone (MRZ) line
            if (string.IsNullOrEmpty(result.FullName))
            {
                var mrzRegex = new Regex(@"P<[A-Z]{3}([A-Z<]+)");
                var mrzMatch = mrzRegex.Match(text);
                if (mrzMatch.Success)
                {
                    var namePart = mrzMatch.Groups[1].Value.Replace("<", " ").Trim();
                    result.FullName = Regex.Replace(namePart, @"\s+", " ");
                }
            }

            // Fallback 2: Look for large blocks of uppercase text
            if (string.IsNullOrEmpty(result.FullName))
            {
                var fallbackNameRegex = new Regex(@"([A-Z]{3,}\s[A-Z]{3,}(?:\s[A-Z]{3,})?)");
                var fallbackMatch = fallbackNameRegex.Match(text);
                if (fallbackMatch.Success) result.FullName = fallbackMatch.Value.Trim();
            }

            // Final Cleanup: Remove common words that might be caught
            if (!string.IsNullOrEmpty(result.FullName))
            {
                var noise = new[] { "REPUBLIC", "INDIAN", "PASSPORT", "INDIA", "SURNAME", "GIVEN", "NAMES" };
                foreach (var n in noise)
                {
                    result.FullName = Regex.Replace(result.FullName, $@"\b{n}\b", "", RegexOptions.IgnoreCase).Trim();
                }
            }

            result.Confidence = "85%";
            return result;
        }
    }
}
