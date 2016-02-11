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
            Dictionary<string, Dictionary<string, double>> ClassTrainingDict = new Dictionary<string, Dictionary<string, double>>();
            List <Dictionary<string, double>> TrainingDict = new List<Dictionary<string,double>>();
            Dictionary<String, int> ConfusionDict = new Dictionary<string, int>();
            Dictionary<String, int> Vocab = new Dictionary<string,int>();
            List<String> ClassList = new List<string>();
            string line,key,classlabel;
            int index,docCount=0;

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
                    Dictionary<string, double> TrainInst = new Dictionary<string, double>();
                    docCount++;
                    ClassList.Add(classlabel);
                    for (int i = 1; i < words.Length; i++)
                    {
                        index = words[i].IndexOf(":");
                        key = words[i].Substring(0,index);
                        //this will give me the number of documents the words occurs in
                        if (Vocab.ContainsKey(key))
                            Vocab[key]++;
                        else
                            Vocab.Add(key,1);
                        
                        value = Convert.ToDouble(words[i].Substring(index+1));
                        //this gives summary at the level of features
                        if (TrainInst.ContainsKey(key))
                            TrainInst[key] += value;
                        else
                            TrainInst.Add(key, value);

                        //this gives summary at the level of class. It tells us the number of documents in which a particular feature occurs per class. We dont need the value becuase our feature function is binary
                        if (ClassTrainingDict.ContainsKey(classlabel) && ClassTrainingDict[classlabel].ContainsKey(key))
                            ClassTrainingDict[classlabel][key] ++;
                        else if (ClassTrainingDict.ContainsKey(classlabel))
                            ClassTrainingDict[classlabel].Add(key, 1);
                        else
                        {
                            Dictionary<string, double> temp = new Dictionary<string, double>();
                            temp.Add(key, value);
                            ClassTrainingDict.Add(classlabel, temp);
                        }
                    }
                    TrainingDict.Add(TrainInst);
                    
                }
                ClassList = ClassList.Distinct().ToList();
                //Vocab = Vocab.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
            }

            if (ModelFilePresent )
            {
                Dictionary<String, Dictionary<String, double>> ModelFeatureClassProb = new Dictionary<string, Dictionary<String, double>>();
                ReadModelFile(ModelFile, ref  ModelFeatureClassProb);
                Dictionary<String, Dictionary<String, double>> FeatureExpectation = new Dictionary<String, Dictionary<String, double>>();
                foreach (var Instance in TrainingDict)
                {
                    Dictionary<string, double> ClassScore = new Dictionary<string, double>();
                    double totalScore=0;
                    //we are calculating the classprob for this doc using our model parameters in this for loop
                    foreach (var cl in ClassList)
                    {
                        var ClassFeatureLamdaVals = ModelFeatureClassProb[cl];
                        double score=0;
                        
                        foreach (var featurePair in Instance)
                        {
                            score += ClassFeatureLamdaVals[featurePair.Key];
                        }
                        score += ClassFeatureLamdaVals["<default>"];
                        score = System.Math.Exp(score);
                        ClassScore.Add(cl, score);
                        totalScore += score;
                    }
                    //now we have scores for each class along with the total score at the end of the above for loop. We are all set to calculate prob of each class and hence work the expectation formula.
                    foreach (var cl in ClassList)
                    {
                        double ClassProb = ClassScore[cl]/totalScore;
                        foreach (var featurePair in Instance)
                        {

                            if (FeatureExpectation.ContainsKey(cl) && FeatureExpectation[cl].ContainsKey(featurePair.Key))
                                FeatureExpectation[cl][featurePair.Key] += ClassProb;
                            else if (FeatureExpectation.ContainsKey(cl))
                                FeatureExpectation[cl].Add(featurePair.Key, ClassProb);
                            else
                            {
                                Dictionary<string, double> temp = new Dictionary<string, double>();
                                temp.Add(featurePair.Key, ClassProb);
                                FeatureExpectation.Add(cl, temp);
                            }
                        }
                    }

                }
                //this is to wirte the file down
                using (StreamWriter Sw = new StreamWriter(OutPutFile))
                {
                    foreach (var cl in ClassList)
                    {
                        var FeatureExpectationPerClass = FeatureExpectation[cl];
                        foreach (var wordPair in Vocab)
                        {
                            key = wordPair.Key;
                            if (FeatureExpectationPerClass.ContainsKey(key))
                                value = FeatureExpectationPerClass[key];
                            else
                                value = 0.0;
                            double expectation = ( double )value / (docCount); // here we dont use ClassList.Count because we already multipled by the prob
                            Sw.WriteLine(cl + " " + wordPair.Key + " " + Convert.ToString(expectation) + " " + expectation * docCount);
                        }
                    }
                }
            }
            else
            {
                using (StreamWriter Sw = new StreamWriter(OutPutFile))
                {
                    foreach (var cl in ClassList)
                    {
                        //here I can use the general vocab becuase all classes have same prob. 
                        foreach (var wordPair in Vocab)
                        {
                            double expectation = (double) wordPair.Value /(ClassList.Count * docCount) ;
                            Sw.WriteLine(cl + " " + wordPair.Key + " " + Convert.ToString(expectation) + " " + expectation * docCount);
                        }
                    }
                }
                
            }
            
        }

       
        public static void ReadModelFile (string ModelFile, ref Dictionary<String, Dictionary<String, double>> ModelFeatureClassProb)
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
