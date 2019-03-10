using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tagger2.Extension;

namespace TaggerPred
{
    public class PicData1
    {
        public string PicId;

        public string PicUrl;

        public string PicTags;

        public string DownloadUrl;

        public string SampleUrl;

        public double Likeliness;

        public string Date;

        public PicData1(string id, string url, string tags, string DownUrl, string SampleUrl, string creation)
        {
            this.PicId = id;
            this.PicUrl = url;
            this.PicTags = tags;
            this.DownloadUrl = DownUrl;
            this.SampleUrl = SampleUrl;
            this.Date = creation;
        }

        public double PicLikeliness(Dictionary<string,float> TagRatio, Dictionary<string,int> TagCount, int VotesNeeded, string[] BlackList)
        {
            double TotalLikeliness = 0; //Segun https://upload.wikimedia.org/math/f/1/d/f1d1c65ee72c294f1fc9b4eb156f5768.png
            double producto1 = 1;
            double producto2 = 1;
            //Pillar las tags
            string[] tags = this.PicTags.Split(' ');
            foreach (string tag in tags)
            {
                //Miramos si esta en el array de la blacklist
                if(BlackList.Contains(tag))
                {
                    this.Likeliness = -1;
                    Console.WriteLine(this.PicId + " had "+ tag + " was banned");
                    return -1;
                }
                //Miramos si esta la tag en nuestra lista Y tiene los votos minimos
                if (TagRatio.ContainsKey(tag) && TagCount[tag] >= VotesNeeded)
                {//Si lo esta lo multiplicamos
                    //Console.WriteLine("Tag: " + tag + " ratio: " + TagRatio[tag].ToString());
                    double prob = ((1 + TagRatio[tag]) / 2).Clamp(0.1f, 0.9f);//TBD: Poner este valor en opciones
                    producto1 *= prob;
                    producto2 *= 1 - prob;
                }
                else
                {//Si no lo esta asumimos 50%
                    producto1 *= 0.5f;
                    producto2 *= 0.5f;
                }
            }
            TotalLikeliness = producto1 / (producto1 + producto2);
            //Console.WriteLine(this.PicId + " Total: " + TotalLikeliness.ToString());
            this.Likeliness = TotalLikeliness;
            return TotalLikeliness;
        }
    }
}
