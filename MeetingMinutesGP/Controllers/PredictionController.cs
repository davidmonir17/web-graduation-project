using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using F23.StringSimilarity;
using System.Threading.Tasks;
using System.IO;
using System.Collections;
using MeetingMinutesGP;
using MeetingMinutesGP.Models;
using Newtonsoft.Json;

namespace MeetingMinutesGP.Controllers
{
    public class PredictionController : Controller
    {
        static List<string> phrases = new List<string>();
        static void OnLoop(List<List<string>> marrpLists, int[] marriIndex)
        {
            string ph = "";
            ph += marrpLists[0][marriIndex[0]];
            for (int idx = 1; idx < marriIndex.Length; idx++)
            {
                ph += (" " + marrpLists[idx][marriIndex[idx]]);
            }
            phrases.Add(ph);
        }

        static void Increment(int idx, int[] marriIndex, List<List<string>> marrpLists)
        {
            marriIndex[idx]++;
            if (marriIndex[idx] >= marrpLists[idx].Count)
            {
                if (idx > 0)
                {
                    Increment(idx - 1, marriIndex, marrpLists);
                    marriIndex[idx] = 0;
                }
            }
        }

        static void GetPhrases(List<List<string>> marrpLists)
        {
            int[] marriIndex = new int[marrpLists.Count];

            for (int idx = 0; idx < marriIndex.Length; idx++)
                marriIndex[idx] = 0;
            while (marriIndex[0] < marrpLists[0].Count)
            {
                OnLoop(marrpLists, marriIndex);
                Increment(marriIndex.Length - 1, marriIndex, marrpLists);
            }
        }

        // GET: Prediction
        public ActionResult Index(string id)
        {
            id = id.ToLower();
            string[] arrToCheck = new string[] { "ourselves", "hers", "between", "yourself", "but", "again", "there", "about", "once", "during", "out", "very", "having", "with", "they", "own", "an", "be", "some", "for", "do", "its", "yours", "such", "into", "of", "most", "itself", "other", "off", "is", "s", "am", "or", "who", "as", "from", "him", "each", "the", "themselves", "until", "below", "are", "we", "these", "your", "his", "through", "don", "nor", "me", "were", "her", "more", "himself", "this", "down", "should", "our", "their", "while", "above", "both", "up", "to", "ours", "had", "she", "all", "no", "when", "at", "any", "before", "them", "same", "and", "been", "have", "in", "will", "on", "does", "yourselves", "then", "that", "because", "what", "over", "why", "so", "can", "did", "not", "now", "under", "he", "you", "herself", "has", "just", "where", "too", "only", "myself", "which", "those", "i", "after", "few", "whom", "t", "being", "if", "theirs", "my", "against", "a", "by", "doing", "it", "how", "further", "was", "here", "than", "" };
            string input = (id);
            string[] arr = input.Split(' ');
            List<string> AfterRemovingPunc = new List<string>();
            string output = "";
            bool flag = false;
            foreach (string value in arr)
            {
                AfterRemovingPunc.Add(TrimPunctuation(value));
            }
            for (int i = 0; i < AfterRemovingPunc.Count; i++)
            {
                if (!arrToCheck.Contains(AfterRemovingPunc[i]))
                {

                    if (flag == false)
                    {
                        output = output + AfterRemovingPunc[i];
                        flag = true;
                    }
                    else
                    {
                        output = output + " " + AfterRemovingPunc[i];
                    }
                }
            }
            string[] TopicWords = output.Split(' ');
            TopicWords = TopicWords.Distinct().ToArray();
            List<List<string>> WordWithMeanings = new List<List<string>>();
            using (var thesaurus = new Thesaurus())
            {
                foreach (var term in TopicWords)
                {
                    List<string> lst = new List<string>();
                    lst.Add(term);
                    foreach (var value in thesaurus.GetSynonyms(term))
                    {
                        lst.Add(value);
                    }
                    WordWithMeanings.Add(lst);
                }
            }
            phrases.Clear();
            GetPhrases(WordWithMeanings);
            
            GPEntities db = new GPEntities();
            var topicList = db.Topics.ToList();
            List<TopicTimePred> SimilarTopics = new List<TopicTimePred>();
            for(int i = 0;i < phrases.Count; i++)
            {
                for(int j=0;j< topicList.Count; j++)
                {
                    string TopicTimePredTopicTime = topicList[j].TopicTime.ToString();
                    if (TopicTimePredTopicTime == null|| TopicTimePredTopicTime=="")
                    {
                        continue;
                    }
                    Cosine t = new Cosine();
                    double r = t.Similarity(phrases[i], topicList[j].TopicImpWords);
                    if (r > 0)
                    {
                        TopicTimePred obj = new TopicTimePred();
                        obj.TopicName = topicList[j].TopicImpWords;
                        obj.TopicTime = int.Parse(TopicTimePredTopicTime);
                        
                        
                        if (topicList[j].TopicPriority == "Urgent")
                        {
                            obj.similarity = r;
                        }
                        else if (topicList[j].TopicPriority == "High")
                        {
                            obj.similarity = r * .90;
                        }
                        else if (topicList[j].TopicPriority == "Medium")
                        {
                            obj.similarity = r * .75;
                        }
                        else
                        {
                            obj.similarity = r * .50;
                        }
                        
                        SimilarTopics.Add(obj);
                    }
                    //Console.WriteLine(d + "," + phrases[i] + "," + r);
                }
                
            }
            SimilarTopics = SimilarTopics.OrderByDescending(o => o.similarity).ToList();

            SimilarTopics = SimilarTopics.Where(o => o.similarity == SimilarTopics[0].similarity).ToList();
            Double predictedtimedouble = SimilarTopics.Sum(item => item.TopicTime) / SimilarTopics.Count;
            int predictedtime =(int) Math.Ceiling(predictedtimedouble);

            string valueoj = string.Empty;
            valueoj = JsonConvert.SerializeObject(predictedtime.ToString(), Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });
            return Content(valueoj);
        }
        static string TrimPunctuation(string value)
        {
            // Count start punctuation.
            int removeFromStart = 0;
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromStart++;
                }
                else
                {
                    break;
                }
            }

            // Count end punctuation.
            int removeFromEnd = 0;
            for (int i = value.Length - 1; i >= 0; i--)
            {
                if (char.IsPunctuation(value[i]))
                {
                    removeFromEnd++;
                }
                else
                {
                    break;
                }
            }
            // No characters were punctuation.
            if (removeFromStart == 0 &&
                removeFromEnd == 0)
            {
                return value;
            }
            // All characters were punctuation.
            if (removeFromStart == value.Length &&
                removeFromEnd == value.Length)
            {
                return "";
            }
            // Substring.
            return value.Substring(removeFromStart,
                value.Length - removeFromEnd - removeFromStart);
        }
    }
}
public static class Combiations
{
    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {
        if (sequences == null)
        {
            return null;
        }

        IEnumerable<IEnumerable<T>> emptyProduct = new[] { Enumerable.Empty<T>() };

        return sequences.Aggregate(
            emptyProduct,
            (accumulator, sequence) => accumulator.SelectMany(
                accseq => sequence,
                (accseq, item) => accseq.Concat(new[] { item })));
    }
}

public class Thesaurus : IDisposable
{

    private Microsoft.Office.Interop.Word.Application appWord;
    private object objLanguage;

    public Thesaurus()
    {
        appWord = new Microsoft.Office.Interop.Word.Application();
        objLanguage = Microsoft.Office.Interop.Word.WdLanguageID.wdEnglishUS;
    }

    public IEnumerable<string> GetSynonyms(string term)
    {
        var si = appWord.get_SynonymInfo(term, ref (objLanguage)); //Microsoft.Office.Interop.Word.SynonymInfo
        int counter = 0;
        foreach (var meaning in (si.MeaningList as Array))
        {
            yield return meaning.ToString();
            counter++;
            if (counter == 4)
                break;
        }
        System.Runtime.InteropServices.Marshal.ReleaseComObject(si);
        si = null;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        objLanguage = null;
        if (disposing)
        {
            if (appWord != null)
            {
                appWord.Quit(); //if this isn't called, you'll see winword.exe hanging around in your processes
                System.Runtime.InteropServices.Marshal.ReleaseComObject(appWord);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        appWord = null;
    }
}