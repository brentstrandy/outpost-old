using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;

/// <summary>
/// Generic de/serializer used to save and load XML files.
/// Owner: John Fitzgerald
/// </summary>
/// <typeparam name="T"></typeparam>
public static class XMLParser<T>
{
    static bool ShowDebugLogs = false;
    static string[] AvailableClasses = { "enemyspawndata", "towerdata", "enemydata", "leveldata", "notificationdata", "levelprogressdata"};


    #region LIST DE/SERIALIZERS
    /// <summary>
    /// Serialize a List of classes to an XML file.
    /// </summary>
    /// <param name="obj"></param>
    public static void XMLSerializer_Local(List<T> obj, string fileName)
    {
        string currentClasss = typeof(T).ToString().ToLower();

        if (AvailableClasses.Contains(currentClasss))
            SerializeToXML_Local(obj, fileName);
        else
            LogError("Unknown class serialized: " + typeof(T));
    }

    /// <summary>
    /// Deserialize a List of classes from an XML file.
    /// </summary>
    /// <returns></returns>
    public static List<T> XMLDeserializer_Local(string filename)
    {
        string currentClasss = typeof(T).ToString().ToLower();

        if (AvailableClasses.Contains(currentClasss))
            return (List<T>)DeserializeFromXML_Local(filename);
        else
        {
            LogError("Unknown class deserialized: " + typeof(T));
            return default(List<T>);
        }
    }

	public static List<T> XMLDeserializer_Server(string xmlData)
	{
		string currentClasss = typeof(T).ToString().ToLower();

		if (AvailableClasses.Contains(currentClasss))
			return (List<T>)DeserializeFromXML_Server(xmlData);
		else
		{
			LogError("Unknown class deserialized: " + typeof(T));
			return default(List<T>);
		}
	}

    private static void SerializeToXML_Local<U>(List<U> obj, string fileName)
    {
        XmlSerializer serializer = null;
        FileStream stream = null;
        serializer = new XmlSerializer(typeof(List<U>));

        // create directory if it doesn't exist
        if (!File.Exists(fileName))
            Directory.CreateDirectory(Path.GetDirectoryName(fileName));

        stream = new FileStream(fileName, FileMode.Create);

        serializer.Serialize(stream, obj);
        stream.Close();

        Log("Serialized: " + Path.GetFileName(fileName));
    }

    private static List<T> DeserializeFromXML_Server(string xmlData)
    {
        XmlSerializer deserializer = new XmlSerializer(typeof(List<T>));
        TextReader reader = null;
        List<T> classList = new List<T>();

		reader = new StringReader(xmlData);

        classList = (List<T>)deserializer.Deserialize(reader);
        reader.Close();

        Log("Deserialized");

        return classList;
    }

	private static List<T> DeserializeFromXML_Local(string filename)
	{
		XmlSerializer deserializer = new XmlSerializer(typeof(List<T>));
		TextReader reader = null;
		List<T> classList = new List<T>();
		
		reader = new StreamReader(filename);

		classList = (List<T>)deserializer.Deserialize(reader);

		reader.Close();
		
		Log("Deserialized: " + filename);
		
		return classList;
	}
    #endregion

    #region NON-LIST DE/SERIALIZERS
    /// <summary>
    /// Serialize a class to an XML file.
    /// </summary>
    /// <param name="obj"></param>
    public static void XMLSerializer(T obj)
    {
        ////string[] availableClasses = { "spawnactionsmanager" };
        ////string currentClasss = typeof(T).ToString().ToLower();

        ////if (availableClasses.Contains(currentClasss))

        //if (typeof(T) == typeof(SpawnActionsManager))
        //{
        //    SerializeToXML(obj);
        //}
        //else
        //{
        //    LogError("Unknown class serialized: " + typeof(T));
        //}
    }

    /// <summary>
    /// Deserialize a class from an XML file.
    /// </summary>
    /// <returns></returns>
    public static T XMLDeserializer()
    {
        //if (typeof(T) == typeof(SpawnActionsManager_Serializable))
        //{
        //    string name = "";
        //    return (T)DeserializeFromXML(name);
        //}
        //else
        //{
        //    LogError("Unknown class deserialized" + typeof(T));
        return default(T);
        //}
    }

    private static T DeserializeFromXML(string name)
    {
        //XmlSerializer deserializer = new XmlSerializer(typeof(T));
        //TextReader reader = null;

        //if (typeof(T) == typeof(SpawnActionsManager_Serializable))
        //{
        //    name = Application.streamingAssetsPath + "/EnemySpawnInfo.xml";
        //    reader = new StreamReader(name);
        //}

        //var obj = (T)deserializer.Deserialize(reader);
        //reader.Close();

        //Log("Deserialized: " + name);

        //return obj;

        return default(T);
    }
    private static void SerializeToXML<U>(U obj)
    {
        //XmlSerializer serializer = null;
        //FileStream stream = null;
        //string name = "";
        //object obj_serializable = null;

        //if (typeof(U) == typeof(SpawnActionsManager))
        //{
        //    serializer = new XmlSerializer(typeof(SpawnActionsManager_Serializable));
        //    obj_serializable = new SpawnActionsManager_Serializable(obj as SpawnActionsManager);

        //    name = Application.streamingAssetsPath + "/EnemySpawnInfo.xml";
        //    if (!File.Exists(name))
        //        Directory.CreateDirectory(Path.GetDirectoryName(name));

        //    stream = new FileStream(name, FileMode.Create);
        //}

        //serializer.Serialize(stream, obj_serializable);
        //stream.Close();

        //Log("Serialized: " + Path.GetFileName(name));
    }
    #endregion

    #region ENCRYPTION DE/SERIALIZERS

    /// <summary>
    /// Test of XML encryption serializer.
    /// </summary>
    /// <param name="obj"></param>
    public static void XMLSerializer_Encrypt(T obj)
    {
        try
        {
            string original = "Here is some data to encrypt!";

            // Create a new instance of the Aes class. This generates a new key and initialization vector (IV). 
            using (Aes myAes = Aes.Create())
            {
                //if (obj is EnemySpawner)
                //{
                //    // encrypt
                //    byte[] encrypted_bytes = Encrypt_AES(original, myAes.Key, myAes.IV);
                //}

                // encrypt
                byte[] encrypted_bytes = Encrypt_AES(original, myAes.Key, myAes.IV);

                // Decrypt the bytes to a string. 
                string roundtrip = Decrypt_AES(encrypted_bytes, myAes.Key, myAes.IV);

                //Display the original data and the decrypted data.
                if (ShowDebugLogs)
                {
                    Debug.Log("Original: " + original);
                    string encrypted_string = Convert.ToBase64String(encrypted_bytes, 0, encrypted_bytes.Length);
                    Debug.Log("Encrypted String: " + encrypted_string);
                    Debug.Log("Round Trip " + roundtrip);
                }
            }

        }
        catch (Exception e)
        {
            LogError("[Serializer] " + e.Message);
        }
    }

    /// <summary>
    /// Encrypts using AES (used instead of Rijndael).
    /// </summary>
    private static byte[] Encrypt_AES(string plainText, byte[] Key, byte[] IV)
    {
        // Check arguments
        if (plainText == null || plainText.Length <= 0)
            throw new ArgumentNullException("plainText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("IV");

        byte[] encrypted;

        // Create an Aes object with the specified key and IV
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decrytor to perform the stream transform.
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for encryption. 
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        //Write all data to the stream.
                        swEncrypt.Write(plainText);
                    }

                    encrypted = msEncrypt.ToArray();
                }
            }
        }

        // Return the encrypted bytes from the memory stream. 
        return encrypted;
    }

    /// <summary>
    /// Decrypts using AES (used instead of Rijndael).
    /// </summary>
    private static string Decrypt_AES(byte[] cipherText, byte[] Key, byte[] IV)
    {
        // Check arguments. 
        if (cipherText == null || cipherText.Length <= 0)
            throw new ArgumentNullException("cipherText");
        if (Key == null || Key.Length <= 0)
            throw new ArgumentNullException("Key");
        if (IV == null || IV.Length <= 0)
            throw new ArgumentNullException("Key");

        // Declare the string used to hold 
        // the decrypted text. 
        string plaintext = null;

        // Create an Aes object 
        // with the specified key and IV. 
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = Key;
            aesAlg.IV = IV;

            // Create a decrytor to perform the stream transform.
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

            // Create the streams used for decryption. 
            using (MemoryStream msDecrypt = new MemoryStream(cipherText))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        // Read the decrypted bytes from the decrypting stream
                        // and place them in a string.
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }

        return plaintext;
    }

    /// <summary>
    /// Encrypts using Rijndael (use AES instead).
    /// </summary>
    private static string Encrypt(string stream)
    {
        // determine if there's a better key generation (RNG?)
        byte[] key = UTF8Encoding.UTF8.GetBytes("12323453456563214254365859602712");
        // AES-256 key
        byte[] toEncrypt = UTF8Encoding.UTF8.GetBytes(stream);
        RijndaelManaged rij = new RijndaelManaged();
        rij.Key = key;
        // choosing CipherMode:
        // ECB -- should never be used on strings (data must not be related)
        // 
        rij.Mode = CipherMode.ECB;
        rij.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rij.CreateEncryptor();
        byte[] result = cTransform.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
        return Convert.ToBase64String(result, 0, result.Length);
    }

    /// <summary>
    /// Decrypts using Rijndael (use AES instead).
    /// </summary>
    private static string Decrypt(string stream)
    {
        byte[] key = UTF8Encoding.UTF8.GetBytes("12323453456563214254365859602712");
        byte[] toEncrypt = Convert.FromBase64String(stream);
        RijndaelManaged rij = new RijndaelManaged();
        rij.Key = key;
        rij.Mode = CipherMode.ECB;
        rij.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rij.CreateDecryptor();
        byte[] result = cTransform.TransformFinalBlock(toEncrypt, 0, toEncrypt.Length);
        return UTF8Encoding.UTF8.GetString(result);
    }
    #endregion

    #region MessageHandling
    public static void Log(string message)
    {
        if (ShowDebugLogs)
            Debug.Log("[XMLParser] " + message);
    }

    public static void LogError(string message)
    {
    	Debug.LogError("[XMLParser] " + message);
    }
    #endregion
}