using System.Windows;

namespace Arcanum.UI.Components.UserControls.MainMenuScreen;

public partial class FeatureCard
{
   public FeatureCard()
   {
      InitializeComponent();
   }

   public string Title
   {
      get => (string)GetValue(TitleProperty);
      set => SetValue(TitleProperty, value);
   }

   public static readonly DependencyProperty TitleProperty =
      DependencyProperty.Register(nameof(Title), typeof(string), typeof(FeatureCard), new("Feature Title"));

   public string Description
   {
      get => (string)GetValue(DescriptionProperty);
      set => SetValue(DescriptionProperty, value);
   }

   public static readonly DependencyProperty DescriptionProperty =
      DependencyProperty.Register(nameof(Description), typeof(string), typeof(FeatureCard), new("Feature Description"));
}