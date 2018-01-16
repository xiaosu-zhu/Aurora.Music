# Query Intro

This is an introduction of the query in [Extension Development](./Extension%20Development.md).

## Before You Start

Insure you've learned how to implement an app service and the usage of `PropertySet`.

## Lyric Provider

This kind of extensions let Aurora Music have the ability to fetch specific lyrics.

### Receive
For example, when the app wants the lyric of `Love Me Do` by `The Beatles`, here's the staffs you will receive:

| Query key | Value | Description |
| --- | --- | --- |
| `q` | `lyric` | The query action |
| `title` | `Love Me Do` | The title of song |
| *\*`artist`* | `The Beatles` | The Performer of song |

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

**TODO**

## Meta-data Provider

This kind of extensions let Aurora Music have the ability to show the info of albums, performers, etc.

### Receive
When the app wants to show the album info of `Abbey Road` by `The Beatles`, you will receive:

| Query key | Value | Description |
| --- | --- | --- |
| `q` | `online_meta` | The query action |
| `action` | `album` | Type of info to query |
| `album` | `Abbey Road` | The name of album |
| `artist` | `The Beatles` | The artist of album |

**Note**: the `q` value is `online_meta`, which means app wants to query online meta-data. And the `action` value is `album`, which means app wants the album info currently.

### Send
After you got the info, you can send data as below:

| Key | Value | Description |
| --- | --- | --- |
| `status` | `1` | `1 => success, 0 => failed` |
| `album_result` | `PropertySet` | See below |

The data of `album_result` shows below:

| Key | Value | Description |
| --- | --- | --- |
| `name` | `The Beatles` | The name of album |
| `artwork` | `string` | An absolute `Uri` string |
| `desc` | `string` | The description of album |
| `artist` | `The Beatles` | The artist of album |
| `year` | `2009` | `uint`, The publish year of album(you can just send `0`) |

**Note**: the `desc` value should be in the format of Markdown, then app can easily show it in rich-style.

Here's the result:

*TODO*
