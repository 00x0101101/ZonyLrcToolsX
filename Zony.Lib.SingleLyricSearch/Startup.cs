﻿using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Zony.Lib.Infrastructures.Common.Exceptions;
using Zony.Lib.Net;
using Zony.Lib.Net.JsonModels.NetEase;
using Zony.Lib.Plugin;
using Zony.Lib.Plugin.Attributes;
using Zony.Lib.Plugin.Common;
using Zony.Lib.Plugin.Common.Extensions;
using Zony.Lib.Plugin.Interfaces;
using Zony.Lib.Plugin.Models;
using Zony.Lib.UIComponents.Box;

namespace Zony.Lib.SongListDownload
{
    [PluginInfo("网易云歌单获取插件", "Zony", "1.1.0.0", "http://www.myzony.com", "从用户给定的歌单 URL 当中获取歌曲信息。")]
    public class Startup : IPluginExtensions, IPlugin
    {
        private readonly HttpMethodUtils _httpClient = new HttpMethodUtils();

        public Dictionary<string, Dictionary<string, object>> PluginOptions { get; set; }

        public void InitializePlugin(IPluginManager plugManager)
        {
            GlobalContext.Instance.UIContext.AddPluginButton("网易云歌单歌词下载", _button_Click);
        }

        private async void _button_Click(object sender, System.EventArgs e)
        {
            GlobalContext.Instance.UIContext.DisableTopButtons();
            ClearAllContainer();

            var id = GetMusicDetailId();
            if (string.IsNullOrEmpty(id))
            {
                GlobalContext.Instance.UIContext.EnableTopButtons();
                return;
            }

            try
            {
                NetEaseResultModel jsonModel = RequestNetEase(id);
                List<MusicInfoModel> infos = ConvertJsonModelToMusicInfoModel(jsonModel);
                await FillListView(infos);

                GlobalContext.Instance.MusicInfos.AddRange(infos);
                GlobalContext.Instance.UIContext.EnableTopButtons();
            }
            catch (ProxyException)
            {
                MessageBox.Show("代理服务器无法正常访问。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 将 Json 模型转换为 MusicInfoModel
        /// </summary>
        /// <param name="jsonObject">请求返回的 JSON 对象</param>
        /// <returns>转换成功的 MusicInfo 列表，如果失败返回 NULL</returns>
        private List<MusicInfoModel> ConvertJsonModelToMusicInfoModel(NetEaseResultModel jsonObject)
        {
            if (jsonObject.code != "200") return null;
            if (jsonObject.result == null) return null;
            if (jsonObject.result.tracks == null) return null;

            List<MusicInfoModel> result = new List<MusicInfoModel>();
            foreach (var info in jsonObject.result.tracks)
            {
                result.Add(new MusicInfoModel
                {
                    Song = info.name,
                    Artist = info.artists[0].name,
                    Album = info.album.name
                });
            }

            return result;
        }

        /// <summary>
        /// 对网易云 API 发起请求，获取歌单信息
        /// </summary>
        /// <param name="id">歌单 ID</param>
        private NetEaseResultModel RequestNetEase(string id)
        {
            var jsonResult = _httpClient.Get("http://music.163.com/api/playlist/detail", new
            {
                id,
                offset = 0,
                total = true,
                limit = 1000,
                n = 1000,
                csrf_token = string.Empty
            }, "http://music.163.com");

            return JsonConvert.DeserializeObject<NetEaseResultModel>(jsonResult);
        }

        /// <summary>
        /// 获取歌单 ID
        /// </summary>
        private string GetMusicDetailId()
        {
            var inputBox = new InputBox("请输入歌单 URL 或者歌单 ID", "网页版歌URL，例如:\r\n" +
                                         "http://music.163.com/#/playlist?id=716152005\r\n或者输入Id:716152005");
            inputBox.ShowDialog();

            if (string.IsNullOrWhiteSpace(inputBox.ResultText)) return null;

            if (new Regex("^[0-9]*$").IsMatch(inputBox.ResultText)) return inputBox.ResultText;

            var regex = new Regex(@"(?<=id=)\d+");
            var match = regex.Match(inputBox.ResultText);
            if (!match.Success) return null;
            return match.Value;
        }

        #region > UI 相关 <
        /// <summary>
        /// 填充 ListView 组件
        /// </summary>
        /// <param name="infos">歌曲信息列表</param>
        private async Task FillListView(List<MusicInfoModel> infos)
        {
            await Task.Run(() =>
            {
                for (int index = 0; index < infos.Count; index++)
                {
                    infos[index].Index = index;
                    GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items.Add(new ListViewItem(new[]
                    {
                    infos[index].Song,
                    infos[index].Artist,
                    infos[index].Album,
                    "未知",
                    "等待下载"
                    }));
                }
            });
        }

        /// <summary>
        /// 清空歌曲列表容器与 ListView 条目
        /// </summary>
        private void ClearAllContainer()
        {
            GlobalContext.Instance.UIContext.Center_ListViewNF_MusicList.Items.Clear();
            if (GlobalContext.Instance.MusicInfos == null) GlobalContext.Instance.MusicInfos = new List<MusicInfoModel>();
            GlobalContext.Instance.MusicInfos.Clear();
        }
        #endregion
    }
}