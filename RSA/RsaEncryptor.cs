using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA
{
    internal class RsaEncryptor
    {
        private List<string> _steps = new List<string>();
        public IReadOnlyList<string> Steps => _steps.AsReadOnly();

        public event Action<string> OnStep;

        public List<long> Encrypt(string message, long e, long n)
        {
            _steps.Clear();

            // Проверка на пустое сообщение
            if (string.IsNullOrEmpty(message))
            {
                AddStep("⚠ ПРЕДУПРЕЖДЕНИЕ: Пустое сообщение. Нечего шифровать.");
                return new List<long>();
            }

            // Проверка валидности ключа
            if (n < 256)
            {
                AddStep($"⚠ ПРЕДУПРЕЖДЕНИЕ: Модуль n = {n} слишком мал. " +
                       "Некоторые ASCII символы могут быть больше n и не зашифруются корректно.");
            }

            var encrypted = new List<long>();

            AddStep($"Начинаем шифрование с публичным ключом (e={e}, n={n})");
            AddStep($"Формула: C = M^e mod n\n");
            AddStep($"Исходное сообщение: \"{message}\"");
            AddStep($"Длина сообщения: {message.Length} символов\n");

            for (int i = 0; i < message.Length; i++)
            {
                char c = message[i];
                long m = (long)c;

                // Проверка, что символ можно зашифровать
                if (m >= n)
                {
                    string error = $"✗ ОШИБКА: Символ '{c}' (ASCII {m}) не может быть зашифрован.\n" +
                                  $"Причина: ASCII код ({m}) >= n ({n}).\n" +
                                  $"Каждый символ должен быть меньше модуля n.";
                    AddStep(error);
                    throw new Exception($"Невозможно зашифровать символ '{c}'. {error}");
                }

                long encryptedChar = RsaMath.ModPow(m, e, n);

                string step = $"Символ {i + 1}: '{c}' (ASCII {m:D3})\n" +
                             $"  C = {m}^{e} mod {n} = {encryptedChar}";
                AddStep(step);

                encrypted.Add(encryptedChar);
            }

            AddStep($"\n✓ Шифрование завершено успешно");
            AddStep($"Зашифрованные данные: [{string.Join(", ", encrypted)}]");

            return encrypted;
        }

        private void AddStep(string step)
        {
            _steps.Add(step);
            OnStep?.Invoke(step);
        }
    }
}
