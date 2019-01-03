// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.ViewModels;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using TagLib;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“内容对话框”项模板

namespace Aurora.Music.Controls
{
    public sealed partial class TagDialog : ContentDialog, INotifyPropertyChanged
    {
        internal SongViewModel Model { get; private set; }
        private BitmapImage artwork;
        public BitmapImage Artwork
        {
            get { return artwork; }
            set { SetProperty(ref artwork, value); }
        }

        internal TagDialog(SongViewModel song)
        {
            InitializeComponent();
            RequestedTheme = Settings.Current.Theme;
            Model = song;
            ID = song.ID;
            FilePath = song.FilePath;

            Artwork = new BitmapImage();
            var t = ThreadPool.RunAsync(async x =>
            {
                await Init();
            });
        }

        private async Task Init()
        {
            var path = Model.FilePath;
            file = await StorageFile.GetFileFromPathAsync(path);
            if (Model.IsOnline || path.IsNullorEmpty())
            {
                PrimaryButtonText = null;
                return;
            }
            using (var tagTemp = TagLib.File.Create(file.AsAbstraction()))
            {
                var song = Model.Song;
                var prop = await file.GetViolatePropertiesAsync();
                await song.UpdatePropertiesAsync(tagTemp.Tag, path, prop, tagTemp.Properties, null);
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                {
                    SongTitle = song.Title;
                    Duration = song.Duration;
                    BitRate = song.BitRate;
                    Rating = oldRating = prop.rating;
                    MusicBrainzArtistId = song.MusicBrainzArtistId;
                    MusicBrainzDiscId = song.MusicBrainzDiscId;
                    MusicBrainzReleaseArtistId = song.MusicBrainzReleaseArtistId;
                    MusicBrainzReleaseCountry = song.MusicBrainzReleaseCountry;
                    MusicBrainzReleaseId = song.MusicBrainzReleaseId;
                    MusicBrainzReleaseStatus = song.MusicBrainzReleaseStatus;
                    MusicBrainzReleaseType = song.MusicBrainzReleaseType;
                    MusicBrainzTrackId = song.MusicBrainzTrackId;
                    MusicIpId = song.MusicIpId;
                    BeatsPerMinute = song.BeatsPerMinute;
                    Album = song.Album;
                    AlbumArtists = song.AlbumArtists;
                    AlbumArtistsSort = song.AlbumArtistsSort;
                    AlbumSort = song.AlbumSort;
                    AmazonId = song.AmazonId;
                    TitleSort = song.TitleSort;
                    Track = song.Track;
                    TrackCount = song.TrackCount;
                    ReplayGainTrackGain = song.ReplayGainTrackGain;
                    ReplayGainTrackPeak = song.ReplayGainTrackPeak;
                    ReplayGainAlbumGain = song.ReplayGainAlbumGain;
                    ReplayGainAlbumPeak = song.ReplayGainAlbumPeak;
                    Comment = song.Comment;
                    Disc = song.Disc;
                    Composers = song.Composers;
                    ComposersSort = song.ComposersSort;
                    Conductor = song.Conductor;
                    DiscCount = song.DiscCount;
                    Copyright = song.Copyright;
                    Genres = song.Genres;
                    Grouping = song.Grouping;
                    Lyrics = song.Lyrics;
                    Performers = song.Performers;
                    PerformersSort = song.PerformersSort;
                    Year = song.Year;
                    SampleRate = tagTemp.Properties.AudioSampleRate;
                    AudioChannels = tagTemp.Properties.AudioChannels;

                    if (!song.PicturePath.IsNullorEmpty())
                    {
                        using (var stream = await (await StorageFile.GetFileFromPathAsync(song.PicturePath)).OpenReadAsync())
                        {
                            await Artwork.SetSourceAsync(stream);
                        }
                    }
                    else
                    {
                        Artwork.UriSource = new Uri(Consts.NowPlaceholder);
                    }
                });
            }
        }

        private async void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            var succeed = true;
            await PlaybackEngine.PlaybackEngine.Current.DetachCurrentItem();
            try
            {
                if (file.FileType.Equals(".wav", StringComparison.InvariantCultureIgnoreCase))
                {
                    var props = await file.Properties.GetMusicPropertiesAsync();
                    props.Album = Album;
                    props.AlbumArtist = string.Join(';', AlbumArtists);
                    props.Artist = string.Join(';', Performers);
                    props.Genre.Clear();
                    foreach (var item in Genres)
                    {
                        props.Genre.Add(item);
                    }
                    props.Title = SongTitle;
                    props.TrackNumber = Track;
                    props.Year = Year;
                    await props.SavePropertiesAsync();
                }
                else
                {
                    using (var tagTemp = TagLib.File.Create(file.AsAbstraction()))
                    {
                        tagTemp.Tag.Title = SongTitle;
                        tagTemp.Tag.MusicBrainzArtistId = MusicBrainzArtistId;
                        tagTemp.Tag.MusicBrainzDiscId = MusicBrainzDiscId;
                        tagTemp.Tag.MusicBrainzReleaseArtistId = MusicBrainzReleaseArtistId;
                        tagTemp.Tag.MusicBrainzReleaseCountry = MusicBrainzReleaseCountry;
                        tagTemp.Tag.MusicBrainzReleaseId = MusicBrainzReleaseId;
                        tagTemp.Tag.MusicBrainzReleaseStatus = MusicBrainzReleaseStatus;
                        tagTemp.Tag.MusicBrainzReleaseType = MusicBrainzReleaseType;
                        tagTemp.Tag.MusicBrainzTrackId = MusicBrainzTrackId;
                        tagTemp.Tag.MusicIpId = MusicIpId;
                        tagTemp.Tag.BeatsPerMinute = BeatsPerMinute;
                        tagTemp.Tag.Album = Album;
                        tagTemp.Tag.AlbumArtists = AlbumArtists;
                        tagTemp.Tag.AlbumArtistsSort = AlbumArtistsSort;
                        tagTemp.Tag.AlbumSort = AlbumSort;
                        tagTemp.Tag.AmazonId = AmazonId;
                        tagTemp.Tag.TitleSort = TitleSort;
                        tagTemp.Tag.Track = Track;
                        tagTemp.Tag.TrackCount = TrackCount;
                        tagTemp.Tag.ReplayGainTrackGain = ReplayGainTrackGain;
                        tagTemp.Tag.ReplayGainTrackPeak = ReplayGainTrackPeak;
                        tagTemp.Tag.ReplayGainAlbumGain = ReplayGainAlbumGain;
                        tagTemp.Tag.ReplayGainAlbumPeak = ReplayGainAlbumPeak;
                        tagTemp.Tag.Comment = Comment;
                        tagTemp.Tag.Disc = Disc;
                        tagTemp.Tag.Composers = Composers;
                        tagTemp.Tag.ComposersSort = ComposersSort;
                        tagTemp.Tag.Conductor = Conductor;
                        tagTemp.Tag.DiscCount = DiscCount;
                        tagTemp.Tag.Copyright = Copyright;
                        tagTemp.Tag.PerformersSort = Genres;
                        tagTemp.Tag.Grouping = Grouping;
                        tagTemp.Tag.Lyrics = Lyrics;
                        tagTemp.Tag.Performers = Performers;
                        tagTemp.Tag.PerformersSort = PerformersSort;
                        tagTemp.Tag.Year = Year;

                        tagTemp.Save();
                    }
                }
                await FileReader.UpdateSongAsync(Model.Song);
            }
            catch (Exception)
            {
                MainPage.Current.PopMessage("Updating tags failed");
                succeed = false;
            }
            if (oldRating != newRating)
            {
                try
                {
                    await Model.Song.WriteRatingAsync(Rating);
                }
                catch (Exception)
                {
                    MainPage.Current.PopMessage("Updating rating failed");
                    succeed = false;
                }
            }
            await PlaybackEngine.PlaybackEngine.Current.ReAttachCurrentItem();
            if (succeed)
                MainPage.Current.PopMessage("Succeed");
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private DelegateCommand ChangeArtwork
        {
            get => new DelegateCommand(async () =>
              {
                  var picker = new Windows.Storage.Pickers.FileOpenPicker
                  {
                      ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail,
                      SuggestedStartLocation =
                      Windows.Storage.Pickers.PickerLocationId.PicturesLibrary
                  };
                  picker.FileTypeFilter.Add(".jpg");
                  picker.FileTypeFilter.Add(".jpeg");
                  picker.FileTypeFilter.Add(".png");

                  var img = await picker.PickSingleFileAsync();
                  if (img != null)
                  {
                      await PlaybackEngine.PlaybackEngine.Current.DetachCurrentItem();

                      Artwork = new BitmapImage(new Uri(img.Path));

                      using (var tagTemp = TagLib.File.Create(file.AsAbstraction()))
                      {
                          using (var stream = await img.OpenReadAsync())
                          {
                              if (tagTemp.Tag.Pictures != null && tagTemp.Tag.Pictures.Length > 0)
                              {
                                  var p = new List<IPicture>();
                                  p.AddRange(tagTemp.Tag.Pictures);
                                  p.RemoveAt(0);
                                  p.Insert(0, new Picture(ByteVector.FromStream(stream.AsStream())));
                                  tagTemp.Tag.Pictures = p.ToArray();
                              }
                              else
                              {
                                  var p = new List<Picture>
                                  {
                                      new Picture(ByteVector.FromStream(stream.AsStream()))
                                  };
                                  tagTemp.Tag.Pictures = p.ToArray();
                              }
                          }
                          tagTemp.Save();
                      }

                      var options = new Windows.Storage.Search.QueryOptions(Windows.Storage.Search.CommonFileQuery.DefaultQuery, new string[] { ".jpg", ".png", ".bmp" })
                      {
                          ApplicationSearchFilter = $"System.FileName:{Model.ID}.*"
                      };

                      var query = ApplicationData.Current.TemporaryFolder.CreateFileQueryWithOptions(options);
                      var files = await query.GetFilesAsync();
                      if (Model.ID.ToString() != "0" && files.Count > 0)
                      {
                          foreach (var file in files)
                          {
                              await file.DeleteAsync(StorageDeleteOption.PermanentDelete);
                          }
                      }

                      await PlaybackEngine.PlaybackEngine.Current.ReAttachCurrentItem();
                  }
                  else
                  {

                  }
                  MainPage.Current.PopMessage("Succeed");
              });
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetProperty<T>(ref T backingField, T Value, [CallerMemberName] string propertyName = null)
        {
            var changed = !EqualityComparer<T>.Default.Equals(backingField, Value);
            if (changed)
            {
                backingField = Value;
                this.RaisePropertyChanged(propertyName);
            }
            return changed;
        }

        public Visibility BooltoVisibility(bool b)
        {
            return b ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility BoolNottoVisibility(bool b)
        {
            return !b ? Visibility.Visible : Visibility.Collapsed;
        }
        public Visibility NullableBooltoVisibility(bool? b)
        {
            if (b is bool a)
            {
                return a ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }
        public Visibility NullableBoolNottoVisibility(bool? b)
        {
            if (b is bool a)
            {
                return !a ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }
        public Visibility CollapseIfEmpty(string s)
        {
            return s.IsNullorEmpty() ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotEmpty(string s)
        {
            return !s.IsNullorEmpty() ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNull(object s)
        {
            return s == null ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotNull(object s)
        {
            return s != null ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfZero(int count)
        {
            return count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfZero(uint count)
        {
            return count == 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotZero(int count)
        {
            return count != 0 ? Visibility.Collapsed : Visibility.Visible;
        }
        public Visibility CollapseIfNotZero(uint count)
        {
            return count != 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public string StrArrToString(string[] arr)
        {
            if (arr == null)
            {
                return string.Empty;
            }
            return string.Join("; ", arr);
        }

        public void StringToPerformers(string text)
        {
            var arr = text.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arr)
            {
                item.Trim();
            }
            Performers = arr.Where(x => !x.IsNullorEmpty()).ToArray();
            Model.Song.Performers = Performers;
        }

        public void StringToAlbumArtists(string text)
        {
            var arr = text.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arr)
            {
                item.Trim();
            }
            AlbumArtists = arr.Where(x => !x.IsNullorEmpty()).ToArray();
            Model.Song.AlbumArtists = AlbumArtists;
        }

        public void StringToGenres(string text)
        {
            var arr = text.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var item in arr)
            {
                item.Trim();
            }
            Genres = arr.Where(x => !x.IsNullorEmpty()).ToArray();
            Model.Song.Genres = Genres;
        }

        public void StringToTrack(string text)
        {
            if (uint.TryParse(text, out var r))
            {
                Track = r;
                Model.Track = r;
            }
            else
            {
                if (!text.IsNullorEmpty())
                {
                    Track = Model.Track;
                }
            }
        }

        public void StringToTrackCount(string text)
        {
            if (uint.TryParse(text, out var r))
            {
                TrackCount = r;
                Model.Song.TrackCount = r;
            }
            else
            {
                if (!text.IsNullorEmpty())
                {
                    TrackCount = Model.Song.TrackCount;
                }
            }
        }

        public void StringToDisc(string text)
        {
            if (uint.TryParse(text, out var r))
            {
                Disc = r;
                Model.Song.Disc = r;
            }
            else
            {
                if (!text.IsNullorEmpty())
                {
                    Disc = Model.Song.Disc;
                }
            }
        }

        public void StringToDiscCount(string text)
        {
            if (uint.TryParse(text, out var r))
            {
                DiscCount = r;
                Model.Song.DiscCount = r;
            }
            else
            {
                if (!text.IsNullorEmpty())
                {
                    DiscCount = Model.Song.DiscCount;
                }
            }
        }

        public void StringToRating(string text)
        {
            if (uint.TryParse(text, out var r))
            {
                Rating = r;
                Model.Rating = r;
            }
            else
            {
                if (!text.IsNullorEmpty())
                {
                    Rating = (uint)(Model.Rating * 20);
                }
            }
        }

        public void StringToYear(string text)
        {
            if (uint.TryParse(text, out var r))
            {
                Year = r;
                Model.Song.Year = r;
            }
            else
            {
                if (!text.IsNullorEmpty())
                {
                    Year = Model.Song.Year;
                }
            }
        }

        public string BitRateToString(uint bitrate)
        {
            return $"{(bitrate / 1000.0).ToString("0.#", CultureInfoHelper.CurrentCulture)} Kbps";
        }

        public string DurationtoString(TimeSpan t1)
        {
            return $"{(int)(Math.Floor(t1.TotalMinutes))}{CultureInfoHelper.CurrentCulture.DateTimeFormat.TimeSeparator}{t1.Seconds.ToString("00")}{CultureInfoHelper.CurrentCulture.NumberFormat.NumberDecimalSeparator}{(t1.Milliseconds / 10).ToString("00")}";
        }

        private TimeSpan duration;
        public TimeSpan Duration
        {
            get { return duration; }
            set { SetProperty(ref duration, value); }
        }
        private uint bitRate;
        public uint BitRate
        {
            get { return bitRate; }
            set { SetProperty(ref bitRate, value); }
        }

        private string filePath;
        public string FilePath
        {
            get { return filePath; }
            set { SetProperty(ref filePath, value); }
        }
        private string picturePath;
        public string PicturePath
        {
            get { return picturePath; }
            set { SetProperty(ref picturePath, value); }
        }

        private string musicBrainzReleaseId;
        public string MusicBrainzReleaseId
        {
            get { return musicBrainzReleaseId; }
            set { SetProperty(ref musicBrainzReleaseId, value); }
        }
        private string musicBrainzDiscId;
        public string MusicBrainzDiscId
        {
            get { return musicBrainzDiscId; }
            set { SetProperty(ref musicBrainzDiscId, value); }
        }
        private string musicIpId;
        public string MusicIpId
        {
            get { return musicIpId; }
            set { SetProperty(ref musicIpId, value); }
        }
        private string amazonId;
        public string AmazonId
        {
            get { return amazonId; }
            set { SetProperty(ref amazonId, value); }
        }
        private string musicBrainzReleaseStatus;
        public string MusicBrainzReleaseStatus
        {
            get { return musicBrainzReleaseStatus; }
            set { SetProperty(ref musicBrainzReleaseStatus, value); }
        }
        private string musicBrainzReleaseType;
        public string MusicBrainzReleaseType
        {
            get { return musicBrainzReleaseType; }
            set { SetProperty(ref musicBrainzReleaseType, value); }
        }
        private string musicBrainzReleaseCountry;
        public string MusicBrainzReleaseCountry
        {
            get { return musicBrainzReleaseCountry; }
            set { SetProperty(ref musicBrainzReleaseCountry, value); }
        }
        private double replayGainTrackGain;
        public double ReplayGainTrackGain
        {
            get { return replayGainTrackGain; }
            set { SetProperty(ref replayGainTrackGain, value); }
        }
        private double replayGainTrackPeak;
        public double ReplayGainTrackPeak
        {
            get { return replayGainTrackPeak; }
            set { SetProperty(ref replayGainTrackPeak, value); }
        }
        private double replayGainAlbumGain;
        public double ReplayGainAlbumGain
        {
            get { return replayGainAlbumGain; }
            set { SetProperty(ref replayGainAlbumGain, value); }
        }
        private double replayGainAlbumPeak;
        public double ReplayGainAlbumPeak
        {
            get { return replayGainAlbumPeak; }
            set { SetProperty(ref replayGainAlbumPeak, value); }
        }
        //public IPicture[] Pictures { get; set; }
        private string firstAlbumArtist;
        public string FirstAlbumArtist
        {
            get { return firstAlbumArtist; }
            set { SetProperty(ref firstAlbumArtist, value); }
        }
        private string firstAlbumArtistSort;
        public string FirstAlbumArtistSort
        {
            get { return firstAlbumArtistSort; }
            set { SetProperty(ref firstAlbumArtistSort, value); }
        }
        private string firstPerformer;
        public string FirstPerformer
        {
            get { return firstPerformer; }
            set { SetProperty(ref firstPerformer, value); }
        }
        private string firstPerformerSort;
        public string FirstPerformerSort
        {
            get { return firstPerformerSort; }
            set { SetProperty(ref firstPerformerSort, value); }
        }
        private string firstComposerSort;
        public string FirstComposerSort
        {
            get { return firstComposerSort; }
            set { SetProperty(ref firstComposerSort, value); }
        }
        private string firstComposer;
        public string FirstComposer
        {
            get { return firstComposer; }
            set { SetProperty(ref firstComposer, value); }
        }
        private string firstGenre;
        public string FirstGenre
        {
            get { return firstGenre; }
            set { SetProperty(ref firstGenre, value); }
        }
        private string joinedAlbumArtists;
        public string JoinedAlbumArtists
        {
            get { return joinedAlbumArtists; }
            set { SetProperty(ref joinedAlbumArtists, value); }
        }
        private string joinedPerformers;
        public string JoinedPerformers
        {
            get { return joinedPerformers; }
            set { SetProperty(ref joinedPerformers, value); }
        }
        private string joinedPerformersSort;
        public string JoinedPerformersSort
        {
            get { return joinedPerformersSort; }
            set { SetProperty(ref joinedPerformersSort, value); }
        }
        private string joinedComposers;
        public string JoinedComposers
        {
            get { return joinedComposers; }
            set { SetProperty(ref joinedComposers, value); }
        }
        private string musicBrainzTrackId;
        public string MusicBrainzTrackId
        {
            get { return musicBrainzTrackId; }
            set { SetProperty(ref musicBrainzTrackId, value); }
        }
        private string musicBrainzReleaseArtistId;
        public string MusicBrainzReleaseArtistId
        {
            get { return musicBrainzReleaseArtistId; }
            set { SetProperty(ref musicBrainzReleaseArtistId, value); }
        }
        private bool isEmpty;
        public bool IsEmpty
        {
            get { return isEmpty; }
            set { SetProperty(ref isEmpty, value); }
        }
        private string musicBrainzArtistId;
        public string MusicBrainzArtistId
        {
            get { return musicBrainzArtistId; }
            set { SetProperty(ref musicBrainzArtistId, value); }
        }
        private string songTitle;
        public string SongTitle
        {
            get { return songTitle; }
            set
            {
                SetProperty(ref songTitle, value);
                Model.Title = value;
            }
        }
        private string titleSort;
        public string TitleSort
        {
            get { return titleSort; }
            set { SetProperty(ref titleSort, value); }
        }
        private string[] performers;
        public string[] Performers
        {
            get { return performers; }
            set { SetProperty(ref performers, value); }
        }
        private string[] performersSort;
        public string[] PerformersSort
        {
            get { return performersSort; }
            set { SetProperty(ref performersSort, value); }
        }
        private string[] albumArtists;
        public string[] AlbumArtists
        {
            get { return albumArtists; }
            set { SetProperty(ref albumArtists, value); }
        }
        private string[] albumArtistsSort;
        public string[] AlbumArtistsSort
        {
            get { return albumArtistsSort; }
            set { SetProperty(ref albumArtistsSort, value); }
        }
        private string[] composers;
        public string[] Composers
        {
            get { return composers; }
            set { SetProperty(ref composers, value); }
        }
        private string[] composersSort;
        public string[] ComposersSort
        {
            get { return composersSort; }
            set { SetProperty(ref composersSort, value); }
        }
        private string album;
        public string Album
        {
            get { return album; }
            set
            {
                SetProperty(ref album, value);
                Model.Album = Album;
            }
        }
        private string joinedGenres;
        public string JoinedGenres
        {
            get { return joinedGenres; }
            set { SetProperty(ref joinedGenres, value); }
        }
        private string albumSort;
        public string AlbumSort
        {
            get { return albumSort; }
            set { SetProperty(ref albumSort, value); }
        }
        private string[] genres;
        public string[] Genres
        {
            get { return genres; }
            set { SetProperty(ref genres, value); }
        }
        private uint year;
        public uint Year
        {
            get { return year; }
            set { SetProperty(ref year, value); }
        }
        private uint track;
        public uint Track
        {
            get { return track; }
            set { SetProperty(ref track, value); }
        }
        private uint trackCount;
        public uint TrackCount
        {
            get { return trackCount; }
            set { SetProperty(ref trackCount, value); }
        }
        private uint disc;
        public uint Disc
        {
            get { return disc; }
            set { SetProperty(ref disc, value); }
        }
        private uint discCount;
        public uint DiscCount
        {
            get { return discCount; }
            set { SetProperty(ref discCount, value); }
        }
        private string lyrics;
        public string Lyrics
        {
            get { return lyrics; }
            set { SetProperty(ref lyrics, value); }
        }
        private string grouping;
        public string Grouping
        {
            get { return grouping; }
            set { SetProperty(ref grouping, value); }
        }
        private uint beatsPerMinute;
        public uint BeatsPerMinute
        {
            get { return beatsPerMinute; }
            set { SetProperty(ref beatsPerMinute, value); }
        }
        private string conductor;
        public string Conductor
        {
            get { return conductor; }
            set { SetProperty(ref conductor, value); }
        }
        private string copyright;
        public string Copyright
        {
            get { return copyright; }
            set { SetProperty(ref copyright, value); }
        }
        private string comment;
        public string Comment
        {
            get { return comment; }
            set { SetProperty(ref comment, value); }
        }
        public int ID { get; set; }
        private int sampleRate;
        public int SampleRate
        {
            get { return sampleRate; }
            set { SetProperty(ref sampleRate, value); }
        }
        private int audioChannels;
        public int AudioChannels
        {
            get { return audioChannels; }
            set { SetProperty(ref audioChannels, value); }
        }
        public bool IsOnline { get; set; }
        public Uri OnlineUri { get; set; }
        public string OnlineID { get; set; }
        private uint oldRating, newRating;
        private StorageFile file;

        public uint Rating
        {
            get { return newRating; }
            set { SetProperty(ref newRating, value); }
        }
        public string OnlineAlbumID { get; set; }
        public string FileType { get; internal set; }

        private void Button_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            ForeGrid.Visibility = Visibility.Visible;
        }

        private void Button_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            ForeGrid.Visibility = Visibility.Collapsed;
        }
    }
}
