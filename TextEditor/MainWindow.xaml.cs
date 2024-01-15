using Microsoft.Win32;
//using PdfSharp.Drawing;
//using PdfSharp.Drawing.Layout;
//using PdfSharp.Pdf;
//using PdfSharp.Pdf.IO;
//using System;
using System.IO;
using System.Windows;
using System.Windows.Documents;
//using System.Windows.Shapes;
//using PdfiumViewer;
using System.Windows.Controls;
using System.Windows.Media;
using System.Drawing;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using System.Windows.Input;
using System.Reflection.Metadata;
using System.Windows.Markup;
using System.ComponentModel;
using System.Windows.Data;

namespace TextEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string currentFileName;
        private int currentPageIndex = 0;
        private double scale = 1;
        private FlowDocument flowDocument;
        private RichTextBox currentRichTextBox;
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double Scale
        {
            get { return scale; }
            set
            {
                if (scale != value)
                {
                    scale = value;
                    OnPropertyChanged(nameof(Scale));
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            InitializeDocument();
            richTextBox.IsEnabled = false;
            richTextBox.Visibility = Visibility.Collapsed;
            PopulateSystemFonts();
            PopulateFontSizes();
            PopulateColors();
            PopulateBackgroundColors();
            UpdateMainWindowTitle();
            richTextBox.TextChanged += RichTextBox_TextChanged;
            Binding binding = new Binding("Scale");
            binding.Source = this;
            scaleSlider.SetBinding(Slider.ValueProperty, binding);
            scaleSlider.ValueChanged += ScaleSlider_ValueChanged;
            UpdateScaleValueLabel();
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Scale = e.NewValue;
            ApplyScale();
            UpdateScaleValueLabel();
        }
        private void UpdateScaleValueLabel()
        {
            scaleValueLabel.Text = $"{Scale*100:F0}";
        }

        private void ApplyScale()
        {
            richTextBox.LayoutTransform = new ScaleTransform(Scale, Scale);
        }

        private void InitializeDocument()
        {
            flowDocument = new FlowDocument();
            AddNewPage();
            richTextBox.Document = flowDocument;
        }
        private void AddNewPage()
        {
            //Paragraph page = new Paragraph();
            //document.Blocks.Add(page);
            // Створення нового RichTextBox
            RichTextBox newRichTextBox = new RichTextBox();
            newRichTextBox.AcceptsReturn = true;

            // Налаштування властивостей нового RichTextBox
            newRichTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            newRichTextBox.Document = new FlowDocument();

            // Створення секції для нової сторінки
            Section section = new Section();
            newRichTextBox.Document.Blocks.Add(section);

            // Додавання нового RichTextBox до вмісту FlowDocument
            flowDocument.Blocks.Add(new BlockUIContainer(newRichTextBox));

            // Встановлення поточного RichTextBox
            currentRichTextBox = newRichTextBox;
        }

        private void NextPage()
        {
            if (currentPageIndex < flowDocument.Blocks.Count - 1)
            {
                currentPageIndex++;
                UpdateMainWindowTitle();
            }
        }

        private void PreviousPage()
        {
            if (currentPageIndex > 0)
            {
                currentPageIndex--;
                UpdateMainWindowTitle();
            }
        }


        private void PopulateSystemFonts()
        {
            // Получаем список системных шрифтов и добавляем их в FontComboBox
            foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = fontFamily.Source;
                FontComboBox.Items.Add(item);
            }
            FontComboBox.SelectedIndex = 2;
        }

        private void PopulateFontSizes()
        {
            // Заполняем ComboBox системными размерами шрифта
            for (int i = 6; i <= 48; i += 2)
            {
                ComboBoxItem item = new ComboBoxItem();
                item.Content = i;
                FontSizeComboBox.Items.Add(item);
            }
            FontSizeComboBox.SelectedIndex = 6;
        }

        private void PopulateColors()
        {
            ColorComboBox.Items.Add(Colors.Black);
            ColorComboBox.Items.Add(Colors.Red);
            ColorComboBox.Items.Add(Colors.Blue);
            ColorComboBox.Items.Add(Colors.Green);
            ColorComboBox.Items.Add(Colors.Orange);
            ColorComboBox.Items.Add(Colors.Yellow);
            ColorComboBox.Items.Add(Colors.DarkBlue);
            ColorComboBox.Items.Add(Colors.Violet);
            ColorComboBox.Items.Add(Colors.White);
            ColorComboBox.SelectedIndex = 0;
        }

        private void PopulateBackgroundColors()
        {
            BackgroundColorComboBox.Items.Add(Colors.White);
            BackgroundColorComboBox.Items.Add(Colors.Black);
            BackgroundColorComboBox.Items.Add(Colors.Red);
            BackgroundColorComboBox.Items.Add(Colors.Blue);
            BackgroundColorComboBox.Items.Add(Colors.Green);
            BackgroundColorComboBox.Items.Add(Colors.Orange);
            BackgroundColorComboBox.Items.Add(Colors.Yellow);
            BackgroundColorComboBox.Items.Add(Colors.DarkBlue);
            BackgroundColorComboBox.Items.Add(Colors.Violet);
            BackgroundColorComboBox.SelectedIndex = 0;
            //}
        }
        private void UpdateMainWindowTitle()
        {
            // Формируйте заголовок окна с учетом текущего имени файла
            if (string.IsNullOrEmpty(currentFileName))
            {
                this.Title = "Text Editor";
            }
            else
            {
                this.Title = $"Text Editor - {System.IO.Path.GetFileName(currentFileName)} (Page {currentPageIndex + 1})";
            }
        }
        private void New_Click(object sender, RoutedEventArgs e)
        {
            // Створення FlowDocument зі специфікаціями A4
            FlowDocument flowDocument = new FlowDocument();
            flowDocument.ColumnWidth = 8.27 * 96; // Ширина колонки в пікселях (A4 ширина в дюймах помножити на 96 пікселів)
            flowDocument.PageHeight = 11.69 * 96; // Висота сторінки в пікселях

            // Створення секції для першої сторінки
            Section section = new Section();
            flowDocument.Blocks.Add(section);
            Paragraph paragraph = new Paragraph(new Run(""));
            section.Blocks.Add(paragraph);
            richTextBox.Document = flowDocument;
            richTextBox.IsEnabled = true;
            richTextBox.Visibility = Visibility.Visible;
            richTextBox.Focus();
            currentFileName = "Новий документ.";
            UpdateMainWindowTitle();
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            NextPage();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            PreviousPage();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveDocument(sender, e);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveDocumentAs(sender, e);
        }


        private void Open_Click(object sender, RoutedEventArgs e) //LoadDocument(string filePath)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string fileExtension = System.IO.Path.GetExtension(openFileDialog.FileName).ToLower();

                try
                {
                    switch (fileExtension)
                    {
                        case ".txt":
                            OpenTextFile(openFileDialog.FileName);
                            break;
                        case ".rtf":
                            OpenRtfFile(openFileDialog.FileName);
                            break;
                        case ".pdf":
                            //OpenPdfFile(openFileDialog.FileName);
                            break;
                        default:
                            MessageBox.Show("Unsupported file format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                    richTextBox.IsEnabled = true;
                    richTextBox.Visibility = Visibility.Visible;
                    UpdateMainWindowTitle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void OpenTextFile(string filePath)
        {
            string content = File.ReadAllText(filePath);
            richTextBox.Document = new FlowDocument(new Paragraph(new Run(content)));
            currentFileName = filePath;
        }

        private void OpenRtfFile(string filePath)
        {
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                range.Load(fs, DataFormats.Rtf);
            }
            currentFileName = filePath;
        }

        //private void OpenPdfFile(string filePath)
        //{
        //    if (File.Exists(filePath))
        //    {
        //        var pdfViewer = new PdfViewer();
        //        pdfViewer.Load(filePath);
        //        windowsFormsHost.Child = pdfViewer;
        //    }
        //    currentFileName = filePath;
        //}

        //private void Save_Click(object sender, RoutedEventArgs e)
        //{
        private void SaveDocument(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(currentFileName) && currentFileName!="Новий документ.")
            {
                try
                {
                    string fileExtension = System.IO.Path.GetExtension(currentFileName).ToLower();

                    switch (fileExtension)
                    {
                        case ".txt":
                            SaveTextFile(currentFileName);
                            break;
                        case ".rtf":
                            SaveRtfFile(currentFileName);
                            break;
                        case ".pdf":
                            SavePdfFile(currentFileName);
                            break;
                        default:
                            MessageBox.Show("Unsupported file format.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                SaveAs_Click(sender, e);
            }
        }

        private void SaveTextFile(string filePath)
        {
            string content = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
            File.WriteAllText(filePath, content);
        }

        private void SaveRtfFile(string filePath)
        {
            TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                range.Save(fs, DataFormats.Rtf);
            }
        }
        private void SavePdfFile(string filePath)
        {
            //PdfSharp.Pdf.PdfDocument pdfDocument = new PdfSharp.Pdf.PdfDocument();
            //PdfPage pdfPage = pdfDocument.AddPage();
            //XGraphics gfx = XGraphics.FromPdfPage(pdfPage);
            //XTextFormatter tf = new XTextFormatter(gfx);

            //string content = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;
            //tf.DrawString(content, new XFont("Arial", 12), XBrushes.Black, new XRect(10, 10, pdfPage.Width.Point, pdfPage.Height.Point), XStringFormats.TopLeft);

            //pdfDocument.Save(filePath);
        }
 
        private void SaveDocumentAs(object sender, RoutedEventArgs e)
        {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";

            if (!string.IsNullOrEmpty(currentFileName))
            {
                saveFileDialog.FileName = currentFileName;
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    string filePath = saveFileDialog.FileName;
                    string content = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd).Text;

                    // Определение выбранного формата
                    string fileExtension = System.IO.Path.GetExtension(filePath);

                    if (fileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
                    {
                        // Сохранение в формате txt
                        File.WriteAllText(filePath, content);
                    }
                    else if (fileExtension.Equals(".rtf", StringComparison.OrdinalIgnoreCase))
                    {
                        // Сохранение в формате rtf
                        TextRange range = new TextRange(richTextBox.Document.ContentStart, richTextBox.Document.ContentEnd);
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            range.Save(fileStream, DataFormats.Rtf);
                        }
                    }
                    else if (fileExtension.Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                    {

                    }

                    currentFileName = filePath; // Обновляем текущее имя файла
                    UpdateMainWindowTitle();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Document = new FlowDocument(); // Очищаем содержимое RichTextBox
            currentFileName = null; // Сбрасываем текущее имя файла

            // Деактивируем и скрываем RichTextBox при закрытии файла
            richTextBox.IsEnabled = false; 
            richTextBox.Visibility = Visibility.Collapsed;
            UpdateMainWindowTitle();
        }
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Find_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, (FontWeight)richTextBox.Selection.GetPropertyValue(TextElement.FontWeightProperty) == FontWeights.Bold ? FontWeights.Normal : FontWeights.Bold);
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Selection.ApplyPropertyValue(TextElement.FontStyleProperty, (System.Windows.FontStyle)richTextBox.Selection.GetPropertyValue(TextElement.FontStyleProperty) == FontStyles.Italic ? FontStyles.Normal : FontStyles.Italic);
        }

        private void Underline_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, (TextDecorationCollection)richTextBox.Selection.GetPropertyValue(Inline.TextDecorationsProperty) == TextDecorations.Underline ? null : TextDecorations.Underline);
        }

        private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontComboBox.SelectedItem != null)
            {
                string selectedFont = ((ComboBoxItem)FontComboBox.SelectedItem).Content.ToString();
                ApplyToSelection(TextElement.FontFamilyProperty, new FontFamily(selectedFont));
            }
        }

        private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FontSizeComboBox.SelectedItem != null)
            {
                string selectedFontSize = ((ComboBoxItem)FontSizeComboBox.SelectedItem).Content.ToString();
                ApplyToSelection(TextElement.FontSizeProperty, double.Parse(selectedFontSize));
            }
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ColorComboBox.SelectedItem != null)
            {
                try
                {
                    // Получаем выбранный цвет
                    System.Windows.Media.Color selectedColor = (System.Windows.Media.Color)ColorComboBox.SelectedItem;

                    // Создаем SolidColorBrush с использованием полученного цвета
                    SolidColorBrush solidColorBrush = new SolidColorBrush(selectedColor);

                    // Применяем к выделенному тексту
                    ApplyToSelection(TextElement.ForegroundProperty, solidColorBrush);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private void BackgroundColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BackgroundColorComboBox.SelectedItem != null)
            {
                try
                {
                    // Получаем выбранный цвет
                    System.Windows.Media.Color selectedColor = (System.Windows.Media.Color)BackgroundColorComboBox.SelectedItem;

                    // Создаем SolidColorBrush с использованием полученного цвета
                    SolidColorBrush solidColorBrush = new SolidColorBrush(selectedColor);

                    // Применяем к выделенному тексту
                    ApplyToSelection(TextElement.BackgroundProperty, solidColorBrush);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BulletList_Click(object sender, RoutedEventArgs e)
        {
            ApplyToList(listType: TextMarkerStyle.Disc);
        }

        private void NumberedList_Click(object sender, RoutedEventArgs e)
        {
            ApplyToList(listType: TextMarkerStyle.Decimal);
        }

        private void sizeUp_Click(object sender, RoutedEventArgs e)
        {
            ChangeFontSize(2);
            UpdateFontSizeComboBox();
        }

        private void sizeDown_Click(object sender, RoutedEventArgs e)
        {
            ChangeFontSize(-2);
            UpdateFontSizeComboBox();
        }

        private void UpdateFontSizeComboBox()
        {
            // Обновляем размер шрифта в FontSizeComboBox
            FontSizeComboBox.SelectedItem = richTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty);
        }

        private void ChangeFontSize(double delta)
        {
            if (!richTextBox.Selection.IsEmpty)
            {
                richTextBox.Selection.ApplyPropertyValue(TextElement.FontSizeProperty, richTextBox.Selection.GetPropertyValue(TextElement.FontSizeProperty) is double currentSize
                    ? (object)(currentSize + delta)
                    : delta);
            }
        }
        private void InsertImage_Click(object sender, RoutedEventArgs e)
        {
            // Implement image insertion logic
        }

        private void ApplyToSelection(DependencyProperty property, object value)
        {
            TextRange selection = new TextRange(richTextBox.Selection.Start, richTextBox.Selection.End);

            if (selection.IsEmpty)
            {
                Paragraph paragraph = richTextBox.CaretPosition.Paragraph;
                if (paragraph != null)
                {
                    ApplyPropertyValueToParagraph(paragraph, property, value);
                }
            }
            else
            {
                ApplyPropertyValueToTextRange(selection, property, value);
            }
        }

        private void ApplyPropertyValueToParagraph(Paragraph paragraph, DependencyProperty property, object value)
        {
            paragraph.SetValue(property, value);
        }

        private void ApplyPropertyValueToTextRange(TextRange range, DependencyProperty property, object value)
        {
            if (range != null)
            {
                range.ApplyPropertyValue(property, value);

                TextPointer start = range.Start.GetPositionAtOffset(1);
                TextPointer end = range.End.GetPositionAtOffset(-1);

                if (start != null && end != null)
                {
                    TextRange modifiedRange = new TextRange(start, end);
                    modifiedRange.ApplyPropertyValue(property, value);
                }
            }
        }

        private void RichTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Перевірка, чи текст виходить за межі поточного RichTextBox
            if (currentRichTextBox != null && currentRichTextBox.Document.Blocks.FirstBlock != null)
            {
                var lastParagraph = currentRichTextBox.Document.Blocks.LastBlock as Paragraph;

                if (lastParagraph != null && lastParagraph.ContentEnd.GetLineStartPosition(0).IsAtLineStartPosition == false)
                {
                    // Якщо текст виходить за межі, додати нову сторінку
                    AddNewPage();
                }
            }
        }

        private void RichTextBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            TextPointer position = richTextBox.GetPositionFromPoint(e.GetPosition(richTextBox), true);

            if (position != null)
            {
                // Получаем текущий элемент под курсором
                Inline currentInline = position.Parent as Inline;
                if (currentInline != null)
                {
                    // Устанавливаем шрифт в FontComboBox
                    FontComboBox.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(font => font.Source == (currentInline.FontFamily ?? richTextBox.FontFamily).Source);

                    // Устанавливаем размер шрифта в FontSizeComboBox
                    FontSizeComboBox.SelectedItem = currentInline.FontSize;
                }
            }
        }

        private void ApplyToList(TextMarkerStyle listType)
        {
            List list = new List();
            list.MarkerStyle = listType;

            TextPointer insertionPosition;

            if (richTextBox.Selection.IsEmpty)
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Margin = new Thickness(0);
                paragraph.Inlines.Add(list.ToString());
                richTextBox.CaretPosition.Paragraph.SiblingBlocks.Add(paragraph);
                insertionPosition = paragraph.ContentEnd;
            }
            else
            {
                richTextBox.Selection.ApplyPropertyValue(Inline.FontWeightProperty, FontWeights.Bold);
                richTextBox.Selection.ApplyPropertyValue(Inline.FontStyleProperty, FontStyles.Italic);
                richTextBox.Selection.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
                richTextBox.Selection.ApplyPropertyValue(Inline.ForegroundProperty, null);
                richTextBox.Selection.ApplyPropertyValue(Inline.BackgroundProperty, null);

                Paragraph paragraph = new Paragraph();
                paragraph.Margin = new Thickness(0);
                paragraph.Inlines.Add(list.ToString());
                richTextBox.Selection.Start.Paragraph.SiblingBlocks.Add(paragraph);
                insertionPosition = paragraph.ContentEnd;
            }

            richTextBox.CaretPosition = insertionPosition;
        }

        private void alignLeft_Click(object sender, RoutedEventArgs e)
        {

        }

        private void alignCentr_Click(object sender, RoutedEventArgs e)
        {

        }

        private void alignRight_Click(object sender, RoutedEventArgs e)
        {

        }

        private void alignJunify_Click(object sender, RoutedEventArgs e)
        {

        }

        private void insertPicButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void markerButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void rotateButton_Click(object sender, RoutedEventArgs e)
        {
            if (richTextBox.Width == 595)
            {
                richTextBox.Height = 595;
                richTextBox.Width = 842;
                richTextBox.Document.PageWidth = 842;
                richTextBox.Document.PageHeight = 595;

            }
            else
            {
                richTextBox.Height = 842;
                richTextBox.Width = 595;
                richTextBox.Document.PageWidth = 595;
                richTextBox.Document.PageHeight = 842;
            }
        }
    }
}