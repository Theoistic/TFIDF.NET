/* Copyright (C) 2021 Theodor Solbjorg - All Rights Reserved
 * You may use, distribute and modify this code under the
 * terms of the GPLv2 license
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace TFIDF.NET
{
    public enum TFProc { Binary, Count, Freq, Log }
    public enum IDFProc { Avg, Sum, Arg }
    public static class TFIDF
    {
        public class Paragraph
        {
            public string Text { get; set; }
            public List<Term> Terms { get; set; }
            public double Score { get; set; }
            public class Term
            {
                public string Word { get; set; }
                public bool Stopword { get; set; }
                public double TF { get; set; }
                public double IDF { get; set; }
                public double Score => TF * IDF;
            }
        }

        public static List<Paragraph> TF(this string[] str, TFProc proc = TFProc.Count, string[] stopwords = null)
        {
            if (stopwords == null)
            {
                stopwords = string.Join(" ", str).Tokenize().Select(x => x.ToLower()).Distinct().Where(x => x.Length <= 3).ToArray();
            }
            List<Paragraph> paragraphs = new List<Paragraph>();
            Func<TFProc, string, string[], double> freq = (f, t, d) =>
            {
                switch (f)
                {
                    case TFProc.Binary:
                        return d.Any(y => y == t) ? 1 : 0;
                    case TFProc.Count:
                        return d.Count(y => y == t);
                    case TFProc.Log:
                        return Math.Log(1 + d.Count(y => y == t));
                    case TFProc.Freq:
                        return d.Count() / 1;
                    /*case TFProc.Arg:
                        return d.Count(y => y == t) / d.Zipf().FirstOrDefault().Value;*/
                    default: throw new Exception($"Enum {typeof(TFProc).Name} is out of bounds.");
                }
            };
            foreach (string s in str)
            {
                string[] wordList = s.Tokenize();
                paragraphs.Add(new Paragraph
                {
                    Text = s,
                    Terms = wordList.Distinct().Select(x => new Paragraph.Term
                    {
                        Word = x,
                        TF = freq(proc, x, wordList),
                        Stopword = stopwords.Contains(x.ToLower())
                    }).ToList()
                });
            }
            return paragraphs.Where(x => x.Text.Length > 0).ToList();
        }

        public static List<Paragraph> IDF(this List<Paragraph> paragraphs, IDFProc proc = IDFProc.Avg)
        {
            double pCount = paragraphs.Count();
            paragraphs.ForEach(p => {
                p.Terms.ForEach(x => {
                    double termCount = (1 + paragraphs.Count(g => g.Terms.Any(t => t.Word.ToLower() == x.Word.ToLower()))); // (1 + (Doc Term Bool).Count)
                    x.IDF = Math.Log(pCount / termCount);
                });
                switch (proc)
                {
                    case IDFProc.Avg: p.Score = p.Terms.Where(x => !x.Stopword).Average(x => x.Score); break;
                    case IDFProc.Sum: p.Score = p.Terms.Where(x => !x.Stopword).Sum(x => x.Score); break;
                    case IDFProc.Arg: p.Score = Math.Log(1 + p.Terms.Where(x => !x.Stopword).Sum(x => x.Score) / paragraphs.Sum(x => x.Terms.Where(g => !g.Stopword).Count())); break;
                }
            });
            return paragraphs;
        }

        public static string[] Tokenize(this string self)
        {
            self = Regex.Replace(self, "<[^<>]+>", "");
            self = Regex.Replace(self, "[0-9]+", "number");
            self = Regex.Replace(self, @"(http|https)://[^\s]*", "httpaddr");
            self = Regex.Replace(self, @"[^\s]+@[^\s]+", "emailaddr");
            self = Regex.Replace(self, "[$]+", "dollar");
            self = Regex.Replace(self, @"@[^\s]+", "username");
            return self.Split(" @$/#.-:&*+=[]?!(){},''\">_<;%\\".ToCharArray());
        }

        public static Dictionary<string, int> Zipf(this string self)
        {
            return self.Tokenize().Zipf();
        }

        public static Dictionary<string, int> Zipf(this string[] self)
        {
            if (self.Any(x => x.Contains(' ')))
            {
                return string.Join(" ", self).Zipf();
            }
            return self.GroupBy(ot => ot).OrderByDescending(x => x.Count()).ToDictionary(ot => ot.Key, ot => ot.Count());
        }

    }
}
