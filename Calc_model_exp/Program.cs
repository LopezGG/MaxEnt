using System;
using System.Collections.Generic;
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

        }
    }
}
