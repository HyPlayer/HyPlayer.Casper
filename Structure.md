# 结构

> 此文件仅供参考,具体以实际代码为准
>
> 项目接口可能随时改变,此文件可能会过时.

## 序言

自 HyPlayer 发布以来,已经进行过一次重构, 诞生了 `HyPlayList`.

在进行 `WinUI 3` 重构之际, 再次进行大重构.

Meet `HyPlayer.PlayCore`

## 概念

### 专用名词概念

* `MusicProvider`: 音乐提供者

  音乐提供者负责提供音乐来源

* `ProviderId`: 音乐提供者 ID

  音乐提供者的唯一 ID

  * 此处为 `ProviderId`, 规定此 ID 为三个字符. 例如:

    * `ncm`
    * `loc`
  * `Name`: 音乐提供者名称. 例如:

    * `网易云音乐`
    * `本地歌曲`

### PlayItem 相关

* `PlayItem`: 播放项

  每一个播放项对应着一个音乐实体.

  一个播放项包含音乐的`ID`以及基本信息

  * `Id`: 音乐 ID

    每一个音乐实体的唯一ID,格式为 `ProviderId`+`MusicId`

  * `ProviderId`: 音乐提供者 ID

    见 `ProviderId` 节

  * `MusicId`: 音乐真实 ID

    此 ID 为此音乐实体在音乐提供者上的 ID

  * `PlayItemInfo`

    见 `PlayItemInfo` 节

* `PlayItemInfo`: 播放项信息

  播放项对应的音乐实体的基本信息

  * `Name`: 名称 

    音乐的原本名称. 对于本地文件若有 Music Tag 则应为 Music Tag 中的标题项

    若无标题则应当为文件名(不包含后缀)

  * `TranslatedName`: 译名

    音乐的译名. 此译名应当为 `Name` 项翻译后的名称. 若有多个翻译请使用 `\` 分割

  * `Description`: 简介 (暂定定义)

    音乐的简介. 例如:
    * `动画片《猫和老鼠》片头曲`
    * `原曲:ハルトマンの妖怪少女(東方地霊殿)`
    
  * `Album`: 专辑信息

    见 `Album` 节

  * `Artists`: \<List\>艺术家信息

    见 `Artist` 节

* `Album`: 专辑信息

  * `Id`: 专辑 ID

    专辑的唯一 ID. 对于本地歌曲此项可为空
    
  * `ProviderId`: 提供者 ID

    见 `ProviderId` 节
    
  * `AlbumId`: 专辑真实 ID

    专辑在音乐提供者上的 ID

  * `Name`: 专辑名称

    专辑的名称

  * `CoverImage`: 专辑封面

    专辑的封面. 如果没有获取到封面请使用 HyPlayer Icon

    [Warning!] 请千万不要将其置 Null!

* `Artist`: 艺术家信息

  * `Id`: 艺术家 ID

    艺术家的唯一 ID. 对于本地歌曲此项可为空

  * `ProviderId`: 提供者 ID

    见 `ProviderId` 节

  * `ArtistId`: 专辑真实 ID

    艺术家在音乐提供者上的 ID

  * `Name`: 艺术家名称

    艺术家名称

  

### MusicProvider 相关

#### IPlaySource
`IPlaySource`: 播放源

  播放源包含了播放项以及一些基本信息 (例如歌单,专辑,歌手热门)

  * `Id`: 播放源唯一 ID

    此 ID 由三个部分组成: `ProviderId`+`PlaySourceType`+`ActrualPlaySourceId`

  * `ProviderId`: 见 `ProviderId` 节

  * `PlaySourceType`: 播放源类型. 限定为两个字符. 例如:

    * `pl`: 播放列表 / 歌单
    * `al`: 专辑
    * `sh`: 歌手热门
    * `rd`: 电台

    ...... (以上例子仅供参考)

  * `Name`: 播放源名称

  * `ActualPlaySourceId`: 音乐提供者上的 ID


#### IMusicProvider
 `IMusicProvider`: 音乐提供者接口

  音乐提供者为提供音乐基础信息的组件. 

  * `Id`: 见 `ProviderId` 节


  ##### 通用方法

  * `GetPlayItemInfo`: 通过 ID 获取音乐信息
  * `GetPlayItemMediaSource`: 通过 ID 获取音乐源
  * `GetPlayListInfo`: 通过 ID 获取歌单信息
  * `GetPlayLists`: 通过 ID 获取收藏的歌单
  * `GetPlaySourceItems`: 通过 ID 获取播单源的播放项
  * `GetPlayItemsByIds`: 通过 ID 列表获取对应的播放项
  * `GetPlayItemLyric`: 通过 ID 获取歌曲的歌词

  

* `IOnlineMusicProvider`: 在线歌曲 (implements from `IMusicProvider`)
  
  * `GetPlayItemTranslatedLyric`: 通过 ID 获取翻译
  
  
  
  
  
  
  
    

