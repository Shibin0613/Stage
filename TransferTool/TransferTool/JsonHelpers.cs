using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TransferTool
{
    public static class JsonHelpers

    {
        // voor de encrypt\decrypt

        static readonly string encryptionKey = "IwHt2Hc0WRPOnJpVX7cvNw==";

        public static bool ContainsText(this string text, List<string> toSearch)
        {
            bool result = false;

            foreach (string CheckEachText in toSearch)
            {
                if (text.Contains(CheckEachText))
                {
                    result = true;
                }
                else
                {
                    result = false;
                }
            }
            // afmaken 
            // loop door de array toSeach en kijk of die text in 'text'staat
            return result;
        }

        /// <summary>

        /// Writes the given object instance to a Json file.

        /// <para>Object type must have a parameterless constructor.</para>

        /// <para>Only Public properties and variables will be written to the file. These can be any type though, even other classes.</para>

        /// <para>If there are public properties/variables that you do not want written to the file, decorate them with the [JsonIgnore] attribute.</para>

        /// </summary>

        /// <typeparam name="T">The type of object being written to the file.</typeparam>

        /// <param name="filePath">The file path to write the object instance to.</param>

        /// <param name="objectToWrite">The object instance to write to the file.</param>

        /// <param name="append">If false the file will be overwritten if it already exists. If true the contents will be appended to the file.</param>

        public static void WriteToJsonFile<T>(this T objectToWrite, string filePath, bool append = false, bool encrypt = false) //where T : new()

        {

            TextWriter writer = null;

            try

            {

                if (objectToWrite != null && !string.IsNullOrEmpty(filePath))

                {

                    if (Path.GetDirectoryName(filePath) is string path)

                    {

                        System.IO.Directory.CreateDirectory(path);

                    }

                    var contentsToWriteToFile = JsonSerializer.Serialize(objectToWrite);

                    writer = new StreamWriter(filePath, append);

                    if (encrypt)

                        writer.Write(EncryptString(encryptionKey, contentsToWriteToFile));

                    else

                        writer.Write(contentsToWriteToFile);

                }

            }

            catch { }

            finally

            {

                if (writer != null)

                    writer.Close();

            }

        }



        /// <summary>

        /// Reads an object instance from an Json file.

        /// <para>Object type must have a parameterless constructor.</para>

        /// </summary>

        /// <typeparam name="T">The type of object to read from the file.</typeparam>

        /// <param name="filePath">The file path to read the object instance from.</param>

        /// <returns>Returns a new instance of the object read from the Json file.</returns>

        public static T ReadFromJsonFile<T>(string filePath, bool decrypt = false) where T : new()

        {

            TextReader reader = null;

            try

            {

                reader = new StreamReader(filePath);

                var fileContents = reader.ReadToEnd();

                if (decrypt)

                    return JsonSerializer.Deserialize<T>(DecryptString(encryptionKey, fileContents));

                else

                    return JsonSerializer.Deserialize<T>(fileContents);

            }

            catch { }

            finally

            {

                if (reader != null)

                    reader.Close();

            }

            return default;

        }



        /// <summary>

        /// Reads an object instance from an Json file.

        /// <para>Object type must have a parameterless constructor.</para>

        /// </summary>

        /// <typeparam name="T">The type of object to read from the file.</typeparam>

        /// <param name="filePath">The file path to read the object instance from.</param>

        /// <returns>Returns a new instance of the object read from the Json file.</returns>

        public static T ReadFromJsonFile<T>(this T objectToReadInto, string filePath, bool decrypt = false)

        {

            TextReader reader = null;

            try

            {

                reader = new StreamReader(filePath);

                var fileContents = reader.ReadToEnd();



                if (decrypt)

                    return JsonSerializer.Deserialize<T>(DecryptString(encryptionKey, fileContents));

                else

                    return JsonSerializer.Deserialize<T>(fileContents);

            }

            catch { }

            finally

            {

                if (reader != null)

                    reader.Close();

            }

            return objectToReadInto;

        }

        public static string EncryptString(string key, string plainText)

        {

            try

            {

                byte[] iv = new byte[16];

                byte[] array;



                using (Aes aes = Aes.Create())

                {

                    aes.Key = Encoding.UTF8.GetBytes(key);

                    aes.IV = iv;



                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);



                    using (MemoryStream memoryStream = new MemoryStream())

                    {

                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))

                        {

                            using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))

                            {

                                streamWriter.Write(plainText);

                            }



                            array = memoryStream.ToArray();

                        }

                    }

                }



                return Convert.ToBase64String(array);

            }

            catch { }

            return plainText;

        }



        public static string DecryptString(string key, string cipherText)

        {

            try

            {

                byte[] iv = new byte[16];

                byte[] buffer = Convert.FromBase64String(cipherText);



                using (Aes aes = Aes.Create())

                {

                    aes.Key = Encoding.UTF8.GetBytes(key);

                    aes.IV = iv;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);



                    using (MemoryStream memoryStream = new MemoryStream(buffer))

                    {

                        using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read))

                        {

                            using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))

                            {

                                return streamReader.ReadToEnd();

                            }

                        }

                    }

                }

            }

            catch { }

            return cipherText;

        }

    }
}
