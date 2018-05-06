# This doc introduces how to activate Aurora Music with Uri.

## `as-music:`

To launch main app.

## `as-music:///home`

To launch HomePage. Similarly,

* `as-music:///library`
* `as-music:///settings`
* `as-music:///douban`
* `as-music:///download`
* `as-music:///about`

to navigate to specific pages.

## `as-music:///library/songs`

To go to the Songs page. Similarly,

* `as-music:///library/albums`
* `as-music:///library/artists`
* `as-music:///library/playlist`

to go to specific pages.

* `as-music:///library/podcast`

can go to the first subscribed podcast, and

* `as-music:///library/podcast/id/{0}`

can go to the subscribed podcast with specific id (database).


## Query Strings

For example: `as-music:///?action=last-play` let the player play last tracks directly.

you can also use it with other segments, such as `as-music:///library/podcast/id/{0}?action=play` to play a podcast directly(not support yet).

| key | value | description
| --- | --- | --- |
| `action` | `last-play` | play the last played tracks |
|   | `play` | start playing |
|   | `pause` | pause |
|   | `seek` | seek to a specific time, must have `value`, format: `m:ss.fff` |
|   | `volume` | set volume, must have `value`(0~100) |
|   | `loop` | toggle loop |
|   | `shuffle` | toggle shuffle |
|   | `mute` | set volume to 0 |
|   | `search` | <!-- focus the search box /-->not support yet |
| `value` | additional value with `seek`, `volume` and `search` |
