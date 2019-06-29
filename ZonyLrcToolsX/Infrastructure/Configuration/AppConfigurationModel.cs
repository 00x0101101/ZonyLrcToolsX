using System.Collections.Generic;

namespace ZonyLrcToolsX.Infrastructure.Configuration
{
    /// <summary>
    /// �����������Ϣ��
    /// </summary>
    public class AppConfigurationModel
    {
        /// <summary>
        /// ֧�ֵĸ����ļ���׺����
        /// </summary>
        public List<string> SuffixName { get; set; }

        /// <summary>
        /// �洢�ĸ���ļ����뷽ʽ��
        /// </summary>
        public int CodePage { get; set; }

        /// <summary>
        /// �Ƿ���������������ܡ�
        /// </summary>
        public bool IsEnableProxy { get; set; }

        /// <summary>
        /// �������������� IP��
        /// </summary>
        public string ProxyIp { get; set; }

        /// <summary>
        /// �������������Ķ˿ںš�
        /// </summary>
        public int ProxyPort { get; set; }

        /// <summary>
        /// ���������ͬ���� Lyric �ļ����Ƿ���и��ǲ�����
        /// </summary>
        public bool IsCoverSourceLyricFile { get; set; }

        /// <summary>
        /// �Ƿ��Զ����������£������Զ������Ϊ True����������Ϊ False��
        /// </summary>
        public bool IsAutoCheckUpdate { get; set; }

        public AppConfigurationModel()
        {
            SuffixName = new List<string>();
        }
    }
}