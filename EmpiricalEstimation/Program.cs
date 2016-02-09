using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpiricalEstimation
{
    class Program
    {
        static void Main (string[] args)
        {
            string TrainingFile = args[0];
            string OutPutFile = args[1];
            string line;

            //Dictionary<String,List<string>> TrainDocs = new Dictionary<string,List<string>>();
            List<String> Vocab = new List<string>();
            Dictionary<String, Dictionary<string, double>> ClassFeatureTemp = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<String, Dictionary<string, double>> ObservedFeatureProb = new Dictionary<string, Dictionary<string, double>>();
            List<String> ClassList = new List<string>();
            string classLabel,key;
            int index,totalFeatures, docCount=0;
            double value;
            // this will give us basic counts for each feature in a class. we will have to convert it into prob
            using(StreamReader Sr = new StreamReader(TrainingFile))
            {
                while((line = Sr.ReadLine())!=null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;
                    string[] words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    classLabel = words[0];
                    ClassList.Add(classLabel);
                    docCount++;
                    for (int i = 1; i < words.Length; i++)
                    {
                        index = words[i].IndexOf(":");
                        key = words[i].Substring(0, index);
                        value = Convert.ToDouble(words[i].Substring(index + 1));
                        if (value <= 0)
                            continue;
                        Vocab.Add(key);
                        if (ClassFeatureTemp.ContainsKey(classLabel) && ClassFeatureTemp[classLabel].ContainsKey(key))
                            ClassFeatureTemp[classLabel][key]++;
                        else if (ClassFeatureTemp.ContainsKey(classLabel))
                            ClassFeatureTemp[classLabel].Add(key, value);
                        else
                        {
                            Dictionary<string, double> temp = new Dictionary<string, double>();
                            temp.Add(key, 1);
                            ClassFeatureTemp.Add(classLabel, temp);
                        }
                    }

                }
            }
            ClassList = ClassList.Distinct().ToList();
            Vocab = Vocab.Distinct().ToList();
            totalFeatures = ClassList.Count * Vocab.Count;
            foreach (var cl in ClassList)
            {
                ObservedFeatureProb.Add(cl, new Dictionary<string, double>());
                var features = ClassFeatureTemp[cl];
                foreach (var word in Vocab)
                {
                    if (features.ContainsKey(word))
                        ObservedFeatureProb[cl].Add(word, features[word]);
                    else
                        ObservedFeatureProb[cl].Add(word, 0);//TODO: See if we have to add smoothing else this is not necessary

                }
                //ObservedFeatureProb[cl]=ObservedFeatureProb[cl].OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }

            //Note we divide by the total Doc Count and not by total feature Count
            using (StreamWriter Sw = new StreamWriter(OutPutFile))
            {
                //write SysOutput
                foreach (var clData in ObservedFeatureProb)
                {
                    var featList = clData.Value;
                    //Sw.WriteLine("FEATURES FOR CLASS " + clData.Key);
                    foreach (var fl in featList)
                    {
                        Sw.WriteLine(clData.Key + " " + fl.Key + " " + fl.Value / docCount + " " + fl.Value);
                    }

                }

            }
            
        }
    }
}
