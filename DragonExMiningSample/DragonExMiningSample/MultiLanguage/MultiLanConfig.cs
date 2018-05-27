using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DragonExMiningSampleCSharp.MultiLanguage
{
    class MultiLanConfig
    {
        /// <summary>
        /// Lock object
        /// </summary>
        private static object lockObj = new object();

        /// <summary>
        /// Instance
        /// </summary>
        private static MultiLanConfig instance;

        /// <summary>
        /// Multi language Dictionary
        /// </summary>
        private Dictionary<string, Dictionary<string, string>> MultiLanDict;

        /// <summary>
        /// Config path
        /// </summary>
        private static string configPath = "\\MultiLanguageConfig.bin";

        /// <summary>
        /// MultiLanKey
        /// </summary>
        private MultiLanguage multiLanKey = MultiLanguage.English;

        /// <summary>
        /// Singleton Instance
        /// </summary>
        public static MultiLanConfig Instance
        {
            get
            {
                if(instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new MultiLanConfig();
                        }
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        private MultiLanConfig()
        {
            Enum.TryParse<MultiLanguage>(Properties.Settings.Default.MultiLanguage, out multiLanKey);
            ConfigFileReader cfr = new ConfigFileReader(Application.StartupPath + configPath);
            MultiLanDict = cfr.ReadConfig();
        }

        /// <summary>
        /// Get value for key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetKeyValue(string key)
        {
            string result = key;
            if (MultiLanDict.ContainsKey(multiLanKey.ToString()))
            {
                if (MultiLanDict[multiLanKey.ToString()].ContainsKey(key))
                {
                    result = MultiLanDict[multiLanKey.ToString()][key];
                }
            }
            return result;
        }

        /// <summary>
        /// Get value for key
        /// </summary>
        /// <param name="key"></param>
        /// <param name="oldValue"></param>
        /// <returns></returns>
        public string GetKeyValue(string key, string oldValue)
        {
            string result = oldValue;
            if (MultiLanDict.ContainsKey(multiLanKey.ToString()))
            {
                if (MultiLanDict[multiLanKey.ToString()].ContainsKey(key))
                {
                    result = MultiLanDict[multiLanKey.ToString()][key];
                }
            }
            return result;
        }

        /// <summary>
        /// Change multi language
        /// </summary>
        /// <param name="form"></param>
        public void ChangeMultiLan(Form form)
        {
            if(form.Tag != null)
            {
                var tag = form.Tag.ToString();
                if (!string.IsNullOrEmpty(tag)
                    && tag.StartsWith("ML_"))
                {
                    form.Text = MultiLanConfig.Instance.GetKeyValue(tag.Trim(), form.Text);
                }
            }
            foreach (Control ctl in form.Controls)
            {
                ChangeMultiLan(ctl);
            }
        }

        /// <summary>
        /// Change multi language
        /// </summary>
        /// <param name="ctl"></param>
        public void ChangeMultiLan(Control ctl)
        {
            if (ctl.Tag != null)
            {
                var tag = ctl.Tag.ToString();
                if (!string.IsNullOrEmpty(tag)
                    && tag.StartsWith("ML_"))
                {
                    ctl.Text = MultiLanConfig.Instance.GetKeyValue(tag.Trim(), ctl.Text);
                }
            }
            if (ctl is Panel)
            {
                foreach (Control subCtl in ctl.Controls)
                {
                    ChangeMultiLan(subCtl);
                }
            }
        }
    }

    enum MultiLanguage
    {
        English,
        Chinese
    }
}
