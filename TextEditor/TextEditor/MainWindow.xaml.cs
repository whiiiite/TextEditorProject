//using PdfSharp.Drawing;
//using PdfSharp.Drawing.Layout;
//using PdfSharp.Pdf;
//using PdfSharp.Pdf.IO;
//using System;
using System.ComponentModel;
using System.Windows;
//using System.Windows.Shapes;
//using PdfiumViewer;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TextEditor.FileSystem;
using TextEditor.Util;
using FontFamily = System.Windows.Media.FontFamily;

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
        Binding binding = new Binding("Scale");
        private TextPointer selectionStart;
        private TextPointer selectionEnd;


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
            Populator.PopulateSystemFonts(FontComboBox);
            Populator.PopulateFontSizes(FontSizeComboBox);
            Populator.PopulateColors(ColorComboBox);
            Populator.PopulateBackgroundColors(BackgroundColorComboBox);
            UpdateMainWindowTitle();
            richTextBox.TextChanged += RichTextBox_TextChanged;
            //Binding binding = new Binding("Scale");
            binding.Source = this;
            scaleSlider.SetBinding(Slider.ValueProperty, binding);
            scaleSlider.ValueChanged += ScaleSlider_ValueChanged;
            UpdateScaleValueLabel();
            richTextBox.PreviewMouseDown += RichTextBox_PreviewMouseDown;
            richTextBox.PreviewMouseMove += RichTextBox_PreviewMouseMove;
            richTextBox.PreviewMouseUp += RichTextBox_PreviewMouseUp;
        }

        private Point startPoint;
        private bool isResizing = false;

        private void RichTextBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                startPoint = e.GetPosition(richTextBox);
                isResizing = true;
            }
        }

        private void RichTextBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                Point currentPoint = e.GetPosition(richTextBox);
                double widthChange = currentPoint.X - startPoint.X;
                double heightChange = currentPoint.Y - startPoint.Y;

                // Получаем текущий элемент под курсором
                TextPointer position = richTextBox.GetPositionFromPoint(currentPoint, true);
                Inline currentInline = position?.Parent as Inline;

                if (currentInline != null)
                {
                    // Обновляем шрифт в FontComboBox
                    FontComboBox.SelectedItem = Fonts.SystemFontFamilies.FirstOrDefault(font => font.Source == (currentInline.FontFamily ?? richTextBox.FontFamily).Source);

                    // Обновляем размер шрифта в FontSizeComboBox
                    FontSizeComboBox.SelectedItem = currentInline.FontSize;
                }

                // Ваш код для изменения размера контейнера (например, изменение ширины и высоты)
                ResizeContainer(widthChange, heightChange);

                // Обновляем startPoint
                startPoint = currentPoint;
            }
        }

        private void ResizeContainer(double widthChange, double heightChange)
        {
            InlineUIContainer selectedContainer = GetSelectedImageContainer();

            if (selectedContainer != null)
            {
                Image image = selectedContainer.Child as Image;

                if (image != null)
                {
                    try
                    {
                        // Изменение размера изображения
                        image.Width += widthChange;
                        image.Height += heightChange;

                    }
                    catch { }
                }
            }
        }

        private InlineUIContainer GetSelectedImageContainer()
        {
            TextPointer selectionStart = richTextBox.Selection.Start;
            TextPointer selectionEnd = richTextBox.Selection.End;

            if (selectionStart != null && selectionEnd != null)
            {
                TextRange selectedTextRange = new TextRange(selectionStart, selectionEnd);

                foreach (Inline inline in selectedTextRange.Start.Paragraph.Inlines)
                {
                    if (inline is InlineUIContainer inlineUIContainer && inlineUIContainer.Child is Image)
                    {
                        return inlineUIContainer;
                    }
                }
            }

            return null;
        }

        private void RichTextBox_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isResizing = false;
            }
        }

        private void ScaleSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Scale = e.NewValue;
            ApplyScale();
            UpdateScaleValueLabel();
        }
        private void UpdateScaleValueLabel()
        {
            scaleValueLabel.Text = $"{Scale*100:F0}%";
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
            DocumentsHandler.SaveDocument(sender, e, currentFileName, richTextBox);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            currentFileName = DocumentsHandler.SaveDocumentAs(currentFileName, richTextBox);
            UpdateMainWindowTitle();
        }


        private void Open_Click(object sender, RoutedEventArgs e) //LoadDocument(string filePath)
        {
            currentFileName = DocumentsHandler.Open(richTextBox, currentFileName);
            MessageBox.Show(currentFileName);
            UpdateMainWindowTitle();
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
            richTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Left);
        }

        private void alignCentr_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Center);
        }

        private void alignRight_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Right);
        }

        private void alignJustify_Click(object sender, RoutedEventArgs e)
        {
            richTextBox.Selection.ApplyPropertyValue(Paragraph.TextAlignmentProperty, TextAlignment.Justify);
        }

        private void insertPicButton_Click(object sender, RoutedEventArgs e)
        {

            // Проверка, открыт ли документ
            if (richTextBox.CaretPosition.Paragraph != null)
            {
                // Создание диалогового окна для выбора файла
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Filter = "Изображения (*.png; *.jpg; *.jpeg; *.gif; *.bmp)|*.png;*.jpg;*.jpeg;*.gif;*.bmp|Все файлы (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    // Получение пути к выбранному файлу
                    string imagePath = openFileDialog.FileName;

                    // Получение текущей ширины страницы
                    double pageWidth = richTextBox.ExtentWidth;
                    double newWidth;
                    double newHeight;
                    // Создание объекта BitmapImage
                    BitmapImage bitmap = new BitmapImage(new Uri(imagePath));

                    // Вычисление пропорционального размера изображения
                    double aspectRatio = bitmap.Width / bitmap.Height;
                    if (bitmap.Width > pageWidth)
                    {
                        newWidth = pageWidth; // новая ширина равна ширине страницы
                        newHeight = newWidth / aspectRatio; // новая высота с учетом соотношения сторон
                    }
                    else
                    {
                        newWidth = bitmap.Width;
                        newHeight = bitmap.Height;
                    }
                    // Создание элемента Image
                    Image image = new Image();
                    image.Source = bitmap;
                    image.Width = newWidth;
                    image.Height = newHeight;

                    // Создание контейнера для вставки изображения в RichTextBox
                    InlineUIContainer container = new InlineUIContainer(image, richTextBox.CaretPosition);

                    // Добавление контейнера к RichTextBox
                    richTextBox.CaretPosition.Paragraph.Inlines.Add(container);
                }
            }
            else
            {
                // Ваш код для случая, когда документ не открыт
                MessageBox.Show("The document is not open. Open your document before inserting the image.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            DocumentsHandler.SaveDocument(sender, e, currentFileName, richTextBox);
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

        private void zoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            Scale -= 0.01;
            scaleSlider.SetBinding(Slider.ValueProperty, binding);
        }

        private void zoomInButton_Click(object sender, RoutedEventArgs e)
        {
            Scale += 0.01;
            scaleSlider.SetBinding(Slider.ValueProperty, binding);
        }
    }
}