// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Aurora.Music.Core.Models
{
    public class OnlineMusicItem : GenericMusicItem
    {
        public string[] OnlineID { get; }
        public string OnlineAlbumId { get; }

        public OnlineMusicItem(string title, string description, string addtional, string[] id, string albumId, string pi = null)
        {
            Title = title;
            Description = description;
            Addtional = addtional;
            OnlineID = id;
            IsOnline = true;
            OnlineAlbumId = albumId;
            PicturePath = pi;
        }
    }
}
