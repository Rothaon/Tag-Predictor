using System;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;
using System.IO;
using System.Xml.Serialization;
using System.Net.Http;
using System.Net;
using Microsoft.VisualBasic.FileIO;
using Tagger2.Extension;


namespace TaggerPred
{
  public partial class Form1 : Form
  {


    public static TaggerOptions TaggerOpt = new TaggerOptions();

    public HttpWebRequest request;
    public string ApiData;

    // Fixed settings.

    public int PicBufferSize = 2; // When buffer goes below this, we load more pictures.
    public int PicInitialBuffer = 10; // # Pictures loaded upon start.
    public static int PicBatchSize = 10;// # Pics loaded each batch.
    public int TagFixNum = 1;
    public int TagPoolSize = 500;

    //  https://e621.net/post/index.xml?limit=10&tags=order:random%20id:>=100 -tag -tag 

    public static string APIbase = "https://e621.net/post/index.xml?limit=10&tags=order:id_asc%20id:%3E" + TaggerOpt.LastId + TaggerOpt.SearchTags;
    public List<PicData1> PicturesToLoad = new List<PicData1>();

    public string LastPicUrl;
    public Dictionary<string, int> TagCount = new Dictionary<string, int>();
    public Dictionary<string, int> TagVotes = new Dictionary<string, int>();
    public Dictionary<string, float> TagRatio = new Dictionary<string, float>();
    public List<string> TagListPeek = new List<string>();
    public List<int> TagCountPeek = new List<int>();

    public string SaveLocVotes = "./Votes.txt";
    public string SaveLocCount = "./Count.txt";
    public string SaveLocLinks = "./Links.txt";
    public string SaveLocStats = "./Stats.txt";
    public string SaveLocOptions = "./Options.xml";
    public string SaveDebugData = "./Debug.txt";
    public double AgroCounter = 0;


    public double LowestLikedBatch;
    public bool HighestUnlikedBatch;

    public Form1 ()
    {
      InitializeComponent();
      this.KeyPress += new KeyPressEventHandler( Form1_KeyPress );
      pb_main.LoadProgressChanged += Pb_main_LoadProgressChanged;
    }

    private void Pb_main_LoadProgressChanged ( object sender , ProgressChangedEventArgs e )
    {
      lbl_prog.Text = e.ProgressPercentage.ToString();
    }

    private void OnProcessExit ( object sender , EventArgs e )
    {
      TaggerOpt.LowestLiked.Add( LowestLikedBatch );
      SaveoptionsToXml();
      SaveVotes();
      Console.WriteLine( "Out!" );
    }

    private void Form1_Load ( object sender , EventArgs e )
    {
      AppDomain.CurrentDomain.ProcessExit += new EventHandler( OnProcessExit );
      GetOptions();
      GetVotes();
      GetPicUrls();
      ChangePic( -3 );
    }

    void Form1_KeyPress ( object sender , KeyPressEventArgs e )
    {

      char PressedKey = Char.ToUpper((char)e.KeyChar);

      //Console.WriteLine( "Pressed: " + PressedKey.ToString() );

      if ( PressedKey == ( char ) Keys.S )
      {
        StoreUrl();
      }
      if ( PressedKey == ( char ) Keys.Space )
      {
        StoreUrl();
        lbl_prog.Text = "0";
        DownloadPic( PicturesToLoad[0] );
        ChangePic( 1 );
      }
      if ( PressedKey == ( char ) Keys.J )
      {
        ChangePic( 1 );
      }
      if ( PressedKey == ( char ) Keys.K )
      {
        ChangePic( 0 );
      }
      if ( PressedKey == ( char ) Keys.L )
      {
        ChangePic( -1 );
      }
      if ( PressedKey == ( char ) Keys.H )
      {
        ChangePic( -2 );
      }
      if ( PressedKey == ( char ) Keys.W )
      {
        double TotalLikeliness = 0; // Follows this formula: https://upload.wikimedia.org/math/f/1/d/f1d1c65ee72c294f1fc9b4eb156f5768.png
        double producto1 = 1;
        double producto2 = 1;
        using ( FileStream fs = new FileStream( SaveDebugData , FileMode.Append , FileAccess.Write ) )
        using ( StreamWriter sw = new StreamWriter( fs ) )
        {
          sw.WriteLine( "ID " + PicturesToLoad[0].PicId );
          foreach ( var tag in PicturesToLoad[0].PicTags.Split( ' ' ) )
          {
            if ( TagRatio.ContainsKey( tag ) && TagCount[tag] >= TaggerOpt.VotesNeeded )
            {
              double prob = ((1 + TagRatio[tag]) / 2).Clamp(0.1f,0.9f) ;//TBD: Move to settings.
              producto1 *= prob;
              producto2 *= 1.0D - prob;
              sw.WriteLine( tag + " - " + prob );
            }
            else
            {//Si no lo esta asumimos 50%
              sw.WriteLine( tag + " - " + 0.5 );
              producto1 *= 0.5f;
              producto2 *= 0.5f;
            }
            sw.WriteLine( "Up to now: " + producto1 + " " + producto2 );
          }
          TotalLikeliness = producto1 / ( producto1 + producto2 );
          sw.WriteLine( "Total: " + TotalLikeliness );
          sw.WriteLine( "Predicted: " + PicturesToLoad[0].Likeliness );
        }
      }
      if ( PressedKey == ( char ) Keys.Q )
      {
        GetPicUrls();
        ChangePic( -3 ); //Force refresh.
      }
      if ( PressedKey == ( char ) Keys.A )
      {
        SaveStats();
      }
    }

    public void UpdateRatioList ( string tags )
    {
      string[] taglist = tags.Split(' ');

      TagRatio.Clear();
      lb_Score.Items.Clear();
      lb_Score.BeginUpdate();

      foreach ( KeyValuePair<string , int> entry in TagCount )
      {
        TagRatio[entry.Key] = ( ( float ) TagVotes[entry.Key] / ( float ) TagCount[entry.Key] );

      }
      TagRatio = TagRatio.OrderByDescending( x => x.Value ).ToDictionary( x => x.Key , x => x.Value );

      foreach ( KeyValuePair<string , float> entry in TagRatio )
      {

        if ( TagCount[entry.Key] < TaggerOpt.VotesNeeded || !taglist.Contains<string>( entry.Key ) )
        {
          continue;
        }
        lb_Score.Items.Add( string.Format( "{0:00.00} {1} {2} of {3}" , 100 * entry.Value , entry.Key , TagVotes[entry.Key] , TagCount[entry.Key] ) );
      }

      lb_Score.EndUpdate();
    }

    public void SaveVotes ()
    {
      if ( TagVotes.Count != 0 )
      {
        // Write votes
        TextWriter txtwrt = new StreamWriter(SaveLocVotes);
        Serialize( txtwrt , TagVotes );
        txtwrt.Close(); txtwrt.Dispose();
        // Write count.
        txtwrt = new StreamWriter( SaveLocCount );
        Serialize( txtwrt , TagCount );
        txtwrt.Close(); txtwrt.Dispose();
      }
    }

    public void GetVotes ()
    {
      if ( File.Exists( SaveLocVotes ) )
      {
        // Read votes.
        Console.WriteLine( "Getting Votes" );
        TextReader txtwrt = new StreamReader(SaveLocVotes);
        Deserialize( txtwrt , TagVotes );
        txtwrt.Close(); txtwrt.Dispose();
        // Read count.
        txtwrt = new StreamReader( SaveLocCount );
        Deserialize( txtwrt , TagCount );
        txtwrt.Close(); txtwrt.Dispose();
        UpdateRatioList( "" );
      }
    }

    public void SaveStats ()
    {
      if ( File.Exists( SaveLocStats ) )
      {
        //Delete
        File.Delete( SaveLocStats );
      }
      // Save votes.
      using ( FileStream fs = new FileStream( SaveLocStats , FileMode.Append , FileAccess.Write ) )
      using ( StreamWriter sw = new StreamWriter( fs ) )
      {
        foreach ( var item in TagCount )
        {
          sw.WriteLine( item.Key + "," + TagVotes[item.Key] + "," + item.Value );
        }
      }
    }

    public void ChangePic ( int vote )
    {
      if ( vote > -2 )
      {
        string[] tags = PicturesToLoad[0].PicTags.Split(' ');
        foreach ( string tag in tags )
        {
          if ( TagCount.ContainsKey( tag ) )
          {
            TagCount[tag]++;
            if ( vote == 1 )
            {
              TagVotes[tag]++;
            }
            else if ( vote == -1 )
            {
              TagVotes[tag]--;
            }
          }
          else
          {
            TagCount.Add( tag , 1 );
            if ( vote == 1 )
            {
              TagVotes.Add( tag , 1 );
            }
            else if ( vote == 0 )
            {
              TagVotes.Add( tag , 0 );
            }
            else if ( vote == -1 )
            {
              TagVotes.Add( tag , -1 );
            }
          }
          TagRatio[tag] = ( ( float ) TagVotes[tag] / ( float ) TagCount[tag] );
        }


        TagRatio = TagRatio.OrderByDescending( x => x.Value ).ToDictionary( x => x.Key , x => x.Value );
      }
      // For stats.
      if ( HighestUnlikedBatch == true && vote == -1 )
      {
        TaggerOpt.HighestUnliked.Add( PicturesToLoad[0].Likeliness );
        Console.WriteLine( "Highest Unliked: " + LowestLikedBatch );
        HighestUnlikedBatch = false;
      }
      if ( vote == 1 )
      {
        Console.WriteLine( "New Lowest Like: " + PicturesToLoad[0].Likeliness );
        LowestLikedBatch = PicturesToLoad[0].Likeliness;
      }
      //Sumamos el agro
      if ( vote == 0 || vote == -1 )
      {
        AgroCounter += ( 1 - PicturesToLoad[0].Likeliness ) * ( Math.Abs( vote ) + 1 );
      }
      else if ( vote == 1 )
      {
        AgroCounter = 0;
      }

      if ( AgroCounter >= TaggerOpt.AgroLimit )
      {
        AgroCounter = 0;
        lbl_agro.Text = "Fetching...";
        PicturesToLoad.Clear();
        lbl_agro.Text = "Agro: " + AgroCounter.ToString();
        GetPicUrls();
      }

      // vote -3 indicates the forst load so we don't delete the picture at the first position.
      if ( vote > -3 && PicturesToLoad.Count > 0 )
      {
        PicturesToLoad.RemoveAt( 0 );
      }

      // After saving everything image related:
      SaveVotes();
      //Check how many are left.

      if ( PicturesToLoad.Count <= TaggerOpt.BufferSize )
      {
        lbl_prog.Text = "0";
        GetPicUrls();
      }

      if ( PicturesToLoad.Count == 0 )
      {
        Application.Exit();
      }
      else
      {
        LastPicUrl = PicturesToLoad[0].PicUrl;
        lbl_prog.Text = "0";
        pb_main.LoadAsync( PicturesToLoad[0].PicUrl );
        lbl_buffer.Text = PicturesToLoad.Count.ToString() + " ID: " + PicturesToLoad[0].PicId + "-" + PicturesToLoad[0].Likeliness.ToString();
        lbl_agro.Text = "Agro: " + AgroCounter.ToString();
        lbl_date.Text = PicturesToLoad[0].Date;
        UpdateRatioList( PicturesToLoad[0].PicTags );
        this.Text = "ID: " + PicturesToLoad[0].PicId + " on " + PicturesToLoad[0].Date + " with " + PicturesToLoad[0].Likeliness;
      }
    }

    public void GetOptions ()
    {
      FileInfo fileInfo = new FileInfo(SaveLocOptions);
      if ( File.Exists( SaveLocOptions ) )
      {
        XmlSerializer xmlSerializer = new XmlSerializer(typeof(TaggerOptions));
        TextReader textReader = new StreamReader(SaveLocOptions, Encoding.UTF8);
        TaggerOpt = xmlSerializer.Deserialize( textReader ) as TaggerOptions;
      }
    }

    public void SaveoptionsToXml ()
    {
      XmlSerializer Serializer = new XmlSerializer(typeof(TaggerOptions)); ;
      XmlWriter xmlWriter = XmlWriter.Create(SaveLocOptions);
      Serializer.Serialize( xmlWriter , TaggerOpt );
      xmlWriter.Flush();
      xmlWriter.Close();
    }

    public string GetXmlStream ()
    {
      APIbase = "https://e621.net/post/index.xml?limit=" + TaggerOpt.PicBatch.ToString() + "&tags=order:id_asc%20id:%3E" + TaggerOpt.LastId + TaggerOpt.SearchTags;
      Console.WriteLine( "Loading: " + APIbase );
      request = ( HttpWebRequest ) WebRequest.Create( APIbase );
      // If required by the server, set the credentials.
      request.Credentials = CredentialCache.DefaultCredentials;
      request.UserAgent = "TagLikeliness/1.1 (by ciruelitas on e621)";
      request.KeepAlive = false;
      // Get the response.
      HttpWebResponse response = (HttpWebResponse)request.GetResponse();
      // Display the status.
      Console.WriteLine( response.StatusDescription );
      // Get the stream containing content returned by the server.
      StreamReader loResponseStream = new StreamReader(response.GetResponseStream());

      ApiData = loResponseStream.ReadToEnd();

      return ApiData; //Esto es el Xml que retorna la API
    }

    public void StoreUrl ()
    {
      if ( !File.Exists( SaveLocLinks ) )
      {
        // Create a file to write to.
        StreamWriter sw = File.CreateText(SaveLocLinks);
        sw.Close();
      }

      // This text is always added, making the file longer over time
      // if it is not deleted.
      using ( StreamWriter sw = File.AppendText( SaveLocLinks ) )
      {
        sw.WriteLine( PicturesToLoad[0].PicUrl );
      }
    }

    public void GetPicUrls ()
    {
      SaveVotes();
      if ( LowestLikedBatch > 0 )
      {
        // Avoids writing at the start.
        TaggerOpt.LowestLiked.Add( LowestLikedBatch );
      }
      Console.WriteLine( "Lowest Liked: " + LowestLikedBatch );
      HighestUnlikedBatch = true;
      XmlDocument doc = new XmlDocument();
      ApiData = GetXmlStream();

      List<string> PicsToLoad = new List<string>();
      List<string> TagsToLoad = new List<string>();
      List<string> IdsToLoad = new List<string>();
      List<string> UrlsToLoad = new List<string>();
      List<string> DatesToLoad = new List<string>();
      List<string> SamplesToLoad = new List<string>();

      using ( StringReader s = new StringReader( ApiData ) )
      {
        doc.Load( s );
      }

      // Select all nodes
      XmlNodeList nodeList = doc.SelectNodes("//post/sample_url");

      //Console.WriteLine("nodes " + nodeList.Count);
      foreach ( XmlNode post in nodeList )
      {
        PicsToLoad.Add( post.InnerText );
      }

      // Select all nodes
      nodeList = doc.SelectNodes( "//post/tags" );
      foreach ( XmlNode post in nodeList )
      {
        TagsToLoad.Add( post.InnerText );
      }
      // Select all nodes
      nodeList = doc.SelectNodes( "//post/id" );
      foreach ( XmlNode post in nodeList )
      {
        IdsToLoad.Add( post.InnerText );
        Int32.TryParse( post.InnerText , out TaggerOpt.LastId );
        //Console.WriteLine("Id: " + post.InnerText);
      }
      // Select all nodes
      nodeList = doc.SelectNodes( "//post/file_url" );
      foreach ( XmlNode post in nodeList )
      {
        UrlsToLoad.Add( post.InnerText );
      }
      // Select all nodes
      nodeList = doc.SelectNodes( "//post/sample_url" );
      foreach ( XmlNode post in nodeList )
      {
        SamplesToLoad.Add( post.InnerText );
      }
      // Select all nodes
      nodeList = doc.SelectNodes( "//post/created_at" );
      foreach ( XmlNode post in nodeList )
      {
        DatesToLoad.Add( post.InnerText.ToString().Substring( 0 , 10 ) );
      }

      // Add all the data.
      for ( int i = 0 ; i < PicsToLoad.Count ; i++ )
      {
        PicturesToLoad.Add( new PicData1( IdsToLoad[i] , PicsToLoad[i] , TagsToLoad[i] , UrlsToLoad[i] , SamplesToLoad[i] , DatesToLoad[i] ) );
      }//Falta calcular el likeliness

      foreach ( PicData1 pic in PicturesToLoad )
      {
        pic.PicLikeliness( TagRatio , TagCount , TaggerOpt.VotesNeeded , TaggerOpt.BlackList );
      }
      // Remove blacklisted images.

      for ( int i = 0 ; i < PicturesToLoad.Count ; i++ )
      {
        if ( PicturesToLoad[i].Likeliness == -1 )
        {
          PicturesToLoad.RemoveAt( i );
          i--;
        }
      }
      //Reorder by likeliness
      PicturesToLoad.Sort( ( y , x ) => x.Likeliness.CompareTo( y.Likeliness ) );
    }

    public void DownloadPic ( PicData1 pic )
    {
      string[] foo = pic.PicUrl.Split('.');
      WebClient Client = new WebClient();
      Client.DownloadFile( pic.DownloadUrl , TaggerOpt.DownloadFolder + pic.PicId + "." + foo.Last() );
      Console.WriteLine( "Downloaded to " + TaggerOpt.DownloadFolder + pic.PicId + "." + foo.Last() );
    }

    public static void Serialize ( TextWriter writer , IDictionary dictionary )
    {
      List<Entry> entries = new List<Entry>(dictionary.Count);
      foreach ( object key in dictionary.Keys )
      {
        entries.Add( new Entry( key , dictionary[key] ) );
      }
      XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>));
      serializer.Serialize( writer , entries );
    }
    public static void Deserialize ( TextReader reader , IDictionary dictionary )
    {
      dictionary.Clear();
      XmlSerializer serializer = new XmlSerializer(typeof(List<Entry>));
      List<Entry> list = (List<Entry>)serializer.Deserialize(reader);
      foreach ( Entry entry in list )
      {
        dictionary[entry.Key] = entry.Value;
      }
    }
    public class Entry
    {
      public object Key;
      public object Value;
      public Entry ()
      {
      }

      public Entry ( object key , object value )
      {
        Key = key;
        Value = value;
      }
    }

    private void label1_Click ( object sender , EventArgs e )
    {

    }
  }
}
