using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TFIDF.NET.Tests
{
    [TestClass]
    public class TFIDFTESTS
    {
        [TestMethod]
        public void Test1()
        {
            string[] text = new string[] { 
                "This is some sample text which contains no real information",
                "Donald Trump is an idiot",
                "Twitter sentiment is an alright sounds for ML",
                "Python vs Swifty, i guess one can pick the lesser of two evils"
            };
            var result = TFIDF.TF(text, TFProc.Freq, new string[] { }).IDF(IDFProc.Avg);
            


        }
    }
}
