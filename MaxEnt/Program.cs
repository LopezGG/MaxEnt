using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxEnt
{
    class Program
    {
        static void Main (string[] args)
        {
            string TestFile = args[0];
            string ModelFile = args[1];
            string sysOutput = args[2];
            string line;
            Dictionary<String, int> ConfusionDict = new Dictionary<string, int>();

            //read the model into a DS
            Dictionary<String, Dictionary<String,double>> ModelFeatureClassProb = new Dictionary<string, Dictionary<String,double>>();
            List<String> ClassList = new List<string>();
            string classLabel="", key;
            double value;
            using(StreamReader Sr =  new StreamReader(ModelFile))
            {
                while((line = Sr.ReadLine())!=null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;
                    if (line.Contains("FEATURES FOR CLASS"))
                    {
                        classLabel = line.Substring(line.IndexOf("FEATURES FOR CLASS") + 19);
                        ClassList.Add(classLabel);
                        continue;
                    }
                    else
                    {
                        string[] words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                        key = words[0];
                        value = Convert.ToDouble(words[1]);
                        if (ModelFeatureClassProb.ContainsKey(classLabel) && !ModelFeatureClassProb[classLabel].ContainsKey(key))
                            ModelFeatureClassProb[classLabel].Add(key, value);
                        else if (!ModelFeatureClassProb.ContainsKey(classLabel))
                        {
                            Dictionary<String,double> temp = new Dictionary<string,double>();
                            temp.Add(key,value);
                            ModelFeatureClassProb.Add(classLabel, temp);
                        }
                            

                    }
                }
            }
            ClassList = ClassList.Distinct().ToList();

            string actualClass = "",predictedClass="";
            int index,totalDoc=0;
            StreamWriter Sw = new StreamWriter(sysOutput);
            //reading the TestFile
            using (StreamReader Sr = new StreamReader(TestFile))
            {
                while((line = Sr.ReadLine())!=null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;
                    Dictionary<string, double> TestClassScore = new Dictionary<string, double>();
                    string[] words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    actualClass = words[0];

                    double totalScore = 0;
                    foreach (var predClass in ClassList)
                    {
                        double score =0;
                        var featureList = ModelFeatureClassProb[predClass];
                        for (int i = 1; i < words.Length; i++)
                        {
                            index = words[i].IndexOf(":");
                            key = words[i].Substring(0,index);
                            value = Convert.ToDouble(words[i].Substring(index + 1));
                            if (value <= 0)
                                continue;
                            if (featureList.ContainsKey(key))
                                score += featureList[key];
                            else
                                score += featureList["<default>"];
                        }
                        TestClassScore.Add(predClass, score);
                        totalScore += score;
                    }
                    //sorting these and dividing by total score to get class prob from scores
                    TestClassScore = TestClassScore.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value / totalScore);
                    //write to sysOutput
                    Sw.Write("array:" + totalDoc + "\t");
                    totalDoc++;
                    foreach (var item in TestClassScore)
                    {
                        Sw.Write(item.Key + "\t" + item.Value + "\t");
                    }
                    Sw.WriteLine();
                    predictedClass = TestClassScore.Keys.First();
                    key = actualClass + "_" + predictedClass;
                    if (ConfusionDict.ContainsKey(key))
                        ConfusionDict[key]++;
                    else
                        ConfusionDict.Add(key, 1);

                }
            }
            Sw.Close();


            //Write confusion Matrix to console. totalDoc is ok without -- because we counted from zero when we wrote the document.
            WriteConfusionMatrix(ClassList, ConfusionDict, "test", totalDoc);
            Console.ReadLine();
        }

        public static void WriteConfusionMatrix (List<String> ClassBreakDown, Dictionary<String, int> ConfusionDict, string testOrTrain, int totalInstances)
        {
            int correctPred = 0;
            Console.WriteLine("Confusion matrix for the " + testOrTrain + " data:\n row is the truth, column is the system output");
            Console.Write("\t\t\t");
            foreach (var actClass in ClassBreakDown)
            {
                Console.Write(actClass + "\t");
            }
            Console.WriteLine();
            foreach (var actClass in ClassBreakDown)
            {

                Console.Write(actClass + "\t");
                foreach (var predClass in ClassBreakDown)
                {

                    if (ConfusionDict.ContainsKey(actClass + "_" + predClass))
                    {
                        Console.Write(ConfusionDict[actClass + "_" + predClass] + "\t");
                        if (actClass == predClass)
                            correctPred += ConfusionDict[actClass + "_" + predClass];
                    }
                    else
                        Console.Write("0" + "\t");

                }
                Console.WriteLine();
            }
            Console.WriteLine(testOrTrain + " accuracy=" + Convert.ToString(correctPred / ( double )totalInstances));
            Console.WriteLine();


        }
    }
}
