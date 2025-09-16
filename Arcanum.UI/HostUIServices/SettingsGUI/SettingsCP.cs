// using Arcanum.UI.GeneralUiElements.InputConfirmationControls;
// using Acnc = Arcanum.UI.AcnColors.AcnColors;
//
// namespace Arcanum.UI.HostUIServices.SettingsGUI;
//
// /// <summary>
// /// Provides methods for creating any control used in the plugin AgsSettings UI.
// /// </summary>
// public static class SettingsCP
// {
//     public static TabPage CreateSettingsTabPage(string pluginName)
//     {
//         var tabPage = new TabPage(pluginName)
//         {
//             Name = $"Settings_{pluginName}",
//             Text = pluginName,
//             Tag = pluginName,
//             ToolTipText = $"AgsSettings for {pluginName}",
//             BackColor = Acnc.DefaultBackground,
//             ForeColor = Acnc.DefaultText,
//             BorderStyle = BorderStyle.FixedSingle,
//         };
//         
//         var flowLayoutPanel = new FlowLayoutPanel
//         {
//             Dock = DockStyle.Fill,
//             AutoScroll = true,
//             BackColor = Acnc.DefaultBackground,
//             ForeColor = Acnc.DefaultText,
//             FlowDirection = FlowDirection.TopDown,
//             WrapContents = false,
//             Padding = new (0, 0, 10, 0),
//         };
//
//         flowLayoutPanel.SizeChanged += (_, _) =>
//         {
//             flowLayoutPanel.SuspendLayout();
//             foreach (Control ctrl in flowLayoutPanel.Controls)
//                 ctrl.Width = flowLayoutPanel.ClientSize.Width - 10;
//             flowLayoutPanel.ResumeLayout();
//         };
//         
//         tabPage.Controls.Add(flowLayoutPanel);
//         return tabPage;
//     }
//
//     /// <summary>
//     /// Creates a control of the type that matches the specified value
//     /// and returns it along with its type.
//     /// Supported types include: int, long, float, double, string, bool, and Enum.
//     /// </summary>
//     /// <typeparam name="T">The type of the value used to determine the control type.</typeparam>
//     /// <param name="value">The value to match against for creating the appropriate control.</param>
//     /// <returns>A tuple containing the created control and its type.</returns>
//     public static Control CreateControlOfMatchingType<T>(T value) where T : notnull
//     {
//         if (value is null)
//             throw new ArgumentNullException(nameof(value), "Value cannot be null.");
//         
//         IConfirmedInput control;
//         var type = value.GetType();
//
//         if (type == typeof(int) || type == typeof(long))
//             control = new ConfirmNumeric
//             {
//                 Minimum = int.MinValue,
//                 Maximum = int.MaxValue,
//                 Value = Convert.ToDecimal(value)
//             };
//         else if (type == typeof(float) || type == typeof(double))
//             control = new ConfirmNumeric
//             {
//                 DecimalPlaces = 2,
//                 Minimum = decimal.MinValue,
//                 Maximum = decimal.MaxValue,
//                 Value = Convert.ToDecimal(value)
//             };
//         else if (type == typeof(string))
//             control = new ConfirmTextBox
//             {
//                 Text = value.ToString()
//             };
//         else if (type == typeof(bool))
//             control = new ConfirmCheckBox
//             {
//                 Checked = Convert.ToBoolean(value)
//             };
//         else if (type.IsEnum)
//             control = new ConfirmComboBox
//             {
//                 DataSource = Enum.GetValues(type),
//                 SelectedItem = value,
//                 DropDownStyle = ComboBoxStyle.DropDownList
//             };
//         else
//             throw new NotSupportedException($"Type {type.Name} is not supported for control creation.");
//
//         ((Control)control).BackColor = Acnc.DefaultBackground;
//         ((Control)control).ForeColor = Acnc.DefaultText;
//         ((Control)control).Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
//         return (Control)control;
//     }
//
//     public static TableLayoutPanel CreateNameAndDescriptionPanel<T>(string name, T value, string? description,
//                                                                     ToolTip toolTip) where T : notnull
//     {
//         var panel = new TableLayoutPanel
//         {
//             ColumnCount = 2,
//             RowCount = 1,
//             Dock = DockStyle.None,
//             BackColor = Acnc.DefaultBackground,
//             ForeColor = Acnc.DefaultText,
//         };
//         
//         panel.ColumnStyles.Add(new (SizeType.Percent, 50));
//         panel.ColumnStyles.Add(new (SizeType.Percent, 50));
//         
//         panel.RowStyles.Add(new (SizeType.Absolute, 27));
//         
//         panel.Height =27;
//         
//         var nameLabel = new Label
//         {
//             Text = name,
//             Dock = DockStyle.Fill,
//             TextAlign = ContentAlignment.MiddleLeft,
//             BackColor = Acnc.DefaultBackground,
//             ForeColor = Acnc.DefaultText
//         };
//         
//         var control = CreateControlOfMatchingType(value);
//         
//         toolTip.SetToolTip(control, description);
//         toolTip.SetToolTip(nameLabel, description);
//         
//         panel.Controls.Add(nameLabel, 0, 0);
//         panel.Controls.Add(control, 1, 0);
//
//         return panel;
//     }
// }
//

