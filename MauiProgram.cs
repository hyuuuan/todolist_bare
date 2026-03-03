using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;

namespace ToDoMaui_Listview;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

#if IOS
		// Remove the keyboard toolbar (Done/checkmark button) on iOS
		EntryHandler.Mapper.AppendToMapping("NoInputAccessory", (handler, view) =>
		{
			handler.PlatformView.InputAccessoryView = null;
		});
		EditorHandler.Mapper.AppendToMapping("NoInputAccessory", (handler, view) =>
		{
			handler.PlatformView.InputAccessoryView = null;
		});
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
