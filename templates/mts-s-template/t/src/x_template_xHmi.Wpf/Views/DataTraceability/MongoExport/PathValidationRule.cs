using System;
using System.Globalization;
using System.IO;
using System.Windows.Controls;

namespace x_template_xHmi.Wpf.Data.MongoExport

{
    public class PathValidationRule : ValidationRule
    {
        public Type ValidationType { get; set; }
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string path = Convert.ToString(value);

            if (string.IsNullOrEmpty(path)) return new ValidationResult(false, $"An export path is required!");
            ///Not possible to check if Directoty is ReadOnly on Windows
            return File.Exists(path) || Directory.Exists(path) ? new ValidationResult(true, null) : new ValidationResult(false, $"Directory does not exist!");
        }
    }
}
