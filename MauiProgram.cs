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

		// Keep wrapped inputs visually clean by removing native field borders on iOS.
		EntryHandler.Mapper.AppendToMapping("BorderlessInput", (handler, view) =>
		{
			handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
			handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
		});
		EditorHandler.Mapper.AppendToMapping("BorderlessInput", (handler, view) =>
		{
			handler.PlatformView.BackgroundColor = UIKit.UIColor.Clear;
			handler.PlatformView.Layer.BorderWidth = 0;
		});
#endif

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
