using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Management;
using System.Linq;

namespace CeVIO_crack
{
    public static class ProductLicense
    {
        public static DateTime DescrambleDateTime(byte[] value)
        {
            return new DateTime(BitConverter.ToInt64(Cipher.Decrypt(value, Authorizer.HDPrimaryVolumeSerialNo), 0));
        }

        public static byte[] ScrambleDateTime(DateTime value)
        {
            return Cipher.Encrypt(BitConverter.GetBytes(value.Ticks), Authorizer.HDPrimaryVolumeSerialNo);
        }
    }

    public static class Authorizer
    {
        public static string HDPrimaryVolumeSerialNo
        {
            get
            {
                return Authorizer.hdPrimaryVolumeSerialNo.Value;
            }
        }

        private static Lazy<string> hdPrimaryVolumeSerialNo = new Lazy<string>(() => new ManagementObjectSearcher("select VolumeSerialNumber from Win32_LogicalDisk where DeviceID=\"C:\"").Get().Cast<ManagementBaseObject>().First<ManagementBaseObject>()["VolumeSerialNumber"].ToString());
    }

    public static class Cipher
    {
        public static byte[] Encrypt(byte[] value, byte[] keyToken)
        {
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(keyToken, Cipher.salt, 1024);
            ICryptoTransform cryptoTransform = Cipher.gizmo.CreateEncryptor(rfc2898DeriveBytes.GetBytes(Cipher.gizmo.KeySize / 8), rfc2898DeriveBytes.GetBytes(Cipher.gizmo.BlockSize / 8));
            return Cipher.Compute(value, cryptoTransform);
        }

        public static byte[] Encrypt(byte[] value, string keyToken)
        {
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(keyToken, Cipher.salt, 1024);
            ICryptoTransform cryptoTransform = Cipher.gizmo.CreateEncryptor(rfc2898DeriveBytes.GetBytes(Cipher.gizmo.KeySize / 8), rfc2898DeriveBytes.GetBytes(Cipher.gizmo.BlockSize / 8));
            return Cipher.Compute(value, cryptoTransform);
        }

        public static byte[] Decrypt(byte[] value, byte[] keyToken)
        {
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(keyToken, Cipher.salt, 1024);
            ICryptoTransform cryptoTransform = Cipher.gizmo.CreateDecryptor(rfc2898DeriveBytes.GetBytes(Cipher.gizmo.KeySize / 8), rfc2898DeriveBytes.GetBytes(Cipher.gizmo.BlockSize / 8));
            return Cipher.Compute(value, cryptoTransform);
        }

        public static byte[] Decrypt(byte[] value, string keyToken)
        {
            Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(keyToken, Cipher.salt, 1024);
            ICryptoTransform cryptoTransform = Cipher.gizmo.CreateDecryptor(rfc2898DeriveBytes.GetBytes(Cipher.gizmo.KeySize / 8), rfc2898DeriveBytes.GetBytes(Cipher.gizmo.BlockSize / 8));
            return Cipher.Compute(value, cryptoTransform);
        }

        private static byte[] Compute(byte[] value, ICryptoTransform transform)
        {
            byte[] array;
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, transform, CryptoStreamMode.Write))
                    {
                        cryptoStream.Write(value, 0, value.Length);
                        cryptoStream.FlushFinalBlock();
                        array = memoryStream.ToArray();
                    }
                }
            }
            catch
            {
                array = null;
            }
            return array;
        }

        private const int iterationCount = 1024;

        private static readonly byte[] salt = Encoding.UTF8.GetBytes("saltって何？塩？");

        private static readonly Rijndael gizmo = new RijndaelManaged
        {
            Mode = CipherMode.ECB,
            Padding = PaddingMode.ISO10126
        };
    }
}

