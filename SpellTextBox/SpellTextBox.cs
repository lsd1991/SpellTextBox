using NHunspell;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace SpellTextBox
{
    public class SpellTextBox : TextBox
    {
        static SpellTextBox()
        {
            TextProperty.OverrideMetadata(typeof(SpellTextBox), new FrameworkPropertyMetadata(new PropertyChangedCallback(TextPropertyChanged)));
        }

        public SpellTextBox() : base()
        {
            this.SelectionChanged += this.OnSelectionChanged;
            CreateTimer();
        }

        #region Timer

        static System.Timers.Timer timer = new System.Timers.Timer(500);

        void CreateTimer()
        {
            timer.AutoReset = false;
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
        }

        private static void TextPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            timer.Stop();
            timer.Start();
        }

        private void timer_Elapsed(object sender,
        System.Timers.ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new System.Action(() => 
            { 
                Checker.CheckSpelling(Text);
                RaiseSpellcheckCompletedEvent();
            }));
        }

        #endregion

        #region SpellcheckCompleted Event

        public static readonly RoutedEvent SpellcheckCompletedEvent = EventManager.RegisterRoutedEvent(
            "SpellcheckCompleted", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SpellTextBox));

        public event RoutedEventHandler SpellcheckCompleted
        {
            add { AddHandler(SpellcheckCompletedEvent, value); }
            remove { RemoveHandler(SpellcheckCompletedEvent, value); }
        }

        void RaiseSpellcheckCompletedEvent()
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(SpellTextBox.SpellcheckCompletedEvent);
            RaiseEvent(newEventArgs);
        }

        #endregion

        public static readonly DependencyProperty DictionaryPathProperty =
            DependencyProperty.Register(
            "DictionaryPath",
            typeof(string),
            typeof(SpellTextBox));

        public string DictionaryPath
        {
            get { return (string)this.GetValue(DictionaryPathProperty); }
            set { this.SetValue(DictionaryPathProperty, value); }
        }

        public static readonly DependencyProperty CustomDictionaryPathProperty =
            DependencyProperty.Register(
            "CustomDictionaryPath",
            typeof(string),
            typeof(SpellTextBox));

        public string CustomDictionaryPath
        {
            get { return (string)this.GetValue(CustomDictionaryPathProperty) ?? "custom.txt"; }
            set { this.SetValue(CustomDictionaryPathProperty, value); }
        }

        public static readonly DependencyProperty IsSpellCheckEnabledProperty =
            DependencyProperty.Register(
            "IsSpellCheckEnabled",
            typeof(bool),
            typeof(SpellTextBox));

        public bool IsSpellCheckEnabled
        {
            get { return (bool)this.GetValue(IsSpellCheckEnabledProperty); }
            set { this.SetValue(IsSpellCheckEnabledProperty, value); }
        }

        public void Initialize()
        {
            var myAdornerLayer = AdornerLayer.GetAdornerLayer(this);
            myAdornerLayer.Add(new RedUnderlineAdorner(this));
            CreateContextMenu();
        }

        private SpellChecker checker;

        public SpellChecker Checker
        {
            get { return checker ?? CreateSpellCheker(); }
            set { checker = value; }
        }

        private SpellChecker CreateSpellCheker()
        {
            checker = new SpellChecker(new Hunspell(DictionaryPath + ".aff", DictionaryPath + ".dic"), this);
            checker.LoadCustomDictionary();
            return checker;
        }

        public void CreateContextMenu()
        {
            if (checker != null)
            {
                var cm = new ContextMenu();
                foreach (var item in Checker.MenuActions)
                {
                    var mi = new MenuItem();
                    mi.Header = item.Name;
                    mi.Command = item.Command;
                    cm.Items.Add(mi);
                }
                this.ContextMenu = cm;
            }
        }

        public void ReplaceSelectedWord(Word WordToReplaceWith)
        {
            if (WordToReplaceWith.Text != StringResources.NoSuggestions)
            {
                int index = Checker.SelectedMisspelledWord.Index;
                string replacement = WordToReplaceWith.Text;
                Text = Text.Remove(index, Checker.SelectedMisspelledWord.Length).Insert(index, replacement);
                SelectionStart = index + WordToReplaceWith.Length;
            }
        }

        public void FireTextChangeEvent()
        {
            int c = SelectionStart;
            string s = Text;
            Text = s + " ";
            Text = s;
            SelectionStart = c;
        }

        protected void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (Checker.MisspelledWords.Any(w => SelectionStart >= w.Index && SelectionStart <= w.Index + w.Length))
                Checker.SelectedMisspelledWord = Checker.MisspelledWords.First(w => SelectionStart >= w.Index && SelectionStart <= w.Index + w.Length);
            else
                Checker.SelectedMisspelledWord = null;
        }

        private ICommand _replace;
        public ICommand Replace
        {
            get
            {
                return _replace ?? (_replace = new DelegateCommand(
                        delegate
                        {
                            ReplaceSelectedWord(checker.SelectedSuggestedWord);
                        }));
            }
        }
    }
}
