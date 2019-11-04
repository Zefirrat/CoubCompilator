using System;
using System.Collections.Generic;
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
        public CompilatorsBody()
        {
            _coubApi = new CoubAPI();
            _coubs = new List<Coub>();
            _coubUris = new List<CoubInfo>();
        }

        private void _loadCoubs(int coubsPage)
        {
            Console.WriteLine($"Loading coubs.");
            Welcome coubResult = _coubApi.GetPage(coubsPage, 25);
            Console.WriteLine($"Loaded coubs count: {coubResult.Coubs.Count}");
            _coubs.AddRange(coubResult.Coubs);
        }
        private void _filterCoubs()
        {

        }

        private void _loadVideosInfo()
        {
            Console.WriteLine("Loading videos and audios Uris for each coub");
            foreach (var coub in _coubs)
            {
                _coubUris.Add(new CoubInfo(coub.FileVersions.Html5.Video.Higher.Url, coub.FileVersions.Html5.Audio.High.Url, coub.Permalink, coub.Title));
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
            } while (_coubs.Count < 100);

            _loadVideosInfo();
            _downloadCoubs();

            _compileVideos();
        }

        private void _compileVideos()
        {
            foreach (var coub in _coubUris)
            {
                _compileCoub(Path.Combine(_compilatorPathFolder, coub.Permalink));
            }
            _sortCoubs();
        }

        private void _sortCoubs()
        {

        }

        private void _compileCoub(string pathToCoub)
        {
            if (File.Exists(Path.Combine(pathToCoub, "Coub.mp4")))
            {
                Console.WriteLine($"Coub {pathToCoub} is alredy exist");
                return;
            }
            ExecuteCommand executeCommand = new ExecuteCommand();
            Console.WriteLine($"Compiling coub {pathToCoub}");
            string command = $"cd {pathToCoub} && ffmpeg -i Video.mp4 -i Audio.mp3 -map 0:v -map 1:a -c:a copy -c:v copy Coub.mp4 -y";
            executeCommand.Execute(command);
        }
    }

}
