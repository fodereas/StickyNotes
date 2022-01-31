﻿using System;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;

/// <summary>
/// 自动更新程序
/// </summary>

namespace AutoUpdateTool
{
    internal class Program
    {
        // 字符串版本转换成对应的整型版本 每个版本位置为4位
        // 例如 v3.3.3 转换为 300030003000再进行比较
        public static int LocalVersion;
        public static int GithubVersion;
        public static string StickyNotesExePath;

        /// <summary>
        /// </summary>
        /// <param name="args">1、版本 2、StickyNotes.exe路径</param>
        private static void Main(string[] args)
        {
            if (args.Length == 2)
            {
                // 当前版本
                LocalVersion = ConvertVersion(args[0]);
                StickyNotesExePath = args[1];
                Console.WriteLine("当前版本" + args[0]);
                Console.WriteLine("" + StickyNotesExePath);
            }
            else
            {
                LocalVersion = ConvertVersion("v1.1.1");
                StickyNotesExePath = "D:\\StikyNotes\\StikyNotes\\bin\\Debug\\StikyNotes.exe";
                Console.WriteLine("参数不正确,程序退出");
                //                return;
            }

            var res = WebUtil.HttpGet(RepositoryInfo.RepositoryAddr);
            var responseResult = res.Result;

            var model = JsonConvert.DeserializeObject<GithubReleaseModel>(responseResult);
            GithubVersion = ConvertVersion(model.tag_name);
            Console.WriteLine("查询到Github最新版本：" + GithubVersion);
            if (GithubVersion <= LocalVersion) return;
            // 下载新版本进行替换
            // 思路：
            // 用StickyNotes启动本程序
            // 1. 关闭StickyNotes.exe程序
            // 2. 然后删除本地的StickyNotes.exe
            // 3. 启动本地的StickyNotes.exe程序
            // 4. 退出本脚本
            Console.WriteLine("发现新版本，开始关闭进程");
            KillProcess("STICKYNOTES");
            Console.WriteLine("发现Github上有新版本，正在下载中，长时间未下载完成请科学上网");
            string downloadUrl = "";
            foreach (var item in model.assets)
            {
                if (item.name.ToLower().Contains("StickyNotes".ToLower()))
                {
                    downloadUrl = item.browser_download_url;
                    break;
                }
            }
            var result = WebUtil.HttpDownLoadFile(downloadUrl, "StickyNotes.exe");
            try
            {
                if (result.Result)
                    Console.WriteLine("下载完成......");

            }
            catch (Exception ex)
            {
                Console.WriteLine("下载过程中出现异常，可能是网络连接不上");
                Console.WriteLine(ex.Message + ex.StackTrace);
                Console.WriteLine("按Enter回车退出更新（多次更新不上请将更新程序删掉后再打开StickyNotes）");
                Console.ReadLine();
            }
            RunProcess(StickyNotesExePath);
        }
        public static void KillProcess(string strProcessesByName)//关闭线程
        {
            foreach (Process p in Process.GetProcesses())//GetProcessesByName(strProcessesByName))
            {
                if (p.ProcessName.ToUpper().Contains(strProcessesByName))
                {
                    try
                    {
                        p.Kill();
                        p.WaitForExit(); // possibly with a timeout
                    }
                    catch (Exception e)
                    {
                    }
                }
            }
        }
        public static void RunProcess(string filePath, string argument = "")
        {
            var p = new Process();

            p.StartInfo.FileName = filePath; // "iexplore.exe";   //IE
            p.StartInfo.Arguments = argument;
            p.Start();
        }

        private static int ConvertVersion(string modelTagName)
        {
            modelTagName = modelTagName.Substring(1);
            var versions = modelTagName.Split('.');
            var result = 0;
            foreach (var version in versions) result = result * 1000 + int.Parse(version);

            return result;
        }
    }
}