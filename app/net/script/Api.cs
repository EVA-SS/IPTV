using PanGu;
using System;

namespace script
{
    internal static class Api
    {
        #region 筛选配置

        internal static List<string> filter(this string path)
        {
            var arr = File.ReadAllLines(path);
            var r = new List<string>();
            foreach (var ite in arr)
            {
                string it = ite.Trim();
                if (!string.IsNullOrEmpty(it) && !it.StartsWith("# "))
                {
                    r.Add(it);
                }
            }
            return r;
        }

        internal static List<IFilter> ifilter(this string path)
        {
            if (File.Exists(path))
            {
                var arr = File.ReadAllLines(path);
                var r = new List<IFilter>();
                foreach (var ite in arr)
                {
                    string it = ite.Trim();
                    if (!string.IsNullOrEmpty(it) && !it.StartsWith("# "))
                    {
                        if (it.Contains("*"))
                        {
                            var txt_arr = it.ArrSplit('*', out var len);
                            if (len > 1)
                            {
                                r.Add(new IFilter
                                {
                                    type = IFilterType.OR,
                                    data = txt_arr
                                });
                            }
                            else
                            {
                                if (it.StartsWith("*"))
                                {
                                    r.Add(new IFilter
                                    {
                                        type = IFilterType.End,
                                        data = new List<string> { it.TrimStart('*') }
                                    });
                                }
                                else if (it.EndsWith("*"))
                                {
                                    r.Add(new IFilter
                                    {
                                        type = IFilterType.Start,
                                        data = new List<string> { it.TrimEnd('*') }
                                    });
                                }
                                else
                                {
                                    r.Add(new IFilter
                                    {
                                        type = IFilterType.OR,
                                        data = txt_arr
                                    });
                                }
                            }
                        }
                        else
                        {
                            r.Add(new IFilter
                            {
                                type = IFilterType.All,
                                data = new List<string> { it }
                            });
                        }
                    }
                }
                return r;
            }
            return new List<IFilter>();
        }

        internal static List<string> ArrSplit(this string it, char c, out int len)
        {
            var txt_arr = new List<string>();
            var arr = it.Split(c);
            len = arr.Length;
            foreach (var item in arr)
            {
                string its = item.Trim();
                if (!string.IsNullOrEmpty(its)) txt_arr.Add(its);
            }
            return txt_arr;
        }


        #endregion

        internal static bool filter(this string key, List<string> filter)
        {
            foreach (var it in filter)
            {
                if (key.Contains(it)) return true;
            }
            return false;
        }
        internal static bool ifilter(this string key, List<IFilter> filter)
        {
            foreach (var it in filter)
            {
                if (it.type == IFilterType.All)
                {
                    foreach (var d in it.data)
                    {
                        if (d == key) return false;
                    }
                }
                else if (it.type == IFilterType.Start)
                {
                    int count = 0;
                    foreach (var d in it.data)
                    {
                        if (key.StartsWith(d)) count++;
                    }
                    if (count == it.data.Count) return false;
                }
                else if (it.type == IFilterType.End)
                {
                    int count = 0;
                    foreach (var d in it.data)
                    {
                        if (key.EndsWith(d)) count++;
                    }
                    if (count == it.data.Count) return false;
                }
                else
                {
                    int count = 0;
                    foreach (var d in it.data)
                    {
                        if (key.Contains(d)) count++;
                    }
                    if (count == it.data.Count) return false;
                }
            }
            return true;
        }

        static Segment segment = new Segment();
        internal static bool ifilter_in(this string key)
        {
            if (key.Length < 2) return true;
            if (int.TryParse(key.TrimEnd('.'), out _)) return true;//过滤前面数字的
            var words = segment.DoSegment(key);
            if (int.TryParse(words.First().Word.TrimEnd('.'), out _)) return true;//过滤前面数字的
            foreach (var item in words)
            {
                if (item.Word == "DJ" || item.Word.Contains("舞曲") || item.Word.Contains("歌")) return true;
            }
            return false;
        }

        internal static List<IFilter> filters = new List<IFilter>();
        internal static List<IFilter> filterurls = new List<IFilter>();

        internal static void xtxt(this string path, ref Dictionary<string, List<string>> dir)
        {
            var arr = File.ReadAllLines(path);
            foreach (var ite in arr)
            {
                string it = ite.Trim();
                if (!string.IsNullOrEmpty(it))
                {
                    if (it.StartsWith("#EXTM3U") || it.StartsWith("#EXTINF:")) return;
                    var arr_ = it.Split(',');
                    if (arr_.Length > 1)
                    {
                        string name = arr_[0].Trim(), uri = arr_[arr_.Length - 1].Trim();
                        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(uri) && !uri.StartsWith("#") && name.ToLower().ifilter(filters) && uri.ToLower().ifilter(filterurls))
                        {
                            System.Diagnostics.Debug.WriteLine(name.ToLower());
                            if (name.ifilter_in()) continue;
                            if (uri.StartsWith("ttp://") || uri.StartsWith("ttps://")) uri = "h" + uri;

                            if (Uri.TryCreate(uri, UriKind.Absolute, out _))
                            {
                                bool ok = false;
                                if (uri.StartsWith("http"))
                                {
                                    ok = TestHttp(uri);
                                }
                                else if (uri.StartsWith("rtsp"))
                                {
                                    ok = false;
                                }
                                else if (uri.StartsWith("rtmp"))
                                {
                                    ok = false;
                                }
                                else
                                {
                                    ok = false;
                                }

                                if (ok)
                                {
                                    if (dir.ContainsKey(name)) { if (!dir[name].Contains(uri)) dir[name].Add(uri); }
                                    else dir.Add(name, new List<string> { uri });
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static bool TestHttp(string uri)
        {
            try
            {
                bool run = true;
                var c = new CancellationTokenSource();
                Task.Run(() =>
                {
                    Thread.Sleep(1000);
                    if (run)
                        c.Cancel();
                });
                var web = new HttpClient().GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, c.Token).Result;
                run = false;
                if (web.IsSuccessStatusCode)
                {
                    return true;
                }
            }
            catch { }
            return false;
        }
        internal static List<Model> xtxt(this Dictionary<string, List<string>> dir)
        {
            var r = new List<Model>();
            foreach (var item in dir)
            {
                r.Add(new Model
                {
                    name = item.Key,
                    uri = item.Value
                });
            }
            return r;
        }
    }

    public class Model
    {
        public string name { get; set; }
        public List<string> uri { get; set; }

        public override string ToString()
        {
            return name + "," + uri.Count + "," + uri[0];
        }
    }
    public class IFilter
    {
        public IFilterType type { get; set; }
        public List<string> data { get; set; }
        public override string ToString()
        {
            switch (type)
            {
                case IFilterType.All:
                    return "all=" + data[0];
                case IFilterType.Start:
                    return data[0] + "*";
                case IFilterType.End:
                    return "*" + data[0];
                default:
                    return string.Join(" OR ", data);
            }
        }
    }
    public enum IFilterType
    {
        All,
        Start,
        End,
        OR
    }
}
