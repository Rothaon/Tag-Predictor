using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaggerPred
{
  [Serializable]
  public class TaggerOptions
  {

    public string SearchTags = " -mlp -female -breasts";

    public string[] BlackList = new string[] { "vore","feral", "five_nights_at_freddy's", "feral", "space_dandy" , "gore", "3d","nightmare_fuel"};

    public int LastId;

    public int BufferSize = 2;

    public int PicBatch = 100;

    public int VotesNeeded = 3;

    public int AgroLimit = 3;

    public string DownloadFolder = "./Downloaded/";

    // Stuff for stats.

    public List<double> HighestUnliked = new List<double>(); //Ratio of the first unliked image of a batch.

    public List<double> LowestLiked = new List<double>();  //Ratio of the lowest liked image in the batch.

  }
}
