# AccurateTimer
 
 C＃自带的定时器（System.Timers.Timer, System.Threading.Timer和System.Windows.Form.Timer）一般延迟在15ms左右，实时性很差且很不稳定， 
 
 因为WINDOWS是多任务的操作系统，定时器的延时可能是由于其它任务占用CPU资源，没能及时响应定时器事件，如果能够改变定时器线程的优先级应该也可以改善定时器的精度。


使用WIN32API函数
  
/// 
/// Pointer to a variable that receives the current performance-counter value, in counts. 
/// 
/// 
/// If the function succeeds, the return value is nonzero. 
/// 
[DllImport("Kernel32.dll")] 
private static extern bool QueryPerformanceFrequency(out long lpPerformanceCount);
 
 
/// Pointer to a variable that receives the current performance-counter frequency, 
/// in counts per second. 
/// If the installed hardware does not support a high-resolution performance counter, 
/// this parameter can be zero. 
/// 
/// 
/// If the installed hardware supports a high-resolution performance counter, 
/// the return value is nonzero. 
/// 
[DllImport("Kernel32.dll")] 
private static extern bool QueryPerformanceFrequency(out  long lpFrequency);
 
 QueryPerformanceFrequency 函数是读取系统所支持的硬件定时器计数频率，也就是每秒的计数值，QueryPerformanceFrequency 应该是从开机时的计数值。这样通过读取两次计数值之差，再除以计数频率也就是两次读取时钟间隔的秒数。然后通过查询两次的间隔来确定是否响应服务函数。

  

  如果系统对实时性要求比较高，可以不对定时器进行封装，将定时器的主进程当作控制器采样的主进程，减少函数调用造成的CPU资源浪费，精度会更高。
 
 **参考**
 
 1. [C#实现高精度定时器](https://blog.csdn.net/dd_zhouqian/article/details/84529992)