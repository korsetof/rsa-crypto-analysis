using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA
{
    internal class RsaManager
    {

        public static RsaManager Instance { get; private set; }

        public RsaKeyPair KeyPair { get; private set; }
        public List<long> EncryptedData { get; private set; }

        private RsaKeyGenerator _keyGenerator;
        private RsaEncryptor _encryptor;
        private RsaDecryptor _decryptor;

        public event Action<string> OnGenerationStep;
        public event Action<string> OnEncryptionStep;
        public event Action<string> OnDecryptionStep;

        public RsaManager()
        {
            _keyGenerator = new RsaKeyGenerator();
            _encryptor = new RsaEncryptor();
            _decryptor = new RsaDecryptor();

            _keyGenerator.OnStep += (step) => OnGenerationStep?.Invoke(step);
            _encryptor.OnStep += (step) => OnEncryptionStep?.Invoke(step);
            _decryptor.OnStep += (step) => OnDecryptionStep?.Invoke(step);

            // Сохраняем как статический экземпляр
            Instance = this;
        }

        public void GenerateKeys(long p, long q)
        {
            KeyPair = _keyGenerator.GenerateKeys(p, q);
        }

        public void GenerateKeysAuto(long min, long max)
        {
            KeyPair = _keyGenerator.GenerateKeysAuto(min, max);
        }

        public List<long> Encrypt(string message)
        {
            if (KeyPair == null)
                throw new InvalidOperationException("Сначала сгенерируйте ключи");

            EncryptedData = _encryptor.Encrypt(message, KeyPair.E, KeyPair.N);
            return EncryptedData;
        }

        public string Decrypt()
        {
            if (EncryptedData == null || KeyPair == null)
                throw new InvalidOperationException("Нет данных для дешифрования");

            return _decryptor.Decrypt(EncryptedData, KeyPair.D, KeyPair.N);
        }

        public string Decrypt(List<long> encryptedData)
        {
            return _decryptor.Decrypt(encryptedData, KeyPair.D, KeyPair.N);
        }
    }
}
