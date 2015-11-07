using System;
using System.Linq;
using CefSharp;
using System.Windows.Forms;
using MP3CutAd.App.Controls;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using MP3CutAd.Core;

namespace MP3CutAd.App {
    class VersionInfo {
        public string Chromium { get; set; }
        public string Cef { get; set; }
        public string CefSharp { get; set; }
        public string Bitness { get; set; }
    }

    /// <summary>
    /// 用于为JS提供C#接口的Bound对象
    /// </summary>
    class BoundObject {
        private MainForm form;

        public BoundObject(MainForm form) {
            this.form = form;
        }

        /// <summary>
        /// 显示一些框架的版本信息
        /// </summary>
        public VersionInfo Versions { get; set; }

        /// <summary>
        /// 显示DevTools
        /// </summary>
        public void ShowDevTools() {
            form.Browser.ShowDevTools();
        }

        /// <summary>
        /// 打开目录选择对话框
        /// </summary>
        /// <param name="filter">文件过滤器：例 *.mp3</param>
        /// <param name="callback">回调，返回该目录内所有符合过滤规则的文件（不递归）</param>
        public void OpenDirectory(string filter, IJavascriptCallback callback) {
            var dialog = new FolderBrowserDialog();
            var result = form.Invoke<DialogResult>(dialog.ShowDialog);

            var fileNames = new string[0];
            if (result == DialogResult.OK) {
                var dir = new DirectoryInfo(dialog.SelectedPath);
                fileNames = dir.GetFiles(filter, SearchOption.TopDirectoryOnly)
                                .Select(f => f.FullName)
                                .ToArray();
            }

            callback.ExecuteJsonAsync(null, fileNames);
        }

        public void SelectDirectory(IJavascriptCallback callback) {
            var dialog = new FolderBrowserDialog();
            var result = form.Invoke<DialogResult>(dialog.ShowDialog);

            var dir = "";
            if (result == DialogResult.OK) {
                dir = dialog.SelectedPath;
            }

            callback.ExecuteJsonAsync(null, dir);
        }

        /// <summary>
        /// 打开文件选择对话框
        /// </summary>
        /// <param name="title">标题</param>
        /// <param name="filter">过滤器：例 音频文件(*.mp3)|*.mp3</param>
        /// <param name="callback">回调，返回选中的所有文件</param>
        public void OpenFile(string title, string filter, IJavascriptCallback callback) {
            var dialog = new OpenFileDialog();
            dialog.Multiselect = true;
            dialog.Title = title;
            dialog.Filter = filter;

            var result = form.Invoke<DialogResult>(dialog.ShowDialog);

            var fileNames = new string[0];
            if (result == DialogResult.OK) {
                fileNames = dialog.FileNames;
            }

            callback.ExecuteJsonAsync(null, fileNames);
        }

        public void DetectAD(string json, IJavascriptCallback callback, IJavascriptCallback progress) {
            var task = new Task(() => {
                var files = JsonConvert.DeserializeObject<string[]>(json);

                var notify = new Action<float>(p => {
                    progress.ExecuteAsync(p);
                });

                var result = CutAD.DetectAD(files, notify);

                var ret = result.Select(kv => {
                    var ranges = kv.Key;
                    var length = kv.Value;
                    return new {
                        length = length * 100,
                        ads = ranges.Select(r => new {
                            gid = 0,
                            start = r.begin * 100,
                            end = r.end * 100,
                        }).ToArray()
                    };
                }).ToArray();

                callback.ExecuteJsonAsync(null, ret);
            });
            task.Start();
        }

        public void Cut(string json, string outputDirectory, IJavascriptCallback callback, IJavascriptCallback progress) {
            var task = new Task(() => {
                var example = new[] {
                    new {
                        fullname = "",
                        filename = "",
                        directory = "",
                        basename = "",
                        extname = "",
                        length = "",
                        ads = new[] {
                            new {
                                ignored = false,
                                gid = 0,
                                start = 0,
                                end = 0,
                            }
                        },
                    }
                };
                var files = JsonConvert.DeserializeAnonymousType(json, example);

                var args = files.ToDictionary(
                    file => file.fullname,
                    file => file.ads
                        .Where(ad => !ad.ignored)
                        .Select(ad => new Range(ad.start / 100, ad.end / 100))
                        .ToArray()
                );

                var notify = new Action<float>(p => {
                    progress.ExecuteAsync(p);
                });

                CutAD.Cut(args, outputDirectory, notify);

                callback.ExecuteJsonAsync(null, true);
            });
            task.Start();
        }
    }
}
