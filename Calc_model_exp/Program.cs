using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Calc_model_exp
{
    class Program
    {
        static void Main (string[] args)
        {
            bool ModelFilePresent=false;
            if (args.Length == 3)
                ModelFilePresent = true;
            else
                ModelFilePresent = false;
            string trainingFile = args[0];
            string OutPutFile = args[1];
            string ModelFile="";
            if(ModelFilePresent)
                ModelFile =args[2];
            Dictionary<string, Dictionary<string, double>> TrainingDict = new Dictionary<string, Dictionary<string, double>>();
            List<String> Vocab = new List<string>();
            List<String> ClassList = new List<string>();
            string line,key,classlabel;
            int index;
            double value;
            using (StreamReader Sr = new StreamReader(trainingFile))
            {

                //all I need is to get vocab , list of classes
                while ((line = Sr.ReadLine()) != null)
                {
                    if (String.IsNullOrEmpty(line))
                        continue;
                    string[] words = line.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
                    classlabel = words[0];
                    ClassList.Add(classlabel);
                    for (int i = 1; i < words.Length; i++)
                    {
                        index = words[i].IndexOf(":");
                        key = words[i].Substring(0,index);
                        Vocab.Add(key);
                        value = Convert.ToDouble(words[i].Substring(index+1));
                        if (TrainingDict.ContainsKey(classlabel) && TrainingDict[classlabel].ContainsKey(key))
                            TrainingDict[classlabel][key] += value;
                        else if (TrainingDict.ContainsKey(classlabel))
                            TrainingDict[classlabel].Add(key, value);
                        else
                        {
                            Dictionary<string, double> temp = new Dictionary<string, double>();
                            temp.Add(key, value);
                            TrainingDict.Add(classlabel, temp);
                        }
                    }
                    
                }
                ClassList = ClassList.Distinct().ToList();
                Vocab = Vocab.Distinct().ToList();
            }

            if (ModelFilePresent )
            {
                Dictionary<String, Dictionary<String, double>> ModelFeatureClassProb = new Dictionary<string, Dictionary<String, double>>();
                List<String> ClassListModel = new List<string>();
                ReadModelFile(ModelFile, ref  ModelFeatureClassProb, ref  ClassListModel);
                //I have model expectations. Now i have to multiply it with number of training instances per class to get count
                using (StreamWriter Sw = new StreamWriter(OutPutFile))
                {
                    foreach (var cl in ClassList)
                    {
                        int ClassCount = TrainingDict[cl].Count;
                        foreach (var word in Vocab)
                        {
                            Sw.WriteLine()
                        }
                    }
                }
                
                
            }
            else
            {
                //if model file is not present then I will have to create 
            }
            
        }

        public static void ReadModelFile (string ModelFile, ref Dictionary<String, Dictionary<String, double>> ModelFeatureClassProb, ref List<String>  ClassList)
        {
            string line;
            string classLabel = "", key;
            double value;
            using (StreamReader Sr = new StreamReader(ModelFile))
            {
                while ((line = Sr.ReadLine()) != null)
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
                            Dictionary<String, double> temp = new Dictionary<string, double>();
                            temp.Add(key, value);
                            ModelFeatureClassProb.Add(classLabel, temp);
                        }


                    }
                }
            }
        }
    }
}
