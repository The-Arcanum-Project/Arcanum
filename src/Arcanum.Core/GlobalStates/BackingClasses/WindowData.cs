namespace Arcanum.Core.GlobalStates.BackingClasses;

public class WindowData
{
   public List<WindowStateData> WindowStateData { get; set; } = [];

   public static void AddWindowStateData(WindowStateData data)
   {
      var existingData = Config.WindowData.WindowStateData.FirstOrDefault(w => w.WindowName == data.WindowName);
      if (existingData != null)
      {
         existingData.Left = data.Left;
         existingData.Top = data.Top;
         existingData.Width = data.Width;
         existingData.Height = data.Height;
         existingData.WindowState = data.WindowState;
      }
      else
         Config.WindowData.WindowStateData.Add(data);
   }

   public static WindowStateData? GetWindowStateData(Type windowType)
   {
      var windowName = windowType.Name;
      return Config.WindowData.WindowStateData.FirstOrDefault(w => w.WindowName == windowName);
   }
}

public class WindowStateData
{
   public WindowStateData(string windowName, double left, double top, double width, double height, int windowState)
   {
      WindowName = windowName;
      Left = left;
      Top = top;
      Width = width;
      Height = height;
      WindowState = windowState;
   }

   public WindowStateData()
   {
   }

   public string WindowName { get; set; } = string.Empty;
   public double Left { get; set; }
   public double Top { get; set; }
   public double Width { get; set; }
   public double Height { get; set; }
   public int WindowState { get; set; }
}