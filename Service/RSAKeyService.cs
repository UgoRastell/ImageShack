using System.Security.Cryptography;

namespace Services
{
    public class RSAKeyService
    {
        private string _keyFilePath;
        private RSA _rsa;

        public RSAKeyService(string keyFilePath)
        {
            _keyFilePath = keyFilePath;
            _rsa = RSA.Create();
        }

        public void EnsureKey()
        {
            if (!File.Exists(_keyFilePath))
            {
                SaveKey();
            }
            else
            {
                LoadKey();
            }
        }

        private void SaveKey()
        {
            try
            {
                var key = _rsa.ExportRSAPrivateKey();
                File.WriteAllBytes(_keyFilePath, key);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not export or save RSA private key.", ex);
            }
        }

        private void LoadKey()
        {
            try
            {
                var key = File.ReadAllBytes(_keyFilePath);
                _rsa.ImportRSAPrivateKey(key, out _);
            }
            catch (Exception ex)
            {
                throw new Exception("Could not import RSA private key.", ex);
            }
        }

        public RSA Rsa => _rsa;
    }
}
