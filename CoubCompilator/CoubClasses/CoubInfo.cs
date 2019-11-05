using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoubCompilator.CoubClasses
{
     public class CoubInfo
     {
         public CoubInfo() { }

         public CoubInfo(Uri videoUri, Uri audioUri):this()
         {
             VideoUri = videoUri;
             AudioUri = audioUri;
         }

         public CoubInfo(Uri videoUri, Uri audioUri, string permalink) : this(videoUri, audioUri)
         {
             Permalink = permalink;
         }

         public CoubInfo(Uri videoUri, Uri audioUri, string permalink, string name) : this(videoUri, audioUri, permalink)
         {
             Name = name;
         }

         public CoubInfo(Uri videoUri, Uri audioUri, string permalink, string name, double duration, string path) : this(videoUri, audioUri,
             permalink,name)
         {
             Duration = duration;
             Path = path;
         }
         public string Permalink { get; set; }
         public string Name { get; set; }
         public Uri VideoUri { get; set; }
         public Uri AudioUri { get; set; }

         public double Duration { get; set; }
         public string Path { get; set; }
     }
}
