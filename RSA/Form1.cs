using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RSA;

namespace RSA
{
    public partial class Form1 : Form
    {
        private RsaManager _rsaManager;
        private MenuStrip _menuStrip;
        private TextBox txtP, txtQ, txtMessage;
        private Button btnGenerateKeys, btnEncrypt, btnDecrypt;
        private Button btnGeneratePrimes, btnAutoGenerate;
        private ComboBox cmbRange;
        private RichTextBox rtbGenerationSteps, rtbEncryptionSteps, rtbDecryptionSteps;
        private Label lblPublicKey, lblPrivateKey;
        private TextBox txtEncryptedResult;
        private Label lblDecryptedResult;

        public Form1()
        {
            InitializeRsa();
            InitializeCustomComponent();
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // или FixedDialog, Fixed3D
            this.MaximizeBox = false;  // Убирает кнопку "Развернуть"
            this.MinimizeBox = true;   // Можно оставить кнопку "Свернуть" (по желанию)
        }

        private void InitializeRsa()
        {
            _rsaManager = new RsaManager();  // Это автоматически установит Instance
            _rsaManager.OnGenerationStep += AppendGenerationStep;
            _rsaManager.OnEncryptionStep += AppendEncryptionStep;
            _rsaManager.OnDecryptionStep += AppendDecryptionStep;
        }

        private void InitializeCustomComponent()
        {
            this.Text = "RSA";
            this.Size = new Size(1200, 720);
            this.Icon = new Icon("freekey.ico");
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke;

            CreateMenuStrip();

            var leftPanel = CreateLeftPanel();
            var centerPanel = CreateCenterPanel();
            var rightPanel = CreateRightPanel();

            leftPanel.Top = _menuStrip.Height + 5;
            centerPanel.Top = _menuStrip.Height + 5;
            rightPanel.Top = _menuStrip.Height + 5;

            this.Controls.Add(leftPanel);
            this.Controls.Add(centerPanel);
            this.Controls.Add(rightPanel);
        }

        private void CreateMenuStrip()
        {
            _menuStrip = new MenuStrip();
            _menuStrip.BackColor = Color.LightSteelBlue;
            _menuStrip.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Меню "Файл"
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("📁 Файл");
            ToolStripMenuItem exitItem = new ToolStripMenuItem("Выход");
            exitItem.Click += (s, e) => Application.Exit();
            fileMenu.DropDownItems.Add(exitItem);

            // Меню "Демонстрация"
            ToolStripMenuItem demoMenu = new ToolStripMenuItem("🔐 Демонстрация");

            ToolStripMenuItem scenarioItem = new ToolStripMenuItem("🎬 Обучающий сценарий");
            scenarioItem.Click += (s, e) =>
            {
                ScenarioForm scenarioForm = new ScenarioForm();
                scenarioForm.Show();
            };

            demoMenu.DropDownItems.Add(scenarioItem);

            // Меню "Теория"
            ToolStripMenuItem theoryMenu = new ToolStripMenuItem("📚 Теория");

            ToolStripMenuItem aboutRSAItem = new ToolStripMenuItem("ℹ️ О алгоритме RSA");
            aboutRSAItem.Click += (s, e) => ShowRSAAbout();

            ToolStripMenuItem mathBasisItem = new ToolStripMenuItem("🔢 Математические основы");
            mathBasisItem.Click += (s, e) => ShowMathBasis();

            theoryMenu.DropDownItems.Add(aboutRSAItem);
            theoryMenu.DropDownItems.Add(mathBasisItem);

            // Меню "Помощь"
            ToolStripMenuItem helpMenu = new ToolStripMenuItem("❓ Помощь");
            ToolStripMenuItem howToUseItem = new ToolStripMenuItem("📖 Как использовать");
            howToUseItem.Click += (s, e) => ShowHowToUse();
            helpMenu.DropDownItems.Add(howToUseItem);

            ToolStripMenuItem crackItem = new ToolStripMenuItem("🔓 Криптоанализ RSA");
            crackItem.Click += (s, e) =>
            {
                CrackForm crackForm = new CrackForm();
                crackForm.Show();
            };

            demoMenu.DropDownItems.Add(scenarioItem);
            demoMenu.DropDownItems.Add(new ToolStripSeparator());
            demoMenu.DropDownItems.Add(crackItem);  // ← добавьте эту строку

            _menuStrip.Items.Add(fileMenu);
            _menuStrip.Items.Add(demoMenu);
            _menuStrip.Items.Add(theoryMenu);
            _menuStrip.Items.Add(helpMenu);

            this.MainMenuStrip = _menuStrip;
            this.Controls.Add(_menuStrip);
        }

        private Panel CreateLeftPanel()
        {
            Panel panel = new Panel
            {
                Location = new Point(10, 10),
                Size = new Size(300, 640),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Label lblTitle = new Label
            {
                Text = "ГЕНЕРАЦИЯ КЛЮЧЕЙ",
                Location = new Point(10, 10),
                Size = new Size(280, 25),
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Автоматическая генерация
            Label lblAuto = new Label
            {
                Text = "Автоматическая генерация:",
                Location = new Point(10, 40),
                Size = new Size(280, 20),
                Font = new Font("Arial", 8, FontStyle.Bold)
            };

            cmbRange = new ComboBox
            {
                Location = new Point(10, 62),
                Size = new Size(280, 21),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            var ranges = PrimeNumberGenerator.GetRecommendedRanges();
            foreach (var range in ranges)
            {
                cmbRange.Items.Add($"{range.Key} [{range.Value.min}-{range.Value.max}]");
            }
            cmbRange.SelectedIndex = 0;

            btnAutoGenerate = new Button
            {
                Text = "🎲 Сгенерировать автоматически",
                Location = new Point(10, 90),
                Size = new Size(280, 30),
                BackColor = Color.LightSteelBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnAutoGenerate.Click += BtnAutoGenerate_Click;

            // Ручной ввод
            Label lblManual = new Label
            {
                Text = "Ручной ввод:",
                Location = new Point(10, 130),
                Size = new Size(280, 20),
                Font = new Font("Arial", 8, FontStyle.Bold)
            };

            Label lblP = new Label { Text = "p:", Location = new Point(10, 153), Size = new Size(25, 20) };
            txtP = new TextBox { Location = new Point(40, 150), Size = new Size(115, 20), Text = "61" };

            Label lblQ = new Label { Text = "q:", Location = new Point(165, 153), Size = new Size(25, 20) };
            txtQ = new TextBox { Location = new Point(190, 150), Size = new Size(100, 20), Text = "53" };

            btnGeneratePrimes = new Button
            {
                Text = "🎯 Подобрать простые числа",
                Location = new Point(10, 178),
                Size = new Size(280, 25),
                BackColor = Color.LightGoldenrodYellow,
                FlatStyle = FlatStyle.Flat
            };
            btnGeneratePrimes.Click += BtnGeneratePrimes_Click;

            btnGenerateKeys = new Button
            {
                Text = "🔑 Сгенерировать ключи",
                Location = new Point(10, 210),
                Size = new Size(280, 30),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnGenerateKeys.Click += BtnGenerateKeys_Click;

            // Ключи
            lblPublicKey = new Label
            {
                Text = "Открытый ключ: (e, n)\r\nОжидает генерации...",
                Location = new Point(10, 250),
                Size = new Size(280, 45),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightGreen
            };

            lblPrivateKey = new Label
            {
                Text = "Закрытый ключ: (d, n)\r\nОжидает генерации...",
                Location = new Point(10, 305),
                Size = new Size(280, 45),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightCoral
            };

            Button btnClear = new Button
            {
                Text = "🗑 Очистить всё",
                Location = new Point(10, 360),
                Size = new Size(280, 25),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnClear.Click += BtnClear_Click;

            panel.Controls.AddRange(new Control[] {
                lblTitle, lblAuto, cmbRange, btnAutoGenerate,
                lblManual, lblP, txtP, lblQ, txtQ,
                btnGeneratePrimes, btnGenerateKeys,
                lblPublicKey, lblPrivateKey, btnClear
            });

            return panel;
        }

        private Panel CreateCenterPanel()
        {
            Panel panel = new Panel
            {
                Location = new Point(320, 10),
                Size = new Size(430, 640),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Label lblTitle = new Label
            {
                Text = "ЭТАПЫ",
                Location = new Point(10, 10),
                Size = new Size(410, 25),
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblGen = new Label { Text = "Генерация ключей:", Location = new Point(10, 40), Size = new Size(200, 20), Font = new Font("Arial", 8, FontStyle.Bold) };
            rtbGenerationSteps = new RichTextBox
            {
                Location = new Point(10, 60),
                Size = new Size(410, 160),
                ReadOnly = true,
                BackColor = Color.AliceBlue,
                Font = new Font("Consolas", 8)
            };

            Label lblEnc = new Label { Text = "Шифрование:", Location = new Point(10, 230), Size = new Size(200, 20), Font = new Font("Arial", 8, FontStyle.Bold) };
            rtbEncryptionSteps = new RichTextBox
            {
                Location = new Point(10, 250),
                Size = new Size(410, 160),
                ReadOnly = true,
                BackColor = Color.Honeydew,
                Font = new Font("Consolas", 8)
            };

            Label lblDec = new Label { Text = "Дешифрование:", Location = new Point(10, 420), Size = new Size(200, 20), Font = new Font("Arial", 8, FontStyle.Bold) };
            rtbDecryptionSteps = new RichTextBox
            {
                Location = new Point(10, 440),
                Size = new Size(410, 180),
                ReadOnly = true,
                BackColor = Color.MistyRose,
                Font = new Font("Consolas", 8)
            };

            panel.Controls.AddRange(new Control[] {
                lblTitle, lblGen, rtbGenerationSteps,
                lblEnc, rtbEncryptionSteps,
                lblDec, rtbDecryptionSteps
            });

            return panel;
        }

        private Panel CreateRightPanel()
        {
            Panel panel = new Panel
            {
                Location = new Point(760, 10),
                Size = new Size(420, 640),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };

            Label lblTitle = new Label
            {
                Text = "ШИФРОВАНИЕ / ДЕШИФРОВАНИЕ",
                Location = new Point(10, 10),
                Size = new Size(400, 25),
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };

            Label lblMessage = new Label { Text = "Введите сообщение:", Location = new Point(10, 45), Size = new Size(200, 20) };
            txtMessage = new TextBox
            {
                Location = new Point(10, 70),
                Size = new Size(400, 50),
                Multiline = true,
                Text = "Привет, RSA!"
            };

            btnEncrypt = new Button
            {
                Text = "Зашифровать",
                Location = new Point(10, 130),
                Size = new Size(195, 30),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnEncrypt.Click += BtnEncrypt_Click;

            btnDecrypt = new Button
            {
                Text = "Расшифровать",
                Location = new Point(215, 130),
                Size = new Size(195, 30),
                BackColor = Color.LightCoral,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            btnDecrypt.Click += BtnDecrypt_Click;

            Label lblEncrypted = new Label { Text = "Зашифрованное сообщение:", Location = new Point(10, 170), Size = new Size(200, 20) };
            txtEncryptedResult = new TextBox
            {
                Location = new Point(10, 190),
                Size = new Size(400, 100),
                Multiline = true,
                ReadOnly = true
            };

            Label lblDecrypted = new Label { Text = "Расшифрованное сообщение:", Location = new Point(10, 300), Size = new Size(200, 20) };
            lblDecryptedResult = new Label
            {
                Location = new Point(10, 320),
                Size = new Size(400, 30),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.LightYellow,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            panel.Controls.AddRange(new Control[] {
                lblTitle, lblMessage, txtMessage,
                btnEncrypt, btnDecrypt,
                lblEncrypted, txtEncryptedResult,
                lblDecrypted, lblDecryptedResult
            });

            return panel;
        }

        // Обработчики кнопок
        private void BtnAutoGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                btnAutoGenerate.Enabled = false;
                btnGenerateKeys.Enabled = false;

                var ranges = PrimeNumberGenerator.GetRecommendedRanges();
                int index = cmbRange.SelectedIndex;
                long min = 0, max = 0;
                int i = 0;
                foreach (var range in ranges)
                {
                    if (i == index) { min = range.Value.min; max = range.Value.max; break; }
                    i++;
                }

                rtbGenerationSteps.Clear();
                rtbEncryptionSteps.Clear();
                rtbDecryptionSteps.Clear();
                txtEncryptedResult.Clear();
                lblDecryptedResult.Text = "";

                _rsaManager.GenerateKeysAuto(min, max);

                txtP.Text = _rsaManager.KeyPair.P.ToString();
                txtQ.Text = _rsaManager.KeyPair.Q.ToString();
                lblPublicKey.Text = $"Открытый ключ: (e, n)\r\n({_rsaManager.KeyPair.E}, {_rsaManager.KeyPair.N})";
                lblPrivateKey.Text = $"Закрытый ключ: (d, n)\r\n({_rsaManager.KeyPair.D}, {_rsaManager.KeyPair.N})";
                btnEncrypt.Enabled = true;
                btnDecrypt.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnAutoGenerate.Enabled = true;
                btnGenerateKeys.Enabled = true;
            }
        }

        private void BtnGeneratePrimes_Click(object sender, EventArgs e)
        {
            try
            {
                var pair = PrimeNumberGenerator.GeneratePrimePair(10, 200);
                txtP.Text = pair.p.ToString();
                txtQ.Text = pair.q.ToString();
                rtbGenerationSteps.Clear();
                rtbGenerationSteps.AppendText($"Подобраны простые числа:\r\np = {pair.p}\r\nq = {pair.q}\r\n\r\nНажмите 'Сгенерировать ключи'.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGenerateKeys_Click(object sender, EventArgs e)
        {
            try
            {
                rtbGenerationSteps.Clear();
                rtbEncryptionSteps.Clear();
                rtbDecryptionSteps.Clear();
                txtEncryptedResult.Clear();
                lblDecryptedResult.Text = "";

                if (!long.TryParse(txtP.Text, out long p) || !long.TryParse(txtQ.Text, out long q))
                {
                    MessageBox.Show("Введите корректные числа!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                _rsaManager.GenerateKeys(p, q);
                lblPublicKey.Text = $"Открытый ключ: (e, n)\r\n({_rsaManager.KeyPair.E}, {_rsaManager.KeyPair.N})";
                lblPrivateKey.Text = $"Закрытый ключ: (d, n)\r\n({_rsaManager.KeyPair.D}, {_rsaManager.KeyPair.N})";
                btnEncrypt.Enabled = true;
                btnDecrypt.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnEncrypt.Enabled = false;
                btnDecrypt.Enabled = false;
            }
        }

        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                rtbEncryptionSteps.Clear();
                txtEncryptedResult.Clear();
                lblDecryptedResult.Text = "";
                btnDecrypt.Enabled = false;

                if (string.IsNullOrEmpty(txtMessage.Text))
                {
                    MessageBox.Show("Введите сообщение!", "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var encrypted = _rsaManager.Encrypt(txtMessage.Text);
                if (encrypted.Count > 0)
                {
                    txtEncryptedResult.Text = string.Join(", ", encrypted);
                    btnDecrypt.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnDecrypt.Enabled = false;
            }
        }

        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                rtbDecryptionSteps.Clear();
                string decrypted = _rsaManager.Decrypt();
                lblDecryptedResult.Text = string.IsNullOrEmpty(decrypted) ? "(пустое сообщение)" : decrypted;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblDecryptedResult.Text = "Ошибка дешифрования";
            }
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            rtbGenerationSteps.Clear();
            rtbEncryptionSteps.Clear();
            rtbDecryptionSteps.Clear();
            txtEncryptedResult.Clear();
            lblDecryptedResult.Text = "";
            lblPublicKey.Text = "Открытый ключ: (e, n)\r\nОжидает генерации...";
            lblPrivateKey.Text = "Закрытый ключ: (d, n)\r\nОжидает генерации...";
            btnEncrypt.Enabled = false;
            btnDecrypt.Enabled = false;
        }

        // Обновление логов
        private void AppendGenerationStep(string step)
        {
            if (rtbGenerationSteps.InvokeRequired)
            {
                rtbGenerationSteps.Invoke(new Action<string>(AppendGenerationStep), step);
                return;
            }
            rtbGenerationSteps.AppendText(step + "\n\n");
            rtbGenerationSteps.ScrollToCaret();
        }

        private void AppendEncryptionStep(string step)
        {
            if (rtbEncryptionSteps.InvokeRequired)
            {
                rtbEncryptionSteps.Invoke(new Action<string>(AppendEncryptionStep), step);
                return;
            }
            rtbEncryptionSteps.AppendText(step + "\n\n");
            rtbEncryptionSteps.ScrollToCaret();
        }

        private void AppendDecryptionStep(string step)
        {
            if (rtbDecryptionSteps.InvokeRequired)
            {
                rtbDecryptionSteps.Invoke(new Action<string>(AppendDecryptionStep), step);
                return;
            }
            rtbDecryptionSteps.AppendText(step + "\n\n");
            rtbDecryptionSteps.ScrollToCaret();
        }

        // Теория и помощь
        private void ShowRSAAbout()
        {
            MessageBox.Show(
                "RSA (Rivest-Shamir-Adleman) - криптографический алгоритм с открытым ключом.\n\n" +
                "ПРИНЦИП РАБОТЫ:\n" +
                "1. Выбираются два простых числа p и q\n" +
                "2. Вычисляется n = p × q\n" +
                "3. Вычисляется φ(n) = (p-1)(q-1)\n" +
                "4. Выбирается e, взаимно простое с φ(n)\n" +
                "5. Вычисляется d = e⁻¹ mod φ(n)\n\n" +
                "Шифрование: C = M^e mod n\n" +
                "Дешифрование: M = C^d mod n",
                "О алгоритме RSA", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowMathBasis()
        {
            MessageBox.Show(
                "ПРОСТЫЕ ЧИСЛА: числа, имеющие ровно два делителя (1 и само себя)\n\n" +
                "ФУНКЦИЯ ЭЙЛЕРА φ(n): количество чисел меньше n, взаимно простых с n\n" +
                "Для простого p: φ(p) = p-1\n" +
                "Для p×q: φ(p×q) = (p-1)(q-1)\n\n" +
                "МОДУЛЬНАЯ АРИФМЕТИКА: a ≡ b (mod n)\n\n" +
                "АЛГОРИТМ ЕВКЛИДА: нахождение НОД и обратного числа по модулю",
                "Математические основы", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ShowHowToUse()
        {
            MessageBox.Show(
                "1. ГЕНЕРАЦИЯ КЛЮЧЕЙ:\n" +
                "   • Введите два простых числа (p и q)\n" +
                "   • Нажмите 'Сгенерировать ключи'\n\n" +
                "2. ШИФРОВАНИЕ:\n" +
                "   • Введите сообщение\n" +
                "   • Нажмите 'Зашифровать'\n\n" +
                "3. ДЕШИФРОВАНИЕ:\n" +
                "   • Нажмите 'Расшифровать'\n\n" +
                "4. СЦЕНАРИЙ:\n" +
                "   • Меню 'Демонстрация' → 'Обучающий сценарий'",
                "Как использовать", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
