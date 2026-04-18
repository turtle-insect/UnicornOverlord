using System.Windows.Input;

namespace UnicornOverlord
{
	internal class ActionCommand : ICommand
	{
#pragma warning disable CS0067
		public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067
		private readonly Action<object?> mAction;

		public ActionCommand(Action<object?> action) => mAction = action;

		public bool CanExecute(object? parameter) => true;

		public void Execute(object? parameter) => mAction(parameter);
	}
}
