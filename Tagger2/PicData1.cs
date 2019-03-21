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

    public PicData1 ( string id , string url , string tags , string DownUrl , string SampleUrl , string creation )
    {
      this.PicId = id;
      this.PicUrl = url;
      this.PicTags = tags;
      this.DownloadUrl = DownUrl;
      this.SampleUrl = SampleUrl;
      this.Date = creation;
    }

    public double PicLikeliness ( Dictionary<string , float> TagRatio , Dictionary<string , int> TagCount , int VotesNeeded , string[] BlackList )
    {
      double TotalLikeliness = 0; //Segun https://upload.wikimedia.org/math/f/1/d/f1d1c65ee72c294f1fc9b4eb156f5768.png
      double p_1 = 1;
      double p_2 = 1;

      // Fetch tags.

      string[] tags = this.PicTags.Split(' ');
      foreach ( string tag in tags )
      {
        // Is it on the blacklist?
        if ( BlackList.Contains( tag ) )
        {
          this.Likeliness = -1;
          Console.WriteLine( this.PicId + " had " + tag + " was banned" );
          return -1;
        }

        // Is it is the list, with minimum votes?

        if ( TagRatio.ContainsKey( tag ) && TagCount[tag] >= VotesNeeded )
        {
          // Yes: Multiply the ratio.

          double prob = ((1 + TagRatio[tag]) / 2).Clamp(0.1f, 0.9f);//TODO: Add to options.
          p_1 *= prob;
          p_2 *= 1 - prob;
        }
        else
        {
          // If it isn't we assume 50%.
          p_1 *= 0.5f;
          p_2 *= 0.5f;
        }
      }

      TotalLikeliness = p_1 / ( p_1 + p_2 );

      this.Likeliness = TotalLikeliness;
      return TotalLikeliness;
    }
  }
}
