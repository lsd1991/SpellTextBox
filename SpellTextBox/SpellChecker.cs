using NHunspell;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Input;
using System;
using System.IO;

namespace SpellTextBox
{
    public class SpellChecker : INotifyPropertyChanged
    {
        private SpellTextBox box;
        private Hunspell hunSpell;
        private List<Word> words;
        private ObservableCollection<Word> misspelledWords;
        private ObservableCollection<Word> suggestedWords;
        private List<Word> ignoredWords;
        private Word selectedMisspelledWord;
        private Word selectedSuggestedWord;

        public SpellChecker(Hunspell HunSpell, SpellTextBox Parent)
        {
            hunSpell = HunSpell; 
            box = Parent;
            Words = new List<Word>();
            MisspelledWords = new ObservableCollection<Word>();
            IgnoredWords = new List<Word>();
            SuggestedWords = new ObservableCollection<Word>();
        }

        public void Dispose()
        {
            hunSpell.Dispose();
        }

        public List<Word> Words
        {
            get { return words; }
            set
            {
                words = value;
                OnPropertyChanged("Words");
            }
        }

        public ObservableCollection<Word> MisspelledWords
        {
            get { return misspelledWords; }
            set
            {
                misspelledWords = value;
                OnPropertyChanged("MisspelledWords");
                if (SelectedMisspelledWord != null)
                    SelectedMisspelledWord = null;
            }
        }

        public ObservableCollection<MenuAction> MenuActions
        {
            get
            {
                List<MenuAction> commands = SuggestedWords.Select(w => new MenuAction()
                {
                    Name = w.Text,
                    Command = new DelegateCommand(
                        delegate
                        {
                            box.ReplaceSelectedWord(w);
                        })
                }).ToList();

                if (commands.Count == 0)
                {
                    commands.Add(new MenuAction()
                    {
                        Name = StringResources.Copy,
                        Command = ApplicationCommands.Copy
                    });
                    commands.Add(new MenuAction()
                    {
                        Name = StringResources.Cut,
                        Command = ApplicationCommands.Cut
                    });
                    commands.Add(new MenuAction()
                    {
                        Name = StringResources.Paste,
                        Command = ApplicationCommands.Paste
                    });
                }
                else
                {
                    commands.Add(new MenuAction()
                    {
                        Name = StringResources.AddCustom,
                        Command = new DelegateCommand(
                            delegate
                            {
                                SaveToCustomDictionary(SelectedMisspelledWord.Text);

                                box.FireTextChangeEvent();
                            })
                    });
                }

                return new ObservableCollection<MenuAction>(commands);
            }
        }

        public ObservableCollection<Word> SuggestedWords
        {
            get { return suggestedWords; }
            set
            {
                suggestedWords = value;
                OnPropertyChanged("SuggestedWords");
                box.CreateContextMenu();
            }
        }

        public List<Word> IgnoredWords
        {
            get { return ignoredWords; }
            set
            {
                ignoredWords = value;
                OnPropertyChanged("IgnoredWords");
            }
        }

        public Word SelectedMisspelledWord
        {
            get { return selectedMisspelledWord; }
            set
            {
                selectedMisspelledWord = value;
                LoadSuggestions(value);
                OnPropertyChanged("SelectedMisspelledWord");
                OnPropertyChanged("IsReplaceEnabled");
                box.CreateContextMenu();
            }
        }

        public Word SelectedSuggestedWord
        {
            get { return selectedSuggestedWord; }
            set
            {
                selectedSuggestedWord = value;
                OnPropertyChanged("SelectedSuggestedWord");
                OnPropertyChanged("IsReplaceEnabled");
            }
        }

        public void LoadSuggestions(Word misspelledWord)
        {
            if (misspelledWord != null)
            {
                SuggestedWords = new ObservableCollection<Word>(hunSpell.Suggest(misspelledWord.Text).Select(s => new Word(s, misspelledWord.Index)));
                if (SuggestedWords.Count == 0) SuggestedWords = new ObservableCollection<Word> { new Word(StringResources.NoSuggestions, 0) };
            }
            else
            {
                SuggestedWords = new ObservableCollection<Word>();
            }
        }

        public void ClearLists()
        {
            Words.Clear();
            MisspelledWords.Clear();
        }

        public void CheckSpelling(string content)
        {
            if (box.IsSpellCheckEnabled)
            {
                ClearLists();

                var matches = Regex.Matches(content, @"\w+[^\s]*\w+|\w");

                foreach (Match match in matches)
                {
                    Words.Add(new Word(match.Value.Trim(), match.Index));
                }

                foreach (var word in Words)
                {
                    bool isExist = hunSpell.Spell(word.Text);
                    if (!isExist) MisspelledWords.Add(word);
                }

                OnPropertyChanged("MisspelledWords");
            }
        }

        public void LoadCustomDictionary()
        {
            string[] strings = File.ReadAllLines(box.CustomDictionaryPath);
            foreach (var str in strings)
            {
                hunSpell.Add(str);
            }
        }

        public void SaveToCustomDictionary(string word)
        {
            File.AppendAllText(box.CustomDictionaryPath, string.Format("{0}{1}", word.ToLower(), Environment.NewLine));
            hunSpell.Add(word);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null) this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
