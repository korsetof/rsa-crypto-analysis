using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSA
{
    public partial class CrackForm : Form
    {
        private RsaCrackAnalyzer _analyzer;
        private RichTextBox _rtbOutput;
        private Label _lblStatus;
        private Label _lblKeyInfo;
        private Button _btnBruteForce;
        private Button _btnFactorization;
        private Button _btnAnalysis;
        private Button _btnAll;
        private Button _btnImportCurrent;
        private DataGridView _dgvComparison;
        private TabControl _tabControl;

        public CrackForm()
        {
            InitializeForm();
            _analyzer = new RsaCrackAnalyzer();
            _analyzer.OnStep += AddOutput;
            TryImportKeys();
            this.Icon = new Icon("freekey.ico");
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // или FixedDialog, Fixed3D
            this.MaximizeBox = false;  // Убирает кнопку "Развернуть"
            this.MinimizeBox = true;   // Можно оставить кнопку "Свернуть" (по желанию)
        }

        private void InitializeForm()
        {
            this.Text = "Криптоанализ RSA";
            this.Size = new Size(1000, 720);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.White;

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "КРИПТОАНАЛИЗ RSA: Проверка стойкости ключей",
                Font = new Font("Arial", 14, FontStyle.Bold),
                ForeColor = Color.DarkRed,
                Location = new Point(10, 10),
                Size = new Size(970, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Информация о ключах
            _lblKeyInfo = new Label
            {
                Text = "Ключи не загружены. Нажмите 'Взять ключи'",
                Location = new Point(10, 45),
                Size = new Size(970, 50),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow,
                Font = new Font("Consolas", 9)
            };

            // Панель кнопок
            Panel btnPanel = new Panel
            {
                Location = new Point(10, 100),
                Size = new Size(970, 35),
                BackColor = Color.WhiteSmoke
            };

            _btnImportCurrent = new Button
            {
                Text = "📥 Взять ключи",
                Location = new Point(5, 3),
                Size = new Size(140, 28),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            _btnImportCurrent.Click += (s, e) => TryImportKeys();

            _btnBruteForce = new Button
            {
                Text = "1. Перебор d",
                Location = new Point(150, 3),
                Size = new Size(130, 28),
                BackColor = Color.LightCoral,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _btnBruteForce.Click += (s, e) => RunBruteForce();

            _btnFactorization = new Button
            {
                Text = "2. Факторизация n",
                Location = new Point(285, 3),
                Size = new Size(140, 28),
                BackColor = Color.LightSalmon,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _btnFactorization.Click += (s, e) => RunFactorization();

            _btnAnalysis = new Button
            {
                Text = "3. Анализ сообщения",
                Location = new Point(430, 3),
                Size = new Size(150, 28),
                BackColor = Color.LightYellow,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            _btnAnalysis.Click += (s, e) => RunAnalysis();

            _btnAll = new Button
            {
                Text = "▶ Все тесты",
                Location = new Point(585, 3),
                Size = new Size(120, 28),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Arial", 9, FontStyle.Bold),
                Enabled = false
            };
            _btnAll.Click += (s, e) => RunAllTests();

            Button btnClear = new Button
            {
                Text = "🗑 Очистить",
                Location = new Point(710, 3),
                Size = new Size(120, 28),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnClear.Click += (s, e) => _rtbOutput.Clear();

            btnPanel.Controls.AddRange(new Control[] {
                _btnImportCurrent, _btnBruteForce, _btnFactorization,
                _btnAnalysis, _btnAll, btnClear
            });

            _lblStatus = new Label
            {
                Text = "Готов к работе. Загрузите ключи и выберите метод анализа.",
                Location = new Point(10, 140),
                Size = new Size(970, 20),
                ForeColor = Color.Gray
            };

            // Вкладки
            _tabControl = new TabControl
            {
                Location = new Point(10, 165),
                Size = new Size(970, 500)
            };

            // Вкладка 1: Результаты анализа
            TabPage resultsTab = new TabPage("📊 Результаты анализа");
            _rtbOutput = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.LimeGreen,
                Font = new Font("Consolas", 9)
            };
            resultsTab.Controls.Add(_rtbOutput);

            // Вкладка 2: Таблица сравнения стойкости
            TabPage comparisonTab = new TabPage("📈 Сравнение стойкости");

            _dgvComparison = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                BackgroundColor = Color.White,
                RowHeadersVisible = false
            };
            comparisonTab.Controls.Add(_dgvComparison);

            _tabControl.TabPages.Add(resultsTab);
            _tabControl.TabPages.Add(comparisonTab);

            this.Controls.AddRange(new Control[] {
                lblTitle, _lblKeyInfo, btnPanel, _lblStatus, _tabControl
            });

            // Создаём таблицу ПОСЛЕ добавления всего на форму
            CreateComparisonTable();
        }

        private void CreateComparisonTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("Категория");
            dt.Columns.Add("Бит");
            dt.Columns.Add("Пример n");
            dt.Columns.Add("Цифр");
            dt.Columns.Add("Время перебора d");
            dt.Columns.Add("Время факторизации");
            dt.Columns.Add("Безопасность");
            dt.Columns.Add("Статус");

            dt.Rows.Add("Учебный", "8", "~256", "3", "0.0001 сек", "0.0001 сек", "❌ Никакой", "НЕДОПУСТИМО");
            dt.Rows.Add("Учебный", "12", "~4 000", "4", "0.001 сек", "0.001 сек", "❌ Никакой", "НЕДОПУСТИМО");
            dt.Rows.Add("Учебный", "16", "~65 000", "5", "0.05 сек", "0.01 сек", "❌ Критически слабо", "НЕДОПУСТИМО");
            dt.Rows.Add("Слабый", "20", "~1 000 000", "7", "1 сек", "0.1 сек", "❌ Очень слабо", "НЕДОПУСТИМО");
            dt.Rows.Add("Слабый", "24", "~16 млн", "8", "15 сек", "1 сек", "❌ Слабо", "НЕ РЕКОМЕНДУЕТСЯ");
            dt.Rows.Add("Слабый", "28", "~268 млн", "9", "4 мин", "10 сек", "❌ Слабо", "НЕ РЕКОМЕНДУЕТСЯ");
            dt.Rows.Add("Устаревший", "32", "~4 млрд", "10", "1 час", "5 мин", "⚠ Слабо", "УСТАРЕЛО");
            dt.Rows.Add("Устаревший", "40", "~1 трлн", "13", "12 дней", "2 часа", "⚠ Слабо", "УСТАРЕЛО");
            dt.Rows.Add("Устаревший", "48", "~281 трлн", "15", "9 лет", "3 дня", "⚠ Недостаточно", "УСТАРЕЛО");
            dt.Rows.Add("Средний", "56", "~72 квдр", "17", "2000 лет", "1 год", "⚠ Средне", "НЕ РЕКОМЕНДУЕТСЯ");
            dt.Rows.Add("Средний", "64", "~18 квнт", "20", "500 000 лет", "300 лет", "✓ Средне", "МИНИМУМ");
            dt.Rows.Add("Хороший", "128", "~10^38", "39", "10^18 лет", "10^9 лет", "✓✓ Хорошо", "РЕКОМЕНДУЕТСЯ");
            dt.Rows.Add("Хороший", "256", "~10^77", "78", "10^57 лет", "10^38 лет", "✓✓✓ Отлично", "РЕКОМЕНДУЕТСЯ");
            dt.Rows.Add("Стандарт", "512", "~10^154", "155", "10^134 лет", "10^77 лет", "🔒 Отлично", "СТАНДАРТ");
            dt.Rows.Add("Стандарт", "1024", "~10^308", "309", "10^288 лет", "10^154 лет", "🔒 Превосходно", "СТАНДАРТ");
            dt.Rows.Add("Стандарт", "2048", "~10^616", "617", "10^596 лет", "10^308 лет", "🔒🔒 Идеально", "РЕКОМЕНДУЕТСЯ");
            dt.Rows.Add("Максимум", "4096", "~10^1233", "1234", "10^1213 лет", "10^616 лет", "🔒🔒🔒 Абсолютно", "ИЗБЫТОЧНО");

            _dgvComparison.DataSource = dt;

            // Форматирование таблицы
            _dgvComparison.CellFormatting += (s, e) =>
            {
                if (e.RowIndex >= 0 && e.ColumnIndex == 7) // Статус
                {
                    string status = e.Value?.ToString() ?? "";
                    if (status.Contains("НЕДОПУСТИМО"))
                        e.CellStyle.BackColor = Color.LightPink;
                    else if (status.Contains("НЕ РЕКОМЕНДУЕТСЯ"))
                        e.CellStyle.BackColor = Color.MistyRose;
                    else if (status.Contains("УСТАРЕЛО"))
                        e.CellStyle.BackColor = Color.LightYellow;
                    else if (status.Contains("РЕКОМЕНДУЕТСЯ") || status.Contains("СТАНДАРТ") || status.Contains("МИНИМУМ"))
                        e.CellStyle.BackColor = Color.LightGreen;
                    else if (status.Contains("ИЗБЫТОЧНО"))
                        e.CellStyle.BackColor = Color.LightBlue;
                }

                if (e.RowIndex >= 0 && e.ColumnIndex == 6) // Безопасность
                {
                    string level = e.Value?.ToString() ?? "";
                    if (level.Contains("❌"))
                        e.CellStyle.ForeColor = Color.Red;
                    else if (level.Contains("⚠"))
                        e.CellStyle.ForeColor = Color.OrangeRed;
                    else
                        e.CellStyle.ForeColor = Color.DarkGreen;
                }
            };

            // Подпись под таблицей
            Label lblLegend = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 40,
                BackColor = Color.WhiteSmoke,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Arial", 8)
            };

            // Добавляем легенду в ту же вкладку
            TabPage comparisonTab = _tabControl.TabPages[1];
            comparisonTab.Controls.Add(lblLegend);
        }

        private void TryImportKeys()
        {
            try
            {
                var manager = RsaManager.Instance;

                if (manager == null || manager.KeyPair == null)
                {
                    _lblKeyInfo.Text = "❌ Ключи не сгенерированы! Сначала сгенерируйте ключи в основном окне (p=61, q=53).";
                    _lblKeyInfo.BackColor = Color.LightPink;
                    EnableButtons(false);
                    return;
                }

                var kp = manager.KeyPair;
                int bits = (int)Math.Floor(Math.Log(kp.N, 2)) + 1;

                _lblKeyInfo.Text = $"✅ КЛЮЧИ ЗАГРУЖЕНЫ | p={kp.P}, q={kp.Q} | n={kp.N} | e={kp.E} | d={kp.D} | Длина: {bits} бит";
                _lblKeyInfo.BackColor = Color.LightGreen;

                EnableButtons(true);
                _lblStatus.Text = $"Ключи загружены ({bits} бит). Выберите метод анализа.";

                AddOutput("\n╔══════════════════════════════════════╗");
                AddOutput("║   КЛЮЧИ УСПЕШНО ИМПОРТИРОВАНЫ     ║");
                AddOutput("╚══════════════════════════════════════╝");
                AddOutput($"  p = {kp.P} (простое: {RsaMath.IsPrime(kp.P)})");
                AddOutput($"  q = {kp.Q} (простое: {RsaMath.IsPrime(kp.Q)})");
                AddOutput($"  n = p × q = {kp.N}");
                AddOutput($"  Длина ключа: {bits} бит");
                AddOutput("");

                if (bits < 32)
                    AddOutput("  ⚠ Ключ в КРАСНОЙ зоне! Абсолютно небезопасен!");
                else if (bits < 128)
                    AddOutput("  ⚠ Ключ в ЖЁЛТОЙ зоне. Недостаточно надёжен.");
                else
                    AddOutput("  ✓ Ключ в ЗЕЛЁНОЙ зоне. Надёжен.");
                AddOutput("");
            }
            catch (Exception ex)
            {
                _lblKeyInfo.Text = $"❌ Ошибка: {ex.Message}";
                _lblKeyInfo.BackColor = Color.LightPink;
                EnableButtons(false);
            }
        }

        private void EnableButtons(bool enable)
        {
            _btnBruteForce.Enabled = enable;
            _btnFactorization.Enabled = enable;
            _btnAnalysis.Enabled = enable;
            _btnAll.Enabled = enable;
        }

        private RsaKeyPair GetCurrentKeys()
        {
            var manager = RsaManager.Instance;
            if (manager == null || manager.KeyPair == null)
                throw new InvalidOperationException("Ключи не сгенерированы!");
            return manager.KeyPair;
        }

        private async void RunBruteForce()
        {
            try
            {
                var keys = GetCurrentKeys();
                EnableButtons(false);
                _rtbOutput.Clear();
                _tabControl.SelectedIndex = 0;
                _lblStatus.Text = "🔍 Выполняется полный перебор d...";

                await System.Threading.Tasks.Task.Run(() =>
                {
                    _analyzer.BruteForcePrivateKey(keys.E, keys.N);
                });

                _lblStatus.Text = "✅ Полный перебор завершён";
                AddComparisonToOutput("перебора");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { EnableButtons(true); }
        }

        private async void RunFactorization()
        {
            try
            {
                var keys = GetCurrentKeys();
                EnableButtons(false);
                _rtbOutput.Clear();
                _tabControl.SelectedIndex = 0;
                _lblStatus.Text = "🔍 Выполняется факторизация n...";

                await System.Threading.Tasks.Task.Run(() =>
                {
                    _analyzer.FactorizeN(keys.N);
                });

                _lblStatus.Text = "✅ Факторизация завершена";
                AddComparisonToOutput("факторизации");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { EnableButtons(true); }
        }

        private async void RunAnalysis()
        {
            try
            {
                var keys = GetCurrentKeys();
                EnableButtons(false);
                _rtbOutput.Clear();
                _tabControl.SelectedIndex = 0;
                _lblStatus.Text = "🔍 Выполняется анализ сообщения...";

                var manager = RsaManager.Instance;
                var encrypted = manager.Encrypt("HELLO");

                await System.Threading.Tasks.Task.Run(() =>
                {
                    _analyzer.AnalyzeEncryptedMessage(encrypted, keys.E, keys.N);
                });

                _lblStatus.Text = "✅ Анализ завершён";
                AddComparisonToOutput("анализа");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { EnableButtons(true); }
        }

        private async void RunAllTests()
        {
            try
            {
                var keys = GetCurrentKeys();
                EnableButtons(false);
                _rtbOutput.Clear();
                _tabControl.SelectedIndex = 0;
                _lblStatus.Text = "🔍 Запущены ВСЕ тесты...";

                var manager = RsaManager.Instance;
                var encrypted = manager.Encrypt("TEST");

                await System.Threading.Tasks.Task.Run(() =>
                {
                    _analyzer.BruteForcePrivateKey(keys.E, keys.N);
                    _analyzer.FactorizeN(keys.N);
                    _analyzer.AnalyzeEncryptedMessage(encrypted, keys.E, keys.N);
                    AddOutput(_analyzer.GetSummary());
                });

                _lblStatus.Text = "✅ Все тесты завершены!";
                AddComparisonToOutput("всех тестов");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { EnableButtons(true); }
        }

        private void AddComparisonToOutput(string testName)
        {
            try
            {
                var keys = GetCurrentKeys();
                int bits = (int)Math.Floor(Math.Log(keys.N, 2)) + 1;

                AddOutput("\n╔══════════════════════════════════════╗");
                AddOutput("║   СРАВНЕНИЕ С ТАБЛИЦЕЙ СТОЙКОСТИ   ║");
                AddOutput("╚══════════════════════════════════════╝");
                AddOutput($"  Ваш ключ: {bits} бит (n = {keys.N})");
                AddOutput($"  Возраст Вселенной: 13 800 000 000 лет");
                AddOutput("");

                if (bits < 32)
                    AddOutput("  🔴 Ключ СЛИШКОМ короткий! Увеличьте p и q!");
                else if (bits < 128)
                    AddOutput("  🟡 Ключ недостаточно длинный. Рекомендуется усилить.");
                else
                    AddOutput("  🟢 Ключ достаточно надёжен.");

                AddOutput("  📊 Подробнее на вкладке 'Сравнение стойкости'");
                AddOutput("");
            }
            catch { /* игнорируем */ }
        }

        private void AddOutput(string text)
        {
            if (_rtbOutput.InvokeRequired)
            {
                _rtbOutput.Invoke(new Action<string>(AddOutput), text);
                return;
            }
            _rtbOutput.AppendText(text + "\n");
            _rtbOutput.ScrollToCaret();
        }

        private void CrackForm_Load(object sender, EventArgs e)
        {

        }
    }
}
