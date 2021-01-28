using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace TelegramBotReminder
{
    static class Functions
    {

        public static bool SaveToFile(string filePath, string textToSave)
        {
            try
            {
                TextWriter writer;
                using (writer = new StreamWriter(filePath, append: false))
                {
                    writer.WriteLine(textToSave);
                }
                return true;
            }
            catch (Exception e)
            {
                LogException(e);
                return false;
            }
        }
        public static string ReadFromFile(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                {
                    return "";
                }
                return System.IO.File.ReadAllText("MyTextFile.txt");
            }
            catch (Exception e)
            {
                LogException(e);
                return "";
            }
        }
        public static void LogEvent(string LcTexto)
        {
            Console.WriteLine($"{System.DateTime.Now} - {LcTexto}");
        }
        public static void LogException(Exception e)
        {
            Console.WriteLine($"{e.GetType().Name}: {e.Message}");
        }

        public static string RemoveAccents(this string text)
        {
            StringBuilder sbReturn = new StringBuilder();
            var arrayText = text.Normalize(NormalizationForm.FormD).ToCharArray();
            foreach (char letter in arrayText)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(letter) != UnicodeCategory.NonSpacingMark)
                    sbReturn.Append(letter);
            }
            return sbReturn.ToString();
        }
        public static string ConfigurationRead(string key)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile($@"{AppDomain.CurrentDomain.BaseDirectory}appsettings.json", true, true).Build();
            var keyValue = configuration[key];
            return keyValue;
        }
    }
}
