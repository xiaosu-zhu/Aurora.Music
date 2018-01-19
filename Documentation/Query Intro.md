# Query Intro

This is an introduction of the query in [Extension Development](./Extension%20Development.md).

## Before You Start

Insure you've learned how to implement an app service and the usage of `ValueSet`.
## Lyric Provider

This kind of extensions let Aurora Music have the ability to fetch specific lyrics.

### Receive
For example, when the app wants the lyric of "Love Me Do" by "The Beatles", here's the staffs you will receive:

| Query key | Value | Description |
| --- | --- | --- |
| `q` | `"lyric"` | The query action |
| `title` | `"Love Me Do"` | The title of song |
| *\*`artist`* | `"The Beatles"` | The Performer of song |
| *\*`album`*  | `"Album Name"`  | The album name of the song |
| *\*`ID`* | `"OnlineID"` | If you are an online music provider you may need this |

*\*: This key is optional, which means sometimes the value is empty.*

### Send
Then, after your extension got lyric, you should send data as following:

| Key | Value | Description |
| --- | --- | --- |
| `status` | `1` | `1 => success, 0 => failed` |
| `result` | `The raw string of lyric` | See below |

**Note that**, the value of `result` is a `string`, which is in the format of lrc, it is something like:

`[00:13.49]Love, love me do[00:16.23]You know I love you[00:19.58]I'll always be true[00:22.80]so please, love me do...`

Meanwhile, if you only provide lyric sentences by sentences, just join them by `\r\n` and send, the app can process.

here's the final result:

![](https://i.loli.net/2018/01/16/5a5df87b3be08.png "Lyric Result")

## Meta-data Provider

This kind of extensions let Aurora Music have the ability to show the info of albums, performers, etc.

### Receive
When the app wants to show the album info of "Abbey Road" by "The Beatles", you will receive:

| Query key | Value | Description |
| --- | --- | --- |
| `q` | `"online_meta"` | The query action |
| `action` | `"album"` | Type of info to query |
| `album` | `"Abbey Road"` | The name of album |
| `artist` | `"The Beatles"` | The artist of album |

**Note**: the `q` value is `"online_meta"`, which means app wants to query online meta-data. And the `action` value is `"album"`, which means app wants the album info currently.

### Send
After you got the info, you can send data as below:

| Key | Value | Description |
| --- | --- | --- |
| `status` | `1` | `1 => success, 0 => failed` |
| `album_result` | `json` | See below |

The data of `album_result` shows below:

| Key | Value | Description |
| --- | --- | --- |
| `name` | `"The Beatles"` | The name of album |
| `artwork` | `string` | An absolute `Uri` string |
| `desc` | `string` | The description of album |
| `artist` | `"The Beatles"` | The artist of album |
| `year` | `2009` | `uint`, The publish year of album(you can just send `0`) |

**Note**: 
1. You can first build the `album_result`from a `ValueSet` and then serialize it to `json`.
2. The `desc` value should be in the format of Markdown, then app can easily show it in rich-style.

Here's the result:

![](https://i.loli.net/2018/01/16/5a5df879d0db4.png "Album Info Result")


### Artist Info is Similarly
### Receive
When the app wants to show the artist info "The Beatles", you will receive:

| Query key | Value | Description |
| --- | --- | --- |
| `q` | `"online_meta"` | The query action |
| `action` | `"artist"` | Type of info to query |
| `artist` | `"The Beatles"` | The artist of album |

**Note**: the `q` value is `"online_meta"`, which means app wants to query online meta-data. And the `action` value is `"album"`, which means app wants the album info currently.

### Send
After you got the info, you can send data as below:

| Key | Value | Description |
| --- | --- | --- |
| `status` | `1` | `1 => success, 0 => failed` |
| `artist_result` | `json` | See below |

The data of `artist_result` shows below:

| Key | Value | Description |
| --- | --- | --- |
| `name` | `"The Beatles"` | The name of artist |
| `avatar` | `string` | An absolute `Uri` string |
| `desc` | `string` | The description of artist |

**Note**:
1. You can first build the `artist_result`from a `ValueSet` and then serialize it to `json`.
2. the `desc` value should be in the format of Markdown, then app can easily show it in rich-style.

Here's the result:

![](https://i.loli.net/2018/01/16/5a5df87b3bf74.png "Album Info Result")
