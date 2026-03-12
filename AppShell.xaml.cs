namespace ToDoMaui_Listview;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		Routing.RegisterRoute(nameof(AddToDoPage), typeof(AddToDoPage));
		Routing.RegisterRoute(nameof(EditToDoPage), typeof(EditToDoPage));
		Routing.RegisterRoute(nameof(EditCompletedPage), typeof(EditCompletedPage));
	}
}
