using System;
using System.Security.Cryptography;
using System.Text;

namespace ExtraQL
{
  internal class Cypher
  {
    private static readonly byte[] entropy = Encoding.UTF8.GetBytes("Some salt to spice up the password");

    public static string EncryptString(string input)
    {
      byte[] encryptedData = ProtectedData.Protect(
        Encoding.UTF8.GetBytes(input),
        entropy,
        DataProtectionScope.CurrentUser);
      return Convert.ToBase64String(encryptedData);
    }

    public static string DecryptString(string encryptedData)
    {
      try
      {
        byte[] decryptedData = ProtectedData.Unprotect(
          Convert.FromBase64String(encryptedData),
          entropy,
          DataProtectionScope.CurrentUser);
        return Encoding.UTF8.GetString(decryptedData);
      }
      catch
      {
        return null;
      }
    }
  }
}