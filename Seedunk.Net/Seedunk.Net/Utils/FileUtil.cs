
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

namespace  Seedunk.Net.Utils
{
    public class FileCopyProgress
    {
        /// <summary>
        /// 当前已复制的文件数
        /// </summary>
        public int CopiedFiles { get; set; }

        /// <summary>
        /// 总文件数
        /// </summary>
        public int TotalFiles { get; set; }

        /// <summary>
        /// 当前正在复制的文件名
        /// </summary>
        public string CurrentFile { get; set; }

        /// <summary>
        /// 当前文件进度 (0-100)
        /// </summary>
        public int CurrentFileProgress { get; set; }

        /// <summary>
        /// 整体进度百分比 (0-100)
        /// </summary>
        public int OverallProgress => TotalFiles > 0 ? (CopiedFiles * 100 / TotalFiles) : 0;

        /// <summary>
        /// 已复制的字节数
        /// </summary>
        public long CopiedBytes { get; set; }

        /// <summary>
        /// 总字节数
        /// </summary>
        public long TotalBytes { get; set; }

        /// <summary>
        /// 整体字节进度百分比
        /// </summary>
        public int ByteProgress => TotalBytes > 0 ? (int)((CopiedBytes * 100.0) / TotalBytes) : 0;
    }
    public static class FileUtil
    {
        private static readonly int _bufferSize = 81920; // 80KB 缓冲区，适合大文件复制

        /// <summary>
        /// 异步复制目录，实时报告进度
        /// </summary>
        /// <param name="sourceDir">源目录</param>
        /// <param name="destDir">目标目录</param>
        /// <param name="onProgress">进度回调</param>
        /// <param name="cancellationToken">取消令牌</param>
        /// <param name="overwrite">是否覆盖已存在文件</param>
        /// <param name="excludePatterns">排除的文件模式（如 *.tmp, *.log）</param>
        public static async Task CopyAsync(
            string sourceDir,
            string destDir,
            Action<FileCopyProgress> onProgress = null,
            CancellationToken cancellationToken = default,
            bool overwrite = false,
            List<string> excludePatterns = null)
        {
            // 1. 先扫描所有文件，计算总数和总大小
            var fileList = new List<FileInfo>();
            long totalBytes = 0;

            await Task.Run(() => ScanDirectory(sourceDir, fileList, excludePatterns));
            totalBytes = fileList.Sum(f => f.Length);

            if (fileList.Count == 0)
            {
                // 没有文件，直接创建目标目录
                Directory.CreateDirectory(destDir);
                onProgress?.Invoke(new FileCopyProgress { TotalFiles = 0, TotalBytes = 0 });
                return;
            }

            // 2. 初始化进度对象
            var progress = new FileCopyProgress
            {
                TotalFiles = fileList.Count,
                TotalBytes = totalBytes,
                CopiedFiles = 0,
                CopiedBytes = 0
            };

            // 3. 逐个复制文件
            foreach (var file in fileList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string relativePath = GetRelativePath(sourceDir, file.FullName);
                string destFile = Path.Combine(destDir, relativePath);

                // 确保目标目录存在
                Directory.CreateDirectory(Path.GetDirectoryName(destFile));

                // 更新进度：当前正在复制的文件
                progress.CurrentFile = relativePath;
                progress.CurrentFileProgress = 0;
                onProgress?.Invoke(progress);

                // 异步复制当前文件（带字节级进度）
                await CopyFileAsync(
                    file.FullName,
                    destFile,
                    overwrite,
                    (bytesCopied) =>
                    {
                        progress.CurrentFileProgress = file.Length > 0 ? (int)((bytesCopied * 100.0) / file.Length) : 100;
                        progress.CopiedBytes += bytesCopied; // 注意：这里增量更新
                        onProgress?.Invoke(progress);
                    },
                    cancellationToken
                );

                // 更新进度：一个文件完成
                progress.CopiedFiles++;
                onProgress?.Invoke(progress);
            }
        }

        /// <summary>
        /// 带字节进度反馈的文件异步复制
        /// </summary>
        private static async Task CopyFileAsync(
            string sourceFile,
            string destFile,
            bool overwrite,
            Action<long> onBytesCopied,
            CancellationToken cancellationToken)
        {
            // 检查目标文件是否存在
            if (!overwrite && File.Exists(destFile))
            {
                // 文件已存在且不覆盖，直接算复制完成
                onBytesCopied?.Invoke(new FileInfo(sourceFile).Length);
                return;
            }

            using var sourceStream = new FileStream(
                sourceFile,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                _bufferSize,
                FileOptions.Asynchronous);

            using var destStream = new FileStream(
                destFile,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                _bufferSize,
                FileOptions.Asynchronous);

            byte[] buffer = new byte[_bufferSize];
            long totalBytesCopied = 0;
            int bytesRead;

            while ((bytesRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                await destStream.WriteAsync(buffer, 0, bytesRead, cancellationToken);

                totalBytesCopied += bytesRead;
                onBytesCopied?.Invoke(totalBytesCopied);
            }
        }

        /// <summary>
        /// 递归扫描目录，收集所有文件
        /// </summary>
        private static void ScanDirectory(string dir, List<FileInfo> fileList, List<string> excludePatterns)
        {
            var directoryInfo = new DirectoryInfo(dir);

            // 添加当前目录的所有文件（过滤排除项）
            foreach (var file in directoryInfo.GetFiles())
            {
                if (!ShouldExclude(file.Name, excludePatterns))
                {
                    fileList.Add(file);
                }
            }

            // 递归扫描子目录
            foreach (var subDir in directoryInfo.GetDirectories())
            {
                ScanDirectory(subDir.FullName, fileList, excludePatterns);
            }
        }

        /// <summary>
        /// 获取相对路径
        /// </summary>
        private static string GetRelativePath(string baseDir, string fullPath)
        {
            if (!fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFileName(fullPath);
            }
            return fullPath.Substring(baseDir.Length).TrimStart(Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// 判断文件是否应该被排除
        /// </summary>
        private static bool ShouldExclude(string fileName, List<string> patterns)
        {
            if (patterns == null || patterns.Count == 0) return false;

            foreach (string pattern in patterns)
            {
                // 通配符匹配（*.tmp）
                if (pattern.StartsWith("*."))
                {
                    string extension = pattern.Substring(1); // 得到 ".tmp"
                    if (fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
                // 精确匹配
                else if (fileName.Equals(pattern, StringComparison.OrdinalIgnoreCase))
                    return true;
                // 复杂通配符（file*.txt）
                else if (pattern.Contains("*") || pattern.Contains("?"))
                {
                    string regexPattern = "^" + pattern.Replace(".", "\\.").Replace("*", ".*").Replace("?", ".") + "$";
                    if (System.Text.RegularExpressions.Regex.IsMatch(fileName, regexPattern,
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        return true;
                }
            }
            return false;
        }
    }
}
