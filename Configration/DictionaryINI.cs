using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace YCabz.Configration
{
    public class DictionaryINI
    {
        /// <summary>
        /// INI File 경로
        /// </summary>
        public string Filepath
        {
            get => _Filepath;
            set
            {
                _Filepath = value;
                isLoaded = false;
            }
        }
        private string _Filepath;

        /// <summary>
        /// Encoding
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.Default;

        /// <summary>
        /// Comment 구분자
        /// </summary>
        /// <example>Key='Value' ;Comment</example>
        public static char CommentSeparator
        {
            get => _CommentSeparator;
            set
            {
                if (value == '[' || value == ']' || value == '=' || value == '\'')
                {
                    throw new InvalidDataException("Invalid Splitor: ('[', ']', '=', '\'')");
                }

                _CommentSeparator = value;
            }
        }
        private static char _CommentSeparator = ';';


        private Dictionary<DictionaryINIString, DictionaryINIKeyValuePairs> dictionary = new Dictionary<DictionaryINIString, DictionaryINIKeyValuePairs>();
        private string commentSplitorHint;
        private bool isLoaded;


        public DictionaryINIKeyValuePairs this[DictionaryINIString section]
        {
            get
            {
                if (isLoaded == false)
                {
                    Load();
                }

                if (dictionary.ContainsKey(section) == false)
                {
                    dictionary.Add(section, new DictionaryINIKeyValuePairs());
                }

                return dictionary[section];
            }
            set
            {
                dictionary[section] = value;
            }
        }


        public DictionaryINI()
        {
        }

        public DictionaryINI(string filepath)
        {
            Filepath = filepath;
        }

        /// <summary>
        /// INIFIle초기화
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();
        }

        /// <summary>
        /// INI파일 정보에 쓰기
        /// </summary>
        public void Load()
        {
            Clear();

            if (File.Exists(Filepath) == false)
            {
                throw new FileNotFoundException($"{Filepath}");
            }

            try
            {
                var textLines = File.ReadAllLines(Filepath, Encoding);
                var section = default(DictionaryINIString);
                foreach (var textLine in textLines)
                {
                    var trimmed = textLine.Trim();

                    // Find Section
                    if (string.IsNullOrEmpty(trimmed) == true)
                    {
                        continue;
                    }

                    var braketStartMarkIndex = trimmed.IndexOf('[');
                    var braketEndMarkIndex = trimmed.IndexOf(']');
                    var equalMarkIndex = trimmed.IndexOf('=');

                    if (braketStartMarkIndex >= 0 && braketStartMarkIndex < braketEndMarkIndex
                        && (equalMarkIndex < 0 || equalMarkIndex > braketEndMarkIndex))
                    {
                        section = trimmed.Substring(braketStartMarkIndex + 1, braketEndMarkIndex - braketStartMarkIndex - 1);
                        dictionary[section] = new DictionaryINIKeyValuePairs();
                        continue;
                    }

                    // If No Section
                    // Comment=# 'Separator Hint'
                    if (string.IsNullOrEmpty(section) == true && trimmed.StartsWithIgnoreCase("Comment"))
                    {
                        var maybeSeparatorHintLine = trimmed.Split('=');
                        if (maybeSeparatorHintLine.Length > 1 && char.TryParse(maybeSeparatorHintLine[1].Trim(' ', '\'', '\t'), out var splitor))
                        {
                            commentSplitorHint = textLine;
                            CommentSeparator = splitor;
                        }
                        continue;
                    }

                    if (equalMarkIndex > 0 && trimmed.Last() != '=')
                    {
                        var key = trimmed.Substring(0, equalMarkIndex).Trim();
                        var valueCommentPairs = trimmed.Substring(equalMarkIndex + 1, trimmed.Length - 1 - equalMarkIndex)
                            .SplitWithoutEmptyEntries(CommentSeparator);

                        // valueCommentPairs의 길이가 1보다 크면  Comment가 존재함
                        dictionary[section][key] = new DictionaryINIValue(valueCommentPairs[0].Trim('\'', '\t', ' '), valueCommentPairs.Length > 1 ? valueCommentPairs[1].Trim() : string.Empty);
                    }
                }

                isLoaded = true;
            }
            catch (Exception ex)
            {
                isLoaded = false;
                throw new Exception($"INIFile.Load, {ex.Message}");
            }
        }

        /// <summary>
        /// INI파일 정보에 쓰기
        /// </summary>
        public void Load(string filepath)
        {
            Filepath = filepath;
            Load();
        }

        /// <summary>
        /// INI File Save
        /// </summary>
        public void Save()
        {
            if (string.IsNullOrWhiteSpace(Filepath) == true)
            {
                throw new InvalidDataException($"Invalid Filepath");
            }

            var stringbuilder = new StringBuilder(1024);

            if (string.IsNullOrEmpty(commentSplitorHint) == false)
            {
                stringbuilder.AppendLine(commentSplitorHint);
                stringbuilder.AppendLine();
            }

            foreach (var skPair in dictionary)
            {
                var iniString = skPair.Key;
                var keyValuePairs = skPair.Value;

                // Section
                stringbuilder.Append('[');
                stringbuilder.Append(iniString);
                stringbuilder.Append(']');
                stringbuilder.AppendLine();

                foreach (var kvPair in keyValuePairs.Dictionary)
                {
                    var key = kvPair.Key;
                    var value = kvPair.Value.Value;
                    var comment = kvPair.Value.Comment;

                    // key=value; comment
                    stringbuilder.Append(key);
                    stringbuilder.Append('=');
                    stringbuilder.Append('\'');
                    stringbuilder.Append(value);
                    stringbuilder.Append('\'');

                    if (string.IsNullOrWhiteSpace(comment) == false)
                    {
                        stringbuilder.Append('\t', 2);
                        stringbuilder.Append(CommentSeparator);
                        stringbuilder.Append(comment);
                    }
                    stringbuilder.AppendLine();
                }
                stringbuilder.AppendLine();
            }

            File.WriteAllText(Filepath, stringbuilder.ToString(), Encoding);
        }
    }
}
