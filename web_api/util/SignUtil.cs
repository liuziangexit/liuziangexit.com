using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

/** 
 * @author  liuziang
 * @contact liuziang@liuziangexit.com
 * @date    2/11/2019
 * 
 * SignUtil
 * 
 */

namespace WebApi.Util
{
    static class SignUtil
    {
        static public byte[] Sign(byte[] data, X509Certificate2 certificateWithPrivateKey)
        {
            RSA csp = (RSA)certificateWithPrivateKey.PrivateKey;
            SHA1Managed sha1 = new SHA1Managed();
            byte[] hash = sha1.ComputeHash(data);
            return csp.SignHash(hash, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        }

        static public bool Verify(byte[] data, byte[] signature, X509Certificate2 certificate)
        {
            RSA csp = (RSA)certificate.PublicKey.Key;
            SHA1Managed sha1 = new SHA1Managed();
            byte[] hash = sha1.ComputeHash(data);
            return csp.VerifyHash(hash, signature, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
        }

        static public string Sign(string data, X509Certificate2 certificateWithPrivateKey)
        {
            byte[] dataAsBytes = Encoding.ASCII.GetBytes(data);
            byte[] signAsBytes = Sign(dataAsBytes, certificateWithPrivateKey);
            StringBuilder sb = new StringBuilder(signAsBytes.Length * 2);
            foreach (var b in signAsBytes)
                sb.AppendFormat("{0:x2}", b);//as hex
            return sb.ToString();
        }

        static public bool Verify(string data, string signature, X509Certificate2 certificate)
        {
            if (signature.Length % 2 != 0)
                throw new ArgumentException("signature invalid");
            byte[] signatureAsBytes = new byte[signature.Length / 2];
            for (int i = 0; i < signature.Length / 2; i++)
                signatureAsBytes[i] = Convert.ToByte(signature.Substring(i * 2, 2), 16);
            return Verify(Encoding.ASCII.GetBytes(data), signatureAsBytes, certificate);
        }
    }
}
