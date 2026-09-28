using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA
{
    internal class RsaKeyGenerator
    {
        private List<string> _steps = new List<string>();
        public IReadOnlyList<string> Steps => _steps.AsReadOnly();
        public RsaKeyPair KeyPair { get; private set; }

        public event Action<string> OnStep;

        // Существующий метод генерации ключей
        public RsaKeyPair GenerateKeys(long p, long q)
        {
            ValidateInput(p, q);

            _steps.Clear();
            KeyPair = new RsaKeyPair { P = p, Q = q };

            Step1_ShowPrimes();
            Step2_CalculateN();
            Step3_CalculatePhi();
            Step4_FindE();
            Step5_CalculateD();
            Step6_VerifyKeys();

            return KeyPair;
        }

        // Новый метод: автоматическая генерация простых чисел и ключей
        public RsaKeyPair GenerateKeysAuto(long min, long max)
        {
            AddStep("=== АВТОМАТИЧЕСКАЯ ГЕНЕРАЦИЯ ПРОСТЫХ ЧИСЕЛ ===\n");
            AddStep($"Диапазон поиска: [{min}, {max}]");

            // Показываем статистику диапазона
            string statistics = PrimeNumberGenerator.GetRangeStatistics(min, max);
            AddStep(statistics + "\n");

            // Показываем процесс поиска простых чисел
            List<string> primeSteps;
            var primes = PrimeNumberGenerator.GetPrimesDemonstration(min, max, out primeSteps);

            foreach (var step in primeSteps)
            {
                AddStep(step);
            }

            AddStep("\n=== ГЕНЕРАЦИЯ ПАРЫ ПРОСТЫХ ЧИСЕЛ ===\n");

            // Генерируем пару различных простых чисел
            var pair = PrimeNumberGenerator.GeneratePrimePair(min, max);

            AddStep($"Случайно выбраны:\n" +
                   $"p = {pair.p} (простое: {RsaMath.IsPrime(pair.p)})\n" +
                   $"q = {pair.q} (простое: {RsaMath.IsPrime(pair.q)})\n");

            // Генерируем ключи
            return GenerateKeys(pair.p, pair.q);
        }

        private void ValidateInput(long p, long q)
        {
            var errors = new List<string>();

            // Проверка на простоту
            if (!RsaMath.IsPrime(p))
                errors.Add($"p = {p} не является простым числом");

            if (!RsaMath.IsPrime(q))
                errors.Add($"q = {q} не является простым числом");

            // Проверка на минимальные значения
            if (p < 2)
                errors.Add("p должно быть >= 2");

            if (q < 2)
                errors.Add("q должно быть >= 2");

            // Проверка на одинаковые числа
            if (p == q)
                errors.Add("p и q должны быть разными простыми числами");

            // Проверка на то, что n будет достаточно большим
            long n = p * q;
            if (n < 256) // Минимальный размер для кодирования ASCII
                errors.Add($"n = {n} слишком мало. Произведение p×q должно быть >= 256 " +
                          $"для корректного шифрования ASCII символов");

            if (errors.Count > 0)
                throw new ArgumentException(string.Join("\n", errors));
        }

        private void Step1_ShowPrimes()
        {
            string step = $"Шаг 1: Выбраны простые числа\n" +
                         $"p = {KeyPair.P} (простое: {RsaMath.IsPrime(KeyPair.P)})\n" +
                         $"q = {KeyPair.Q} (простое: {RsaMath.IsPrime(KeyPair.Q)})";
            AddStep(step);
        }

        private void Step2_CalculateN()
        {
            KeyPair.N = KeyPair.P * KeyPair.Q;
            string step = $"Шаг 2: Вычисляем модуль n\n" +
                         $"n = p × q = {KeyPair.P} × {KeyPair.Q} = {KeyPair.N}\n" +
                         $"Длина ключа: {Math.Floor(Math.Log(KeyPair.N, 2)) + 1} бит";
            AddStep(step);
        }

        private void Step3_CalculatePhi()
        {
            KeyPair.Phi = (KeyPair.P - 1) * (KeyPair.Q - 1);
            string step = $"Шаг 3: Вычисляем функцию Эйлера φ(n)\n" +
                         $"φ(n) = (p-1) × (q-1) = " +
                         $"({KeyPair.P}-1) × ({KeyPair.Q}-1) = " +
                         $"{KeyPair.P - 1} × {KeyPair.Q - 1} = {KeyPair.Phi}";
            AddStep(step);
        }

        private void Step4_FindE()
        {
            try
            {
                KeyPair.E = RsaMath.FindCoprime(KeyPair.Phi);
                string step = $"Шаг 4: Подбираем открытую экспоненту e\n" +
                             $"Условия: 1 < e < {KeyPair.Phi}, НОД(e, {KeyPair.Phi}) = 1\n" +
                             $"Выбрано e = {KeyPair.E}\n" +
                             $"Проверка: НОД({KeyPair.E}, {KeyPair.Phi}) = " +
                             $"{RsaMath.GCD(KeyPair.E, KeyPair.Phi)} ✓";
                AddStep(step);
            }
            catch (Exception ex)
            {
                AddStep($"ОШИБКА подбора e: {ex.Message}");
                throw;
            }
        }

        private void Step5_CalculateD()
        {
            try
            {
                KeyPair.D = RsaMath.ModInverse(KeyPair.E, KeyPair.Phi);
                string step = $"Шаг 5: Вычисляем закрытую экспоненту d\n" +
                             $"d = e⁻¹ mod φ(n)\n" +
                             $"d = {KeyPair.E}⁻¹ mod {KeyPair.Phi}\n" +
                             $"d = {KeyPair.D}\n" +
                             $"Проверка: (e × d) mod φ(n) = " +
                             $"({KeyPair.E} × {KeyPair.D}) mod {KeyPair.Phi} = " +
                             $"{(KeyPair.E * KeyPair.D) % KeyPair.Phi} ✓";
                AddStep(step);
            }
            catch (Exception ex)
            {
                AddStep($"ОШИБКА вычисления d: {ex.Message}");
                throw;
            }
        }

        private void Step6_VerifyKeys()
        {
            // Проверяем, что ключи работают
            long testValue = 42; // тестовое значение
            if (testValue >= KeyPair.N)
                testValue = KeyPair.N / 2;

            long encrypted = RsaMath.ModPow(testValue, KeyPair.E, KeyPair.N);
            long decrypted = RsaMath.ModPow(encrypted, KeyPair.D, KeyPair.N);

            string step = $"Шаг 6: Проверка ключей (тестовое значение: {testValue})\n" +
                         $"Шифруем: {testValue}^{KeyPair.E} mod {KeyPair.N} = {encrypted}\n" +
                         $"Расшифровываем: {encrypted}^{KeyPair.D} mod {KeyPair.N} = {decrypted}\n" +
                         $"Результат: {(testValue == decrypted ? "✓ Ключи корректны" : "✗ Ошибка!")}";
            AddStep(step);

            if (testValue != decrypted)
                throw new Exception("Сгенерированные ключи не прошли проверку!");
        }

        // ЕДИНСТВЕННЫЙ метод AddStep в классе
        private void AddStep(string step)
        {
            _steps.Add(step);
            OnStep?.Invoke(step);
        }
    }
}
