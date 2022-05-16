using System.Windows.Input;

namespace UI.ViewModel {
    public static class CustomCommands {
		public static readonly RoutedUICommand Escape = new(
			"Escape",
			"Escape",
			typeof(CustomCommands),
			new InputGestureCollection() { new KeyGesture(Key.Escape) }
		);

		public static readonly RoutedUICommand Left = new(
			"PreviousPicture",
			"PreviousPicture",
			typeof(CustomCommands),
			new InputGestureCollection() {
				new KeyGesture(Key.Left)
			}
		);

		public static readonly RoutedUICommand Right = new(
			"NextPicture",
			"NextPicture",
			typeof(CustomCommands),
			new InputGestureCollection() {
				new KeyGesture(Key.Right)
			}
		);

		public static readonly RoutedUICommand Space = new(
			"Space",
			"Space",
			typeof(CustomCommands),
			new InputGestureCollection() {
				new KeyGesture(Key.Space)
			}
		);
	}
}
