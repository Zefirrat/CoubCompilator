﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoubCompilator.CompilingVideo;
using CoubCompilator.CoubClasses;

namespace CoubCompilator
{
    public class CompilatorsBody
    {
        private CoubAPI _coubApi;
        private List<Coub> _coubs;
        private List<CoubInfo> _coubUris;
        private const string _compilatorPathFolder = "C:/CoubCompilator/";

        #region settings

        private string _renderSpeed => "ultrafast";
        private string _categoryLoadFrom => "tag/erotic";
        #endregion

        public CompilatorsBody()
        {
            _coubApi = new CoubAPI();
            _coubs = new List<Coub>();
            _coubUris = new List<CoubInfo>();
        }

        private void _loadCoubs(int coubsPage)
        {
            Console.WriteLine($"Loading coubs.");
            Welcome coubResult = _coubApi.GetPage(coubsPage, 25, _categoryLoadFrom);
            Console.WriteLine($"Loaded coubs count: {coubResult.Coubs.Count}");
            _coubs.AddRange(coubResult.Coubs);
        }
        private void _filterCoubs()
        {
            List<Coub> newList = _coubs.Where(i => i.Duration > 7).ToList();
            _coubs = new List<Coub>();
            string[] blacklist;
            using (StreamReader sr = new StreamReader(Path.Combine(_compilatorPathFolder, "blacklist.txt")))
            {
                blacklist = sr.ReadToEnd().Split(new string[] { Environment.NewLine }, StringSplitOptions.None);
            }

            foreach (var coub in newList)
            {
                if (_coubs.All(i => i.Permalink != coub.Permalink) && blacklist.All(i => i != coub.Permalink))
                    _coubs.Add(coub);
                else
                {
                    Console.WriteLine($"Filtered {coub.Permalink}");
                }
            }

        }

        private void _loadVideosInfo()
        {
            Console.WriteLine("Loading videos and audios Uris for each coub");
            foreach (var coub in _coubs)
            {
                _coubUris.Add(new CoubInfo(coub.FileVersions.Html5.Video.Higher.Url, coub.FileVersions.Html5.Audio.High.Url, coub.Permalink, coub.Title, coub.Duration, Path.Combine(_compilatorPathFolder, coub.Permalink)));
            }
            Console.WriteLine($"Loading info complete.{Environment.NewLine}");
        }
        private void _downloadCoubs()
        {
            Console.WriteLine($"Folder to downloading: {_compilatorPathFolder}");
            Directory.CreateDirectory(_compilatorPathFolder);
            foreach (var coubInfo in _coubUris)
            {
                Console.WriteLine($"Downloading: {coubInfo.Permalink}");
                string folderCoub = Path.Combine(_compilatorPathFolder, coubInfo.Permalink);
                if (Directory.Exists(folderCoub))
                {
                    Console.WriteLine($"{coubInfo.Permalink} is alredy downloaded. Continue.");
                    continue;
                }

                Directory.CreateDirectory(folderCoub);
                using (var client = new WebClient())
                {
                    client.DownloadFile(coubInfo.VideoUri.AbsoluteUri, Path.Combine(folderCoub, "Video.mp4"));
                    client.DownloadFile(coubInfo.AudioUri.AbsoluteUri, Path.Combine(folderCoub, "Audio.mp3"));
                }
                Console.WriteLine();

            }
            Console.WriteLine("Downloading coubs to folder complete.");
        }

        public void Work()
        {
            Console.WriteLine($"{Environment.NewLine}Starting work.");
            int i = 0;
            do
            {
                _loadCoubs(i);
                _filterCoubs();
                Console.WriteLine($"Coubs in store: {_coubs.Count}{Environment.NewLine}");
                i++;
            } while (_coubs.Count < 75);

            _loadVideosInfo();
            _downloadCoubs();

            _compileVideos();
        }

        private void _compileVideos()
        {
            foreach (var coub in _coubUris)
            {
                _compileCoub(coub);
            }
            _sortCoubs();
            _finalCompile();
        }

        private void _finalCompile()
        {
            string videosTxt = Path.Combine(_compilatorPathFolder, "videos.txt");
            Console.WriteLine($"{Environment.NewLine}Final compile start.");
            FileStream fileStream = File.Create(videosTxt);
            //File.WriteAllText(videosTxt, "");
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                using (StreamWriter blacklist = new StreamWriter(Path.Combine(_compilatorPathFolder, "blacklist.txt")))
                {

                    //streamWriter.WriteLine($"file 'Intro.mp4'");
                    foreach (var coub in _coubUris)
                    {
                        if (!File.Exists(Path.Combine(_compilatorPathFolder, coub.Permalink, "Coub.mp4")))
                            continue;

                        streamWriter.WriteLine($"file '{coub.Permalink}/Coub.mp4'");
                        blacklist.WriteLine(coub.Permalink);
                        //streamWriter.WriteLine("file 'Separator.mp4'");
                    }
                }
            }
            ExecuteCommand executeCommand = new ExecuteCommand();
            Console.WriteLine("Starting rendering.");
            File.Delete(Path.Combine(_compilatorPathFolder, "Compilation.mp4"));
            string command = $"cd {_compilatorPathFolder} & ffmpeg -f concat -safe 0 -i videos.txt -lavfi \"[0:v]scale=1920*2:1080*2,boxblur=luma_radius=min(h\\,w)/20:luma_power=1:chroma_radius=min(cw\\,ch)/20:chroma_power=1[bg];[0:v]scale=-1:1080[ov];[bg][ov]overlay=(W-w)/2:(H-h)/2,crop=w=1920:h=1080\" " +
                                                          $" -c:v libx264 -preset {_renderSpeed} -crf 18 -c:a aac Compilation.mp4";
            executeCommand.Execute(command);
            Console.WriteLine("Rendering complete.");
        }

        private void _sortCoubs()
        {
        }

        private void _compileCoub(CoubInfo coubInfo)
        {
            if (File.Exists(Path.Combine(coubInfo.Path, "Coub.mp4")))
            {
                Console.WriteLine($"Coub {coubInfo.Permalink} is alredy exist");
                return;
            }
            ExecuteCommand executeCommand = new ExecuteCommand();
            Console.WriteLine($"Compiling coub {coubInfo.Permalink}");
            string command = $"cd {coubInfo.Path} && ffmpeg -i Video.mp4 -t {coubInfo.Duration.ToString("0.0", CultureInfo.InvariantCulture)} -i Audio.mp3 -map 0:v -map 1:a -c:a copy -c:v  copy Coub.mp4 -y";
            executeCommand.Execute(command);
        }
    }

}
