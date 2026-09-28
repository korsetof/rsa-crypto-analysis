using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA
{
    public static class PrimeNumberGenerator
    {
        private static Random _random = new Random();

        // Генерация случайного простого числа в заданном диапазоне
        public static long GeneratePrime(long min, long max)
        {
            if (min < 2)
                throw new ArgumentException("Минимальное значение должно быть >= 2");
            if (max <= min)
                throw new ArgumentException("Максимальное значение должно быть больше минимального");
            if (max > 1000000)
                throw new ArgumentException("Максимальное значение не должно превышать 1 000 000");

            List<long> primesInRange = GetPrimesInRange(min, max);

            if (primesInRange.Count == 0)
                throw new ArgumentException($"В диапазоне [{min}, {max}] нет простых чисел");

            int index = _random.Next(primesInRange.Count);
            return primesInRange[index];
        }

        // Генерация пары различных простых чисел
        public static (long p, long q) GeneratePrimePair(long min, long max)
        {
            if (max - min < 1)
                throw new ArgumentException("Диапазон слишком мал для генерации двух разных чисел");

            long p = GeneratePrime(min, max);
            long q;

            int attempts = 0;
            do
            {
                q = GeneratePrime(min, max);
                attempts++;

                if (attempts > 100)
                    throw new Exception("Не удалось найти два разных простых числа в заданном диапазоне");

            } while (q == p);

            return (p, q);
        }

        // Получение всех простых чисел в диапазоне (решето Эратосфена)
        public static List<long> GetPrimesInRange(long min, long max)
        {
            List<long> primes = new List<long>();

            // Для небольших чисел используем перебор
            if (max <= 10000)
            {
                for (long i = min; i <= max; i++)
                {
                    if (RsaMath.IsPrime(i))
                        primes.Add(i);
                }
            }
            else // Для больших чисел используем решето Эратосфена
            {
                primes = SieveOfEratosthenes(min, max);
            }

            return primes;
        }

        // Решето Эратосфена с пошаговой демонстрацией
        private static List<long> SieveOfEratosthenes(long min, long max)
        {
            int size = (int)(max - min + 1);
            bool[] isPrime = new bool[size];

            // Изначально все числа считаем простыми
            for (int i = 0; i < size; i++)
                isPrime[i] = true;

            // 0 и 1 не простые
            if (min <= 1)
            {
                for (long i = min; i <= Math.Min(1, max); i++)
                    isPrime[i - min] = false;
            }

            // Отсеиваем составные числа
            for (long i = 2; i * i <= max; i++)
            {
                long start = Math.Max(i * i, ((min + i - 1) / i) * i);

                for (long j = start; j <= max; j += i)
                {
                    isPrime[j - min] = false;
                }
            }

            // Собираем простые числа
            List<long> primes = new List<long>();
            for (long i = min; i <= max; i++)
            {
                if (isPrime[i - min])
                    primes.Add(i);
            }

            return primes;
        }

        // Получение списка простых чисел с шагом для демонстрации
        public static List<long> GetPrimesDemonstration(long min, long max, out List<string> steps)
        {
            steps = new List<string>();
            List<long> primes = new List<long>();

            steps.Add($"Поиск простых чисел в диапазоне [{min}, {max}]");
            steps.Add($"Метод: проверка делителей до √n\n");

            int totalChecked = 0;
            int totalPrimes = 0;

            for (long i = min; i <= max; i++)
            {
                totalChecked++;
                bool isPrime = RsaMath.IsPrime(i);

                if (isPrime)
                {
                    totalPrimes++;
                    primes.Add(i);

                    if (totalPrimes <= 10 || i >= max - 5) // Показываем первые 10 и последние
                    {
                        string divisors = GetDivisorsInfo(i);
                        steps.Add($"✓ {i} - ПРОСТОЕ {divisors}");
                    }
                    else if (totalPrimes == 11)
                    {
                        steps.Add($"... (пропущено {GetEstimatedPrimesCount(min, max) - 10} чисел) ...");
                    }
                }
            }

            steps.Add($"\nРезультат: найдено {totalPrimes} простых чисел из {totalChecked} проверенных");
            steps.Add($"Процент простых чисел: {(double)totalPrimes / totalChecked * 100:F1}%");

            return primes;
        }

        private static string GetDivisorsInfo(long number)
        {
            if (number <= 3)
                return "(делится только на 1 и на себя)";

            long sqrt = (long)Math.Sqrt(number);
            return $"(проверены делители от 2 до {sqrt})";
        }

        private static int GetEstimatedPrimesCount(long min, long max)
        {
            // Приблизительная оценка по теореме о распределении простых чисел
            if (max < 2) return 0;
            double estimate = max / Math.Log(max) - (min - 1) / Math.Log(Math.Max(min, 2));
            return Math.Max(0, (int)estimate);
        }

        // Получение статистики по диапазону
        public static string GetRangeStatistics(long min, long max)
        {
            var primes = GetPrimesInRange(min, max);

            if (primes.Count == 0)
                return $"В диапазоне [{min}, {max}] нет простых чисел";

            return $"Диапазон: [{min}, {max}]\n" +
                   $"Всего чисел: {max - min + 1}\n" +
                   $"Простых чисел: {primes.Count}\n" +
                   $"Плотность: {(double)primes.Count / (max - min + 1) * 100:F2}%\n" +
                   $"Минимальное: {primes[0]}\n" +
                   $"Максимальное: {primes[primes.Count - 1]}";
        }

        // Рекомендуемые диапазоны для разных уровней
        public static Dictionary<string, (long min, long max)> GetRecommendedRanges()
        {
            return new Dictionary<string, (long min, long max)>
            {
                ["Учебный (маленькие числа)"] = (3, 100),
                ["Стандартный (средние числа)"] = (50, 500),
                ["Продвинутый (большие числа)"] = (100, 2000),
                ["Профессиональный"] = (1000, 10000)
            };
        }
    }
}
