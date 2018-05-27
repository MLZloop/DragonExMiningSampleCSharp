using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonExMiningSampleCSharp.MultiLanguage
{
    class ConfigFileReader
    {
        /// <summary>
        /// File name
        /// </summary>
        private string fileName;

        /// <summary>
        /// Error message
        /// </summary>
        private string errorMessage;

        /// <summary>
        /// Config file reader
        /// </summary>
        /// <param name="filePath">File path</param>
        public ConfigFileReader(string filePath)
        {
            this.fileName = filePath;
        }

        /// <summary>
        /// Read Config
        /// </summary>
        /// <returns>Config Dictionary</returns>
        public Dictionary<string, Dictionary<string, string>> ReadConfig()
        {
            Dictionary<string, Dictionary<string, string>> result = null;

            if (!string.IsNullOrEmpty(fileName))
            {
                if (File.Exists(fileName))
                {
                    try
                    {
                        using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                        {
                            using (var fileReader = new StreamReader(fileStream, Encoding.UTF8))
                            {
                                result = new Dictionary<string, Dictionary<string, string>>();
                                string line = null;
                                string key = null;
                                while (!fileReader.EndOfStream)
                                {
                                    line = fileReader.ReadLine();
                                    // If not empty and not comment
                                    if (!string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                                    {
                                        line = line.Trim();
                                        if (line.StartsWith("[") && line.EndsWith("]"))
                                        {
                                            key = line.Substring(1, line.Length - 2);
                                        }
                                        else if (!string.IsNullOrEmpty(key) && line.IndexOf("=") > 0)
                                        {
                                            int equalIndex = line.IndexOf("=");
                                            var subKey = line.Substring(0, equalIndex);
                                            var value = line.Substring(equalIndex + 1);
                                            if (!result.ContainsKey(key))
                                            {
                                                result.Add(key, new Dictionary<string, string>());
                                            }
                                            if (!result[key].ContainsKey(subKey))
                                            {
                                                result[key].Add(subKey, value);
                                            }
                                            else
                                            {
                                                result[key][subKey] = value;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        errorMessage = "Error happened when reading file.Detail:" + ex.ToString();
                    }
                }
                else
                {
                    errorMessage = "File Not Exists.";
                }
            }
            else
            {
                errorMessage = "File Path is empty.";
            }

            return result;
        }
    }
}
