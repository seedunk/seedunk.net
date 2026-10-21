using System; 

namespace Seedunk.Net
{
    public class ICommand<T> : System.Windows.Input.ICommand
    {
        //创建委托类型，用于后面接收构造函数里面的参数
        Action<T> executeAction;
        public ICommand(Action<T> action)
        {
            executeAction = action;
        }
        public event EventHandler CanExecuteChanged;
        //允许执行
        public bool CanExecute(object parameter)
        {
            return true;
        }
        //把Mycommand接收的委托参数用于执行
        public void Execute(object parameter)
        {
            if (parameter == null)
            { 
                executeAction(default(T)); 
            }
            else executeAction((T)parameter);

        }
    }

}
