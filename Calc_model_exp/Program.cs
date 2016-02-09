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
            List<String> TrainingClassList = new List<string>();
            List<String> Vocab = new List<string>();
            using (StreamReader Sr = new StreamReader(trainingFile))
            {

                //all I need is to get vocab ,
            }

            if (ModelFilePresent )
            {
                Dictionary<String, Dictionary<String, double>> ModelFeatureClassProb = new Dictionary<string, Dictionary<String, double>>();
                List<String> ClassList = new List<string>();
                ReadModelFile(ModelFile, ref  ModelFeatureClassProb, ref  ClassList);
                //I have model expectations. Now i have to multiply it with number of training instances per class to get count
                
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
