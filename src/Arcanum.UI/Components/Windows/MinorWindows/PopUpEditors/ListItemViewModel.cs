using CommunityToolkit.Mvvm.ComponentModel;

namespace Arcanum.UI.Components.Windows.MinorWindows.PopUpEditors;

public class ListItemViewModel<T> : ViewModelBase where T : notnull
{
   public T Value { get; }
   public EditState InitialState { get; }
   private EditState _state;
   public EditState State
   {
      get => _state;
      set => SetProperty(ref _state, value);
   }
   public string DisplayText => Value.ToString() ?? "null";

   public ListItemViewModel(T value, EditState initialState)
   {
      Value = value;
      InitialState = initialState;
      _state = initialState;
   }
}