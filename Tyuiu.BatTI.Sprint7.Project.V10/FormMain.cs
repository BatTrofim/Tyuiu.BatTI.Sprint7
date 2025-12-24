using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Tyuiu.BatTI.Sprint7.Project.V10.Lib;


namespace Project.V10
{
    public partial class FormMain : Form
    {
        // --- КОМПОНЕНТЫ UI ---
        private TabControl tabControlMain_BTI;
        private TabPage tabPageData_BTI, tabPageAnalytics_BTI;

        // Меню
        private Panel panelMenu_BTI;

        // Вкладка 1: Таблица и Управление
        private DataGridView gridData_BTI;
        private Panel panelControls_BTI;
        private TextBox txtCode_BTI, txtName_BTI, txtQty_BTI, txtPrice_BTI, txtSupplier_BTI, txtDate_BTI, txtDesc_BTI;
        private ComboBox cmbCategoryInput_BTI;
        private Button btnAdd_BTI, btnEdit_BTI, btnDelete_BTI;

        // Фильтры и Поиск
        private Panel panelFilter_BTI;
        private TextBox txtSearch_BTI;
        private ComboBox cmbCategoryFilter_BTI;

        // Детальная статистика
        private GroupBox grpStats_BTI;
        private Label lblStatCount_BTI, lblStatSum_BTI, lblStatAvg_BTI, lblStatMin_BTI, lblStatMax_BTI;

        // Вкладка 2: Графики
        private Chart chartBar_BTI, chartPie_BTI;
        private TableLayoutPanel layoutCharts_BTI;

        // Служебные
        private ContextMenuStrip contextMenuGrid_BTI; // ПКМ меню
        private DataService ds_BTI;
        private List<ItemModel> dataList_BTI;
        private List<ItemModel> currentViewList_BTI; // Список (с учетом фильтра)
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;

        // Переменная для сортировки
        private bool isAscending = true;

        public FormMain()
        {
            this.Text = "Склад | Sprint 7 | Бат.Т.И.";
            this.Size = new Size(1350, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9F);

            ds_BTI = new DataService();
            dataList_BTI = new List<ItemModel>();
            currentViewList_BTI = new List<ItemModel>();

            InitializeUI();
        }

        private void InitializeUI()
        {
            // 1. ЛЕВОЕ МЕНЮ
            panelMenu_BTI = new Panel { Dock = DockStyle.Left, Width = 200, BackColor = Color.FromArgb(33, 37, 41) };
            Label logo = new Label { ForeColor = Color.White, Font = new Font("Segoe UI", 18, FontStyle.Bold), Dock = DockStyle.Top, Height = 100, TextAlign = ContentAlignment.MiddleCenter };

            panelMenu_BTI.Controls.Add(CreateMenuBtn("Выход", 280, (s, e) => Application.Exit()));
            panelMenu_BTI.Controls.Add(CreateMenuBtn("О программе", 220, (s, e) =>
            {
                string info = "Автоматизированная информационная система «Оптовая база»\n" +

                              "Разработчик: Бат.Т.И\n" +
                              "Предметная область: Заказы\n" +
                              "Учебное задание: Спринт 7 вариант 10\n\n" +

                              "Назначение программы:\n" +
                              "Приложение предназначено для автоматизации процессов учета товаров, " +
                              "анализа остатков и формирования отчетности. Позволяет сократить время " +
                              "на обработку накладных и визуализировать финансовые показатели.\n\n" +

                              "Основные возможности:\n" +
                              "• Учет поступления и списания товаров (CSV БД);\n" +
                              "• Многофакторный поиск (Артикул, Поставщик, Дата);\n" +
                              "• Динамическая статистика и аналитические графики;\n" +
                              "• Контроль дефицита товара (цветовая индикация);\n" +
                              "• Сортировка и фильтрация данных.\n\n";

                MessageBox.Show(info, "Справка | Оптовая база", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }));
            panelMenu_BTI.Controls.Add(CreateMenuBtn("Сохранить БД", 160, (s, e) => SaveFile()));
            panelMenu_BTI.Controls.Add(CreateMenuBtn("Загрузить БД", 100, (s, e) => LoadFile()));
            panelMenu_BTI.Controls.Add(logo);

            // 2. ВКЛАДКИ
            tabControlMain_BTI = new TabControl { Dock = DockStyle.Fill };
            tabPageData_BTI = new TabPage("База товаров");
            tabPageAnalytics_BTI = new TabPage("Статистика");
            tabControlMain_BTI.Controls.Add(tabPageData_BTI);
            tabControlMain_BTI.Controls.Add(tabPageAnalytics_BTI);

            // --- ВКЛАДКА 1: ДАННЫЕ ---

            // Панель фильтров (Верх)
            panelFilter_BTI = new Panel { Dock = DockStyle.Top, Height = 100, BackColor = Color.WhiteSmoke };

            // 1. Поиск (Заголовок)
            Label lblSearch = new Label
            {
                Text = "Поиск",
                Location = new Point(10, 8),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            // Поле ввода
            txtSearch_BTI = new TextBox { Location = new Point(10, 32), Width = 250 };
            txtSearch_BTI.TextChanged += (s, e) => ApplyFilters();

            // Мелкий текст под полем поиска
            Label lblSearchHint = new Label
            {
                Text = "Артикул / Наименование / Поставщик / Дата",
                Location = new Point(10, 58),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8),
                AutoSize = true
            };

            // Фильтр по категории
            Label lblCatFilter = new Label
            {
                Text = "Фильтр по категории:",
                Location = new Point(280, 8),
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                AutoSize = true
            };

            cmbCategoryFilter_BTI = new ComboBox { Location = new Point(280, 32), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategoryFilter_BTI.Items.Add("Все");
            cmbCategoryFilter_BTI.SelectedIndex = 0;
            cmbCategoryFilter_BTI.SelectedIndexChanged += (s, e) => ApplyFilters();

            // ГРУППА СТАТИСТИКИ
            grpStats_BTI = new GroupBox { Text = "Оперативная статистика", Location = new Point(500, 5), Size = new Size(600, 85) };
            lblStatCount_BTI = CreateStatLabel(grpStats_BTI, "Позиций: 0", 20, 25);
            lblStatSum_BTI = CreateStatLabel(grpStats_BTI, "Общая стоимость: 0 ₽", 150, 25);
            lblStatAvg_BTI = CreateStatLabel(grpStats_BTI, "Средняя цена: 0 ₽", 350, 25);
            lblStatMin_BTI = CreateStatLabel(grpStats_BTI, "Мин. цена: 0 ₽", 20, 55);
            lblStatMax_BTI = CreateStatLabel(grpStats_BTI, "Макс. цена: 0 ₽", 150, 55);

            panelFilter_BTI.Controls.Add(grpStats_BTI);
            panelFilter_BTI.Controls.Add(cmbCategoryFilter_BTI);
            panelFilter_BTI.Controls.Add(lblCatFilter);
            panelFilter_BTI.Controls.Add(lblSearchHint);
            panelFilter_BTI.Controls.Add(txtSearch_BTI);
            panelFilter_BTI.Controls.Add(lblSearch);

            // Панель редактирования
            panelControls_BTI = new Panel { Dock = DockStyle.Right, Width = 260, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle };
            panelControls_BTI.Controls.Add(new Label { Text = "Карточка товара", Location = new Point(10, 10), Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true });

            txtCode_BTI = CreateInput(panelControls_BTI, "Артикул:", 50);
            txtName_BTI = CreateInput(panelControls_BTI, "Название:", 100);

            panelControls_BTI.Controls.Add(new Label { Text = "Категория:", Location = new Point(10, 150), AutoSize = true });
            cmbCategoryInput_BTI = new ComboBox { Location = new Point(10, 170), Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbCategoryInput_BTI.Items.AddRange(new object[] { "Строительство", "Отделка", "Инструменты", "Электрика", "Сантехника", "Разное" });
            cmbCategoryInput_BTI.SelectedIndex = 0;
            panelControls_BTI.Controls.Add(cmbCategoryInput_BTI);

            txtQty_BTI = CreateInput(panelControls_BTI, "Количество:", 200);
            txtPrice_BTI = CreateInput(panelControls_BTI, "Цена:", 250);
            txtSupplier_BTI = CreateInput(panelControls_BTI, "Поставщик:", 300);
            txtDate_BTI = CreateInput(panelControls_BTI, "Дата поставки:", 350);

            panelControls_BTI.Controls.Add(new Label { Text = "Описание:", Location = new Point(10, 400), AutoSize = true, ForeColor = Color.DimGray });
            txtDesc_BTI = new TextBox { Location = new Point(10, 420), Width = 230, Height = 50, Multiline = true, ScrollBars = ScrollBars.Vertical };
            panelControls_BTI.Controls.Add(txtDesc_BTI);

            btnAdd_BTI = CreateBtn(panelControls_BTI, "ДОБАВИТЬ", Color.SeaGreen, 490, (s, e) => ActionAdd());
            btnEdit_BTI = CreateBtn(panelControls_BTI, "СОХРАНИТЬ", Color.Orange, 540, (s, e) => ActionEdit());
            btnDelete_BTI = CreateBtn(panelControls_BTI, "УДАЛИТЬ", Color.IndianRed, 590, (s, e) => ActionDelete());

            // Таблица
            gridData_BTI = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.White,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                BorderStyle = BorderStyle.None
            };
            gridData_BTI.SelectionChanged += (s, e) => GridSelectionChanged();

            gridData_BTI.ColumnHeaderMouseClick += GridData_BTI_ColumnHeaderMouseClick;

            contextMenuGrid_BTI = new ContextMenuStrip();
            contextMenuGrid_BTI.Items.Add("Продать 1 шт.", null, (s, e) => ActionSellOne());
            contextMenuGrid_BTI.Items.Add("Удалить", null, (s, e) => ActionDelete());
            gridData_BTI.ContextMenuStrip = contextMenuGrid_BTI;

            tabPageData_BTI.Controls.Add(gridData_BTI);
            tabPageData_BTI.Controls.Add(panelControls_BTI);
            tabPageData_BTI.Controls.Add(panelFilter_BTI);


            // --- ВКЛАДКА ГРАФИКИ ---
            layoutCharts_BTI = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1 };
            layoutCharts_BTI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
            layoutCharts_BTI.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));

            // ГРАФИК (Столбцы)
            chartBar_BTI = new Chart { Dock = DockStyle.Fill };
            chartBar_BTI.ChartAreas.Add(new ChartArea("A1"));
            chartBar_BTI.Legends.Add(new Legend("L1"));
            chartBar_BTI.Series.Add(new Series("S1") { ChartType = SeriesChartType.Column });

            // ГРАФИК (Круг)
            chartPie_BTI = new Chart { Dock = DockStyle.Fill };
            chartPie_BTI.ChartAreas.Add(new ChartArea("A2"));
            var legendPie = new Legend("LegendPie")
            {
                Docking = Docking.Bottom,
                Alignment = StringAlignment.Center
            };
            chartPie_BTI.Legends.Add(legendPie);
            chartPie_BTI.Series.Add(new Series("S2") { ChartType = SeriesChartType.Doughnut });

            layoutCharts_BTI.Controls.Add(chartBar_BTI, 0, 0);
            layoutCharts_BTI.Controls.Add(chartPie_BTI, 1, 0);
            tabPageAnalytics_BTI.Controls.Add(layoutCharts_BTI);

            this.Controls.Add(tabControlMain_BTI);
            this.Controls.Add(panelMenu_BTI);

            openFileDialog = new OpenFileDialog { Filter = "CSV|*.csv" };
            saveFileDialog = new SaveFileDialog { Filter = "CSV|*.csv", FileName = "SkladData.csv" };
        }


        private void LoadFile()
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                dataList_BTI = ds_BTI.LoadData(openFileDialog.FileName);

                var cats = dataList_BTI.Select(x => x.Category).Distinct().OrderBy(x => x).ToArray();
                cmbCategoryFilter_BTI.Items.Clear();
                cmbCategoryFilter_BTI.Items.Add("Все");
                cmbCategoryFilter_BTI.Items.AddRange(cats);
                cmbCategoryFilter_BTI.SelectedIndex = 0;

                ApplyFilters();
                UpdateCharts();
            }
        }

        private void SaveFile()
        {
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                ds_BTI.SaveData(saveFileDialog.FileName, dataList_BTI);
                MessageBox.Show("Успешно сохранено!", "Система");
            }
        }

        private void ApplyFilters()
        {
            string q = txtSearch_BTI.Text.Trim().ToLower();
            string cat = cmbCategoryFilter_BTI.SelectedItem?.ToString() ?? "Все";

            // Фильтрация
            currentViewList_BTI = dataList_BTI.Where(x =>
                (
                    (x.Name != null && x.Name.ToLower().Contains(q)) ||
                    (x.Code != null && x.Code.ToLower().Contains(q)) ||
                    (x.Supplier != null && x.Supplier.ToLower().Contains(q)) ||
                    (x.DeliveryDate != null && x.DeliveryDate.ToLower().Contains(q))
                )
                &&
                (cat == "Все" || (x.Category != null && x.Category == cat))
            ).ToList();

            gridData_BTI.DataSource = null;
            gridData_BTI.DataSource = currentViewList_BTI;

            ConfigureGrid();
            UpdateStatsPanel();
        }

        private void UpdateStatsPanel()
        {
            lblStatCount_BTI.Text = $"Позиций: {currentViewList_BTI.Count}";
            lblStatSum_BTI.Text = $"Общая стоимость: {ds_BTI.GetTotalStockValue(currentViewList_BTI):C2}";
            lblStatAvg_BTI.Text = $"Средняя цена: {ds_BTI.GetAveragePrice(currentViewList_BTI):C2}";
            lblStatMin_BTI.Text = $"Мин. цена: {ds_BTI.GetMinPrice(currentViewList_BTI):C2}";
            lblStatMax_BTI.Text = $"Макс. цена: {ds_BTI.GetMaxPrice(currentViewList_BTI):C2}";
        }

        private void ConfigureGrid()
        {
            if (gridData_BTI.Columns["Code"] != null) gridData_BTI.Columns["Code"].HeaderText = "Артикул";
            if (gridData_BTI.Columns["Name"] != null) gridData_BTI.Columns["Name"].HeaderText = "Наименование";
            if (gridData_BTI.Columns["Category"] != null) gridData_BTI.Columns["Category"].HeaderText = "Категория";
            if (gridData_BTI.Columns["Quantity"] != null) gridData_BTI.Columns["Quantity"].HeaderText = "Остаток";
            if (gridData_BTI.Columns["Price"] != null) { gridData_BTI.Columns["Price"].HeaderText = "Цена"; gridData_BTI.Columns["Price"].DefaultCellStyle.Format = "C2"; }
            if (gridData_BTI.Columns["Description"] != null) gridData_BTI.Columns["Description"].HeaderText = "Описание";
            if (gridData_BTI.Columns["Supplier"] != null) gridData_BTI.Columns["Supplier"].HeaderText = "Поставщик";
            if (gridData_BTI.Columns["DeliveryDate"] != null) gridData_BTI.Columns["DeliveryDate"].HeaderText = "Дата";
        }


        private void ActionAdd()
        {
            try
            {
                var item = new ItemModel
                {
                    Code = txtCode_BTI.Text,
                    Name = txtName_BTI.Text,
                    Category = cmbCategoryInput_BTI.Text,
                    Quantity = int.Parse(txtQty_BTI.Text),
                    Price = decimal.Parse(txtPrice_BTI.Text),
                    Supplier = txtSupplier_BTI.Text,
                    DeliveryDate = txtDate_BTI.Text,
                    Description = txtDesc_BTI.Text
                };
                dataList_BTI.Add(item);
                RefreshAll();
            }
            catch { MessageBox.Show("Проверьте формат чисел!"); }
        }

        private void ActionEdit()
        {
            if (gridData_BTI.SelectedRows.Count > 0)
            {
                var item = (ItemModel)gridData_BTI.SelectedRows[0].DataBoundItem;
                item.Code = txtCode_BTI.Text;
                item.Name = txtName_BTI.Text;
                item.Category = cmbCategoryInput_BTI.Text;
                int.TryParse(txtQty_BTI.Text, out int q); item.Quantity = q;
                decimal.TryParse(txtPrice_BTI.Text, out decimal p); item.Price = p;
                item.Supplier = txtSupplier_BTI.Text;
                item.DeliveryDate = txtDate_BTI.Text;
                item.Description = txtDesc_BTI.Text;
                RefreshAll();
            }
        }

        private void ActionDelete()
        {
            if (gridData_BTI.SelectedRows.Count > 0)
            {
                var item = (ItemModel)gridData_BTI.SelectedRows[0].DataBoundItem;
                if (MessageBox.Show($"Удалить {item.Name}?", "Подтверждение", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    dataList_BTI.Remove(item);
                    RefreshAll();
                }
            }
        }

        private void ActionSellOne()
        {
            if (gridData_BTI.SelectedRows.Count > 0)
            {
                var item = (ItemModel)gridData_BTI.SelectedRows[0].DataBoundItem;
                if (item.Quantity > 0)
                {
                    item.Quantity--;
                    RefreshAll();
                }
                else
                {
                    MessageBox.Show("Товара нет на складе!");
                }
            }
        }

        private void RefreshAll()
        {
            ApplyFilters();
            UpdateCharts();
        }


        private void GridSelectionChanged()        // Изменение выбора строки
        {
            if (gridData_BTI.SelectedRows.Count > 0)
            {
                // Ее значения
                var item = (ItemModel)gridData_BTI.SelectedRows[0].DataBoundItem; 
                txtCode_BTI.Text = item.Code;
                txtName_BTI.Text = item.Name;
                cmbCategoryInput_BTI.Text = item.Category;
                txtQty_BTI.Text = item.Quantity.ToString();
                txtPrice_BTI.Text = item.Price.ToString();
                txtSupplier_BTI.Text = item.Supplier;
                txtDate_BTI.Text = item.DeliveryDate;
                txtDesc_BTI.Text = item.Description;
            }
        }

        private void GridData_BTI_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            string colName = gridData_BTI.Columns[e.ColumnIndex].DataPropertyName;

            if (isAscending)
                currentViewList_BTI = currentViewList_BTI.OrderBy(x => x.GetType().GetProperty(colName).GetValue(x, null)).ToList();
            else
                currentViewList_BTI = currentViewList_BTI.OrderByDescending(x => x.GetType().GetProperty(colName).GetValue(x, null)).ToList();

            isAscending = !isAscending;
            gridData_BTI.DataSource = currentViewList_BTI;
        }

        private void UpdateCharts()
        {
            // ГИСТОГРАММА ---
            chartBar_BTI.Series[0].Points.Clear();
            chartBar_BTI.Palette = ChartColorPalette.SeaGreen;
            chartBar_BTI.Titles.Clear();
            chartBar_BTI.Titles.Add("Топ дорогих позиций");

            chartBar_BTI.Series[0].Name = "Стоимость запасов (руб.)";

            chartBar_BTI.Series[0].IsValueShownAsLabel = true;
            chartBar_BTI.Series[0].IsXValueIndexed = true;

            // Настройка осей
            var axisX = chartBar_BTI.ChartAreas[0].AxisX;
            axisX.Interval = 1;
            axisX.LabelStyle.Interval = 1;
            axisX.LabelStyle.Angle = -45;
            axisX.LabelStyle.Font = new Font("Segoe UI", 9);

            var top = currentViewList_BTI.OrderByDescending(x => x.Price * x.Quantity).Take(10).ToList();
            foreach (var i in top)
            {
                chartBar_BTI.Series[0].Points.AddXY(i.Name, i.Price * i.Quantity);
            }

            // --- ГРАФИК: КРУГОВАЯ ---
            chartPie_BTI.Series[0].Points.Clear();
            chartPie_BTI.Palette = ChartColorPalette.BrightPastel;
            chartPie_BTI.Titles.Clear();
            chartPie_BTI.Titles.Add("Доли категорий (в деньгах)");

            var cats = currentViewList_BTI.GroupBy(x => x.Category)
                                          .Select(g => new { N = g.Key, V = g.Sum(x => x.Price * x.Quantity) });

            foreach (var c in cats)
            {
                if (c.V > 0)
                {
                    int idx = chartPie_BTI.Series[0].Points.AddXY(c.N, c.V);

                    chartPie_BTI.Series[0].Points[idx].Label = "#PERCENT";
                    chartPie_BTI.Series[0].Points[idx].LegendText = c.N;
                }
            }
        }

        private Button CreateMenuBtn(string t, int top, EventHandler act)
        {
            var b = new Button { Text = t, Top = top, Left = 0, Width = 200, Height = 50, FlatStyle = FlatStyle.Flat, ForeColor = Color.LightGray, BackColor = Color.Transparent, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0) };
            b.Click += act; return b;
        }
        private TextBox CreateInput(Panel p, string l, int t)
        {
            p.Controls.Add(new Label { Text = l, Location = new Point(10, t), AutoSize = true, ForeColor = Color.DimGray });
            var tb = new TextBox { Location = new Point(10, t + 20), Width = 230 }; p.Controls.Add(tb); return tb;
        }
        private Button CreateBtn(Panel p, string t, Color c, int top, EventHandler act)
        {
            var b = new Button { Text = t, BackColor = c, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Location = new Point(10, top), Size = new Size(230, 40) };
            b.Click += act; p.Controls.Add(b); return b;
        }
        private Label CreateStatLabel(GroupBox g, string txt, int x, int y)
        {
            var l = new Label { Text = txt, Location = new Point(x, y), AutoSize = true, Font = new Font("Segoe UI", 9, FontStyle.Bold) };
            g.Controls.Add(l); return l;
        }
    }
}