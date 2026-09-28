using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RSA
{
    public class ScenarioForm : Form
    {
        private RsaManager _rsaManager;
        private Timer _animationTimer;
        private List<Character> _characters;
        private List<Arrow> _arrows;
        private List<MessageBlock> _messageBlocks;
        private string _originalMessage;
        private List<long> _encryptedMessage;
        private string _decryptedMessage;
        private Label _lblInfo;
        private RichTextBox _rtbDetails;

        private enum ScenarioState
        {
            Idle,
            GeneratingKeys,
            ShowingPublicKey,
            Encrypting,
            SendingEncrypted,
            ShowingEncrypted,
            SendingToRecipient,
            Decrypting,
            ShowingResult,
            Complete
        }

        private ScenarioState _currentState = ScenarioState.Idle;

        public ScenarioForm()
        {
            InitializeScenario();
            SetupUI();
            this.BackColor = Color.White;
            this.DoubleBuffered = true;
            this.Icon = new Icon("freekey.ico");
            this.FormBorderStyle = FormBorderStyle.FixedSingle; // или FixedDialog, Fixed3D
            this.MaximizeBox = false;  // Убирает кнопку "Развернуть"
            this.MinimizeBox = true;   // Можно оставить кнопку "Свернуть" (по желанию)
        }

        private void InitializeScenario()
        {
            _rsaManager = new RsaManager();

            // Только 3 персонажа: Отправитель, Посредник, Получатель
            _characters = new List<Character>
            {
                new Character("Артём", "Отправитель", new Point(80, 150), Color.LightBlue),
                new Character("Максим", "Посредник", new Point(380, 150), Color.LightGreen),
                new Character("Анна", "Получатель", new Point(680, 150), Color.LightGoldenrodYellow)
            };

            _arrows = new List<Arrow>();
            _messageBlocks = new List<MessageBlock>();
            _originalMessage = "Пара в 19:00";
        }

        private void SetupUI()
        {
            this.Text = "RSA - Обучающий сценарий";
            this.Size = new Size(1000, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;
            this.DoubleBuffered = true;

            // Панель управления снизу
            Panel controlPanel = new Panel
            {
                Location = new Point(10, 520),
                Size = new Size(965, 130),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.WhiteSmoke
            };

            // Поле для сообщения
            Label lblMessage = new Label
            {
                Text = "Сообщение:",
                Location = new Point(10, 15),
                Size = new Size(80, 20)
            };
            TextBox txtMessage = new TextBox
            {
                Location = new Point(95, 12),
                Size = new Size(200, 20),
                Text = "Пара в 19:00"
            };

            // Кнопка запуска
            Button btnStart = new Button
            {
                Text = "▶ Начать",
                Location = new Point(310, 10),
                Size = new Size(120, 35),
                BackColor = Color.LightGreen,
                FlatStyle = FlatStyle.Flat
            };
            btnStart.Click += (s, e) => StartScenario(txtMessage.Text);

            // Кнопка далее (пошаговый режим)
            Button btnNext = new Button
            {
                Text = "⏭ Далее",
                Location = new Point(440, 10),
                Size = new Size(120, 35),
                BackColor = Color.LightBlue,
                FlatStyle = FlatStyle.Flat
            };
            btnNext.Click += (s, e) => AdvanceScenario();

            // Кнопка авто
            Button btnAuto = new Button
            {
                Text = "⏩ Авто",
                Location = new Point(570, 10),
                Size = new Size(120, 35),
                BackColor = Color.LightYellow,
                FlatStyle = FlatStyle.Flat
            };
            btnAuto.Click += (s, e) => AutoPlayScenario(txtMessage.Text);

            // Кнопка сброса
            Button btnReset = new Button
            {
                Text = "↺ Сброс",
                Location = new Point(700, 10),
                Size = new Size(120, 35),
                BackColor = Color.LightGray,
                FlatStyle = FlatStyle.Flat
            };
            btnReset.Click += (s, e) => ResetScenario();

            // Информационная панель
            _lblInfo = new Label
            {
                Location = new Point(10, 55),
                Size = new Size(945, 30),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter,
                Text = "Нажмите 'Начать' для запуска сценария"
            };

            // Детали расшифровки
            _rtbDetails = new RichTextBox
            {
                Location = new Point(10, 90),
                Size = new Size(945, 30),
                ReadOnly = true,
                Font = new Font("Consolas", 8),
                BackColor = Color.White
            };

            controlPanel.Controls.AddRange(new Control[] {
                lblMessage, txtMessage, btnStart, btnNext, btnAuto, btnReset,
                _lblInfo, _rtbDetails
            });
            this.Controls.Add(controlPanel);

            // Таймер для авторежима
            _animationTimer = new Timer { Interval = 2500 };
            _animationTimer.Tick += (s, e) =>
            {
                AdvanceScenario();
                if (_currentState == ScenarioState.Complete)
                    _animationTimer.Stop();
            };
        }

        private void StartScenario(string message)
        {
            _originalMessage = message;
            ResetScenario();
            _rsaManager.GenerateKeys(61, 53);

            _currentState = ScenarioState.ShowingPublicKey;

            _lblInfo.Text = "🔑 Сгенерированы ключи. Нажмите 'Далее'";
            _rtbDetails.Text = $"Открытый ключ: (e={_rsaManager.KeyPair.E}, n={_rsaManager.KeyPair.N})\n" +
                               $"Закрытый ключ: (d={_rsaManager.KeyPair.D}, n={_rsaManager.KeyPair.N})";

            Invalidate();
        }

        private void AutoPlayScenario(string message)
        {
            StartScenario(message);
            _animationTimer.Start();
        }

        private void ResetScenario()
        {
            _animationTimer.Stop();
            _currentState = ScenarioState.Idle;
            _arrows.Clear();
            _messageBlocks.Clear();

            foreach (var character in _characters)
            {
                character.CurrentMessage = "";
                character.HasMessage = false;
                character.IsActive = false;
            }

            _lblInfo.Text = "Нажмите 'Начать' для запуска сценария";
            _rtbDetails.Text = "";
            Invalidate();
        }

        private void AdvanceScenario()
        {
            switch (_currentState)
            {
                case ScenarioState.ShowingPublicKey: ShowPublicKeyStep(); break;
                case ScenarioState.Encrypting: EncryptingStep(); break;
                case ScenarioState.SendingEncrypted: SendEncryptedStep(); break;
                case ScenarioState.ShowingEncrypted: ShowEncryptedStep(); break;
                case ScenarioState.SendingToRecipient: SendToRecipientStep(); break;
                case ScenarioState.Decrypting: DecryptingStep(); break;
                case ScenarioState.ShowingResult: ShowResultStep(); break;
            }
            Invalidate();
        }

        private void ShowPublicKeyStep()
        {
            _characters[0].IsActive = true;
            _characters[0].CurrentMessage = $"Хочу отправить:\n\"{_originalMessage}\"";

            AddArrow(_characters[0].Center, _characters[1].Center, "Публичный ключ (e,n)", Color.Green);

            _lblInfo.Text = "Шаг 1: Артём хочет отправить сообщение. Публичный ключ доступен всем.";
            _rtbDetails.Text = $"Публичный ключ: e={_rsaManager.KeyPair.E}, n={_rsaManager.KeyPair.N}\n" +
                               $"Этот ключ используется для шифрования.";

            _currentState = ScenarioState.Encrypting;
        }

        private void EncryptingStep()
        {
            _encryptedMessage = _rsaManager.Encrypt(_originalMessage);
            _characters[0].CurrentMessage = $"Шифрую...\n{string.Join(",", _encryptedMessage.Take(3))}...";

            string details = "Процесс шифрования (C = M^e mod n):\n";
            for (int i = 0; i < Math.Min(_originalMessage.Length, 5); i++)
            {
                details += $"'{_originalMessage[i]}' (ASCII {((int)_originalMessage[i])}) → {_encryptedMessage[i]}\n";
            }
            if (_originalMessage.Length > 5) details += "...\n";

            _lblInfo.Text = "Шаг 2: Артём шифрует сообщение публичным ключом";
            _rtbDetails.Text = details;

            _currentState = ScenarioState.SendingEncrypted;
        }

        private void SendEncryptedStep()
        {
            _arrows.Clear();
            _characters[0].IsActive = false;
            _characters[1].IsActive = true;
            _characters[1].CurrentMessage = "Получил шифр.\nНе могу прочитать!";

            AddArrow(_characters[0].Center, _characters[1].Center, "Зашифрованное сообщение", Color.Red);

            _lblInfo.Text = "Шаг 3: Максим получает шифр, но НЕ может расшифровать (нет закрытого ключа)";
            _rtbDetails.Text = $"Шифр: [{string.Join(", ", _encryptedMessage)}]\n" +
                               "Максим видит только набор чисел!\n" +
                               "Без закрытого ключа d расшифровка невозможна.";

            _currentState = ScenarioState.ShowingEncrypted;
        }

        private void ShowEncryptedStep()
        {
            _characters[1].CurrentMessage = "Пересылаю дальше...\nВсё ещё шифр!";

            _lblInfo.Text = "Шаг 4: Максим передаёт шифр дальше. Сообщение остаётся защищённым.";
            _rtbDetails.Text = "Даже если кто-то перехватит сообщение,\n" +
                               "без закрытого ключа d его не расшифровать!";

            _currentState = ScenarioState.SendingToRecipient;
        }

        private void SendToRecipientStep()
        {
            _arrows.Clear();
            _characters[1].IsActive = false;
            _characters[2].IsActive = true;
            _characters[2].CurrentMessage = "Получила!\nРасшифровываю...";

            AddArrow(_characters[1].Center, _characters[2].Center, "Доставка получателю", Color.Purple);

            _lblInfo.Text = "Шаг 5: Анна получает шифр. У неё есть ЗАКРЫТЫЙ ключ d!";
            _rtbDetails.Text = $"Закрытый ключ Анны: d={_rsaManager.KeyPair.D}\n" +
                               "Только она может расшифровать сообщение!";

            _currentState = ScenarioState.Decrypting;
        }

        private void DecryptingStep()
        {
            _decryptedMessage = _rsaManager.Decrypt();

            string details = "Процесс расшифровки (M = C^d mod n):\n";
            var encrypted = _encryptedMessage;
            for (int i = 0; i < Math.Min(_decryptedMessage.Length, 5); i++)
            {
                details += $"{encrypted[i]} → '{(char)(_rsaManager.KeyPair.D > 0 ? _decryptedMessage[i] : '?')}' (ASCII {(int)_decryptedMessage[i]})\n";
            }
            if (_decryptedMessage.Length > 5) details += "...\n";

            _lblInfo.Text = "Шаг 6: Анна расшифровывает сообщение закрытым ключом!";
            _rtbDetails.Text = details;

            _currentState = ScenarioState.ShowingResult;
        }

        private void ShowResultStep()
        {
            _arrows.Clear();
            _characters[2].CurrentMessage = $"Готово!\n\"{_decryptedMessage}\"";

            bool success = _originalMessage == _decryptedMessage;

            _lblInfo.Text = success ? "✅ УСПЕХ! Сообщение расшифровано верно!" : "❌ ОШИБКА! Сообщения не совпадают!";
            _rtbDetails.Text = $"Исходное сообщение: \"{_originalMessage}\"\n" +
                               $"Расшифрованное: \"{_decryptedMessage}\"\n" +
                               $"Результат: {(success ? "СОВПАДАЕТ ✓" : "НЕ СОВПАДАЕТ ✗")}";

            _currentState = ScenarioState.Complete;
        }

        private void AddArrow(Point from, Point to, string label, Color color)
        {
            _arrows.Add(new Arrow { From = from, To = to, Label = label, Color = color });
        }

        // ОТРИСОВКА
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Заголовок
            using (Font font = new Font("Arial", 16, FontStyle.Bold))
            {
                g.DrawString("RSA: Передача зашифрованного сообщения", font,
                    Brushes.DarkBlue, 200, 15);
            }

            // Схема
            DrawArrows(g);
            DrawCharacters(g);
        }

        private void DrawCharacters(Graphics g)
        {
            foreach (var ch in _characters)
            {
                Rectangle rect = ch.Bounds;

                // Подсветка активного
                if (ch.IsActive)
                {
                    using (Pen pen = new Pen(Color.Gold, 4))
                        g.DrawRectangle(pen, rect.X - 3, rect.Y - 3, rect.Width + 6, rect.Height + 6);
                }

                // Голова
                g.FillEllipse(new SolidBrush(ch.Color), rect.X + 35, rect.Y + 10, 50, 50);
                // Тело
                g.FillRectangle(new SolidBrush(ch.Color), rect.X + 40, rect.Y + 65, 40, 50);
                // Глаза
                g.FillEllipse(Brushes.Black, rect.X + 45, rect.Y + 25, 8, 8);
                g.FillEllipse(Brushes.Black, rect.X + 67, rect.Y + 25, 8, 8);
                // Улыбка
                g.DrawArc(Pens.Black, rect.X + 50, rect.Y + 35, 20, 15, 0, 180);

                // Имя
                using (Font font = new Font("Arial", 9, FontStyle.Bold))
                    g.DrawString(ch.Name, font, Brushes.Black, rect.X + 30, rect.Y + 120);
                // Роль
                using (Font font = new Font("Arial", 7))
                    g.DrawString(ch.Role, font, Brushes.Gray, rect.X + 20, rect.Y + 135);

                // Облачко с сообщением
                if (!string.IsNullOrEmpty(ch.CurrentMessage))
                {
                    Rectangle bubble = new Rectangle(rect.X + rect.Width + 5, rect.Y, 160, 50);
                    g.FillRectangle(new SolidBrush(Color.FromArgb(230, 255, 255, 200)), bubble);
                    g.DrawRectangle(Pens.Gray, bubble);
                    using (Font font = new Font("Arial", 7))
                        g.DrawString(ch.CurrentMessage, font, Brushes.Black, bubble.X + 5, bubble.Y + 5);
                }
            }
        }

        private void DrawArrows(Graphics g)
        {
            foreach (var arrow in _arrows)
            {
                using (Pen pen = new Pen(arrow.Color, 3))
                {
                    pen.EndCap = LineCap.ArrowAnchor;
                    g.DrawLine(pen, arrow.From, arrow.To);
                }

                // Подпись
                Point mid = new Point((arrow.From.X + arrow.To.X) / 2, (arrow.From.Y + arrow.To.Y) / 2 - 20);
                using (Font font = new Font("Arial", 8))
                {
                    SizeF sz = g.MeasureString(arrow.Label, font);
                    g.FillRectangle(Brushes.White, mid.X - sz.Width / 2 - 3, mid.Y, sz.Width + 6, sz.Height);
                    g.DrawString(arrow.Label, font, Brushes.Black, mid.X - sz.Width / 2, mid.Y);
                }
            }
        }

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScenarioForm));
            this.SuspendLayout();
            // 
            // ScenarioForm
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ScenarioForm";
            this.Load += new System.EventHandler(this.ScenarioForm_Load);
            this.ResumeLayout(false);

        }

        private void ScenarioForm_Load(object sender, EventArgs e)
        {

        }
    }

    // Вспомогательные классы
    public class Arrow
    {
        public Point From { get; set; }
        public Point To { get; set; }
        public string Label { get; set; }
        public Color Color { get; set; }
    }

    public class MessageBlock
    {
        public string Author { get; set; }
        public string Text { get; set; }
        public Point Location { get; set; }
        public Color Color { get; set; }
        public DateTime CreationTime { get; set; }
    }

    public class Character
    {
        public string Name { get; set; }
        public string Role { get; set; }
        public Point Position { get; set; }
        public Size Size { get; set; }
        public Color Color { get; set; }
        public string CurrentMessage { get; set; }
        public bool HasMessage { get; set; }
        public bool IsActive { get; set; }

        public Character(string name, string role, Point position, Color color)
        {
            Name = name;
            Role = role;
            Position = position;
            Size = new Size(120, 155);
            Color = color;
            CurrentMessage = "";
            HasMessage = false;
            IsActive = false;
        }

        public Rectangle Bounds => new Rectangle(Position, Size);
        public Point Center => new Point(Position.X + Size.Width / 2, Position.Y + Size.Height / 3);
    }
}
