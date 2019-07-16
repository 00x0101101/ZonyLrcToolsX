using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using ZonyLrcToolsX.Downloader.Lyric.Exceptions;
using ZonyLrcToolsX.Downloader.Lyric.QQMusic.JsonModels;
using ZonyLrcToolsX.Infrastructure.Configuration;
using ZonyLrcToolsX.Infrastructure.Lyric;
using ZonyLrcToolsX.Infrastructure.MusicTag;
using ZonyLrcToolsX.Infrastructure.Network.Http;

namespace ZonyLrcToolsX.Downloader.Lyric.QQMusic
{
    public class QQMusicCloudMusicLyricDownloader : ILyricDownloader
    {
        private readonly WrappedHttpClient _wrappedHttpClient;

        public QQMusicCloudMusicLyricDownloader()
        {
            _wrappedHttpClient = new WrappedHttpClient();
        }

        public async Task<LyricItemCollection> DownloadAsync(MusicInfo musicInfo)
        {
            var requestParameter = new MusicSearchRequestModel(musicInfo.Name, musicInfo.Artist);
            var searchResult = await _wrappedHttpClient.GetAsync<MusicSearchResponseModel>(
                url: @"http://c.y.qq.com/soso/fcgi-bin/client_search_cp",
                parameters: requestParameter);

            if(searchResult == null || searchResult.StatusCode != 0 || searchResult.Data.Song.SongItems == null)
            {
                throw new RequestErrorException("QQ ���ֽӿ�û���������ؽ��...", musicInfo);
            }
            if (searchResult.Data.Song.SongItems.Count <= 0 ) throw new NotFoundSongException("û��������ָ���ĸ�����",musicInfo);

            var lyricJsonStr = await _wrappedHttpClient.GetAsync(
                url: @"http://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric.fcg",
                parameters: new MusicGetLyricRequest(searchResult.Data.Song.SongItems[0].SongId),
                refererUrl: @"https://y.qq.com/"
            );

            lyricJsonStr = lyricJsonStr.Replace(@"MusicJsonCallback(", string.Empty).TrimEnd(')');
            lyricJsonStr = HttpUtility.HtmlDecode(lyricJsonStr);
            if(lyricJsonStr.Contains("�˸���Ϊû����ʵĴ����֣���������")) return new LyricItemCollection(string.Empty);

            var lyricJsonObj = JsonConvert.DeserializeObject<MusicGetLyricResponse>(lyricJsonStr);

            // TODO: ��ʱ��֧�� QQ ���ֵĸ�ʷ��룬ֱ�ӷ���Դ���Ը�ʡ�
            return new LyricItemCollection(lyricJsonObj.LyricText);
        }
    }
}