using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aurora.Shared.Helpers;
using TagLib;
using Windows.Storage;
using Aurora.Shared.Extensions;
using Aurora.Music.Core.Models;

namespace Aurora.Music.Core.Storage
{
    public class FileReader
    {
        private static readonly string[] FILE_TYPES = new[] { ".mp3", ".m4a", ".flac", ".wav" };

        public event EventHandler<ProgressReport> ProgressUpdated;
        public event EventHandler Completed;
        private ProgressReport report = new ProgressReport();
        public event EventHandler<SongsAddedEventArgs> NewSongsAdded;


        /// <summary>
        /// TODO: Only pick files which not in the database, and find deleted files to delete
        /// </summary>
        /// <param name="folder"></param>
        /// <returns></returns>
        private async Task<IList<StorageFile>> GetFilesAsync(StorageFolder folder)
        {
            // TODO: determine is ondrive on demand
            var files = new List<StorageFile>();
            files.AddRange(await new FileTracker(folder).SearchFolder());
            return files;
        }

        public static async Task<List<Models.Album>> GetAlbumsAsync(string value)
        {
            var opr = SQLOperator.Current();

            // get single song
            // sqlite escaping
            value = SQLOperator.SQLEscaping(value);
            if (value.IsNullorEmpty())
            {
                // anonymous artists, get their songs
                var songs = await opr.GetWithQueryAsync<SONG>("ALBUMARTISTS", value);
                var albumGrouping = from song in songs group song by song.Album;
                return albumGrouping.ToList().ConvertAll(a => new Models.Album(a));
            }

            // get aritst-associated albums
            var albums = await opr.GetWithQueryAsync<ALBUM>("ALBUMARTISTS", value);
            var res = albums.ConvertAll(a => new Models.Album(a));

            var otherSongs = await opr.GetWithQueryAsync<SONG>($"SELECT * FROM SONG WHERE PERFORMERS='{value}' OR ALBUMARTISTS='{value}'");

            // remove duplicated (we suppose that artist's all song is just 1000+, this way can find all song and don't take long time)
            otherSongs.RemoveAll(x => !albums.Where(b => b.Name == x.Album).IsNullorEmpty());
            var otherGrouping = from song in otherSongs group song by song.Album;
            // otherSongs has item
            if (!otherGrouping.IsNullorEmpty())
            {
                res.AddRange(otherGrouping.ToList().ConvertAll(a => new Album(a)));
            }
            return res;
        }

        public static async Task<List<GenericMusicItem>> GetFavListAsync()
        {
            return await SQLOperator.Current().GetFavListAsync();
        }

        public static async Task<List<GenericMusicItem>> GetRandomListAsync()
        {
            var opr = SQLOperator.Current();
            var p = Tools.Random.Next(15);
            var songs = await opr.GetRandomListAsync<SONG>(25 - p);
            var albums = await opr.GetRandomListAsync<ALBUM>(p);
            var list = songs.ConvertAll(x => new GenericMusicItem(x));

            foreach (var album in albums)
            {
                var albumSongs = Array.ConvertAll(album.Songs.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries), (a) =>
                {
                    return int.Parse(a);
                });

                var s = await opr.GetSongsAsync(albumSongs);
                var s1 = s.OrderBy(a => a.Track);
                s1 = s1.OrderBy(a => a.Disc);

                list.Add(new GenericMusicItem(album)
                {
                    IDs = s1.Select(b => b.ID).ToArray()
                });
            }

            list.Shuffle();
            return list;
        }

        public static async Task<IEnumerable<ListWithKey<GenericMusicItem>>> GetHeroListAsync()
        {
            var opr = SQLOperator.Current();
            var songs = await GetRandomListAsync();
            var todaySuggestion = await opr.GetTodayListAsync();
            var fav = await opr.GetFavListAsync();
            songs.Shuffle();
            todaySuggestion.Shuffle();
            fav.Shuffle();
            var res = new List<ListWithKey<GenericMusicItem>>
            {
                new ListWithKey<GenericMusicItem>("Feeling Lucky", songs),
                new ListWithKey<GenericMusicItem>($"{DateTime.Today.DayOfWeek}'s Suggestion", todaySuggestion),
                new ListWithKey<GenericMusicItem>("Favorite Picks", fav)
            };
            res.Shuffle();
            return res;
        }

        public async static Task<List<Album>> GetAlbumsAsync()
        {
            var opr = SQLOperator.Current();
            var albums = await opr.GetAllAsync<ALBUM>();
            return albums.ConvertAll(a => new Album(a));
        }

        public async Task Read(IList<StorageFolder> folder)
        {
            var list = new List<StorageFile>();
            double i = 1;
            report.Stage = 1;
            report.StageTotal = 3;
            report.Percent = 0;
            foreach (var item in folder)
            {
                var files = await GetFilesAsync(item);

                var opr = SQLOperator.Current();
                if (KnownFolders.MusicLibrary.Equals(item))
                {

                }
                else
                {
                    await opr.UpdateFolderAsync(item, files.Count);
                }

                list.AddRange(files);
                report.Stage = 1;
                report.Percent = 100 * i / folder.Count;
                i++;
                ProgressUpdated?.Invoke(this, report);
            }
            report.Stage = 1;
            report.Percent = 100;
            ProgressUpdated?.Invoke(this, report);
            await ReadFileandSave(from a in list group a by a.Path into b select b.First());
        }

        public async Task ReadFileandSave(IEnumerable<StorageFile> files)
        {
            var opr = SQLOperator.Current();
            List<Song> tempList = new List<Song>();
            double i = 1;
            var total = files.Count();
            foreach (var file in files)
            {
                using (var tagTemp = File.Create(file.Path))
                {
                    tempList.Add(await Song.Create(tagTemp.Tag, file.Path, tagTemp.Properties));
                }
                report.Stage = 2;
                report.Percent = 100 * i / total;
                i++;
                ProgressUpdated?.Invoke(this, report);
            }

            report.Stage = 3;
            report.Percent = 0;
            i = 1;
            ProgressUpdated?.Invoke(this, report);
            var newlist = new List<SONG>();
            foreach (var song in tempList)
            {
                var t = await opr.UpdateSongAsync(song);
                if (t != null)
                {
                    newlist.Add(t);
                }
                report.Stage = 3;
                report.Percent = 100 * i / tempList.Count;
                i++;
                ProgressUpdated?.Invoke(this, report);
            }
            if (newlist.Count > 0)
            {
                await AddToAlbums(newlist);
            }
            else
            {
                Completed?.Invoke(this, EventArgs.Empty);
            }
        }

        public async Task<List<Song>> GetSongsAsync()
        {
            var opr = SQLOperator.Current();
            var songs = await opr.GetAllAsync<SONG>();
            return songs.ConvertAll(a => new Song(a));
        }

        public async Task AddToAlbums(IEnumerable<Song> songs)
        {
            await Task.Run(async () =>
            {
                report.Stage = 4;
                report.Percent = 0;
                ProgressUpdated?.Invoke(this, report);
                double i = 1;

                var albums = from song in songs group song by song.Album into album select album;
                var opr = SQLOperator.Current();

                var count = albums.Count();
                foreach (var item in albums)
                {
                    await opr.AddAlbumAsync(item);
                    report.Stage = 4;
                    report.Percent = 100 * (i++) / count;
                    ProgressUpdated?.Invoke(this, report);
                }
                Completed?.Invoke(this, EventArgs.Empty);
            });
        }

        internal async Task AddToAlbums(IEnumerable<SONG> songs)
        {
            await Task.Run(async () =>
            {
                report.Stage = 4;
                report.Percent = 0;
                ProgressUpdated?.Invoke(this, report);
                double i = 1;

                var albums = from song in songs group song by song.Album into album select album;
                var opr = SQLOperator.Current();

                var count = albums.Count();
                foreach (var item in albums)
                {
                    await opr.AddAlbumAsync(item);
                    report.Stage = 4;
                    report.Percent = 100 * (i++) / count;
                    ProgressUpdated?.Invoke(this, report);
                }
                Completed?.Invoke(this, EventArgs.Empty);
            });
        }
    }

    public class ProgressReport
    {
        public int Stage { get; set; }
        public int StageTotal { get; set; }

        private double progress;

        public double Percent
        {
            get { return progress; }
            set
            {
                if (value > 100)
                    progress = 100;
                else
                    progress = value;
            }
        }

    }
}
