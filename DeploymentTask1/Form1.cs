using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using MathLibrary;
using AuthorLibrary;

namespace DeploymentTask1
{
    public partial class Form1 : Form
    {
        // Inputs & actions
        TextBox txtNumber1;
        TextBox txtNumber2;
        Button btnMultiply, btnClear, btnSteps, btnCopy, btnTheme;

        // Output & extras
        Label lblResult;
        ListBox lstHistory;
        RichTextBox rtbSteps;
        Label lblFact;
        ToolTip tip;

        // State
        readonly Queue<string> history = new Queue<string>();
        readonly Random rng = new Random();
        bool darkMode = false;

        // Placeholder tracking
        readonly HashSet<TextBox> placeholders = new HashSet<TextBox>();

        readonly string[] mathFacts = new[]
        {
            "Any number × 0 = 0.",
            "Any number × 1 = itself.",
            "Multiplication is repeated addition.",
            "a×b = b×a (commutative).",
            "(a×b)×c = a×(b×c) (associative).",
            "Distributive: a×(b+c) = a×b + a×c.",
            "12×12 = 144 (a gross).",
            "Even×Any = Even.",
            "If both factors end in 5, product ends in 25.",
            "Squares grow quickly: 20²=400, 30²=900."
        };

        public Form1()
        {
            InitializeComponent();        // designer part
            InitializeCustomUI();         // our dynamic UI
            ApplyTheme();                 // initial theme
        }

        private void InitializeCustomUI()
        {
            Text = "Multiplier by Ashaen";
            MinimumSize = new Size(850, 520);
            StartPosition = FormStartPosition.CenterScreen;

            // Container
            var root = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Padding = new Padding(16),
            };
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Controls.Add(root);

            // Row 0: Inputs + Buttons
            var row0 = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                AutoScroll = true,
                Padding = new Padding(4)
            };

            txtNumber1 = MakeInput("Number 1");
            txtNumber2 = MakeInput("Number 2");

            btnMultiply = MakeButton("Multiply", BtnMultiply_Click);
            btnSteps = MakeButton("Step by Step", BtnSteps_Click);
            btnClear = MakeButton("Clear", BtnClear_Click);
            btnCopy = MakeButton("Copy Result", BtnCopy_Click);
            btnTheme = MakeButton("Toggle Light/Dark", BtnTheme_Click);

            lblResult = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold),
                Margin = new Padding(16, 8, 8, 8),
                Text = "Result: —"
            };

            row0.Controls.Add(txtNumber1);
            row0.Controls.Add(txtNumber2);
            row0.Controls.Add(btnMultiply);
            row0.Controls.Add(btnSteps);
            row0.Controls.Add(btnClear);
            row0.Controls.Add(btnCopy);
            row0.Controls.Add(btnTheme);
            row0.Controls.Add(lblResult);

            root.Controls.Add(row0);
            root.SetColumnSpan(row0, 2);

            // Row 1: Steps + History
            rtbSteps = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 11f),
                BorderStyle = BorderStyle.FixedSingle
            };
            root.Controls.Add(rtbSteps, 0, 1);

            var historyPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(4) };
            var lblHistory = new Label
            {
                Text = "History (last 10):",
                AutoSize = true,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                Dock = DockStyle.Top
            };
            lstHistory = new ListBox
            {
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 10f)
            };
            lstHistory.DoubleClick += (s, e) =>
            {
                if (lstHistory.SelectedItem is string line)
                    Clipboard.SetText(line);
            };
            historyPanel.Controls.Add(lstHistory);
            historyPanel.Controls.Add(lblHistory);
            root.Controls.Add(historyPanel, 1, 1);

            // Row 2: Fact banner
            lblFact = new Label
            {
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10.5f, FontStyle.Italic),
                Padding = new Padding(8),
                AutoEllipsis = true,
                Height = 36
            };
            root.Controls.Add(lblFact);
            root.SetColumnSpan(lblFact, 2);

            // Author name
            var lblAuthor = new Label
            {
                Text = Author.MainAuthor(),
                AutoSize = true,
                Dock = DockStyle.Bottom,
                Font = new Font("Segoe UI", 9f, FontStyle.Italic)
            };
            root.Controls.Add(lblAuthor);
            root.SetColumnSpan(lblAuthor, 2);

            // Tooltips
            tip = new ToolTip();
            tip.SetToolTip(txtNumber1, "Enter the first number (integer or decimal).");
            tip.SetToolTip(txtNumber2, "Enter the second number (integer or decimal).");
            tip.SetToolTip(btnMultiply, "Compute Number1 × Number2.");
            tip.SetToolTip(btnSteps, "Show long multiplication steps (integers).");
            tip.SetToolTip(btnClear, "Clear inputs, result, and steps.");
            tip.SetToolTip(btnCopy, "Copy the current result to the clipboard.");
            tip.SetToolTip(btnTheme, "Toggle between light and dark modes.");
        }

        // --- UI helpers ---
        private TextBox MakeInput(string placeholder)
        {
            var tb = new TextBox
            {
                Width = 140,
                Font = new Font("Segoe UI", 11f),
                Margin = new Padding(4, 8, 4, 8)
            };
            tb.Tag = placeholder;
            tb.GotFocus += (s, e) => Placeholder(tb, true);
            tb.LostFocus += (s, e) => Placeholder(tb, false);
            Placeholder(tb, false);
            return tb;
        }

        private void Placeholder(TextBox tb, bool focus)
        {
            string ph = (string)tb.Tag;
            if (focus)
            {
                if (tb.ForeColor == Color.Gray)
                {
                    tb.Text = "";
                    tb.ForeColor = darkMode ? Color.White : Color.Black;
                    placeholders.Remove(tb);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = ph;
                    tb.ForeColor = Color.Gray;
                    placeholders.Add(tb);
                }
            }
        }

        private Button MakeButton(string text, EventHandler onClick)
        {
            var b = new Button
            {
                Text = text,
                AutoSize = true,
                Font = new Font("Segoe UI", 10.5f),
                Margin = new Padding(6, 6, 6, 6),
                FlatStyle = FlatStyle.System
            };
            b.Click += onClick;
            return b;
        }

        private void ApplyTheme()
        {
            Color bg = darkMode ? Color.FromArgb(28, 28, 30) : Color.White;
            Color fg = darkMode ? Color.White : Color.Black;
            Color alt = darkMode ? Color.FromArgb(44, 44, 46) : Color.FromArgb(245, 245, 248);

            BackColor = bg;
            ForeColor = fg;

            foreach (Control c in Controls) ApplyThemeRecursive(c, bg, fg, alt);

            lblFact.BackColor = alt;

            // Keep placeholders gray
            foreach (var tb in placeholders)
                tb.ForeColor = Color.Gray;
        }

        private void ApplyThemeRecursive(Control c, Color bg, Color fg, Color alt)
        {
            if (c is Panel || c is TableLayoutPanel || c is FlowLayoutPanel)
                c.BackColor = bg;
            else if (c is RichTextBox || c is ListBox)
                c.BackColor = alt;
            else
                c.BackColor = bg;

            c.ForeColor = fg;

            foreach (Control child in c.Controls)
                ApplyThemeRecursive(child, bg, fg, alt);
        }

        // --- Button handlers ---
        private void BtnMultiply_Click(object sender, EventArgs e)
        {
            if (!TryReadNumbers(out double a, out double b)) return;

            double product = Calculator.Multiply(a, b);
            lblResult.Text = $"Result: {product}";
            AddToHistory($"{a} × {b} = {product}");

            lblFact.Text = "Math Fact: " + mathFacts[rng.Next(mathFacts.Length)];

            rtbSteps.Clear();
        }

        private void BtnSteps_Click(object sender, EventArgs e)
        {
            if (!TryReadNumbers(out double a, out double b)) return;

            if (!IsWholeNumber(a) || !IsWholeNumber(b))
            {
                rtbSteps.Text = "Step-by-step is shown for whole numbers.\nTip: enter integers to see long multiplication.";
                return;
            }

            long x = (long)Math.Round(a);
            long y = (long)Math.Round(b);

            string steps = BuildLongMultiplication(x, y, out long result);
            rtbSteps.Text = steps;

            lblResult.Text = $"Result: {result}";
            AddToHistory($"{x} × {y} = {result}");
            lblFact.Text = "Math Fact: " + mathFacts[rng.Next(mathFacts.Length)];
        }

        private void BtnClear_Click(object sender, EventArgs e)
        {
            txtNumber1.Text = "";
            txtNumber2.Text = "";
            Placeholder(txtNumber1, false);
            Placeholder(txtNumber2, false);
            lblResult.Text = "Result: —";
            rtbSteps.Clear();
            txtNumber1.Focus();
        }

        private void BtnCopy_Click(object sender, EventArgs e)
        {
            string text = lblResult.Text.Replace("Result: ", "").Trim();
            if (string.IsNullOrWhiteSpace(text) || text == "—")
            {
                MessageBox.Show("No result to copy yet.", "Copy Result", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Clipboard.SetText(text);
            tip.Show("Copied!", this, PointToClient(Cursor.Position), 1200);
        }

        private void BtnTheme_Click(object sender, EventArgs e)
        {
            darkMode = !darkMode;
            ApplyTheme();
        }

        // --- Helpers ---
        private bool TryReadNumbers(out double a, out double b)
        {
            a = b = 0;
            bool ok1 = double.TryParse(placeholders.Contains(txtNumber1) ? "" : txtNumber1.Text, out a);
            bool ok2 = double.TryParse(placeholders.Contains(txtNumber2) ? "" : txtNumber2.Text, out b);

            if (!ok1 || !ok2)
            {
                MessageBox.Show("Please enter valid numbers in both fields.", "Input Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }

        private void AddToHistory(string line)
        {
            history.Enqueue(line);
            while (history.Count > 10) history.Dequeue();

            lstHistory.BeginUpdate();
            lstHistory.Items.Clear();
            foreach (var h in history) lstHistory.Items.Add(h);
            lstHistory.EndUpdate();
        }

        private static bool IsWholeNumber(double d) => Math.Abs(d - Math.Round(d)) < 0.0000001;

        private static string BuildLongMultiplication(long a, long b, out long product)
        {
            product = a * b;
            var lines = new List<string>();
            string sA = a.ToString();
            string sB = b.ToString();

            var partials = new List<string>();
            for (int i = sB.Length - 1; i >= 0; i--)
            {
                int digit = sB[i] - '0';
                long part = a * digit;
                partials.Add(digit != 0 ? part.ToString() + new string('0', (sB.Length - 1) - i) : "0" + new string('0', (sB.Length - 1) - i));
            }

            int width = Math.Max(sA.Length, sB.Length + 2);
            foreach (var p in partials) width = Math.Max(width, p.Length);
            width = Math.Max(width, product.ToString().Length);
            string pad(string s) => s.PadLeft(width);

            var sb = new StringBuilder();
            sb.AppendLine(pad(sA));
            sb.AppendLine(pad("× " + sB));
            sb.AppendLine(new string('–', width));

            foreach (var p in partials) sb.AppendLine(pad(p));
            if (partials.Count > 1) sb.AppendLine(new string('–', width));

            sb.AppendLine(pad(product.ToString()));
            return sb.ToString();
        }
    }
}
