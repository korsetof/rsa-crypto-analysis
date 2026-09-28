using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA
{
    internal class RsaDecryptor
    {
        private List<string> _steps = new List<string>();
        public IReadOnlyList<string> Steps => _steps.AsReadOnly();

        public event Action<string> OnStep;

        public string Decrypt(List<long> encrypted, long d, long n)
        {
            _steps.Clear();

            // Проверка на пустые данные
            if (encrypted == null || encrypted.Count == 0)
            {
                AddStep("⚠ ПРЕДУПРЕЖДЕНИЕ: Нет данных для дешифрования.");
                return string.Empty;
            }

            var decrypted = new List<char>();

            AddStep($"Начинаем дешифрование с приватным ключом (d={d}, n={n})");
            AddStep($"Формула: M = C^d mod n\n");
            AddStep($"Количество зашифрованных блоков: {encrypted.Count}\n");

            for (int i = 0; i < encrypted.Count; i++)
            {
                long c = encrypted[i];

                // Проверка валидности зашифрованных данных
                if (c >= n)
                {
                    string error = $"✗ ОШИБКА: Зашифрованное значение {c} >= n ({n}).\n" +
                                  "Данные могли быть повреждены.";
                    AddStep(error);
                    throw new Exception(error);
                }

                long decryptedChar = RsaMath.ModPow(c, d, n);

                // Проверка, что получился валидный ASCII символ
                if (decryptedChar < 0 || decryptedChar > 127)
                {
                    AddStep($"⚠ ПРЕДУПРЕЖДЕНИЕ: Результат дешифрования ({decryptedChar}) " +
                           "не является стандартным ASCII символом");
                }

                char character = (char)decryptedChar;

                string step = $"Блок {i + 1}: C = {c}\n" +
                             $"  M = {c}^{d} mod {n} = {decryptedChar} → '{character}'";
                AddStep(step);

                decrypted.Add(character);
            }

            string result = new string(decrypted.ToArray());
            AddStep($"\n✓ Дешифрование завершено успешно");
            AddStep($"Расшифрованное сообщение: \"{result}\"");

            return result;
        }

        private void AddStep(string step)
        {
            _steps.Add(step);
            OnStep?.Invoke(step);
        }
    }
}
