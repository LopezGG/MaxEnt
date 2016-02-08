using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaxEntFullImplementation
{
    class Program
    {
        static void Main (string[] args)
        {
            string TrainingFile = args[0];
            string OutPutFile = args[1];
            string line;
            int totalIterations = 1000;
            int Cmax=0;
            //Dictionary<String,List<string>> TrainDocs = new Dictionary<string,List<string>>();
            List<String> Vocab = new List<string>();
            Dictionary<String, Dictionary<string, double>> ClassFeatureTemp = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<String, Dictionary<string, double>> ObservedFeatureProb = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<String, Dictionary<string, double>> ModelFeatureProb = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<String, Dictionary<string, double>> ClassFeatureLambda = new Dictionary<string, Dictionary<string, double>>();
            Dictionary<String,List<string>> trainingDocs = new Dictionary<string,List<string>>();
            int docId = 0;
            List<String> ClassList = new List<string>();
            string classLabel,key;
            int index,totalFeatures;
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
                    //getting the max # of features in the document
                    int length = words.Length;
                    if(Cmax< length-1)
                        Cmax = length;
                    string sId = Convert.ToString(docId++);
                    trainingDocs.Add(sId, new List<string>());
                    
                    for (int i = 1; i < length; i++)
                    {
                        index = words[i].IndexOf(":");
                        key = words[i].Substring(0, index);
                        value = Convert.ToDouble(words[i].Substring(index + 1));
                        if (value <= 0)
                            continue;
                        Vocab.Add(key);
                        trainingDocs[sId].Add(key);
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
                ModelFeatureProb.Add(cl, new Dictionary<string, double>());
                ClassFeatureLambda.Add(cl, new Dictionary<string, double>());
                var features = ClassFeatureTemp[cl];
                foreach (var word in Vocab)
                {
                    if (features.ContainsKey(word))
                        ObservedFeatureProb[cl].Add(word, features[word] / totalFeatures);
                    else
                        ObservedFeatureProb[cl].Add(word, 0);//TODO: See if we have to add smoothing else this is not necessary
                    //initialize Model prob and lambda to zero
                    ModelFeatureProb[cl].Add(word, 0);
                    ClassFeatureLambda[cl].Add(word, 0);
                }
                
            }
            double score;
            foreach (var doc in trainingDocs)
            {
                var features = doc.Value;
                Dictionary<string, double> ClassesPerDoc = new Dictionary<string, double>();
                double totalScore = 0;
                foreach (var cl in ClassList)
                {
                    score = 0;
                    var Lambdavals = ClassFeatureLambda[cl];
                    foreach (var ft in features)
                    {
                        if (Lambdavals.ContainsKey(ft))
                            score += Lambdavals[ft];
                    }
                    totalScore += score;
                    ClassesPerDoc.Add(cl, score);
                }
                ClassesPerDoc = ClassesPerDoc.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value / totalScore);
                //now we are going to update modelProb

                foreach (var cl in ClassList)
                {
                    double probFraction = ClassesPerDoc[cl] / totalFeatures;
                    foreach (var ft in features)
                    {
                        (ModelFeatureProb[cl])[ft] += probFraction;
                    }
                }
            }
            //Now we have run through all the training docs and obtained a rudimentary prob value
            // we will have to update lambda for next iteration
                            //Now we will go to update lamba
            double minLambdaWeight = Double.MaxValue;
            double maxLambdaWeight = Double.MinValue;
                foreach (var cl in ClassList)
                {
                    foreach (var word in Vocab)
                    {
                        double observedProb = (ObservedFeatureProb[cl])[word];
                        double modelProb = (ModelFeatureProb[cl])[word];
                        if (System.Math.Abs(observedProb - modelProb) <= 1e-8)
                            continue;
                        if (observedProb == 0.0)
                            (ClassFeatureLambda[cl])[word] = Double.MinValue;
                        else if (modelProb == 0.0)
                            (ClassFeatureLambda[cl])[word] = Double.MaxValue;
                        else
                        {
                            //here we do calculation for delta
                            double delta = System.Math.Log(observedProb / modelProb) / Cmax; //TODO: Check if log is base 2 or what ?
                            double newValue = (ClassFeatureLambda[cl])[word] + delta;
                            (ClassFeatureLambda[cl])[word] = newValue;
                            if (minLambdaWeight > newValue)
                                minLambdaWeight = newValue;
                            if (maxLambdaWeight < newValue)
                                maxLambdaWeight = newValue;
                        }

                    }
                }
            //Now we replace all NegInf and PosInf values in lambda
                foreach (var cl in ClassList)
                {
                    foreach (var word in Vocab)
                    {
                        if ((ClassFeatureLambda[cl])[word] == double.MinValue)
                            (ClassFeatureLambda[cl])[word] = minLambdaWeight;
                        else if ((ClassFeatureLambda[cl])[word] == double.MaxValue)
                            (ClassFeatureLambda[cl])[word] = maxLambdaWeight;
                    }
                }


        }
    }
}
