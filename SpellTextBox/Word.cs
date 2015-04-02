using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpellTextBox
{
    public class Word
    {
        int _index;

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        string _text;

        public string Text
        {
            get { return _text; }
            set { _text = value; }
        }

        public int Length
        {
            get { return _text.Length; }
        }

        public Word()
        {
        }

        public Word(string text, int index)
        {
            _index = index;
            _text = text;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
