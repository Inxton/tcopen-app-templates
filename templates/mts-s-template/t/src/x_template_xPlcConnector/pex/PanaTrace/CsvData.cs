using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace PanaTrace
{
    public class CsvData
    {
        public const char Separator = ';';

        #region Properties
        private int idStep;

        /// <summary>
        /// Cislene oznacenie kroku sekvencie v ktorom sa meria zbierana hodnota
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > Povinny parameter.
        /// </remarks>
        public int IdStep
        {
            get { return idStep; }
            set { idStep = value; }
        }

        private string description;

        /// <summary>
        /// Strucny popis kroku sekvencie v ktorom sa meria zbierana hodnota (100 znakov)
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > Povinny parameter.
        /// </remarks>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        private CsvDataTypeValue typeValue;

        /// <summary>
        /// Typ hodnoty pre tracabilitu, 1 - Cislo, 2 - String
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > Povinny parameter.
        /// </remarks>
        public CsvDataTypeValue TypeValue
        {
            get { return typeValue; }
            set { typeValue = value; }
        }

        private double minTol;

        /// <summary>
        /// Minimalna hodnota
        /// </summary>
        /// <remarks>
        /// Volitelny parameter
        /// </remarks>
        public double MinTol
        {
            get { return minTol; }
            set { minTol = value; }
        }

        private double maxTol;

        /// <summary>
        /// Maximalna hodnota
        /// </summary>
        /// <remarks>
        /// Volitelny parameter
        /// </remarks>
        public double MaxTol
        {
            get { return maxTol; }
            set { maxTol = value; }
        }

        private string nominal;

        /// <summary>
        /// Iba ked je vysledna hodnota v stringu (Typ hodnoty je 2) - "OK"
        /// </summary>
        /// <remarks>
        /// Volitelny parameter
        /// </remarks>
        public string Nominal
        {
            get { return nominal; }
            set { nominal = value; }
        }

        private string minElement;

        /// <summary>
        /// Minimalna hodnota (10 znakov) - jednotka napr. mm
        /// </summary>
        /// <remarks>
        /// Volitelny parameter
        /// </remarks>
        public string MinElement
        {
            get { return minElement; }
            set { minElement = value; }
        }

        private string maxElement;

        /// <summary>
        /// Maximalna hodnota (10 znakov) - jednotka napr. mm
        /// </summary>
        /// <remarks>
        /// Volitelny parameter
        /// </remarks>
        public string MaxElement
        {
            get { return maxElement; }
            set { maxElement = value; }
        }

        private double numericValue;

        /// <summary>
        /// Merana hodnota pre typ 1
        /// </summary>
        /// <remarks>
        /// Volitelny parameter
        /// </remarks>
        public double NumericValue
        {
            get { return numericValue; }
            set { this.numericValue = value; }
        }

        private string stringValue;

        /// <summary>
        /// Merana hodnota pre typ 2
        /// </summary>
        /// <remarks>
        /// Volitelny parameter
        /// </remarks>
        public string StringValue
        {
            get { return stringValue; }
            set { this.stringValue = value; }
        }

        private string valueElement;

        /// <summary>
        /// Merana hodnota (10 znakov) - jednotka napr. mm
        /// </summary>
        /// <remarks>
        /// Volitelny parameter
        /// </remarks>
        public string ValueElement
        {
            get { return valueElement; }
            set { valueElement = value; }
        }

        private string result;
        
        /// <summary>
        /// Vysledok kontroly - PASS, FAIL, SKIP
        /// </summary>
        /// <remarks>
        /// > [!IMPORTANT]
        /// > Povinny parameter.
        /// </remarks>
        public string Result
        {
            get { return result; }
            set { result = value; }
        }
        #endregion

        public override string ToString()
        {
            StringBuilder outputString = new StringBuilder();

            switch ((CsvDataTypeValue)typeValue)
            {
                case CsvDataTypeValue.numberType:
                    outputString.Append(IdStep);
                    outputString.Append(Separator);

                    outputString.Append(Description);
                    outputString.Append(Separator);

                    outputString.Append((byte)typeValue);
                    outputString.Append(Separator);

                    outputString.Append(MinTol.ToString("F2", CultureInfo.InvariantCulture));
                    outputString.Append(Separator);

                    outputString.Append(MaxTol.ToString("F2", CultureInfo.InvariantCulture));
                    outputString.Append(Separator);

                    outputString.Append(string.Empty);
                    outputString.Append(Separator);

                    outputString.Append(MinElement);
                    outputString.Append(Separator);

                    outputString.Append(MaxElement);
                    outputString.Append(Separator);

                    outputString.Append(NumericValue.ToString("F2", CultureInfo.InvariantCulture));
                    outputString.Append(Separator);

                    outputString.Append(ValueElement);
                    outputString.Append(Separator);

                    outputString.Append(Result);

                    return outputString.ToString();

                case CsvDataTypeValue.stringType:
                    outputString.Append(IdStep);
                    outputString.Append(Separator);

                    outputString.Append(Description);
                    outputString.Append(Separator);

                    outputString.Append((byte)typeValue);
                    outputString.Append(Separator);

                    outputString.Append(string.Empty);
                    outputString.Append(Separator);

                    outputString.Append(string.Empty);
                    outputString.Append(Separator);

                    outputString.Append(Nominal);
                    outputString.Append(Separator);

                    outputString.Append(string.Empty);
                    outputString.Append(Separator);

                    outputString.Append(string.Empty);
                    outputString.Append(Separator);

                    outputString.Append(StringValue);
                    outputString.Append(Separator);

                    outputString.Append(string.Empty);
                    outputString.Append(Separator);

                    outputString.Append(Result);

                    return outputString.ToString();

                default:
                    break;
            }

            return string.Empty;
        }

    }

    public enum CsvDataTypeValue
    {
        numberType = 1,
        stringType = 2
    }
}
