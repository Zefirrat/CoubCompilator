using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using CoubCompilator.CompilingVideo;
using CoubCompilator.CoubClasses;
using Newtonsoft.Json;

namespace CoubCompilator
{
    public class CompilatorsBody
    {
        private CoubAPI _coubApi;
        private List<Coub> _coubs;
        private const string _compilatorPathFolder = "C:/CoubCompilator/";
        private CoubCompilerEntities _db;
        #region settings

        private string _renderSpeed => "fast";
        private string _categoryLoadFrom => "hot";
        private int _numberOfCoubsToCompile => 70;
        private int _loadingPerPage => 25;
        private bool _clearBlackList => false;
        private string _videosTxt = Path.Combine(_compilatorPathFolder, "videos.txt");
        private string _urisTxt = Path.Combine(_compilatorPathFolder, "URIS.txt");

        #endregion

        #region paths

        private string _blackListPath => Path.Combine(_compilatorPathFolder, $"blacklist.txt");

        #endregion

        public CompilatorsBody()
        {
            _coubApi = new CoubAPI();
            _coubs = new List<Coub>();
            _db = new CoubCompilerEntities();
        }

        #region Public Commands

        //public void Work()
        //{
        //    Console.WriteLine($"{Environment.NewLine}Starting work.");
        //    int i = 0;
        //    do
        //    {
        //        _loadCoubs(i);
        //        _filterCoubs();
        //        Console.WriteLine($"Coubs in store: {_coubs.Count}{Environment.NewLine}");
        //        i++;
        //    } while (_coubs.Count < _numberOfCoubsToCompile);

        //    _loadVideosInfo();
        //    _downloadCoubs();

        //    _compileVideos();
        //}

        public void CompileAll()
        {
            _fillVideosTxt();
            _executeMainCompileCommand();
        }

        public void CompileMain()
        {
            _finalCompile();
        }
        public void AddToBlackList(string permalink)
        {
            _db.UsedCoubs.Add(new UsedCoubs() { ID = _db.UsedCoubs.Count(), Permalink = permalink });
            _db.SaveChanges();
        }

        public void AddCurrentListToUsed()
        {
            foreach (var loadedCoub in _db.LoadedCoubs)
            {
                AddToBlackList(loadedCoub.Permalink);
            }
        }

        public void ClearCurrentCoubsList()
        {
            _db.LoadedCoubs.RemoveRange(_db.LoadedCoubs);
        }

        public void LoadCoubsOfTheDay()
        {
            int i = 0;
            do
            {
                _loadCoubs(i, CoubAPI.Order.newest_popular, "hot");
                _filterCoubs();
                Console.WriteLine($"Coubs in store: {_coubs.Count}{Environment.NewLine}");
                i++;
            } while (_coubs.Count < _numberOfCoubsToCompile);

            _loadVideosInfo();
        }

        public void CompileLoadedCoubs()
        {
            //todo: make compiling
        }

        public void PublishToYouTube()
        {
            //todo: make publishing to youtube
        }
        #endregion

        private void _loadCoubs(int coubsPage, CoubAPI.Order order, string categoryLoadFrom)
        {
            Console.WriteLine($"Loading coubs.");
            Welcome coubResult = _coubApi.GetPage(coubsPage, _loadingPerPage, order, categoryLoadFrom);
            Console.WriteLine($"Loaded coubs count: {coubResult.Coubs.Count}");
            _coubs.AddRange(coubResult.Coubs);
        }
        private void _filterCoubs()
        {
            List<Coub> newList = _coubs.Where(i => i.Duration > 4).ToList();
            _coubs = new List<Coub>();

            var blacklist = _db.UsedCoubs.ToList();

            foreach (var coub in newList)
            {
                if (_coubs.All(i => i.Permalink != coub.Permalink) && blacklist.All(i => i.Permalink != coub.Permalink))
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

                _db.LoadedCoubs.Add(new LoadedCoubs()
                {
                    ID = _db.LoadedCoubs.Count(),
                    CreatedAt = coub.CreatedAt.DateTime,
                    DateLoaded = DateTime.Now,
                    DislikesCount = (int?)coub.DislikesCount,
                    DurationVideo = coub.Duration,
                    LikesCount = (int?)coub.LikesCount,
                    DurationSound = null,
                    Path = string.Empty,
                    PathBlured = string.Empty,
                    PathCoub = string.Empty,
                    ViewsCount = (int?)coub.ViewsCount,
                    RemixesCount = (int?)coub.RemixesCount,
                    RecoubsCount = (int?)coub.RecoubsCount,
                    UrlVideo = string.IsNullOrEmpty(coub.FileVersions.Html5.Video.Higher)
                        ? coub.FileVersions.Html5.Video.Higher
                        : string.IsNullOrEmpty(coub.FileVersions.Html5.Video.High)
                            ? coub.FileVersions.Html5.Video.High
                            : coub.FileVersions.Html5.Video.Med,
                    UrlAudio = string.IsNullOrEmpty(coub.FileVersions.Html5.Audio.High)
                        ? coub.FileVersions.Html5.Audio.High
                        : coub.FileVersions.Html5.Audio.Med
                });
            }

            _db.SaveChanges();

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
                    if (coubInfo.AudioUri != null)
                        client.DownloadFile(coubInfo.AudioUri.AbsoluteUri, Path.Combine(folderCoub, "Audio.mp3"));
                }
                Console.WriteLine();

            }
            Console.WriteLine("Downloading coubs to folder complete.");
        }


        private void _compileVideos()
        {
            foreach (var coub in _coubUris)
            {
                Console.WriteLine($"{Environment.NewLine}>>>>> Compiling {_coubUris.IndexOf(coub)}/{_coubUris.Count - 1} <<<<<<{Environment.NewLine}");
                _compileCoub(coub);
            }
            _sortCoubs();
            _finalCompile();
        }

        private void _finalCompile()
        {
            Console.WriteLine($"{Environment.NewLine}Final compile start.");

            FileStream fileStream = File.Create(_videosTxt);
            //File.WriteAllText(videosTxt, "");
            using (StreamWriter streamWriter = new StreamWriter(fileStream))
            {
                //FileStream blackListStream = File.OpenWrite(_blackListPath);

                //streamWriter.WriteLine($"file 'Intro.mp4'");
                foreach (var coub in _coubUris)
                {
                    if (!File.Exists(Path.Combine(_compilatorPathFolder, coub.Permalink, "Coub.mp4")))
                        continue;

                    streamWriter.WriteLine($"file '{coub.Permalink}/Coub.mp4'");
                    AddToBlackList(coub.Permalink);
                    //streamWriter.WriteLine("file 'Separator.mp4'");
                }
            }
            _executeMainCompileCommand();

        }

        private void _fillVideosTxt()
        {
            string[] directories = Directory.GetDirectories(_compilatorPathFolder);

            using (StreamWriter sw = new StreamWriter(_videosTxt))
            {
                Random rnd = new Random();
                string[] MyRandomArray = directories.OrderBy(x => rnd.Next()).ToArray();
                foreach (var directory in MyRandomArray)
                {
                    string coubCompiledFile = Path.Combine(directory, "Coub.mp4");
                    if (File.Exists(coubCompiledFile))
                        sw.WriteLine($"file '{coubCompiledFile}'");
                }
            }
        }
        private void _executeMainCompileCommand()
        {
            ExecuteCommand executeCommand = new ExecuteCommand();
            Console.WriteLine("Starting rendering.");
            File.Delete(Path.Combine(_compilatorPathFolder, "Compilation.mp4"));
            string command = $"cd {_compilatorPathFolder} & ffmpeg -f concat -safe 0 -i videos.txt -c:v libx264 -preset {_renderSpeed} -crf 13 -c:a aac Compilation.mp4";
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
            string command;
            if (File.Exists(Path.Combine(coubInfo.Path, "Audio.mp3")))
            {
                command =
                    $"cd {coubInfo.Path} && ffmpeg -i Video.mp4 -t {coubInfo.Duration.ToString("0.0", CultureInfo.InvariantCulture)} -lavfi \"split [original][copy];[original]scale=w=-1:h=1080:-1:flags=neighbor+bitexact+accurate_rnd+full_chroma_int+full_chroma_inp+print_info,setsar=1:1[ov];[copy]scale=w=1920+1000*iw/ih:h=ih*1920/iw+1000*ih/iw" +
                    $":flags=neighbor+bitexact+accurate_rnd+full_chroma_int+full_chroma_inp+print_info,setsar=1:1,boxblur=luma_radius=min(h\\,w)/20:luma_power=1:chroma_radius=min(cw\\,ch)/20:chroma_power=1[blur];[blur][ov]overlay=(W-w)/2:(H-h)/2,crop=w=1920:h=1080\" " +
                    $"-i Audio.mp3 -map 0:v -map 1:a -c:a copy Coub.mp4 -y";
                executeCommand.Execute(command);
            }
        }
    }

}
