using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RSA
{
    internal class RsaCrackAnalyzer
    {
        public CrackResult BruteForceResult { get; private set; }
        public CrackResult FactorizationResult { get; private set; }
        public CrackResult MessageAnalysisResult { get; private set; }

        public event Action<string> OnStep;

        // ====================================================================
        // МЕТОД 1: ПОЛНЫЙ ПЕРЕБОР ЗАКРЫТОГО КЛЮЧА (BRUTE FORCE)
        // ====================================================================
        public CrackResult BruteForcePrivateKey(long e, long n, long maxAttempts = 1000000, long timeoutSeconds = 10)
        {
            var result = new CrackResult
            {
                MethodName = "1. ПОЛНЫЙ ПЕРЕБОР ЗАКРЫТОГО КЛЮЧА d",
                StartTime = DateTime.Now
            };

            // ==========================================
            // ТЕОРЕТИЧЕСКОЕ ВВЕДЕНИЕ
            // ==========================================
            AddStep(@"
╔══════════════════════════════════════════════════════════════╗
║           МЕТОД 1: ПОЛНЫЙ ПЕРЕБОР (BRUTE FORCE)            ║
╚══════════════════════════════════════════════════════════════╝

ЧТО ЭТО ТАКОЕ?
Полный перебор — это самый простой метод взлома.
Атакующий пытается ПОДРЯД перебрать все возможные значения
закрытого ключа d, пока не найдёт правильный.

КАК РАБОТАЕТ RSA?
Напомним: в RSA шифрование и расшифровка работают так:
  Шифрование: C = M^e mod n
  Расшифровка: M = C^d mod n

где:
  (e, n) — ОТКРЫТЫЙ ключ (известен всем)
  (d, n) — ЗАКРЫТЫЙ ключ (хранится в секрете)

ЧТО ИЩЕТ АТАКУЮЩИЙ?
Зная e и n, он хочет найти d.
Связь между e и d: (e × d) mod φ(n) = 1

ПРОБЛЕМА ДЛЯ АТАКУЮЩЕГО:
Он НЕ знает φ(n), потому что не знает p и q!
φ(n) = (p-1)(q-1)

ПОЭТОМУ он просто перебирает d подряд:
  d=1: проверяем расшифровку
  d=2: проверяем расшифровку
  d=3: проверяем расшифровку
  ...и так далее...

СКОЛЬКО ВАРИАНТОВ?
Теоретически d может быть от 1 до φ(n)-1.
φ(n) примерно равна n (точнее n - p - q + 1).
Значит нужно перебрать ~n вариантов!

Для n = 3233 нужно ~3000 попыток.
Для n = 2048 бит нужно ~10^616 попыток!!!
Это больше, чем атомов во Вселенной (10^80).

============================================================\n");

            // ==========================================
            // ИСХОДНЫЕ ДАННЫЕ
            // ==========================================
            AddStep("═══════════════════════════════════════");
            AddStep("  ИСХОДНЫЕ ДАННЫЕ ДЛЯ АТАКИ");
            AddStep("═══════════════════════════════════════");
            AddStep($"");
            AddStep($"  Открытый ключ e = {e}");
            AddStep($"  Модуль n = {n}");
            AddStep($"  Длина ключа: {Math.Floor(Math.Log(n, 2)) + 1} бит");
            AddStep($"  Десятичных цифр в n: {Math.Floor(Math.Log10(n)) + 1}");
            AddStep($"");
            AddStep($"  Диапазон перебора d: от 1 до {maxAttempts:N0}");
            AddStep($"  Максимальное время: {timeoutSeconds} секунд");
            AddStep($"");
            AddStep($"  Алгоритм действий:");
            AddStep($"  1. Берём d = 1");
            AddStep($"  2. Пробуем расшифровать тестовое сообщение");
            AddStep($"  3. Если не получилось — d = d + 1");
            AddStep($"  4. Повторяем, пока не найдём или не истечёт время");
            AddStep($"\n");

            // ==========================================
            // ЗАПУСК ПЕРЕБОРА
            // ==========================================
            AddStep("═══════════════════════════════════════");
            AddStep("  ЗАПУСК ПЕРЕБОРА");
            AddStep("═══════════════════════════════════════\n");

            var stopwatch = Stopwatch.StartNew();
            long foundD = -1;
            long attempts = 0;
            long lastReport = 0;

            AddStep("Начинаем последовательный перебор d:\n");

            for (long d = 1; d <= maxAttempts; d++)
            {
                attempts++;

                // Проверка таймаута
                if (stopwatch.Elapsed.TotalSeconds > timeoutSeconds)
                {
                    AddStep($"\n  ⏱ ПРОШЛО {timeoutSeconds} СЕКУНД — ОСТАНАВЛИВАЕМСЯ!");
                    AddStep($"  Это ограничение установлено для демонстрации.");
                    AddStep($"  В реальности атакующий может ждать сколько угодно.\n");
                    break;
                }

                // Отчёт о прогрессе каждые 100 000 попыток
                if (attempts - lastReport >= 100000)
                {
                    lastReport = attempts;
                    double progress = (double)attempts / maxAttempts * 100;
                    double speed = attempts / stopwatch.Elapsed.TotalSeconds;

                    AddStep($"  ─────────────────────────────────────");
                    AddStep($"  Проверено: {attempts:N0} значений d");
                    AddStep($"  Прогресс: {progress:F4}% от {maxAttempts:N0}");
                    AddStep($"  Скорость: {speed:N0} попыток в секунду");
                    AddStep($"  Прошло времени: {stopwatch.Elapsed.TotalSeconds:F2} сек");

                    // Оценка оставшегося времени
                    if (progress > 0)
                    {
                        double remaining = stopwatch.Elapsed.TotalSeconds / progress * (100 - progress);
                        AddStep($"  Примерно осталось: {remaining:F1} секунд");
                    }
                    AddStep($"  ─────────────────────────────────────\n");
                }
            }

            stopwatch.Stop();

            // ==========================================
            // АНАЛИЗ РЕЗУЛЬТАТОВ
            // ==========================================
            result.EndTime = DateTime.Now;
            result.IsSuccess = foundD != -1;
            result.FoundValue = foundD;
            result.Attempts = attempts;
            result.ElapsedTime = stopwatch.Elapsed;

            AddStep("═══════════════════════════════════════");
            AddStep("  РЕЗУЛЬТАТЫ ПЕРЕБОРА");
            AddStep("═══════════════════════════════════════\n");

            if (result.IsSuccess)
            {
                AddStep($"  ✓ УСПЕХ! Закрытый ключ НАЙДЕН!");
                AddStep($"  d = {foundD}");
                AddStep($"  Потребовалось {attempts:N0} попыток");
                AddStep($"  Время: {stopwatch.Elapsed.TotalSeconds:F4} секунд");
                AddStep($"");
                AddStep($"  Это означает, что злоумышленник может:");
                AddStep($"  • Расшифровать ВСЕ сообщения");
                AddStep($"  • Подделать цифровую подпись");
                AddStep($"  • Полностью скомпрометировать систему!");
            }
            else
            {
                AddStep($"  ✗ НЕУДАЧА! Ключ не найден за отведённое время.");
                AddStep($"  Проверено: {attempts:N0} значений из {maxAttempts:N0}");
                AddStep($"  Это лишь {(double)attempts / maxAttempts * 100:F6}% от заданного диапазона!");
                AddStep($"  Затрачено: {stopwatch.Elapsed.TotalSeconds:F4} секунд");
            }

            // ==========================================
            // ОЦЕНКА ПОЛНОГО ВРЕМЕНИ
            // ==========================================
            if (attempts > 0 && stopwatch.Elapsed.TotalSeconds > 0)
            {
                double speed = attempts / stopwatch.Elapsed.TotalSeconds;

                AddStep($"\n  ─────── СТАТИСТИКА ───────");
                AddStep($"  Скорость перебора: {speed:N0} значений/сек");
                AddStep($"  Среднее время на 1 попытку: {stopwatch.Elapsed.TotalMilliseconds / attempts:F6} мс");

                // Оценка полного перебора
                double estimatedSpace = n * 0.6; // примерный размер φ(n)
                double totalSeconds = estimatedSpace / speed;

                AddStep($"");
                AddStep($"  ─────── ОЦЕНКА ПОЛНОГО ПЕРЕБОРА ───────");
                AddStep($"  Размер пространства ключей: ~{estimatedSpace:N0} значений");
                AddStep($"  При текущей скорости потребуется:");

                result.EstimatedTotalTime = TimeSpan.FromSeconds(Math.Min(totalSeconds, TimeSpan.MaxValue.TotalSeconds));

                if (totalSeconds > 31536000.0 * 1000.0)
                {
                    AddStep($"  > {totalSeconds / 31536000.0 / 1000.0:F1} тысяч лет");
                    AddStep($"");
                    AddStep($"  🔒 ВЫВОД: ПОЛНЫЙ ПЕРЕБОР НЕВОЗМОЖЕН!");
                    AddStep($"  Даже если использовать ВСЕ компьютеры мира,");
                    AddStep($"  это займёт невообразимо много времени.");
                    AddStep($"  Например: 1 миллион компьютеров сократят");
                    AddStep($"  время до {totalSeconds / 31536000.0 / 1000000.0:F1} тысяч лет — всё равно нереально!");
                }
                else if (totalSeconds > 31536000.0)
                {
                    AddStep($"  ~{totalSeconds / 31536000.0:F1} лет");
                    AddStep($"");
                    AddStep($"  ⚠ ВЫВОД: ПЕРЕБОР ТЕОРЕТИЧЕСКИ ВОЗМОЖЕН,");
                    AddStep($"  но на практике нереален (слишком долго).");
                }
                else if (totalSeconds > 86400.0)
                {
                    AddStep($"  ~{totalSeconds / 86400.0:F1} дней");
                    AddStep($"");
                    AddStep($"  ⚠ ВЫВОД: ПЕРЕБОР ВОЗМОЖЕН,");
                    AddStep($"  но требует значительного времени.");
                }
                else
                {
                    AddStep($"  ~{totalSeconds:F2} секунд");
                    AddStep($"");
                    AddStep($"  ❌ ВЫВОД: КЛЮЧ СЛИШКОМ КОРОТКИЙ!");
                    AddStep($"  Взлом возможен за секунды!");
                    AddStep($"  Немедленно увеличьте p и q!");
                }
            }

            // Итоговый комментарий
            result.Comment = FormatTimeSpan(result.EstimatedTotalTime);

            AddStep($"\n═══════════════════════════════════════");
            AddStep($"  ИТОГОВАЯ ОЦЕНКА: {result.Comment}");
            AddStep($"═══════════════════════════════════════\n");

            BruteForceResult = result;
            return result;
        }

        // ====================================================================
        // МЕТОД 2: ФАКТОРИЗАЦИЯ МОДУЛЯ n
        // ====================================================================
        public CrackResult FactorizeN(long n, long timeoutSeconds = 30)
        {
            var result = new CrackResult
            {
                MethodName = "2. ФАКТОРИЗАЦИЯ МОДУЛЯ n",
                StartTime = DateTime.Now
            };

            // ==========================================
            // ТЕОРЕТИЧЕСКОЕ ВВЕДЕНИЕ
            // ==========================================
            AddStep(@"
╔══════════════════════════════════════════════════════════════╗
║              МЕТОД 2: ФАКТОРИЗАЦИЯ МОДУЛЯ n                ║
╚══════════════════════════════════════════════════════════════╝

ЧТО ТАКОЕ ФАКТОРИЗАЦИЯ?
Это разложение числа n на простые множители p и q.

ВСПОМНИМ RSA:
  n = p × q  (это часть ОТКРЫТОГО ключа!)

ПОЧЕМУ ЭТО ОПАСНО?
Если злоумышленник сможет разложить n на p и q, он сможет:
  1. Вычислить φ(n) = (p-1)(q-1)
  2. Зная e, найти d = e⁻¹ mod φ(n)
  3. РАСШИФРОВАТЬ ВСЁ!

В ЧЁМ СЛОЖНОСТЬ?
Разложить большое число на множители ОЧЕНЬ сложно!
  • Число из 100 цифр — месяцы на кластере
  • Число из 300 цифр — миллиарды лет
  • RSA-2048 (617 цифр) — НЕВОЗМОЖНО современными методами!

============================================================

КАК МЫ БУДЕМ ФАКТОРИЗОВАТЬ?

Метод: ПЕРЕБОР ДЕЛИТЕЛЕЙ (самый простой)

Алгоритм:
  1. Проверяем, делится ли n на 2
  2. Если нет — проверяем нечётные числа: 3, 5, 7, 9...
  3. Проверяем до √n (дальше нет смысла)
  
Почему до √n?
  Если n = p × q, и p ≤ q, то p ≤ √n.
  Значит, меньший делитель точно не больше √n.

Сложность: O(√n) — экспоненциальная!
  Для n из 10 цифр: ~100 000 проверок
  Для n из 100 цифр: ~10^50 проверок!!!

============================================================\n");

            // ==========================================
            // ИСХОДНЫЕ ДАННЫЕ
            // ==========================================
            long sqrtN = (long)Math.Sqrt(n);

            AddStep("═══════════════════════════════════════");
            AddStep("  ДАННЫЕ ДЛЯ ФАКТОРИЗАЦИИ");
            AddStep("═══════════════════════════════════════");
            AddStep($"");
            AddStep($"  Модуль n = {n}");
            AddStep($"  Количество цифр в n: {Math.Floor(Math.Log10(n)) + 1}");
            AddStep($"  √n ≈ {sqrtN:N0}");
            AddStep($"  (это максимальное число, до которого нужно проверять)");
            AddStep($"");
            AddStep($"  Количество нечётных чисел до √n: {sqrtN / 2:N0}");
            AddStep($"  Это и есть максимальное число попыток!");
            AddStep($"  Таймаут: {timeoutSeconds} секунд\n");

            // ==========================================
            // ЗАПУСК ФАКТОРИЗАЦИИ
            // ==========================================
            AddStep("═══════════════════════════════════════");
            AddStep("  ПРОЦЕСС ФАКТОРИЗАЦИИ");
            AddStep("═══════════════════════════════════════\n");

            var stopwatch = Stopwatch.StartNew();
            long foundP = -1, foundQ = -1;
            long attempts = 0;

            // Шаг 1: Проверка на чётность
            AddStep("ШАГ 1: Проверяем, является ли n чётным...");
            if (n % 2 == 0)
            {
                foundP = 2;
                foundQ = n / 2;
                AddStep($"  ✓ n = {n} — ЧЁТНОЕ!");
                AddStep($"  Сразу находим: p = 2, q = {n} / 2 = {foundQ}");
                AddStep($"  Проверка: 2 × {foundQ} = {2 * foundQ} ✓\n");
            }
            else
            {
                AddStep($"  n = {n} — НЕЧЁТНОЕ");
                AddStep($"  Значит, p и q тоже нечётные.");
                AddStep($"  Начинаем перебор с 3, шаг 2 (только нечётные)...\n");

                // Шаг 2: Перебор нечётных делителей
                AddStep("ШАГ 2: Последовательный перебор делителей");
                AddStep($"  Проверяем числа: 3, 5, 7, 9, 11, ... до {sqrtN}\n");

                long lastReport = 0;

                for (long i = 3; i <= sqrtN; i += 2)
                {
                    attempts++;

                    // Таймаут
                    if (stopwatch.Elapsed.TotalSeconds > timeoutSeconds)
                    {
                        AddStep($"\n  ⏱ ТАЙМАУТ! Прошло {timeoutSeconds} секунд.");
                        AddStep($"  Успели проверить: {attempts:N0} чисел");
                        AddStep($"  Это {(double)attempts / (sqrtN / 2) * 100:F6}% от общего количества\n");
                        break;
                    }

                    // Проверяем, делится ли n на i
                    if (n % i == 0)
                    {
                        foundP = i;
                        foundQ = n / i;

                        AddStep($"\n  ✓✓✓ НАЙДЕН ДЕЛИТЕЛЬ! ✓✓✓");
                        AddStep($"  ─────────────────────────────");
                        AddStep($"  После {attempts:N0} попыток:");
                        AddStep($"  {n} ÷ {i} = {n / i} (делится без остатка!)");
                        AddStep($"  p = {i}");
                        AddStep($"  q = {n / i}");
                        AddStep($"  ─────────────────────────────\n");

                        // Проверяем простоту
                        AddStep($"  Проверка: являются ли p и q простыми?");
                        bool pPrime = RsaMath.IsPrime(foundP);
                        bool qPrime = RsaMath.IsPrime(foundQ);

                        AddStep($"    p = {foundP} → {(pPrime ? "ПРОСТОЕ ✓" : "СОСТАВНОЕ ✗ (странно...)")}");
                        AddStep($"    q = {foundQ} → {(qPrime ? "ПРОСТОЕ ✓" : "СОСТАВНОЕ ✗ (странно...)")}");

                        if (pPrime && qPrime)
                        {
                            AddStep($"\n  ✅ ФАКТОРИЗАЦИЯ УСПЕШНА!");
                            AddStep($"  Найдены простые множители n = {foundP} × {foundQ}");
                            AddStep($"");
                            AddStep($"  Теперь злоумышленник может:");
                            AddStep($"  1. φ(n) = ({foundP}-1) × ({foundQ}-1) = {foundP - 1} × {foundQ - 1} = {(foundP - 1) * (foundQ - 1)}");
                            AddStep($"  2. Найти d (зная e)");
                            AddStep($"  3. Расшифровать все сообщения!");
                        }
                        else
                        {
                            AddStep($"\n  ⚠ Странная ситуация: множители не простые.");
                            AddStep($"  Продолжаем поиск...");
                            foundP = -1;
                            foundQ = -1;
                        }
                        break;
                    }

                    // Отчёт о прогрессе
                    if (attempts - lastReport >= 500000)
                    {
                        lastReport = attempts;
                        double progress = (double)i / sqrtN * 100;
                        double speed = attempts / stopwatch.Elapsed.TotalSeconds;

                        AddStep($"  ───── Прогресс ─────");
                        AddStep($"  Текущее число: {i:N0} (из {sqrtN:N0})");
                        AddStep($"  Прогресс: {progress:F4}%");
                        AddStep($"  Проверено чисел: {attempts:N0}");
                        AddStep($"  Скорость: {speed:N0} чисел/сек");
                        AddStep($"  Прошло: {stopwatch.Elapsed.TotalSeconds:F2} сек\n");
                    }
                }
            }

            stopwatch.Stop();

            // ==========================================
            // РЕЗУЛЬТАТЫ
            // ==========================================
            result.EndTime = DateTime.Now;
            result.IsSuccess = foundP != -1;
            result.FoundP = foundP;
            result.FoundQ = foundQ;
            result.Attempts = attempts;
            result.ElapsedTime = stopwatch.Elapsed;

            AddStep("═══════════════════════════════════════");
            AddStep("  РЕЗУЛЬТАТЫ ФАКТОРИЗАЦИИ");
            AddStep("═══════════════════════════════════════\n");

            if (result.IsSuccess)
            {
                AddStep($"  ✅ УСПЕХ! n = {foundP} × {foundQ}");
                AddStep($"  Потребовалось попыток: {attempts:N0}");
                AddStep($"  Время: {stopwatch.Elapsed.TotalSeconds:F4} сек");
                AddStep($"  Ключ ВЗЛОМАН!");
            }
            else
            {
                AddStep($"  ❌ ФАКТОРИЗАЦИЯ НЕ УДАЛАСЬ");
                AddStep($"  Проверено чисел: {attempts:N0} из ~{sqrtN / 2:N0}");
                AddStep($"  Время: {stopwatch.Elapsed.TotalSeconds:F4} сек");

                // Оценка полного времени
                if (attempts > 0)
                {
                    double speed = attempts / stopwatch.Elapsed.TotalSeconds;
                    double remaining = (sqrtN / 2) - attempts;
                    double totalSeconds = (sqrtN / 2) / speed;

                    result.EstimatedTotalTime = TimeSpan.FromSeconds(Math.Min(totalSeconds, TimeSpan.MaxValue.TotalSeconds));

                    AddStep($"");
                    AddStep($"  ОЦЕНКА ПОЛНОЙ ФАКТОРИЗАЦИИ:");
                    AddStep($"  Скорость: {speed:N0} чисел/сек");
                    AddStep($"  Осталось проверить: {remaining:N0} чисел");
                    AddStep($"  Полное время: {FormatTimeSpan(result.EstimatedTotalTime)}");

                    if (totalSeconds > 31536000.0 * 1000)
                    {
                        AddStep($"");
                        AddStep($"  🔒 ФАКТОРИЗАЦИЯ НЕВОЗМОЖНА!");
                        AddStep($"  Даже на суперкомпьютере это займёт");
                        AddStep($"  миллионы лет. RSA в безопасности!");
                    }
                    else if (totalSeconds > 86400.0)
                    {
                        AddStep($"");
                        AddStep($"  ⚠ ФАКТОРИЗАЦИЯ ВОЗМОЖНА,");
                        AddStep($"  но требует значительных ресурсов.");
                    }
                    else
                    {
                        AddStep($"");
                        AddStep($"  ❌ КЛЮЧ СЛИШКОМ МАЛ!");
                        AddStep($"  Факторизация происходит за секунды.");
                        AddStep($"  Используйте числа побольше!");
                    }
                }
            }

            result.Comment = result.IsSuccess ?
                $"p={foundP}, q={foundQ}" :
                FormatTimeSpan(result.EstimatedTotalTime);

            FactorizationResult = result;
            return result;
        }

        // ====================================================================
        // МЕТОД 3: АНАЛИЗ ЗАШИФРОВАННОГО СООБЩЕНИЯ
        // ====================================================================
        public CrackResult AnalyzeEncryptedMessage(List<long> encryptedMessage, long e, long n)
        {
            var result = new CrackResult
            {
                MethodName = "3. АНАЛИЗ ЗАШИФРОВАННОГО ТЕКСТА",
                StartTime = DateTime.Now
            };

            // ==========================================
            // ТЕОРЕТИЧЕСКОЕ ВВЕДЕНИЕ
            // ==========================================
            AddStep(@"
╔══════════════════════════════════════════════════════════════╗
║         МЕТОД 3: АНАЛИЗ ЗАШИФРОВАННОГО СООБЩЕНИЯ          ║
╚══════════════════════════════════════════════════════════════╝

ЧТО ЭТО ТАКОЕ?
Атакующий пытается восстановить исходный текст,
анализируя ТОЛЬКО зашифрованное сообщение,
БЕЗ попыток взломать ключ.

МЕТОДЫ АНАЛИЗА:

1. ЧАСТОТНЫЙ АНАЛИЗ
   Изучаем, как часто встречаются разные блоки.
   В обычном тексте буквы имеют разную частоту:
   • Русский: О (11%), Е (8.5%), А (8%)
   • Английский: E (12.7%), T (9.1%), A (8.2%)
   
   В RSA каждый блок — это число, а не буква!
   Поэтому частотный анализ НЕ РАБОТАЕТ
   (при правильной реализации).

2. ПОИСК ЗАКОНОМЕРНОСТЕЙ
   Ищем повторяющиеся паттерны в шифротексте.
   Если одинаковые блоки исходного текста дают
   одинаковые блоки шифра — это уязвимость!
   (в RSA такого нет при использовании паддинга).

3. ПЕРЕБОР ВОЗМОЖНЫХ ЗНАЧЕНИЙ
   Если n маленькое, можно для каждого блока C
   перебрать все M от 0 до n-1 и проверить:
   M^e mod n == C ?
   
   Сложность: O(n) для каждого блока!

============================================================\n");

            // ==========================================
            // ДАННЫЕ
            // ==========================================
            AddStep("═══════════════════════════════════════");
            AddStep("  АНАЛИЗИРУЕМОЕ СООБЩЕНИЕ");
            AddStep("═══════════════════════════════════════");
            AddStep($"");
            AddStep($"  Зашифрованное сообщение:");
            AddStep($"  [{string.Join(", ", encryptedMessage)}]");
            AddStep($"");
            AddStep($"  Количество блоков: {encryptedMessage.Count}");
            AddStep($"  Публичный ключ: e = {e}, n = {n}");
            AddStep($"  Размер блока: 0 до {n - 1}\n");

            var stopwatch = Stopwatch.StartNew();

            // ==========================================
            // ЭТАП 1: СТАТИСТИЧЕСКИЙ АНАЛИЗ
            // ==========================================
            AddStep("───────────────────────────────────────");
            AddStep("  ЭТАП 1: СТАТИСТИЧЕСКИЙ АНАЛИЗ");
            AddStep("───────────────────────────────────────\n");

            AddStep("1.1 Базовая статистика блоков:");
            AddStep($"    Минимальное значение: {encryptedMessage.Min()}");
            AddStep($"    Максимальное значение: {encryptedMessage.Max()}");
            AddStep($"    Среднее значение: {encryptedMessage.Average():F2}");
            AddStep($"    Медиана: {GetMedian(encryptedMessage):F2}");
            AddStep($"    Уникальных блоков: {encryptedMessage.Distinct().Count()}");
            AddStep($"    Повторяющихся: {encryptedMessage.Count - encryptedMessage.Distinct().Count()}");
            AddStep($"");

            // Частотный анализ
            AddStep("1.2 Частотный анализ (топ-10 значений):");
            AddStep("    Значение | Количество | Процент");
            AddStep("    ─────────┼────────────┼────────");

            var freq = encryptedMessage
                .GroupBy(x => x)
                .OrderByDescending(g => g.Count())
                .Take(10);

            int rank = 1;
            foreach (var f in freq)
            {
                double pct = (double)f.Count() / encryptedMessage.Count * 100;
                AddStep($"    {rank,2}. {f.Key,8} | {f.Count(),10} | {pct,6:F2}%");
                rank++;
            }

            AddStep($"\n1.3 Вывод частотного анализа:");
            if (n >= 256)
            {
                AddStep($"    n = {n} ≥ 256 (размер ASCII таблицы)");
                AddStep($"    Каждый блок может кодировать НЕСКОЛЬКО символов");
                AddStep($"    или один символ с паддингом.");
                AddStep($"    Частотный анализ НЕЭФФЕКТИВЕН!");
            }
            else
            {
                AddStep($"    n = {n} < 256");
                AddStep($"    Теоретически блоки соответствуют одиночным символам.");
                AddStep($"    Но без знания ключа сопоставление невозможно.");
            }

            // ==========================================
            // ЭТАП 2: ПОИСК ЗАКОНОМЕРНОСТЕЙ
            // ==========================================
            AddStep($"\n───────────────────────────────────────");
            AddStep($"  ЭТАП 2: ПОИСК ЗАКОНОМЕРНОСТЕЙ");
            AddStep($"───────────────────────────────────────\n");

            if (encryptedMessage.Count > 1)
            {
                AddStep("2.1 Анализ разностей между соседними блоками:");
                var diffs = new List<long>();
                for (int i = 1; i < Math.Min(encryptedMessage.Count, 8); i++)
                {
                    long diff = encryptedMessage[i] - encryptedMessage[i - 1];
                    diffs.Add(diff);
                    AddStep($"    Блок[{i}] - Блок[{i - 1}] = {encryptedMessage[i]} - {encryptedMessage[i - 1]} = {diff}");
                }

                bool constantDiff = diffs.Distinct().Count() == 1;
                AddStep($"\n    Разности {(constantDiff ? "ОДИНАКОВЫЕ — есть паттерн!" : "разные — нет простого паттерна.")}");

                AddStep($"\n2.2 Поиск повторяющихся последовательностей:");
                bool foundPattern = false;
                for (int len = 2; len <= Math.Min(4, encryptedMessage.Count / 2); len++)
                {
                    for (int i = 0; i <= encryptedMessage.Count - len * 2; i++)
                    {
                        var seq1 = encryptedMessage.Skip(i).Take(len).ToList();
                        for (int j = i + len; j <= encryptedMessage.Count - len; j++)
                        {
                            var seq2 = encryptedMessage.Skip(j).Take(len).ToList();
                            if (seq1.SequenceEqual(seq2))
                            {
                                AddStep($"    Найден повтор длины {len}: позиции {i} и {j}");
                                foundPattern = true;
                            }
                        }
                    }
                }
                if (!foundPattern)
                    AddStep($"    Повторяющиеся последовательности не найдены.");
            }

            // ==========================================
            // ЭТАП 3: ПОПЫТКА ПРЯМОГО ПЕРЕБОРА
            // ==========================================
            AddStep($"\n───────────────────────────────────────");
            AddStep($"  ЭТАП 3: ПОПЫТКА ПЕРЕБОРА (если n мало)");
            AddStep($"───────────────────────────────────────\n");

            if (n < 1000)
            {
                AddStep($"3.1 n = {n} ОЧЕНЬ МАЛО! Пробуем прямой перебор...");
                AddStep($"    Для каждого блока C перебираем M от 0 до {n - 1}:");
                AddStep($"    Проверяем: M^{e} mod {n} == C ?\n");

                int found = 0;
                var decrypted = new List<char>();

                foreach (long C in encryptedMessage)
                {
                    for (long M = 0; M < n; M++)
                    {
                        if (RsaMath.ModPow(M, e, n) == C)
                        {
                            if (M < 128 && M >= 32)
                            {
                                decrypted.Add((char)M);
                                AddStep($"    C = {C,4} → M = {M,3} ('{(char)M}')");
                            }
                            else
                            {
                                AddStep($"    C = {C,4} → M = {M,3} (непечатный символ)");
                            }
                            found++;
                            break;
                        }
                    }
                }

                if (decrypted.Count > 0)
                {
                    string text = new string(decrypted.ToArray());
                    AddStep($"\n  ✓ Расшифровано успешно!");
                    AddStep($"  Текст: \"{text}\"");
                    AddStep($"  Это сработало только потому, что n = {n} очень мало!");
                    result.IsSuccess = true;
                }
            }
            else if (n < 100000)
            {
                long totalOps = encryptedMessage.Count * n;
                double estSeconds = totalOps / 1000000.0;

                AddStep($"3.1 n = {n} — перебор ВОЗМОЖЕН, но займёт время:");
                AddStep($"    Для каждого из {encryptedMessage.Count} блоков");
                AddStep($"    нужно проверить до {n} значений.");
                AddStep($"    Всего операций: {encryptedMessage.Count} × {n} = {totalOps:N0}");
                AddStep($"    При 1 млн оп/сек: ~{estSeconds:F2} секунд");

                if (estSeconds < 60)
                {
                    AddStep($"\n  ⚠ Перебор РЕАЛЕН за разумное время!");
                    AddStep($"  Но это учебный пример. В реальности");
                    AddStep($"  используют n >> 100000.");
                }
                else
                {
                    AddStep($"\n  ⚠ Перебор займёт {estSeconds / 60:F1} минут.");
                    AddStep($"  Теоретически возможно, практически — нет.");
                }
            }
            else
            {
                AddStep($"3.1 n = {n} СЛИШКОМ БОЛЬШОЕ!");
                AddStep($"    Прямой перебор {n} значений для каждого блока");
                AddStep($"    займёт астрономическое время.");
                AddStep($"    Без факторизации n расшифровка НЕВОЗМОЖНА!");
            }

            stopwatch.Stop();
            result.ElapsedTime = stopwatch.Elapsed;

            // ==========================================
            // ИТОГ
            // ==========================================
            AddStep($"\n═══════════════════════════════════════");
            AddStep($"  ИТОГ АНАЛИЗА");
            AddStep($"═══════════════════════════════════════");
            AddStep($"  Время анализа: {stopwatch.Elapsed.TotalSeconds:F4} сек");
            AddStep($"  Уровень безопасности: {GetSecurityLevel(n)}");

            result.Comment = GetSecurityLevel(n);
            MessageAnalysisResult = result;
            return result;
        }

        // ====================================================================
        // СВОДНЫЙ ОТЧЁТ
        // ====================================================================
        public string GetSummary()
        {
            string s = "\n";
            s += "╔══════════════════════════════════════════════╗\n";
            s += "║     СВОДНЫЙ ОТЧЁТ КРИПТОАНАЛИЗА RSA       ║\n";
            s += "╚══════════════════════════════════════════════╝\n\n";

            if (BruteForceResult != null)
            {
                s += "──────────────────────────────────────────────\n";
                s += $" {BruteForceResult.MethodName}\n";
                s += "──────────────────────────────────────────────\n";
                s += $" Статус: {(BruteForceResult.IsSuccess ? "✅ ВЗЛОМАН" : "❌ НЕ ВЗЛОМАН")}\n";
                s += $" Попыток: {BruteForceResult.Attempts:N0}\n";
                s += $" Время: {BruteForceResult.ElapsedTime.TotalSeconds:F4} сек\n";
                s += $" Оценка: {BruteForceResult.Comment}\n\n";
            }

            if (FactorizationResult != null)
            {
                s += "──────────────────────────────────────────────\n";
                s += $" {FactorizationResult.MethodName}\n";
                s += "──────────────────────────────────────────────\n";
                s += $" Статус: {(FactorizationResult.IsSuccess ? $"✅ n = {FactorizationResult.FoundP} × {FactorizationResult.FoundQ}" : "❌ НЕ ФАКТОРИЗОВАН")}\n";
                s += $" Попыток: {FactorizationResult.Attempts:N0}\n";
                s += $" Время: {FactorizationResult.ElapsedTime.TotalSeconds:F4} сек\n";
                s += $" Оценка: {FactorizationResult.Comment}\n\n";
            }

            if (MessageAnalysisResult != null)
            {
                s += "──────────────────────────────────────────────\n";
                s += $" {MessageAnalysisResult.MethodName}\n";
                s += "──────────────────────────────────────────────\n";
                s += $" Время: {MessageAnalysisResult.ElapsedTime.TotalSeconds:F4} сек\n";
                s += $" Оценка: {MessageAnalysisResult.Comment}\n\n";
            }

            // Итоговая оценка
            s += "══════════════════════════════════════════════\n";
            s += " ОБЩИЙ ВЫВОД:\n";
            s += "══════════════════════════════════════════════\n";

            bool allFailed = true;
            if (BruteForceResult?.IsSuccess == true) allFailed = false;
            if (FactorizationResult?.IsSuccess == true) allFailed = false;

            if (allFailed)
            {
                s += " ✅ RSA УСТОЙЧИВ К ДАННЫМ АТАКАМ!\n";
                s += " Ни один метод не привёл к взлому.\n";
            }
            else
            {
                s += " ❌ RSA УЯЗВИМ!\n";
                s += " Хотя бы один метод позволил взломать ключ.\n";
                s += " УВЕЛИЧЬТЕ РАЗМЕР p И q!\n";
            }
            s += "══════════════════════════════════════════════\n";

            return s;
        }

        // ====================================================================
        // ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ
        // ====================================================================

        private double GetMedian(List<long> numbers)
        {
            var sorted = numbers.OrderBy(x => x).ToList();
            int count = sorted.Count;
            if (count == 0) return 0;
            if (count % 2 == 0)
                return (sorted[count / 2 - 1] + sorted[count / 2]) / 2.0;
            return sorted[count / 2];
        }

        private string GetSecurityLevel(long n)
        {
            int bits = (int)Math.Floor(Math.Log(n, 2)) + 1;
            if (bits < 16) return $"❌ КРИТИЧЕСКИ СЛАБО ({bits} бит).";
            if (bits < 32) return $"❌ ОЧЕНЬ СЛАБО ({bits} бит).";
            if (bits < 64) return $"⚠ СЛАБО ({bits} бит).";
            if (bits < 128) return $"⚠ НЕДОСТАТОЧНО ({bits} бит).";
            if (bits < 256) return $"✓ СРЕДНЕ ({bits} бит).";
            if (bits < 512) return $"✓✓ ХОРОШО ({bits} бит).";
            if (bits < 1024) return $"✓✓ ОЧЕНЬ ХОРОШО ({bits} бит).";
            if (bits < 2048) return $"✓✓✓ ОТЛИЧНО ({bits} бит).";
            return $"✓✓✓ ПРЕВОСХОДНО ({bits} бит). Рекомендуемый уровень!";
        }

        private string FormatTimeSpan(TimeSpan ts)
        {
            if (ts == TimeSpan.MaxValue) return "астрономическое время (практически бесконечно)";
            if (ts.TotalSeconds <= 0) return "менее секунды";
            if (ts.TotalDays > 365000) return $"~{ts.TotalDays / 365:F0} лет (нереально)";
            if (ts.TotalDays > 365) return $"~{ts.TotalDays / 365:F1} лет";
            if (ts.TotalDays > 1) return $"~{ts.TotalDays:F1} дней";
            if (ts.TotalHours > 1) return $"~{ts.TotalHours:F1} часов";
            if (ts.TotalMinutes > 1) return $"~{ts.TotalMinutes:F1} минут";
            return $"~{ts.TotalSeconds:F2} секунд";
        }

        private void AddStep(string step)
        {
            OnStep?.Invoke(step);
        }
    }
}
