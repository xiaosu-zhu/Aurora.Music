// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Aurora.Music.Core.Extension.Json
{
    [DataContract]
    public class ResultItemModel
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "aid")]
        public int Aid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "artist_id")]
        public int Artist_Id { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "sid")]
        public int Sid { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "lrc")]
        public string Lrc { get; set; }

        ///<summary>
        /// 海阔天空
        /// </summary>
        [DataMember(Name = "song")]
        public string Song { get; set; }

    }



    [DataContract]
    public class GecimiJson
    {
        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "count")]
        public int Count { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "code")]
        public int Code { get; set; }

        ///<summary>
        /// 
        /// </summary>
        [DataMember(Name = "result")]
        public List<ResultItemModel> ResultItems { get; set; }
    }
}
