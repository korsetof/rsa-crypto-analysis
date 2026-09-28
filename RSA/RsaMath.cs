using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSA
{
    internal class RsaMath
    {
        // Проверка числа на простоту
        public static bool IsPrime(long number)
        {
            if (number < 2) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            for (long i = 3; i <= Math.Sqrt(number); i += 2)
                if (number % i == 0) return false;
            return true;
        }

        // НОД (алгоритм Евклида)
        public static long GCD(long a, long b)
        {
            a = Math.Abs(a);
            b = Math.Abs(b);

            while (b != 0)
            {
                long temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        // Расширенный алгоритм Евклида
        public static long ExtendedGCD(long a, long b, out long x, out long y)
        {
            if (b == 0)
            {
                x = 1;
                y = 0;
                return a;
            }

            long x1, y1;
            long gcd = ExtendedGCD(b, a % b, out x1, out y1);

            x = y1;
            y = x1 - (a / b) * y1;

            return gcd;
        }

        // Модульное возведение в степень
        public static long ModPow(long baseValue, long exponent, long modulus)
        {
            if (modulus == 1) return 0;
            if (modulus <= 0)
                throw new ArgumentException("Модуль должен быть положительным числом");

            long result = 1;
            baseValue = baseValue % modulus;

            while (exponent > 0)
            {
                if (exponent % 2 == 1)
                    result = (result * baseValue) % modulus;

                exponent = exponent >> 1;
                baseValue = (baseValue * baseValue) % modulus;
            }

            return result;
        }

        // Поиск взаимно простого числа с phi
        public static long FindCoprime(long phi)
        {
            if (phi <= 2)
                throw new ArgumentException("φ(n) должна быть больше 2 для поиска подходящего e");

            // Пробуем стандартные значения
            long[] commonE = { 65537, 17, 5, 3 };

            foreach (long e in commonE)
            {
                if (e < phi && GCD(e, phi) == 1 && IsPrime(e))
                    return e;
            }

            // Если стандартные не подходят, ищем перебором
            for (long e = 3; e < phi; e += 2)
            {
                if (GCD(e, phi) == 1 && IsPrime(e))
                    return e;
            }

            throw new Exception($"Не удалось найти подходящее e для φ(n) = {phi}");
        }

        // Вычисление модульного обратного числа
        public static long ModInverse(long a, long m)
        {
            if (m <= 0)
                throw new ArgumentException("Модуль должен быть положительным");

            a = ((a % m) + m) % m;

            long x, y;
            long gcd = ExtendedGCD(a, m, out x, out y);

            if (gcd != 1)
                throw new ArgumentException($"Обратное число не существует: НОД({a}, {m}) = {gcd} ≠ 1");

            return ((x % m) + m) % m;
        }
    }
}
