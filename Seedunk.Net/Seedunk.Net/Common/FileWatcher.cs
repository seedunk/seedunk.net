using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace  Seedunk.Net.Common
{
    
   
    /// <summary>
    /// 视频文件侦听
    /// </summary>
    public class FileWatcher : IDisposable
    {
        #region 私有字段

        private FileSystemWatcher _watcher;
        private string _currentMonitorPath;
        private readonly ConcurrentQueue<FileChangeEvent> _eventQueue;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task _processTask;
        private bool _isDisposed;
        private readonly object _lockObject = new object();
        private bool _isRunning;

        // 事件委托
        public event EventHandler<FileChangeEventArgs> FileChanged;
        public event EventHandler<Exception> OnError;
        public event EventHandler<string> OnPathSwitched;

        #endregion

        #region 构造函数

        /// <summary>
        /// 初始化文件监控服务
        /// </summary>
        /// <param name="monitorPath">要监控的目录路径</param>
        public FileWatcher()
        {


            // 启动事件处理线程
            _eventQueue = new ConcurrentQueue<FileChangeEvent>();
            _cancellationTokenSource = new CancellationTokenSource();
             


        }

        #endregion

        #region 初始化与配置

        /// <summary>
        /// 初始化文件监控器
        /// </summary>
        private void InitializeWatcher()
        {
            _watcher = new FileSystemWatcher
            {
                Path = _currentMonitorPath,
                IncludeSubdirectories = true,
                NotifyFilter = NotifyFilters.FileName
                              | NotifyFilters.DirectoryName
                              | NotifyFilters.LastWrite
                              | NotifyFilters.CreationTime
                              | NotifyFilters.Size
                              | NotifyFilters.Attributes,
                Filter = "*.*",
                EnableRaisingEvents = _isRunning
            };

            // 注册事件
            _watcher.Created += OnFileChanged;
            _watcher.Changed += OnFileChanged;
            _watcher.Deleted += OnFileChanged;
            _watcher.Renamed += OnFileRenamed;
            _watcher.Error += OnWatcherError;
        }

        /// <summary>
        /// 设置是否监控子目录
        /// </summary>
        public void SetIncludeSubdirectories(bool include)
        {
            lock (_lockObject)
            {
                if (_watcher != null)
                {
                    _watcher.IncludeSubdirectories = include;
                }
            }
        }

        /// <summary>
        /// 设置文件筛选器
        /// </summary>
        /// <param name="filter">例如 "*.txt", "*.log"</param>
        public void SetFilter(string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                filter = "*.*";

            lock (_lockObject)
            {
                if (_watcher != null)
                {
                    _watcher.Filter = filter;
                }
            }
        }

        /// <summary>
        /// 设置监控的变更类型
        /// </summary>
        public void SetNotifyFilter(NotifyFilters filters)
        {
            lock (_lockObject)
            {
                if (_watcher != null)
                {
                    _watcher.NotifyFilter = filters;
                }
            }
        }

        #endregion

        #region 事件处理

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (_isDisposed) return;

            var changeEvent = new FileChangeEvent
            {
                ChangeType = e.ChangeType,
                FullPath = e.FullPath,
                Name = e.Name,
                Timestamp = DateTime.Now
            };

            _eventQueue.Enqueue(changeEvent);
        }

        private void OnFileRenamed(object sender, RenamedEventArgs e)
        {
            if (_isDisposed) return;

            var changeEvent = new FileChangeEvent
            {
                ChangeType = e.ChangeType,
                FullPath = e.FullPath,
                OldFullPath = e.OldFullPath,
                Name = e.Name,
                OldName = e.OldName,
                Timestamp = DateTime.Now
            };

            _eventQueue.Enqueue(changeEvent);
        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            OnError?.Invoke(this, e.GetException());
        }

        #endregion

        #region 事件处理循环

        private async Task ProcessEventsAsync()
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    if (_eventQueue.TryDequeue(out var changeEvent))
                    {
                        // 触发事件通知
                        FileChanged?.Invoke(this, new FileChangeEventArgs(changeEvent));
                    }
                    else
                    {
                        await Task.Delay(50, _cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    OnError?.Invoke(this, ex);
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 切换监控目录
        /// </summary>
        /// <param name="newMonitorPath">新的监控目录路径</param>
        /// <param name="clearQueue">是否清空待处理的事件队列</param>
        public bool TryChangePath(string newMonitorPath, bool clearQueue = false)
        {

            if (!Directory.Exists(newMonitorPath))
            {
              
                return false;
            }



            lock (_lockObject)
            {
                string fullPath = Path.GetFullPath(newMonitorPath);

                // 检查是否相同路径
                if (string.Equals(_currentMonitorPath, fullPath, StringComparison.OrdinalIgnoreCase))
                    return false;

                // 停止当前监控
                StopMonitoring();

                // 清空事件队列
                if (clearQueue)
                {
                    while (_eventQueue.TryDequeue(out _)) { }
                }

                // 更新监控路径
                _currentMonitorPath = fullPath;

                // 重新初始化监控器
                InitializeWatcher();
                if (_processTask == null)
                    _processTask = Task.Run(ProcessEventsAsync, _cancellationTokenSource.Token);

                StartMonitoring();

                // 触发路径切换事件
                if (OnPathSwitched != null)
                    OnPathSwitched?.Invoke(this, _currentMonitorPath);
            }
            return true;
        }

        /// <summary>
        /// 暂停监控
        /// </summary>
        public void Pause()
        {
            lock (_lockObject)
            {
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _isRunning = false;
                }
            }
        }

        /// <summary>
        /// 恢复监控
        /// </summary>
        public void Resume()
        {
            lock (_lockObject)
            {
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = true;
                    _isRunning = true;
                }
            }
        }

        /// <summary>
        /// 停止监控服务
        /// </summary>
        public void StopMonitoring()
        {
            lock (_lockObject)
            {
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = false;
                    _isRunning = false;
                }
            }
        }

        /// <summary>
        /// 启动监控服务
        /// </summary>
        public void StartMonitoring()
        {
            lock (_lockObject)
            {
                if (_watcher != null)
                {
                    _watcher.EnableRaisingEvents = true;
                    _isRunning = true;
                }
            }
        }

        /// <summary>
        /// 检查服务是否正在运行
        /// </summary>
        public bool IsRunning()
        {
            lock (_lockObject)
            {
                return _isRunning && _watcher != null;
            }
        }

        /// <summary>
        /// 获取当前监控路径
        /// </summary>
        public string GetCurrentMonitorPath()
        {
            return _currentMonitorPath;
        }

        /// <summary>
        /// 获取当前监控配置信息
        /// </summary>
        public MonitorInfo GetMonitorInfo()
        {
            lock (_lockObject)
            {
                return new MonitorInfo
                {
                    MonitorPath = _currentMonitorPath,
                    IsRunning = _isRunning,
                    Filter = _watcher?.Filter ?? "*.*",
                    IncludeSubdirectories = _watcher?.IncludeSubdirectories ?? true,
                    PendingEvents = _eventQueue.Count
                };
            }
        }

        /// <summary>
        /// 清空待处理的事件队列
        /// </summary>
        public void ClearEventQueue()
        {
            while (_eventQueue.TryDequeue(out _)) { }
        }

        #endregion

        #region 辅助方法

        private static void EnsureDirectoryExists(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        #endregion

        #region IDisposable 实现

        public void Dispose()
        {
            if (_isDisposed) return;

            _isDisposed = true;

            // 取消任务
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();

            // 停止监控器
            StopMonitoring();

            // 释放监控器
            if (_watcher != null)
            {
                _watcher.Created -= OnFileChanged;
                _watcher.Changed -= OnFileChanged;
                _watcher.Deleted -= OnFileChanged;
                _watcher.Renamed -= OnFileRenamed;
                _watcher.Error -= OnWatcherError;
                _watcher.Dispose();
                _watcher = null;
            }

            // 等待处理任务完成
            try
            {
                _processTask?.Wait(TimeSpan.FromSeconds(3));
            }
            catch
            {
                // 忽略超时异常
            }

            _processTask = null;

            // 清空事件队列
            while (_eventQueue.TryDequeue(out _)) { }

            GC.SuppressFinalize(this);
        }

        #endregion
    }

    #region 辅助类

    /// <summary>
    /// 文件变更事件数据
    /// </summary>
    public class FileChangeEvent
    {
        public WatcherChangeTypes ChangeType { get; set; }
        public string FullPath { get; set; }
        public string Name { get; set; }
        public string OldFullPath { get; set; }
        public string OldName { get; set; }
        public DateTime Timestamp { get; set; }

        public override string ToString()
        {
            string info = $"{ChangeType}: {Name}";
            if (!string.IsNullOrEmpty(OldName))
                info += $" (原名称: {OldName})";
            info += $" at {Timestamp:HH:mm:ss.fff}";
            return info;
        }
    }

    /// <summary>
    /// 文件变更事件参数
    /// </summary>
    public class FileChangeEventArgs : EventArgs
    {
        public FileChangeEvent ChangeEvent { get; }

        public FileChangeEventArgs(FileChangeEvent changeEvent)
        {
            ChangeEvent = changeEvent;
        }
    }

    /// <summary>
    /// 监控信息
    /// </summary>
    public class MonitorInfo
    {
        public string MonitorPath { get; set; }
        public bool IsRunning { get; set; }
        public string Filter { get; set; }
        public bool IncludeSubdirectories { get; set; }
        public int PendingEvents { get; set; }
    }

    #endregion
}